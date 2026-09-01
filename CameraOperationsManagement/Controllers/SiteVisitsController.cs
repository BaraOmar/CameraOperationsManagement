using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.ViewModels.SiteVisits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Roles = "Admin,Editor,Viewer")]
    public class SiteVisitsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SiteVisitsController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var visits = await _context.SiteVisits
                .AsNoTracking()
                .OrderByDescending(v => v.VisitDate)
                .Select(v => new SiteVisitListItemViewModel
                {
                    Id = v.Id,

                    SiteId = v.SiteId,

                    SiteName = v.Site.Name,

                    VisitDate = v.VisitDate,

                    Purpose = v.Purpose,

                    WorkerNames = v.SiteVisitWorkers
                        .OrderBy(vw => vw.Worker.FirstName)
                        .Select(vw =>
                            vw.Worker.FirstName + " " +
                            vw.Worker.SecondName + " " +
                            vw.Worker.LastName)
                        .ToList()
                })
                .ToListAsync();

            return View(visits);
        }


        [Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadSitesAsync();
            await LoadWorkersAsync();

            return View(
                new SiteVisitFormViewModel
                {
                    VisitDate = DateTime.Now
                });
        }


        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SiteVisitFormViewModel viewModel)
        {
            var siteExists = await _context.Sites
                .AnyAsync(s =>
                    s.Id == viewModel.SiteId &&
                    s.IsActive);

            if (!siteExists)
            {
                ModelState.AddModelError(
                    nameof(viewModel.SiteId),
                    "Please select a valid active site.");
            }


            var workerIds = viewModel.WorkerIds
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

                if (validWorkerCount != workerIds.Count)
                {
                    ModelState.AddModelError(
                        nameof(viewModel.WorkerIds),
                        "One or more selected workers are invalid or inactive.");
                }
            }


            if (!ModelState.IsValid)
            {
                await LoadSitesAsync(
                    viewModel.SiteId);

                await LoadWorkersAsync(
                    workerIds);

                return View(viewModel);
            }


            var visit = new SiteVisit
            {
                SiteId = viewModel.SiteId,

                VisitDate =
                    viewModel.VisitDate,

                Purpose =
                    viewModel.Purpose.Trim(),

                Notes =
                    string.IsNullOrWhiteSpace(viewModel.Notes)
                        ? null
                        : viewModel.Notes.Trim()
            };


            foreach (var workerId in workerIds)
            {
                visit.SiteVisitWorkers.Add(
                    new SiteVisitWorker
                    {
                        WorkerId = workerId
                    });
            }


            _context.SiteVisits.Add(visit);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Site visit created successfully.";

            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin,Editor")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var visit = await _context.SiteVisits
                .AsNoTracking()
                .Include(v => v.SiteVisitWorkers)
                .FirstOrDefaultAsync(v =>
                    v.Id == id);

            if (visit == null)
            {
                return NotFound();
            }


            var viewModel =
                new SiteVisitFormViewModel
                {
                    Id = visit.Id,

                    SiteId = visit.SiteId,

                    VisitDate = visit.VisitDate,

                    Purpose = visit.Purpose,

                    Notes = visit.Notes,

                    WorkerIds = visit.SiteVisitWorkers
                        .Select(vw => vw.WorkerId)
                        .ToList()
                };


            await LoadSitesAsync(
                visit.SiteId,
                visit.SiteId);

            await LoadWorkersAsync(
                viewModel.WorkerIds,
                viewModel.WorkerIds);

            return View(viewModel);
        }


        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            SiteVisitFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }


            var visit = await _context.SiteVisits
                .Include(v => v.SiteVisitWorkers)
                .FirstOrDefaultAsync(v =>
                    v.Id == id);

            if (visit == null)
            {
                return NotFound();
            }


            var siteExists = await _context.Sites
                .AnyAsync(s =>
                    s.Id == viewModel.SiteId &&
                    (
                        s.IsActive ||
                        s.Id == visit.SiteId
                    ));

            if (!siteExists)
            {
                ModelState.AddModelError(
                    nameof(viewModel.SiteId),
                    "Please select a valid site.");
            }


            var workerIds = viewModel.WorkerIds
                .Distinct()
                .ToList();


            if (workerIds.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(viewModel.WorkerIds),
                    "Please select at least one worker.");
            }


            if (!ModelState.IsValid)
            {
                await LoadSitesAsync(
                    viewModel.SiteId,
                    visit.SiteId);

                await LoadWorkersAsync(
                    workerIds,
                    visit.SiteVisitWorkers
                        .Select(vw => vw.WorkerId)
                        .ToList());

                return View(viewModel);
            }


            visit.SiteId =
                viewModel.SiteId;

            visit.VisitDate =
                viewModel.VisitDate;

            visit.Purpose =
                viewModel.Purpose.Trim();

            visit.Notes =
                string.IsNullOrWhiteSpace(viewModel.Notes)
                    ? null
                    : viewModel.Notes.Trim();


            _context.SiteVisitWorkers.RemoveRange(
                visit.SiteVisitWorkers);

            visit.SiteVisitWorkers =
                workerIds
                    .Select(workerId =>
                        new SiteVisitWorker
                        {
                            SiteVisitId = visit.Id,
                            WorkerId = workerId
                        })
                    .ToList();


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Site visit updated successfully.";

            return RedirectToAction(nameof(Index));
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


            var workers = await _context.Workers
                .AsNoTracking()
                .Where(w =>
                    w.IsActive ||
                    current.Contains(w.Id))
                .OrderBy(w => w.FirstName)
                .ThenBy(w => w.SecondName)
                .ThenBy(w => w.LastName)
                .ToListAsync();


            ViewBag.Workers =
                workers.Select(w =>
                    new SelectListItem
                    {
                        Value = w.Id.ToString(),

                        Text =
                            $"{w.FirstName} {w.SecondName} {w.LastName}",

                        Selected =
                            selected.Contains(w.Id)
                    })
                .ToList();
        }
    }
}