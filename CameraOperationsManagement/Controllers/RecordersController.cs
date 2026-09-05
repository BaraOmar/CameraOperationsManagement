using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.ViewModels.Recorders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CameraOperationsManagement.Models.Enums;
using CameraOperationsManagement.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Policy = "CanViewInfrastructure")]
    public class RecordersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecordersController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: Recorders
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? siteId,
            RecorderType? type,
            int? switchId,
            string? storage,
            string? status,
            int page = 1)
        {
            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }


            var query = _context.Recorders
                .AsNoTracking()
                .AsQueryable();


            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(r =>
                    r.Name.Contains(search));
            }


            // SITE
            if (!string.IsNullOrWhiteSpace(siteId))
            {
                query = query.Where(r =>
                    r.SiteId == siteId);
            }


            // RECORDER TYPE
            if (type.HasValue)
            {
                query = query.Where(r =>
                    r.Type == type.Value);
            }


            // SWITCH
            if (switchId.HasValue)
            {
                query = query.Where(r =>
                    r.NetworkSwitchId == switchId.Value);
            }


            // STORAGE
            if (storage == "yes")
            {
                query = query.Where(r =>
                    r.HasStorage);
            }
            else if (storage == "no")
            {
                query = query.Where(r =>
                    !r.HasStorage);
            }


            // STATUS
            if (status == "active")
            {
                query = query.Where(r =>
                    r.IsActive);
            }
            else if (status == "inactive")
            {
                query = query.Where(r =>
                    !r.IsActive);
            }


            // TOTAL FILTERED RECORDS
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


            // PAGINATED RECORDERS
            var recorders = await query
                .OrderBy(r => r.Site.Name)
                .ThenBy(r => r.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RecorderListItemViewModel
                {
                    Id = r.Id,

                    Name = r.Name,

                    Type = r.Type,

                    SiteId = r.SiteId,

                    SiteName = r.Site.Name,

                    NetworkSwitchName =
                        r.NetworkSwitch != null
                            ? r.NetworkSwitch.Name
                            : null,

                    HasStorage = r.HasStorage,

                    TotalStorageGb =
                        r.HardDrives
                            .Sum(h => (int?)h.CapacityGb)
                        ?? 0,

                    IsActive = r.IsActive
                })
                .ToListAsync();


            var model =
                new PagedResult<RecorderListItemViewModel>
                {
                    Items = recorders,

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


            // SWITCH FILTER OPTIONS
            var switches = await _context.NetworkSwitches
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToListAsync();


            ViewBag.FilterSwitches =
                new SelectList(
                    switches,
                    "Id",
                    "Name",
                    switchId);


            ViewBag.Search =
                search;

            ViewBag.Type =
                type;

            ViewBag.Storage =
                storage;

            ViewBag.Status =
                status;


            // AJAX REQUEST
            if (Request.Headers["X-Requested-With"] ==
                "XMLHttpRequest")
            {
                return PartialView(
                    "_RecorderList",
                    model);
            }


            return View(model);
        }


        // GET: Recorders/Create
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadSitesAsync();

            ViewBag.Switches =
                new SelectList(
                    Enumerable.Empty<object>());

            return View(new RecorderFormViewModel());
        }


        // POST: Recorders/Create
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            RecorderFormViewModel model)
        {
            model.Name = model.Name.Trim();
            var nameExists = await _context.Recorders
    .AnyAsync(r =>
        r.SiteId == model.SiteId &&
        r.Name == model.Name);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A recorder with this name already exists at the selected site.");
            }

            var siteExists = await _context.Sites
                .AnyAsync(s =>
                    s.Id == model.SiteId &&
                    s.IsActive);

            if (!siteExists)
            {
                ModelState.AddModelError(
                    nameof(model.SiteId),
                    "Please select a valid active site.");
            }
            if (model.Type == RecorderType.NVR &&
    !model.NetworkSwitchId.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.NetworkSwitchId),
                    "A network switch is required for an NVR.");
            }

            if (model.NetworkSwitchId.HasValue)
            {
                var switchIsValid =
                    await _context.NetworkSwitches
                        .AnyAsync(s =>
                            s.Id == model.NetworkSwitchId.Value &&
                            s.SiteId == model.SiteId &&
                            s.IsActive);

                if (!switchIsValid)
                {
                    ModelState.AddModelError(
                        nameof(model.NetworkSwitchId),
                        "The selected switch must belong to the selected site.");
                }
            }


            if (!ModelState.IsValid)
            {
                await LoadSitesAsync(model.SiteId);

                await LoadSwitchesAsync(
                    model.SiteId,
                    model.NetworkSwitchId);

                return View(model);
            }


            var recorder = new Recorder
            {
                Name = model.Name,
                Type = model.Type!.Value,
                SiteId = model.SiteId,
                NetworkSwitchId = model.NetworkSwitchId,
                HasStorage = model.HasStorage,
                IsActive = true
            };


            _context.Recorders.Add(recorder);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Recorder created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // GET: Recorders/Edit/5
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var recorder = await _context.Recorders
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recorder == null)
            {
                return NotFound();
            }


            var model = new RecorderFormViewModel
            {
                Id = recorder.Id,
                Name = recorder.Name,
                Type = recorder.Type,
                SiteId = recorder.SiteId,
                NetworkSwitchId = recorder.NetworkSwitchId,
                HasStorage = recorder.HasStorage
            };


            await LoadSitesAsync(
                recorder.SiteId);

            await LoadSwitchesAsync(
                recorder.SiteId,
                recorder.NetworkSwitchId);

            return View(model);
        }


        // POST: Recorders/Edit/5
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            RecorderFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }


            var recorder = await _context.Recorders
                .Include(r => r.HardDrives)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recorder == null)
            {
                return NotFound();
            }


            model.Name = model.Name.Trim();
            var nameExists = await _context.Recorders
    .AnyAsync(r =>
        r.SiteId == model.SiteId &&
        r.Name == model.Name &&
        r.Id != id);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A recorder with this name already exists at the selected site.");
            }

            var siteIsValid = await _context.Sites
                .AnyAsync(s =>
                    s.Id == model.SiteId &&
                    (
                        s.IsActive ||
                        s.Id == recorder.SiteId
                    ));

            if (!siteIsValid)
            {
                ModelState.AddModelError(
                    nameof(model.SiteId),
                    "Please select a valid site.");
            }
            if (model.Type == RecorderType.NVR &&
!model.NetworkSwitchId.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.NetworkSwitchId),
                    "A network switch is required for an NVR.");
            }


            if (model.NetworkSwitchId.HasValue)
            {
                var switchIsValid =
                    await _context.NetworkSwitches
                        .AnyAsync(s =>
                            s.Id == model.NetworkSwitchId.Value &&
                            s.SiteId == model.SiteId &&
                            (
                                s.IsActive ||
                                s.Id == recorder.NetworkSwitchId
                            ));

                if (!switchIsValid)
                {
                    ModelState.AddModelError(
                        nameof(model.NetworkSwitchId),
                        "The selected switch must belong to the selected site.");
                }
            }


            if (!model.HasStorage &&
                recorder.HardDrives.Any())
            {
                ModelState.AddModelError(
                    nameof(model.HasStorage),
                    "Remove the recorder's hard drives before disabling storage.");
            }


            if (!ModelState.IsValid)
            {
                await LoadSitesAsync(
                    model.SiteId,
                    recorder.SiteId);

                await LoadSwitchesAsync(
                    model.SiteId,
                    model.NetworkSwitchId,
                    recorder.NetworkSwitchId);

                return View(model);
            }


            recorder.Name = model.Name;
            recorder.Type = model.Type!.Value;
            recorder.SiteId = model.SiteId;
            recorder.NetworkSwitchId =
                model.NetworkSwitchId;
            recorder.HasStorage =
                model.HasStorage;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Recorder updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // POST: Recorders/ToggleStatus/5
        [Authorize(Policy = "CanChangeStatus")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(
            int id)
        {
            var recorder =
                await _context.Recorders.FindAsync(id);

            if (recorder == null)
            {
                return NotFound();
            }


            recorder.IsActive =
                !recorder.IsActive;

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                recorder.IsActive
                    ? "Recorder activated successfully."
                    : "Recorder deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // Used by the Recorder form when Site changes
        //[Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public async Task<IActionResult> GetSwitchesBySite(
            string siteId)
        {
            var switches = await _context.NetworkSwitches
                .AsNoTracking()
                .Where(s =>
                    s.SiteId == siteId &&
                    s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    id = s.Id,
                    name = s.Name
                })
                .ToListAsync();

            return Json(switches);
        }


        private async Task LoadSitesAsync(
            string? selectedSiteId = null,
            string? currentSiteId = null)
        {
            var sites = await _context.Sites
                .AsNoTracking()
                .Where(s =>
                    s.IsActive ||
                    s.Id == currentSiteId)
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,

                    DisplayName =
                        $"{s.Name} ({s.Id})"
                })
                .ToListAsync();


            ViewBag.Sites =
                new SelectList(
                    sites,
                    "Id",
                    "DisplayName",
                    selectedSiteId);
        }


        private async Task LoadSwitchesAsync(
            string? siteId,
            int? selectedSwitchId = null,
            int? currentSwitchId = null)
        {
            if (string.IsNullOrWhiteSpace(siteId))
            {
                ViewBag.Switches =
                    new SelectList(
                        Enumerable.Empty<object>());

                return;
            }


            var switches =
                await _context.NetworkSwitches
                    .AsNoTracking()
                    .Where(s =>
                        s.SiteId == siteId &&
                        (
                            s.IsActive ||
                            s.Id == currentSwitchId
                        ))
                    .OrderBy(s => s.Name)
                    .ToListAsync();


            ViewBag.Switches =
                new SelectList(
                    switches,
                    "Id",
                    "Name",
                    selectedSwitchId);
        }
        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            var recorder = await _context.Recorders
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new RecorderHistoryViewModel
                {
                    RecorderId = r.Id,

                    RecorderName = r.Name,

                    RecorderType = r.Type.ToString(),

                    SiteId = r.SiteId,

                    SiteName = r.Site.Name,

                    NetworkSwitchName =
                        r.NetworkSwitch != null
                            ? r.NetworkSwitch.Name
                            : null,

                    IsActive = r.IsActive,

                    Visits = _context.Visits
                        .Where(v =>
                            v.RecorderId == r.Id)
                        .OrderByDescending(v =>
                            v.VisitDate)
                        .Select(v =>
                            new RecorderHistoryVisitViewModel
                            {
                                VisitId = v.Id,

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
                        .ToList()
                })
                .FirstOrDefaultAsync();


            if (recorder == null)
            {
                return NotFound();
            }


            return View(recorder);
        }
    }
}