using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.ViewModels.CameraVisits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Roles = "Admin,Editor,Viewer")]
    public class CameraVisitsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CameraVisitsController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: CameraVisits
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? siteId,
            int? cameraId,
            int? workerId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.CameraVisits
                .AsNoTracking()
                .AsQueryable();


            // SITE
            if (!string.IsNullOrWhiteSpace(siteId))
            {
                query = query.Where(v =>
                    v.Camera.Recorder.SiteId == siteId);
            }


            // CAMERA
            if (cameraId.HasValue)
            {
                query = query.Where(v =>
                    v.CameraId == cameraId.Value);
            }


            // WORKER
            if (workerId.HasValue)
            {
                query = query.Where(v =>
                    v.CameraVisitWorkers.Any(vw =>
                        vw.WorkerId == workerId.Value));
            }


            // FROM DATE
            if (fromDate.HasValue)
            {
                query = query.Where(v =>
                    v.VisitDate >= fromDate.Value.Date);
            }


            // TO DATE
            if (toDate.HasValue)
            {
                var endDate =
                    toDate.Value.Date.AddDays(1);

                query = query.Where(v =>
                    v.VisitDate < endDate);
            }


            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(v =>
                    v.Camera.Name.Contains(search) ||
                    v.Purpose.Contains(search) ||
                    v.Camera.Recorder.Site.Name.Contains(search) ||
                    (v.MalfunctionType != null &&
                     v.MalfunctionType.Contains(search)) ||
                    (v.MalfunctionDescription != null &&
                     v.MalfunctionDescription.Contains(search)) ||
                    (v.RepairResult != null &&
                     v.RepairResult.Contains(search)) ||
                    (v.RepairWorkPerformed != null &&
                     v.RepairWorkPerformed.Contains(search)));
            }


            var visits = await query
                .OrderByDescending(v => v.VisitDate)
                .Select(v =>
                    new CameraVisitListItemViewModel
                    {
                        Id = v.Id,

                        CameraId = v.CameraId,

                        CameraName =
                            v.Camera.Name,

                        RecorderName =
                            v.Camera.Recorder.Name,

                        SiteName =
                            v.Camera.Recorder.Site.Name,

                        VisitDate =
                            v.VisitDate,

                        Purpose =
                            v.Purpose,

                        MalfunctionType =
                            v.MalfunctionType,

                        MalfunctionDescription =
                            v.MalfunctionDescription,

                        RepairWorkPerformed =
                            v.RepairWorkPerformed,

                        RepairResult =
                            v.RepairResult,

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
                                .ToList()
                    })
                .ToListAsync();


            await LoadIndexFiltersAsync(
                siteId,
                cameraId,
                workerId);


            ViewBag.Search = search;

            ViewBag.FromDate =
                fromDate?.ToString("yyyy-MM-dd");

            ViewBag.ToDate =
                toDate?.ToString("yyyy-MM-dd");


            return View(visits);
        }

        // GET: CameraVisits/Create
        [Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCamerasAsync();
            await LoadWorkersAsync();

            var viewModel =
                new CameraVisitFormViewModel
                {
                    VisitDate = DateTime.Now
                };

            return View(viewModel);
        }


        // POST: CameraVisits/Create
        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CameraVisitFormViewModel viewModel)
        {
            var cameraExists =
                await _context.Cameras
                    .AnyAsync(c =>
                        c.Id == viewModel.CameraId &&
                        c.IsActive);

            if (!cameraExists)
            {
                ModelState.AddModelError(
                    nameof(viewModel.CameraId),
                    "Please select a valid active camera.");
            }


            var workerIds =
                viewModel.WorkerIds
                    .Distinct()
                    .ToList();


            if (workerIds.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(viewModel.WorkerIds),
                    "Please select at least one worker.");
            }
            else
            {
                var validWorkerCount =
                    await _context.Workers
                        .CountAsync(w =>
                            workerIds.Contains(w.Id) &&
                            w.IsActive);

                if (validWorkerCount !=
                    workerIds.Count)
                {
                    ModelState.AddModelError(
                        nameof(viewModel.WorkerIds),
                        "One or more selected workers are invalid or inactive.");
                }
            }


            if (!ModelState.IsValid)
            {
                await LoadCamerasAsync(
                    viewModel.CameraId);

                await LoadWorkersAsync(
                    workerIds);

                return View(viewModel);
            }


            var visit = new CameraVisit
            {
                CameraId = viewModel.CameraId,

                VisitDate = viewModel.VisitDate,

                Purpose = viewModel.Purpose.Trim(),

                MalfunctionType =
                    Normalize(viewModel.MalfunctionType),

                MalfunctionDescription =
                    Normalize(viewModel.MalfunctionDescription),

                RepairWorkPerformed =
                    Normalize(viewModel.RepairWorkPerformed),

                RepairResult =
                    Normalize(viewModel.RepairResult),

                Notes =
                    Normalize(viewModel.Notes)
            };


            foreach (var workerId in workerIds)
            {
                visit.CameraVisitWorkers.Add(
                    new CameraVisitWorker
                    {
                        WorkerId = workerId
                    });
            }


            _context.CameraVisits.Add(visit);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Camera visit created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // GET: CameraVisits/Edit/5
        [Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var visit =
                await _context.CameraVisits
                    .AsNoTracking()
                    .Include(v =>
                        v.CameraVisitWorkers)
                    .FirstOrDefaultAsync(v =>
                        v.Id == id);

            if (visit == null)
            {
                return NotFound();
            }


            var viewModel =
                new CameraVisitFormViewModel
                {
                    Id = visit.Id,

                    CameraId =
                        visit.CameraId,

                    VisitDate =
                        visit.VisitDate,

                    Purpose =
                        visit.Purpose,

                    Notes =
                        visit.Notes,

                    WorkerIds =
                        visit.CameraVisitWorkers
                            .Select(vw =>
                                vw.WorkerId)
                            .ToList(),
                    MalfunctionType =
    visit.MalfunctionType,

                    MalfunctionDescription =
    visit.MalfunctionDescription,

                    RepairWorkPerformed =
    visit.RepairWorkPerformed,

                    RepairResult =
    visit.RepairResult,
                };


            await LoadCamerasAsync(
                visit.CameraId,
                visit.CameraId);

            await LoadWorkersAsync(
                viewModel.WorkerIds,
                viewModel.WorkerIds);

            return View(viewModel);
        }


        // POST: CameraVisits/Edit/5
        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CameraVisitFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }


            var visit =
                await _context.CameraVisits
                    .Include(v =>
                        v.CameraVisitWorkers)
                    .FirstOrDefaultAsync(v =>
                        v.Id == id);

            if (visit == null)
            {
                return NotFound();
            }


            var cameraExists =
                await _context.Cameras
                    .AnyAsync(c =>
                        c.Id == viewModel.CameraId &&
                        (
                            c.IsActive ||
                            c.Id == visit.CameraId
                        ));

            if (!cameraExists)
            {
                ModelState.AddModelError(
                    nameof(viewModel.CameraId),
                    "Please select a valid camera.");
            }


            var workerIds =
                viewModel.WorkerIds
                    .Distinct()
                    .ToList();


            if (workerIds.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(viewModel.WorkerIds),
                    "Please select at least one worker.");
            }
            else
            {
                var currentWorkerIds =
                    visit.CameraVisitWorkers
                        .Select(vw =>
                            vw.WorkerId)
                        .ToHashSet();


                var validWorkerCount =
                    await _context.Workers
                        .CountAsync(w =>
                            workerIds.Contains(w.Id) &&
                            (
                                w.IsActive ||
                                currentWorkerIds.Contains(w.Id)
                            ));

                if (validWorkerCount !=
                    workerIds.Count)
                {
                    ModelState.AddModelError(
                        nameof(viewModel.WorkerIds),
                        "One or more selected workers are invalid.");
                }
            }


            if (!ModelState.IsValid)
            {
                await LoadCamerasAsync(
                    viewModel.CameraId,
                    visit.CameraId);

                await LoadWorkersAsync(
                    workerIds,
                    visit.CameraVisitWorkers
                        .Select(vw =>
                            vw.WorkerId)
                        .ToList());

                return View(viewModel);
            }


            visit.CameraId =
                viewModel.CameraId;

            visit.VisitDate =
                viewModel.VisitDate;

            visit.Purpose =
                viewModel.Purpose.Trim();

            visit.Notes =
                string.IsNullOrWhiteSpace(
                    viewModel.Notes)
                    ? null
                    : viewModel.Notes.Trim();


            _context.CameraVisitWorkers
                .RemoveRange(
                    visit.CameraVisitWorkers);


            visit.CameraVisitWorkers =
                workerIds
                    .Select(workerId =>
                        new CameraVisitWorker
                        {
                            CameraVisitId =
                                visit.Id,

                            WorkerId =
                                workerId
                        })
                    .ToList();

            visit.MalfunctionType =
    Normalize(viewModel.MalfunctionType);

            visit.MalfunctionDescription =
                Normalize(viewModel.MalfunctionDescription);

            visit.RepairWorkPerformed =
                Normalize(viewModel.RepairWorkPerformed);

            visit.RepairResult =
                Normalize(viewModel.RepairResult);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Camera visit updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // Load Cameras dropdown
        private async Task LoadCamerasAsync(
            int? selectedCameraId = null,
            int? currentCameraId = null)
        {
            var cameras =
                await _context.Cameras
                    .AsNoTracking()
                    .Where(c =>
                        c.IsActive ||
                        c.Id == currentCameraId)
                    .OrderBy(c =>
                        c.Recorder.Site.Name)
                    .ThenBy(c =>
                        c.Recorder.Name)
                    .ThenBy(c =>
                        c.Name)
                    .Select(c => new
                    {
                        c.Id,

                        DisplayName =
                            $"{c.Name} — " +
                            $"{c.Recorder.Name} — " +
                            $"{c.Recorder.Site.Name}"
                    })
                    .ToListAsync();


            ViewBag.Cameras =
                new SelectList(
                    cameras,
                    "Id",
                    "DisplayName",
                    selectedCameraId);
        }


        // Load Workers
        private async Task LoadWorkersAsync(
            IEnumerable<int>? selectedWorkerIds = null,
            IEnumerable<int>? currentWorkerIds = null)
        {
            var selected =
                selectedWorkerIds?.ToHashSet()
                ?? new HashSet<int>();

            var current =
                currentWorkerIds?.ToHashSet()
                ?? new HashSet<int>();


            var workers =
                await _context.Workers
                    .AsNoTracking()
                    .Where(w =>
                        w.IsActive ||
                        current.Contains(w.Id))
                    .OrderBy(w =>
                        w.FirstName)
                    .ThenBy(w =>
                        w.SecondName)
                    .ThenBy(w =>
                        w.LastName)
                    .ToListAsync();


            ViewBag.Workers =
                workers
                    .Select(w =>
                        new SelectListItem
                        {
                            Value =
                                w.Id.ToString(),

                            Text =
                                $"{w.FirstName} " +
                                $"{w.SecondName} " +
                                $"{w.LastName}",

                            Selected =
                                selected.Contains(w.Id)
                        })
                    .ToList();


        }
        private static string? Normalize(
    string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var visit = await _context.CameraVisits
                .AsNoTracking()
                .Where(v => v.Id == id)
                .Select(v => new CameraVisitDetailsViewModel
                {
                    Id = v.Id,

                    CameraId = v.CameraId,
                    CameraName = v.Camera.Name,

                    RecorderName =
                        v.Camera.Recorder.Name,

                    SiteId =
                        v.Camera.Recorder.SiteId,

                    SiteName =
                        v.Camera.Recorder.Site.Name,

                    VisitDate =
                        v.VisitDate,

                    Purpose =
                        v.Purpose,

                    MalfunctionType =
                        v.MalfunctionType,

                    MalfunctionDescription =
                        v.MalfunctionDescription,

                    RepairWorkPerformed =
                        v.RepairWorkPerformed,

                    RepairResult =
                        v.RepairResult,

                    Notes =
                        v.Notes,

                    WorkerNames =
                        v.CameraVisitWorkers
                            .OrderBy(vw => vw.Worker.FirstName)
                            .ThenBy(vw => vw.Worker.SecondName)
                            .ThenBy(vw => vw.Worker.LastName)
                            .Select(vw =>
                                vw.Worker.FirstName + " " +
                                vw.Worker.SecondName + " " +
                                vw.Worker.LastName)
                            .ToList()
                })
                .FirstOrDefaultAsync();

            if (visit == null)
            {
                return NotFound();
            }

            return View(visit);
        }
        private async Task LoadIndexFiltersAsync(
    string? siteId,
    int? cameraId,
    int? workerId)
        {
            ViewBag.Sites =
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


            ViewBag.FilterCameras =
                new SelectList(
                    await _context.Cameras
                        .AsNoTracking()
                        .OrderBy(c => c.Name)
                        .Select(c => new
                        {
                            c.Id,
                            DisplayName =
                                c.Name + " — " +
                                c.Recorder.Site.Name
                        })
                        .ToListAsync(),
                    "Id",
                    "DisplayName",
                    cameraId);


            ViewBag.FilterWorkers =
                new SelectList(
                    await _context.Workers
                        .AsNoTracking()
                        .OrderBy(w => w.FirstName)
                        .ThenBy(w => w.SecondName)
                        .ThenBy(w => w.LastName)
                        .Select(w => new
                        {
                            w.Id,
                            FullName =
                                w.FirstName + " " +
                                w.SecondName + " " +
                                w.LastName
                        })
                        .ToListAsync(),
                    "Id",
                    "FullName",
                    workerId);
        }
    }
}