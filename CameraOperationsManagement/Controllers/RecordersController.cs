using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.ViewModels.Recorders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Roles = "Admin,Editor,Viewer")]
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
        public async Task<IActionResult> Index()
        {
            var recorders = await _context.Recorders
                .AsNoTracking()
                .OrderBy(r => r.Site.Name)
                .ThenBy(r => r.Name)
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

            return View(recorders);
        }


        // GET: Recorders/Create
        [Authorize(Roles = "Admin,Editor")]
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
        [Authorize(Roles = "Admin,Editor")]
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
        [Authorize(Roles = "Admin,Editor")]
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
        [Authorize(Roles = "Admin,Editor")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin,Editor")]
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
    }
}