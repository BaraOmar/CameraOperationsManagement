using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.ViewModels.NetworkSwitches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Policy = "CanViewInfrastructure")]
    public class NetworkSwitchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NetworkSwitchesController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: NetworkSwitches
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var switches = await _context.NetworkSwitches
                .AsNoTracking()
                .Include(s => s.Site)
                .OrderBy(s => s.Site.Name)
                .ThenBy(s => s.Name)
                .Select(s => new NetworkSwitchListItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    SiteId = s.SiteId,
                    SiteName = s.Site.Name,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return View(switches);
        }


        // GET: NetworkSwitches/Create
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadSitesAsync();

            return View(new NetworkSwitchFormViewModel());
        }


        // POST: NetworkSwitches/Create
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            NetworkSwitchFormViewModel model)
        {
            model.Name = model.Name.Trim();


            // Validate selected Site
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


            // Switch Name must be unique within the selected Site
            var nameExists = await _context.NetworkSwitches
                .AnyAsync(s =>
                    s.SiteId == model.SiteId &&
                    s.Name == model.Name);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A switch with this name already exists at the selected site.");
            }


            if (!ModelState.IsValid)
            {
                await LoadSitesAsync(model.SiteId);

                return View(model);
            }


            var networkSwitch = new NetworkSwitch
            {
                Name = model.Name,
                SiteId = model.SiteId,
                IsActive = true
            };


            _context.NetworkSwitches.Add(networkSwitch);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Network switch created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // GET: NetworkSwitches/Edit/5
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var networkSwitch =
                await _context.NetworkSwitches
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

            if (networkSwitch == null)
            {
                return NotFound();
            }

            var model = new NetworkSwitchFormViewModel
            {
                Id = networkSwitch.Id,
                Name = networkSwitch.Name,
                SiteId = networkSwitch.SiteId
            };

            await LoadSitesAsync(model.SiteId);

            return View(model);
        }


        // POST: NetworkSwitches/Edit/5
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            NetworkSwitchFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!await _context.Sites
                    .AnyAsync(s =>
                        s.Id == model.SiteId &&
                        s.IsActive))
            {
                ModelState.AddModelError(
                    nameof(model.SiteId),
                    "Please select a valid active site.");
            }
            model.Name = model.Name.Trim();

            var nameExists = await _context.NetworkSwitches
                .AnyAsync(s =>
                    s.SiteId == model.SiteId &&
                    s.Name == model.Name &&
                    s.Id != id);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A switch with this name already exists at the selected site.");
            }
            if (!ModelState.IsValid)
            {
                await LoadSitesAsync(model.SiteId);

                return View(model);
            }

            var networkSwitch =
                await _context.NetworkSwitches.FindAsync(id);

            if (networkSwitch == null)
            {
                return NotFound();
            }

            networkSwitch.Name = model.Name.Trim();
            networkSwitch.SiteId = model.SiteId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Network switch updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // POST: NetworkSwitches/ToggleStatus/5
        [Authorize(Policy = "CanChangeStatus")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var networkSwitch =
                await _context.NetworkSwitches.FindAsync(id);

            if (networkSwitch == null)
            {
                return NotFound();
            }

            networkSwitch.IsActive =
                !networkSwitch.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                networkSwitch.IsActive
                    ? "Network switch activated successfully."
                    : "Network switch deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }


        private async Task LoadSitesAsync(
            string? selectedSiteId = null)
        {
            var sites = await _context.Sites
                .AsNoTracking()
                .Where(s =>
                    s.IsActive ||
                    s.Id == selectedSiteId)
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
        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            var networkSwitch = await _context.NetworkSwitches
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new SwitchHistoryViewModel
                {
                    SwitchId = s.Id,

                    SwitchName = s.Name,

                    SiteId = s.SiteId,

                    SiteName = s.Site.Name,

                    IsActive = s.IsActive,

                    Visits = _context.Visits
                        .Where(v =>
                            v.NetworkSwitchId == s.Id)
                        .OrderByDescending(v =>
                            v.VisitDate)
                        .Select(v =>
                            new SwitchHistoryVisitViewModel
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


            if (networkSwitch == null)
            {
                return NotFound();
            }


            return View(networkSwitch);
        }
    }
}