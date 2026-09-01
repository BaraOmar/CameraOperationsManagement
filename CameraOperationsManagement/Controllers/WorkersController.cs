using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.ViewModels.Workers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Roles = "Admin,Editor,Viewer")]
    public class WorkersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkersController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: Workers
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var workers = await _context.Workers
                .AsNoTracking()
                .OrderBy(w => w.FirstName)
                .ThenBy(w => w.SecondName)
                .ThenBy(w => w.LastName)
                .Select(w => new WorkerListItemViewModel
                {
                    Id = w.Id,
                    FirstName = w.FirstName,
                    SecondName = w.SecondName,
                    LastName = w.LastName,
                    IsActive = w.IsActive
                })
                .ToListAsync();

            return View(workers);
        }


        // GET: Workers/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new WorkerFormViewModel());
        }


        // POST: Workers/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            WorkerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var worker = new Worker
            {
                FirstName = model.FirstName.Trim(),
                SecondName = model.SecondName.Trim(),
                LastName = model.LastName.Trim(),
                IsActive = true
            };

            _context.Workers.Add(worker);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Worker created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // GET: Workers/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var worker =
                await _context.Workers.FindAsync(id);

            if (worker == null)
            {
                return NotFound();
            }

            var model = new WorkerFormViewModel
            {
                Id = worker.Id,
                FirstName = worker.FirstName,
                SecondName = worker.SecondName,
                LastName = worker.LastName
            };

            return View(model);
        }


        // POST: Workers/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            WorkerFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var worker =
                await _context.Workers.FindAsync(id);

            if (worker == null)
            {
                return NotFound();
            }

            worker.FirstName = model.FirstName.Trim();
            worker.SecondName = model.SecondName.Trim();
            worker.LastName = model.LastName.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Worker updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // POST: Workers/ToggleStatus/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var worker =
                await _context.Workers.FindAsync(id);

            if (worker == null)
            {
                return NotFound();
            }

            worker.IsActive = !worker.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                worker.IsActive
                    ? "Worker activated successfully."
                    : "Worker deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
        // GET: Workers/History/5
        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            var worker = await _context.Workers
                .AsNoTracking()
                .Where(w => w.Id == id)
                .Select(w => new WorkerHistoryViewModel
                {
                    WorkerId = w.Id,

                    WorkerName =
                        w.FirstName + " " +
                        w.SecondName + " " +
                        w.LastName,

                    IsActive = w.IsActive,


                    SiteVisits = _context.SiteVisitWorkers
                        .Where(vw =>
                            vw.WorkerId == w.Id)
                        .OrderByDescending(vw =>
                            vw.SiteVisit.VisitDate)
                        .Select(vw =>
                            new WorkerSiteVisitHistoryViewModel
                            {
                                VisitId =
                                    vw.SiteVisitId,

                                SiteId =
                                    vw.SiteVisit.SiteId,

                                SiteName =
                                    vw.SiteVisit.Site.Name,

                                VisitDate =
                                    vw.SiteVisit.VisitDate,

                                Purpose =
                                    vw.SiteVisit.Purpose,

                                Notes =
                                    vw.SiteVisit.Notes
                            })
                        .ToList(),


                    CameraVisits = _context.CameraVisitWorkers
                        .Where(vw =>
                            vw.WorkerId == w.Id)
                        .OrderByDescending(vw =>
                            vw.CameraVisit.VisitDate)
                        .Select(vw =>
                            new WorkerCameraVisitHistoryViewModel
                            {
                                VisitId =
                                    vw.CameraVisitId,

                                CameraId =
                                    vw.CameraVisit.CameraId,

                                CameraName =
                                    vw.CameraVisit.Camera.Name,

                                SiteName =
                                    vw.CameraVisit
                                        .Camera
                                        .Recorder
                                        .Site
                                        .Name,

                                RecorderName =
                                    vw.CameraVisit
                                        .Camera
                                        .Recorder
                                        .Name,

                                VisitDate =
                                    vw.CameraVisit.VisitDate,

                                Purpose =
                                    vw.CameraVisit.Purpose,

                                MalfunctionType =
                                    vw.CameraVisit.MalfunctionType,

                                MalfunctionDescription =
                                    vw.CameraVisit.MalfunctionDescription,

                                RepairWorkPerformed =
                                    vw.CameraVisit.RepairWorkPerformed,

                                RepairResult =
                                    vw.CameraVisit.RepairResult,

                                Notes =
                                    vw.CameraVisit.Notes
                            })
                        .ToList()
                })
                .FirstOrDefaultAsync();


            if (worker == null)
            {
                return NotFound();
            }


            return View(worker);
        }
    }
}