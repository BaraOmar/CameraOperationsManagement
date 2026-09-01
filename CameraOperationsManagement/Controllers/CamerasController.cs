using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.Services;
using CameraOperationsManagement.ViewModels.Cameras;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Roles = "Admin,Editor,Viewer")]
    public class CamerasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public CamerasController(
            ApplicationDbContext context,
            IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }


        // =====================================================
        // INDEX
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cameras = await _context.Cameras
                .AsNoTracking()
                .OrderBy(c => c.Recorder.Site.Name)
                .ThenBy(c => c.Recorder.Name)
                .ThenBy(c => c.Name)
                .Select(c => new CameraListItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Brand = c.Brand,
                    Model = c.Model,
                    Type = c.Type,
                    IpAddress = c.IpAddress,

                    InstallationLocation =
                        c.InstallationLocation,

                    RecorderId = c.RecorderId,
                    RecorderName = c.Recorder.Name,

                    SiteId = c.Recorder.SiteId,
                    SiteName = c.Recorder.Site.Name,

                    NetworkSwitchName =
                        c.NetworkSwitch != null
                            ? c.NetworkSwitch.Name
                            : null,

                    IsActive = c.IsActive
                })
                .ToListAsync();

            return View(cameras);
        }


        // =====================================================
        // CREATE GET
        // =====================================================

        [Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadRecordersAsync();

            LoadEmptySwitchList();

            return View(new CameraFormViewModel());
        }


        // =====================================================
        // CREATE POST
        // =====================================================

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    CameraFormViewModel viewModel)
        {
            var recorder = await _context.Recorders
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.Id == viewModel.RecorderId &&
                    r.IsActive);

            if (recorder == null)
            {
                ModelState.AddModelError(
                    nameof(viewModel.RecorderId),
                    "Please select a valid active recorder.");
            }


            if (viewModel.NetworkSwitchId.HasValue &&
                recorder != null)
            {
                if (recorder.NetworkSwitchId !=
                    viewModel.NetworkSwitchId.Value)
                {
                    ModelState.AddModelError(
                        nameof(viewModel.NetworkSwitchId),
                        "The selected switch is not assigned to this recorder.");
                }
            }


            if (!ModelState.IsValid)
            {
                await LoadRecordersAsync(
                    viewModel.RecorderId);

                await LoadSwitchForRecorderAsync(
                    viewModel.RecorderId,
                    viewModel.NetworkSwitchId);

                return View(viewModel);
            }


            var camera = new Camera
            {
                Name = viewModel.Name.Trim(),

                Brand = Normalize(viewModel.Brand),

                Model = Normalize(viewModel.Model),

                SerialNumber =
                    Normalize(viewModel.SerialNumber),

                Type = Normalize(viewModel.Type),

                IpAddress =
                    Normalize(viewModel.IpAddress),

                InstallationLocation =
                    viewModel.InstallationLocation.Trim(),

                InstallationDate =
                    viewModel.InstallationDate,

                Notes =
                    Normalize(viewModel.Notes),

                RecorderId =
                    viewModel.RecorderId,

                NetworkSwitchId =
                    viewModel.NetworkSwitchId,

                IsActive = true
            };


            _context.Cameras.Add(camera);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Camera created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // EDIT GET
        // =====================================================

        [Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var camera = await _context.Cameras
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == id);

            if (camera == null)
            {
                return NotFound();
            }


            var model = new CameraFormViewModel
            {
                Id = camera.Id,
                Name = camera.Name,
                Brand = camera.Brand,
                Model = camera.Model,
                SerialNumber = camera.SerialNumber,
                Type = camera.Type,
                IpAddress = camera.IpAddress,

                InstallationLocation =
                    camera.InstallationLocation,

                InstallationDate =
                    camera.InstallationDate,

                Notes = camera.Notes,

                RecorderId =
                    camera.RecorderId,

                NetworkSwitchId =
                    camera.NetworkSwitchId
            };


            await LoadRecordersAsync(
                camera.RecorderId,
                camera.RecorderId);

            await LoadSwitchForRecorderAsync(
                camera.RecorderId,
                camera.NetworkSwitchId,
                camera.NetworkSwitchId);

            return View(model);
        }


        // =====================================================
        // EDIT POST
        // =====================================================

        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CameraFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }


            var camera = await _context.Cameras
                .FirstOrDefaultAsync(c =>
                    c.Id == id);

            if (camera == null)
            {
                return NotFound();
            }


            var recorder = await _context.Recorders
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.Id == viewModel.RecorderId &&
                    (
                        r.IsActive ||
                        r.Id == camera.RecorderId
                    ));

            if (recorder == null)
            {
                ModelState.AddModelError(
                    nameof(viewModel.RecorderId),
                    "Please select a valid recorder.");
            }


            if (viewModel.NetworkSwitchId.HasValue &&
                recorder != null)
            {
                if (recorder.NetworkSwitchId !=
                    viewModel.NetworkSwitchId.Value)
                {
                    ModelState.AddModelError(
                        nameof(viewModel.NetworkSwitchId),
                        "The selected switch is not assigned to this recorder.");
                }
                else
                {
                    var switchIsValid =
                        await _context.NetworkSwitches
                            .AnyAsync(s =>
                                s.Id == viewModel.NetworkSwitchId.Value &&
                                (
                                    s.IsActive ||
                                    s.Id == camera.NetworkSwitchId
                                ));

                    if (!switchIsValid)
                    {
                        ModelState.AddModelError(
                            nameof(viewModel.NetworkSwitchId),
                            "Please select a valid switch.");
                    }
                }
            }


            if (!ModelState.IsValid)
            {
                await LoadRecordersAsync(
                    viewModel.RecorderId,
                    camera.RecorderId);

                await LoadSwitchForRecorderAsync(
                    viewModel.RecorderId,
                    viewModel.NetworkSwitchId,
                    camera.NetworkSwitchId);

                return View(viewModel);
            }


            camera.Name =
                viewModel.Name.Trim();

            camera.Brand =
                Normalize(viewModel.Brand);

            camera.Model =
                Normalize(viewModel.Model);

            camera.SerialNumber =
                Normalize(viewModel.SerialNumber);

            camera.Type =
                Normalize(viewModel.Type);

            camera.IpAddress =
                Normalize(viewModel.IpAddress);

            camera.InstallationLocation =
                viewModel.InstallationLocation.Trim();

            camera.InstallationDate =
                viewModel.InstallationDate;

            camera.Notes =
                Normalize(viewModel.Notes);

            camera.RecorderId =
                viewModel.RecorderId;

            camera.NetworkSwitchId =
                viewModel.NetworkSwitchId;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Camera updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // ACTIVATE / DEACTIVATE
        // =====================================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(
            int id)
        {
            var camera =
                await _context.Cameras.FindAsync(id);

            if (camera == null)
            {
                return NotFound();
            }


            camera.IsActive =
                !camera.IsActive;

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                camera.IsActive
                    ? "Camera activated successfully."
                    : "Camera deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // AJAX: SWITCH FOR RECORDER
        // =====================================================

        [Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public async Task<IActionResult>
            GetSwitchesByRecorder(int recorderId)
        {
            var result = await _context.Recorders
                .AsNoTracking()
                .Where(r =>
                    r.Id == recorderId &&
                    r.IsActive &&
                    r.NetworkSwitchId != null)
                .Select(r => new
                {
                    id =
                        r.NetworkSwitch!.Id,

                    name =
                        r.NetworkSwitch.Name,

                    isActive =
                        r.NetworkSwitch.IsActive
                })
                .FirstOrDefaultAsync();


            if (result == null ||
                !result.isActive)
            {
                return Json(
                    Array.Empty<object>());
            }


            return Json(new[]
            {
                new
                {
                    result.id,
                    result.name
                }
            });
        }


        // =====================================================
        // DROPDOWN HELPERS
        // =====================================================

        private async Task LoadRecordersAsync(
            int? selectedRecorderId = null,
            int? currentRecorderId = null)
        {
            var recorders =
                await _context.Recorders
                    .AsNoTracking()
                    .Where(r =>
                        r.IsActive ||
                        r.Id == currentRecorderId)
                    .OrderBy(r => r.Site.Name)
                    .ThenBy(r => r.Name)
                    .Select(r => new
                    {
                        r.Id,

                        DisplayName =
                            $"{r.Name} — {r.Site.Name}"
                    })
                    .ToListAsync();


            ViewBag.Recorders =
                new SelectList(
                    recorders,
                    "Id",
                    "DisplayName",
                    selectedRecorderId);
        }


        private async Task LoadSwitchForRecorderAsync(
            int recorderId,
            int? selectedSwitchId = null,
            int? currentSwitchId = null)
        {
            var switches =
                await _context.Recorders
                    .AsNoTracking()
                    .Where(r =>
                        r.Id == recorderId &&
                        r.NetworkSwitchId != null)
                    .Select(r => r.NetworkSwitch!)
                    .Where(s =>
                        s.IsActive ||
                        s.Id == currentSwitchId)
                    .Select(s => new
                    {
                        s.Id,
                        s.Name
                    })
                    .ToListAsync();


            ViewBag.Switches =
                new SelectList(
                    switches,
                    "Id",
                    "Name",
                    selectedSwitchId);
        }


        private void LoadEmptySwitchList()
        {
            ViewBag.Switches =
                new SelectList(
                    Enumerable.Empty<object>());
        }


        private static string? Normalize(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            var camera = await GetCameraHistoryAsync(id);

            if (camera == null)
            {
                return NotFound();
            }

            return View(camera);
        }
        [HttpGet]
        public async Task<IActionResult> ExportHistoryPdf(int id)
        {
            var camera = await GetCameraHistoryAsync(id);

            if (camera == null)
            {
                return NotFound();
            }

            var pdfBytes =
                _pdfService.GenerateCameraHistoryPdf(camera);

            var fileName =
                $"Camera-History-{camera.CameraId}.pdf";

            return File(
                pdfBytes,
                "application/pdf",
                fileName);
        }
        private async Task<CameraHistoryViewModel?>
    GetCameraHistoryAsync(int id)
        {
            return await _context.Cameras
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CameraHistoryViewModel
                {
                    CameraId = c.Id,

                    CameraName = c.Name,

                    Brand = c.Brand,

                    Model = c.Model,

                    SerialNumber = c.SerialNumber,

                    Type = c.Type,

                    IpAddress = c.IpAddress,

                    InstallationLocation =
                        c.InstallationLocation,

                    InstallationDate =
                        c.InstallationDate,

                    RecorderName =
                        c.Recorder.Name,

                    SiteId =
                        c.Recorder.SiteId,

                    SiteName =
                        c.Recorder.Site.Name,

                    IsActive =
                        c.IsActive,

                    Visits = _context.CameraVisits
                        .Where(v =>
                            v.CameraId == c.Id)
                        .OrderByDescending(v =>
                            v.VisitDate)
                        .Select(v =>
                            new CameraHistoryVisitViewModel
                            {
                                VisitId =
                                    v.Id,

                                VisitDate =
                                    v.VisitDate,

                                Purpose =
                                    v.Purpose,

                                WorkerNames =
                                    v.CameraVisitWorkers
                                        .OrderBy(vw =>
                                            vw.Worker.FirstName)
                                        .ThenBy(vw =>
                                            vw.Worker.SecondName)
                                        .ThenBy(vw =>
                                            vw.Worker.LastName)
                                        .Select(vw =>
                                            vw.Worker.FirstName + " " +
                                            vw.Worker.SecondName + " " +
                                            vw.Worker.LastName)
                                        .ToList(),

                                MalfunctionType =
                                    v.MalfunctionType,

                                MalfunctionDescription =
                                    v.MalfunctionDescription,

                                RepairWorkPerformed =
                                    v.RepairWorkPerformed,

                                RepairResult =
                                    v.RepairResult,

                                Notes =
                                    v.Notes
                            })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}