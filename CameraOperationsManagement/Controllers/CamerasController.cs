using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.Services;
using CameraOperationsManagement.ViewModels.Cameras;
using CameraOperationsManagement.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Policy = "CanViewInfrastructure")]
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
        public async Task<IActionResult> Index(
            string? search,
            string? siteId,
            int? recorderId,
            string? cameraType,
            int? switchId,
            string? status,
            int page = 1)
        {
            const int pageSize = 10;


            if (page < 1)
            {
                page = 1;
            }


            var query = BuildCameraFilterQuery(
                search,
                siteId,
                recorderId,
                cameraType,
                switchId,
                status);


            // Total filtered cameras
            var totalItems =
                await query.CountAsync();


            var totalPages =
                (int)Math.Ceiling(
                    totalItems / (double)pageSize);


            // If the requested page no longer exists,
            // move to the last available page.
            if (totalPages > 0 &&
                page > totalPages)
            {
                page = totalPages;
            }


            // Only retrieve the cameras for the current page.
            var cameras = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c =>
                    new CameraListItemViewModel
                    {
                        Id = c.Id,

                        Name = c.Name,

                        Brand = c.Brand,

                        Model = c.Model,

                        Type = c.Type,

                        Environment = c.Environment,

                        IpAddress = c.IpAddress,

                        InstallationLocation =
                            c.InstallationLocation,

                        RecorderName =
                            c.Recorder.Name,

                        SiteId =
                            c.Recorder.SiteId,

                        SiteName =
                            c.Recorder.Site.Name,

                        NetworkSwitchName =
                            c.NetworkSwitch != null
                                ? c.NetworkSwitch.Name
                                : null,

                        IsActive =
                            c.IsActive
                    })
                .ToListAsync();


            var model =
                new PagedResult<CameraListItemViewModel>
                {
                    Items = cameras,

                    Page = page,

                    PageSize = pageSize,

                    TotalItems = totalItems
                };


            ViewBag.Search = search;

            ViewBag.Status = status;


            // AJAX:
            // return only the camera table + pagination.
            if (Request.Headers["X-Requested-With"] ==
                "XMLHttpRequest")
            {
                return PartialView(
                    "_CameraList",
                    model);
            }


            // These dropdowns are only required
            // when rendering the complete page.
            await LoadIndexFiltersAsync(
                siteId,
                recorderId,
                cameraType,
                switchId);


            return View(model);
        }


        // =====================================================
        // CREATE GET
        // =====================================================

        [Authorize(Policy = "CanManageInfrastructure")]
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

        [Authorize(Policy = "CanManageInfrastructure")]
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
                SerialNumber = Normalize(viewModel.SerialNumber),
                Type = Normalize(viewModel.Type),

                Environment = viewModel.Environment!.Value,

                IpAddress = Normalize(viewModel.IpAddress),

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

        [Authorize(Policy = "CanManageInfrastructure")]
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
                Environment = camera.Environment,

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

        [Authorize(Policy = "CanManageInfrastructure")]
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
            camera.Environment =
    viewModel.Environment!.Value;
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

        [Authorize(Policy = "CanChangeStatus")]
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

        //[Authorize(Roles = "Admin,Editor")]
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

            Response.Headers.ContentDisposition =
                $"inline; filename=\"{fileName}\"";

            return File(
                pdfBytes,
                "application/pdf");
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

                    Visits = _context.Visits
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
                v.VisitWorkers
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
        private async Task LoadIndexFiltersAsync(
    string? siteId,
    int? recorderId,
    string? cameraType,
    int? switchId)
        {
            // SITES
            ViewBag.FilterSites =
                new SelectList(
                    await _context.Sites
                        .AsNoTracking()
                        .OrderBy(s => s.Name)
                        .Select(s => new
                        {
                            s.Id,

                            DisplayName =
                                s.Name + " (" + s.Id + ")"
                        })
                        .ToListAsync(),
                    "Id",
                    "DisplayName",
                    siteId);


            // RECORDERS
            ViewBag.FilterRecorders =
                new SelectList(
                    await _context.Recorders
                        .AsNoTracking()
                        .OrderBy(r => r.Name)
                        .Select(r => new
                        {
                            r.Id,

                            DisplayName =
                                r.Name + " — " +
                                r.Site.Name
                        })
                        .ToListAsync(),
                    "Id",
                    "DisplayName",
                    recorderId);


            // CAMERA TYPES
            var types = await _context.Cameras
                .AsNoTracking()
                .Where(c =>
                    c.Type != null &&
                    c.Type != "")
                .Select(c => c.Type!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            ViewBag.FilterTypes =
                new SelectList(
                    types,
                    cameraType);


            // SWITCHES
            ViewBag.FilterSwitches =
                new SelectList(
                    await _context.NetworkSwitches
                        .AsNoTracking()
                        .OrderBy(s => s.Name)
                        .Select(s => new
                        {
                            s.Id,

                            DisplayName =
                                s.Name + " — " +
                                s.Site.Name
                        })
                        .ToListAsync(),
                    "Id",
                    "DisplayName",
                    switchId);
        }
        private IQueryable<Camera> BuildCameraFilterQuery(
            string? search,
            string? siteId,
            int? recorderId,
            string? cameraType,
            int? switchId,
            string? status)
        {
            var query = _context.Cameras
                .AsNoTracking()
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.Name.Contains(search) ||
                    (c.Brand != null &&
                     c.Brand.Contains(search)) ||
                    (c.Model != null &&
                     c.Model.Contains(search)) ||
                    (c.SerialNumber != null &&
                     c.SerialNumber.Contains(search)) ||
                    (c.Type != null &&
                     c.Type.Contains(search)) ||
                    (c.IpAddress != null &&
                     c.IpAddress.Contains(search)) ||
                    c.InstallationLocation.Contains(search));
            }


            if (!string.IsNullOrWhiteSpace(siteId))
            {
                query = query.Where(c =>
                    c.Recorder.SiteId == siteId);
            }


            if (recorderId.HasValue)
            {
                query = query.Where(c =>
                    c.RecorderId == recorderId.Value);
            }


            if (!string.IsNullOrWhiteSpace(cameraType))
            {
                query = query.Where(c =>
                    c.Type == cameraType);
            }


            if (switchId.HasValue)
            {
                query = query.Where(c =>
                    c.NetworkSwitchId == switchId.Value);
            }


            if (status == "active")
            {
                query = query.Where(c =>
                    c.IsActive);
            }
            else if (status == "inactive")
            {
                query = query.Where(c =>
                    !c.IsActive);
            }


            return query;
        }
        [HttpGet]
        public async Task<IActionResult> ExportPdf(
    string? search,
    string? siteId,
    int? recorderId,
    string? cameraType,
    int? switchId,
    string? status)
        {
            var query = BuildCameraFilterQuery(
                search,
                siteId,
                recorderId,
                cameraType,
                switchId,
                status);


            var cameras = await query
                .OrderBy(c => c.Name)
                .Select(c => new CameraListItemViewModel
                {
                    Id = c.Id,

                    Name = c.Name,

                    Brand = c.Brand,

                    Model = c.Model,

                    Type = c.Type,
                    Environment = c.Environment,

                    IpAddress = c.IpAddress,

                    InstallationLocation =
                        c.InstallationLocation,

                    RecorderName =
                        c.Recorder.Name,

                    SiteId =
                        c.Recorder.SiteId,

                    SiteName =
                        c.Recorder.Site.Name,

                    NetworkSwitchName =
                        c.NetworkSwitch != null
                            ? c.NetworkSwitch.Name
                            : null,

                    IsActive =
                        c.IsActive
                })
                .ToListAsync();


            var pdfBytes =
                _pdfService.GenerateCameraListPdf(cameras);


            Response.Headers.ContentDisposition =
                "inline; filename=\"Camera-Report.pdf\"";


            return File(
                pdfBytes,
                "application/pdf");
        }
    }
}