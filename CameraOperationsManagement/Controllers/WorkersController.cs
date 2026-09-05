using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.Models.Enums;
using CameraOperationsManagement.ViewModels.Workers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Policy = "CanViewWorkers")]
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
        [Authorize(Policy = "CanManageWorkers")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new WorkerFormViewModel());
        }


        // POST: Workers/Create
        [Authorize(Policy = "CanManageWorkers")]
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
        [Authorize(Policy = "CanManageWorkers")]
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
        [Authorize(Policy = "CanManageWorkers")]
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
        [Authorize(Policy = "CanChangeStatus")]
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
        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            var worker = await GetWorkerHistoryAsync(id);

            if (worker == null)
            {
                return NotFound();
            }

            return View(worker);
        }
        private async Task<WorkerHistoryViewModel?>
    GetWorkerHistoryAsync(int id)
        {
            return await _context.Workers
                .AsNoTracking()
                .Where(w => w.Id == id)
                .Select(w => new WorkerHistoryViewModel
                {
                    WorkerId = w.Id,

                    FirstName = w.FirstName,

                    SecondName = w.SecondName,

                    LastName = w.LastName,

                    IsActive = w.IsActive,


                    Visits = _context.Visits
                        .Where(v =>
                            v.VisitWorkers.Any(vw =>
                                vw.WorkerId == w.Id))
                        .OrderByDescending(v =>
                            v.VisitDate)
                        .Select(v =>
                            new WorkerHistoryVisitViewModel
                            {
                                VisitId =
                                    v.Id,

                                VisitDate =
                                    v.VisitDate,

                                SiteId =
                                    v.SiteId,

                                SiteName =
                                    v.Site.Name,

                                ComponentType =
                                    v.ComponentType,

                                ComponentName =
                                    v.ComponentType ==
                                        VisitComponentType.Recorder
                                        ? v.Recorder!.Name

                                        : v.ComponentType ==
                                          VisitComponentType.Switch
                                            ? v.NetworkSwitch!.Name

                                            : v.Camera!.Name,

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
                                    v.Notes
                            })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}