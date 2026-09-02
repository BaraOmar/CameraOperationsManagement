using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.Services;
using CameraOperationsManagement.ViewModels.Sites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Roles = "Admin,Editor,Viewer")]
    public class SitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public SitesController(
            ApplicationDbContext context,
            IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }


        // GET: Sites
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sites = await _context.Sites
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new SiteListItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Location = s.Location,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return View(sites);
        }


        // GET: Sites/Create
        [Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateSiteViewModel());
        }


        // POST: Sites/Create
        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateSiteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Normalize input
            model.Id = model.Id.Trim();
            model.Name = model.Name.Trim();
            model.Location = model.Location?.Trim();
            model.Notes = model.Notes?.Trim();


            // Site ID must be unique
            var idExists = await _context.Sites
                .AnyAsync(s => s.Id == model.Id);

            if (idExists)
            {
                ModelState.AddModelError(
                    nameof(model.Id),
                    "A site with this ID already exists.");
            }


            // Site Name must be unique
            var nameExists = await _context.Sites
                .AnyAsync(s => s.Name == model.Name);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A site with this name already exists.");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var site = new Site
            {
                Id = model.Id,
                Name = model.Name,
                Location = model.Location,
                Notes = model.Notes,
                IsActive = true
            };


            _context.Sites.Add(site);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Site created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Sites/Edit/ABC
        [Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var site = await _context.Sites
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (site == null)
            {
                return NotFound();
            }

            var model = new EditSiteViewModel
            {
                Id = site.Id,
                Name = site.Name,
                Location = site.Location,
                Notes = site.Notes
            };

            return View(model);
        }


        // POST: Sites/Edit/ABC
        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            EditSiteViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }
            model.Name = model.Name.Trim();

            var nameExists = await _context.Sites
                .AnyAsync(s =>
                    s.Name == model.Name &&
                    s.Id != id);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A site with this name already exists.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var site =
                await _context.Sites.FindAsync(id);

            if (site == null)
            {
                return NotFound();
            }

            site.Name = model.Name.Trim();
            site.Location = model.Location?.Trim();
            site.Notes = model.Notes?.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Site updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // POST: Sites/ToggleStatus/ABC
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(
            string id)
        {
            var site =
                await _context.Sites.FindAsync(id);

            if (site == null)
            {
                return NotFound();
            }

            site.IsActive = !site.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                site.IsActive
                    ? "Site activated successfully."
                    : "Site deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
        // GET: Sites/History/001
        [HttpGet]
        public async Task<IActionResult> History(string id)
        {
            var site = await GetSiteHistoryAsync(id);

            if (site == null)
            {
                return NotFound();
            }

            return View(site);
        }
        [HttpGet]
        public async Task<IActionResult> ExportHistoryPdf(string id)
        {
            var site = await GetSiteHistoryAsync(id);

            if (site == null)
            {
                return NotFound();
            }

            var pdfBytes =
                _pdfService.GenerateSiteHistoryPdf(site);

            var fileName =
                $"Site-History-{site.SiteId}.pdf";

            Response.Headers.ContentDisposition =
                $"inline; filename=\"{fileName}\"";

            return File(
                pdfBytes,
                "application/pdf");
        }
        private async Task<SiteHistoryViewModel?>
    GetSiteHistoryAsync(string id)
        {
            return await _context.Sites
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new SiteHistoryViewModel
                {
                    SiteId = s.Id,

                    SiteName = s.Name,

                    Location = s.Location,

                    Notes = s.Notes,

                    IsActive = s.IsActive,


                    SiteVisits = _context.SiteVisits
                        .Where(v =>
                            v.SiteId == s.Id)
                        .OrderByDescending(v =>
                            v.VisitDate)
                        .Select(v =>
                            new SiteHistorySiteVisitViewModel
                            {
                                VisitId = v.Id,

                                VisitDate = v.VisitDate,

                                Purpose = v.Purpose,

                                Notes = v.Notes,

                                WorkerNames =
                                    v.SiteVisitWorkers
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
                        .ToList(),


                    CameraVisits = _context.CameraVisits
                        .Where(v =>
                            v.Camera.Recorder.SiteId == s.Id)
                        .OrderByDescending(v =>
                            v.VisitDate)
                        .Select(v =>
                            new SiteHistoryCameraVisitViewModel
                            {
                                VisitId = v.Id,

                                CameraId =
                                    v.CameraId,

                                CameraName =
                                    v.Camera.Name,

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
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}