using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.Models.Enums;
using CameraOperationsManagement.ViewModels.Common;
using CameraOperationsManagement.ViewModels.Visits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Controllers
{
    [Authorize]
    public class VisitsController : Controller
    {
        private readonly ApplicationDbContext _context;


        public VisitsController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================
        // INDEX
        // =========================
        [Authorize(Policy = "CanViewVisits")]
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? siteId,
            VisitComponentType? componentType,
            int? workerId,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }


            var query = _context.Visits
                .AsNoTracking()
                .AsQueryable();


            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(v =>
                    v.Purpose.Contains(search) ||
                    (v.MalfunctionType != null &&
                     v.MalfunctionType.Contains(search)) ||
                    (v.RepairResult != null &&
                     v.RepairResult.Contains(search)) ||
                    (v.Notes != null &&
                     v.Notes.Contains(search)));
            }


            // SITE
            if (!string.IsNullOrWhiteSpace(siteId))
            {
                query = query.Where(v =>
                    v.SiteId == siteId);
            }


            // COMPONENT TYPE
            if (componentType.HasValue)
            {
                query = query.Where(v =>
                    v.ComponentType == componentType.Value);
            }


            // WORKER
            if (workerId.HasValue)
            {
                query = query.Where(v =>
                    v.VisitWorkers.Any(vw =>
                        vw.WorkerId == workerId.Value));
            }


            // FROM DATE
            if (fromDate.HasValue)
            {
                var start =
                    fromDate.Value.Date;

                query = query.Where(v =>
                    v.VisitDate >= start);
            }


            // TO DATE
            if (toDate.HasValue)
            {
                var endExclusive =
                    toDate.Value.Date.AddDays(1);

                query = query.Where(v =>
                    v.VisitDate < endExclusive);
            }


            var totalItems =
                await query.CountAsync();


            var totalPages =
                (int)Math.Ceiling(
                    totalItems / (double)pageSize);


            if (totalPages > 0 &&
                page > totalPages)
            {
                page = totalPages;
            }


            var visits = await query
                .OrderByDescending(v =>
                    v.VisitDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v =>
                    new VisitListItemViewModel
                    {
                        Id = v.Id,

                        VisitDate = v.VisitDate,

                        SiteId = v.SiteId,

                        SiteName = v.Site.Name,

                        ComponentType =
                            v.ComponentType.ToString(),

                        ComponentName =
                            v.ComponentType ==
                                VisitComponentType.Recorder
                                ? v.Recorder!.Name

                                : v.ComponentType ==
                                  VisitComponentType.Switch
                                    ? v.NetworkSwitch!.Name

                                    : v.Camera!.Name,

                        Purpose = v.Purpose,

                        MalfunctionType =
                            v.MalfunctionType,

                        RepairResult =
                            v.RepairResult,

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
                                .ToList()
                    })
                .ToListAsync();


            var model =
                new PagedResult<VisitListItemViewModel>
                {
                    Items = visits,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };


            // SITE FILTER OPTIONS
            var sites = await _context.Sites
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToListAsync();


            ViewBag.FilterSites =
                new SelectList(
                    sites,
                    "Id",
                    "Name",
                    siteId);


            // WORKER FILTER OPTIONS
            var workers = await _context.Workers
                .AsNoTracking()
                .OrderBy(w => w.FirstName)
                .ThenBy(w => w.SecondName)
                .ThenBy(w => w.LastName)
                .Select(w => new
                {
                    w.Id,

                    Name =
                        w.FirstName + " " +
                        w.SecondName + " " +
                        w.LastName
                })
                .ToListAsync();


            ViewBag.FilterWorkers =
                new SelectList(
                    workers,
                    "Id",
                    "Name",
                    workerId);


            ViewBag.Search = search;
            ViewBag.ComponentType = componentType;
            ViewBag.FromDate =
                fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate =
                toDate?.ToString("yyyy-MM-dd");


            if (Request.Headers["X-Requested-With"] ==
                "XMLHttpRequest")
            {
                return PartialView(
                    "_VisitList",
                    model);
            }


            return View(model);
        }


        // =========================
        // CREATE
        // =========================
        [Authorize(Policy = "CanManageVisits")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new VisitFormViewModel
            {
                VisitDate = DateTime.Now
            };


            await LoadFormDataAsync(
                viewModel);


            return View(viewModel);
        }

        [Authorize(Policy = "CanManageVisits")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            VisitFormViewModel viewModel)
        {
            await ValidateVisitAsync(
                viewModel);


            if (!ModelState.IsValid)
            {
                await LoadFormDataAsync(
                    viewModel);

                return View(viewModel);
            }


            var visit = new Visit
            {
                SiteId =
                    viewModel.SiteId,

                ComponentType =
                    viewModel.ComponentType!.Value,

                VisitDate =
                    viewModel.VisitDate,

                Purpose =
                    viewModel.Purpose.Trim(),

                MalfunctionType =
                    Normalize(
                        viewModel.MalfunctionType),

                MalfunctionDescription =
                    Normalize(
                        viewModel.MalfunctionDescription),

                RepairWorkPerformed =
                    Normalize(
                        viewModel.RepairWorkPerformed),

                RepairResult =
                    Normalize(
                        viewModel.RepairResult),

                Notes =
                    Normalize(
                        viewModel.Notes)
            };


            SetComponent(
                visit,
                viewModel.ComponentType.Value,
                viewModel.ComponentId!.Value);


            visit.VisitWorkers =
                viewModel.WorkerIds
                    .Distinct()
                    .Select(workerId =>
                        new VisitWorker
                        {
                            WorkerId = workerId
                        })
                    .ToList();


            _context.Visits.Add(visit);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Visit recorded successfully.";


            return RedirectToAction(
                nameof(Index));
        }


        // =========================
        // EDIT
        // =========================

        [Authorize(Policy = "CanManageVisits")]
        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var visit = await _context.Visits
                .AsNoTracking()
                .Include(v => v.VisitWorkers)
                .FirstOrDefaultAsync(v =>
                    v.Id == id);


            if (visit == null)
            {
                return NotFound();
            }


            var viewModel =
                new VisitFormViewModel
                {
                    Id = visit.Id,

                    SiteId =
                        visit.SiteId,

                    ComponentType =
                        visit.ComponentType,

                    ComponentId =
                        GetComponentId(visit),

                    VisitDate =
                        visit.VisitDate,

                    Purpose =
                        visit.Purpose,

                    MalfunctionType =
                        visit.MalfunctionType,

                    MalfunctionDescription =
                        visit.MalfunctionDescription,

                    RepairWorkPerformed =
                        visit.RepairWorkPerformed,

                    RepairResult =
                        visit.RepairResult,

                    Notes =
                        visit.Notes,

                    WorkerIds =
                        visit.VisitWorkers
                            .Select(vw =>
                                vw.WorkerId)
                            .ToList()
                };


            await LoadFormDataAsync(
                viewModel);


            return View(viewModel);
        }

        [Authorize(Policy = "CanManageVisits")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            VisitFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }


            var visit = await _context.Visits
                .Include(v => v.VisitWorkers)
                .FirstOrDefaultAsync(v =>
                    v.Id == id);


            if (visit == null)
            {
                return NotFound();
            }


            await ValidateVisitAsync(
                viewModel);


            if (!ModelState.IsValid)
            {
                await LoadFormDataAsync(
                    viewModel);

                return View(viewModel);
            }


            visit.SiteId =
                viewModel.SiteId;

            visit.ComponentType =
                viewModel.ComponentType!.Value;

            visit.VisitDate =
                viewModel.VisitDate;

            visit.Purpose =
                viewModel.Purpose.Trim();

            visit.MalfunctionType =
                Normalize(
                    viewModel.MalfunctionType);

            visit.MalfunctionDescription =
                Normalize(
                    viewModel.MalfunctionDescription);

            visit.RepairWorkPerformed =
                Normalize(
                    viewModel.RepairWorkPerformed);

            visit.RepairResult =
                Normalize(
                    viewModel.RepairResult);

            visit.Notes =
                Normalize(
                    viewModel.Notes);


            ClearComponents(visit);

            SetComponent(
                visit,
                viewModel.ComponentType.Value,
                viewModel.ComponentId!.Value);


            _context.VisitWorkers.RemoveRange(
                visit.VisitWorkers);


            visit.VisitWorkers =
                viewModel.WorkerIds
                    .Distinct()
                    .Select(workerId =>
                        new VisitWorker
                        {
                            VisitId =
                                visit.Id,

                            WorkerId =
                                workerId
                        })
                    .ToList();


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Visit updated successfully.";


            return RedirectToAction(
                nameof(Index));
        }


        // =========================
        // COMPONENT API
        // =========================
        [Authorize(Policy = "CanManageVisits")]
        [HttpGet]
        public async Task<IActionResult> GetComponents(
            string siteId,
            VisitComponentType componentType)
        {
            if (string.IsNullOrWhiteSpace(siteId))
            {
                return Json(
                    Array.Empty<object>());
            }


            switch (componentType)
            {

                case VisitComponentType.Recorder:

                    var recorders =
                        await _context.Recorders
                            .AsNoTracking()
                            .Where(r =>
                                r.SiteId == siteId &&
                                r.IsActive)
                            .OrderBy(r => r.Name)
                            .Select(r => new
                            {
                                id = r.Id,

                                name =
                                    r.Name + " — " +
                                    (r.Type == RecorderType.NVR
                                        ? "NVR"
                                        : "DVR")
                            })
                            .ToListAsync();

                    return Json(recorders);


                case VisitComponentType.Switch:

                    var switches =
                        await _context.NetworkSwitches
                            .AsNoTracking()
                            .Where(s =>
                                s.SiteId == siteId &&
                                s.IsActive)
                            .OrderBy(s =>
                                s.Name)
                            .Select(s => new
                            {
                                id = s.Id,

                                name = s.Name
                            })
                            .ToListAsync();

                    return Json(switches);


                case VisitComponentType.Camera:

                    var cameras =
                        await _context.Cameras
                            .AsNoTracking()
                            .Where(c =>
                                c.Recorder.SiteId == siteId &&
                                c.IsActive)
                            .OrderBy(c =>
                                c.Name)
                            .Select(c => new
                            {
                                id = c.Id,

                                name =
                                    c.Name + " — " +
                                    c.Recorder.Name
                            })
                            .ToListAsync();

                    return Json(cameras);


                default:

                    return Json(
                        Array.Empty<object>());
            }
        }


        // =========================
        // VALIDATION
        // =========================

        private async Task ValidateVisitAsync(
            VisitFormViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(
                viewModel.SiteId))
            {
                return;
            }


            var siteExists =
                await _context.Sites
                    .AnyAsync(s =>
                        s.Id == viewModel.SiteId &&
                        s.IsActive);


            if (!siteExists)
            {
                ModelState.AddModelError(
                    nameof(viewModel.SiteId),
                    "The selected site is not available.");
            }


            if (viewModel.WorkerIds.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(viewModel.WorkerIds),
                    "Please select at least one worker.");
            }
            else
            {
                var distinctWorkerIds =
                    viewModel.WorkerIds
                        .Distinct()
                        .ToList();


                var validWorkerCount =
                    await _context.Workers
                        .CountAsync(w =>
                            distinctWorkerIds.Contains(w.Id) &&
                            w.IsActive);


                if (validWorkerCount !=
                    distinctWorkerIds.Count)
                {
                    ModelState.AddModelError(
                        nameof(viewModel.WorkerIds),
                        "One or more selected workers are not available.");
                }
            }


            if (!viewModel.ComponentType.HasValue ||
                !viewModel.ComponentId.HasValue)
            {
                return;
            }


            bool componentIsValid;


            switch (viewModel.ComponentType.Value)
            {
                case VisitComponentType.Recorder:

                    componentIsValid =
                        await _context.Recorders
                            .AnyAsync(r =>
                                r.Id ==
                                    viewModel.ComponentId.Value &&
                                r.SiteId ==
                                    viewModel.SiteId &&
                                r.IsActive);

                    break;


                case VisitComponentType.Switch:

                    componentIsValid =
                        await _context.NetworkSwitches
                            .AnyAsync(s =>
                                s.Id ==
                                    viewModel.ComponentId.Value &&
                                s.SiteId ==
                                    viewModel.SiteId &&
                                s.IsActive);

                    break;


                case VisitComponentType.Camera:

                    componentIsValid =
                        await _context.Cameras
                            .AnyAsync(c =>
                                c.Id ==
                                    viewModel.ComponentId.Value &&
                                c.Recorder.SiteId ==
                                    viewModel.SiteId &&
                                c.IsActive);

                    break;


                default:

                    componentIsValid = false;

                    break;
            }


            if (!componentIsValid)
            {
                ModelState.AddModelError(
                    nameof(viewModel.ComponentId),
                    "The selected component does not belong to the selected site.");
            }
        }


        // =========================
        // FORM DATA
        // =========================

        private async Task LoadFormDataAsync(
            VisitFormViewModel viewModel)
        {
            ViewBag.Sites =
                new SelectList(
                    await _context.Sites
                        .AsNoTracking()
                        .Where(s =>
                            s.IsActive)
                        .OrderBy(s =>
                            s.Name)
                        .Select(s => new
                        {
                            s.Id,

                            DisplayName =
                                s.Name +
                                " (" +
                                s.Id +
                                ")"
                        })
                        .ToListAsync(),
                    "Id",
                    "DisplayName",
                    viewModel.SiteId);


            var workers =
                await _context.Workers
                    .AsNoTracking()
                    .Where(w =>
                        w.IsActive ||
                        viewModel.WorkerIds.Contains(w.Id))
                    .OrderBy(w =>
                        w.FirstName)
                    .ThenBy(w =>
                        w.SecondName)
                    .ThenBy(w =>
                        w.LastName)
                    .Select(w => new
                    {
                        w.Id,

                        FullName =
                            w.FirstName + " " +
                            w.SecondName + " " +
                            w.LastName
                    })
                    .ToListAsync();


            ViewBag.Workers =
                workers.Select(w =>
                    new SelectListItem
                    {
                        Value =
                            w.Id.ToString(),

                        Text =
                            w.FullName,

                        Selected =
                            viewModel.WorkerIds
                                .Contains(w.Id)
                    })
                    .ToList();
        }


        // =========================
        // COMPONENT HELPERS
        // =========================

        private static void SetComponent(
            Visit visit,
            VisitComponentType componentType,
            int componentId)
        {
            switch (componentType)
            {
                case VisitComponentType.Recorder:

                    visit.RecorderId =
                        componentId;

                    break;


                case VisitComponentType.Switch:

                    visit.NetworkSwitchId =
                        componentId;

                    break;


                case VisitComponentType.Camera:

                    visit.CameraId =
                        componentId;

                    break;
            }
        }


        private static void ClearComponents(
            Visit visit)
        {
            visit.RecorderId = null;

            visit.NetworkSwitchId = null;

            visit.CameraId = null;
        }


        private static int? GetComponentId(
            Visit visit)
        {
            return visit.ComponentType switch
            {
                VisitComponentType.Recorder =>
                    visit.RecorderId,

                VisitComponentType.Switch =>
                    visit.NetworkSwitchId,

                VisitComponentType.Camera =>
                    visit.CameraId,

                _ => null
            };
        }


        private static string? Normalize(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        [Authorize(Policy = "CanViewVisitReport")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var visit = await _context.Visits
                .AsNoTracking()
                .Where(v => v.Id == id)
                .Select(v => new VisitDetailsViewModel
                {
                    Id = v.Id,

                    SiteId = v.SiteId,

                    SiteName = v.Site.Name,

                    ComponentType =
                        v.ComponentType.ToString(),

                    ComponentName =
                        v.ComponentType == VisitComponentType.Recorder
                            ? v.Recorder!.Name
                            : v.ComponentType == VisitComponentType.Switch
                                ? v.NetworkSwitch!.Name
                                : v.Camera!.Name,

                    VisitDate = v.VisitDate,

                    Purpose = v.Purpose,

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
                .FirstOrDefaultAsync();


            if (visit == null)
            {
                return NotFound();
            }


            return View(visit);
        }
    }
}