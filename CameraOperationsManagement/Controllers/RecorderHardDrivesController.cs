using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.ViewModels.RecorderHardDrives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Policy = "CanViewInfrastructure")]
    public class RecorderHardDrivesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecorderHardDrivesController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: RecorderHardDrives?recorderId=5
        [HttpGet]
        public async Task<IActionResult> Index(
            int recorderId)
        {
            var recorder = await _context.Recorders
                .AsNoTracking()
                .Include(r => r.Site)
                .Include(r => r.HardDrives)
                .FirstOrDefaultAsync(r =>
                    r.Id == recorderId);

            if (recorder == null)
            {
                return NotFound();
            }

            if (!recorder.HasStorage)
            {
                TempData["ErrorMessage"] =
                    "Storage is not enabled for this recorder.";

                return RedirectToAction(
                    "Index",
                    "Recorders");
            }


            var model = new RecorderHardDriveListViewModel
            {
                RecorderId = recorder.Id,
                RecorderName = recorder.Name,
                SiteName = recorder.Site.Name,
                RecorderIsActive = recorder.IsActive,

                HardDrives = recorder.HardDrives
                    .OrderBy(h => h.Id)
                    .Select(h =>
                        new RecorderHardDriveItemViewModel
                        {
                            Id = h.Id,
                            CapacityGb = h.CapacityGb,
                            SerialNumber = h.SerialNumber
                        })
                    .ToList()
            };

            return View(model);
        }


        // GET: RecorderHardDrives/Create?recorderId=5
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpGet]
        public async Task<IActionResult> Create(
            int recorderId)
        {
            var recorder = await GetManageableRecorderAsync(
                recorderId);

            if (recorder == null)
            {
                return NotFound();
            }

            return View(
                new RecorderHardDriveFormViewModel
                {
                    RecorderId = recorder.Id
                });
        }


        // POST: RecorderHardDrives/Create
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            RecorderHardDriveFormViewModel model)
        {
            var recorder = await GetManageableRecorderAsync(
                model.RecorderId);

            if (recorder == null)
            {
                return NotFound();
            }
            model.SerialNumber =
    string.IsNullOrWhiteSpace(model.SerialNumber)
        ? null
        : model.SerialNumber.Trim();

            if (model.SerialNumber != null)
            {
                var serialExists =
                    await _context.RecorderHardDrives
                        .AnyAsync(h =>
                            h.RecorderId == model.RecorderId &&
                            h.SerialNumber == model.SerialNumber);

                if (serialExists)
                {
                    ModelState.AddModelError(
                        nameof(model.SerialNumber),
                        "This serial number already exists for this recorder.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var hardDrive = new RecorderHardDrive
            {
                RecorderId = recorder.Id,
                CapacityGb = model.CapacityGb,
                SerialNumber = model.SerialNumber
            };


            _context.RecorderHardDrives.Add(hardDrive);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Hard drive added successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    recorderId = recorder.Id
                });
        }


        // GET: RecorderHardDrives/Edit/5
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hardDrive = await _context.RecorderHardDrives
                .AsNoTracking()
                .Include(h => h.Recorder)
                .FirstOrDefaultAsync(h =>
                    h.Id == id);

            if (hardDrive == null ||
                !hardDrive.Recorder.HasStorage ||
                !hardDrive.Recorder.IsActive)
            {
                return NotFound();
            }


            var model =
                new RecorderHardDriveFormViewModel
                {
                    Id = hardDrive.Id,
                    RecorderId = hardDrive.RecorderId,
                    CapacityGb = hardDrive.CapacityGb,
                    SerialNumber = hardDrive.SerialNumber
                };

            return View(model);
        }


        // POST: RecorderHardDrives/Edit/5
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            RecorderHardDriveFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }


            var hardDrive = await _context.RecorderHardDrives
                .Include(h => h.Recorder)
                .FirstOrDefaultAsync(h =>
                    h.Id == id);

            if (hardDrive == null ||
                !hardDrive.Recorder.HasStorage ||
                !hardDrive.Recorder.IsActive)
            {
                return NotFound();
            }


            if (hardDrive.RecorderId != model.RecorderId)
            {
                return BadRequest();
            }

            model.SerialNumber =
string.IsNullOrWhiteSpace(model.SerialNumber)
? null
: model.SerialNumber.Trim();

            if (model.SerialNumber != null)
            {
                var serialExists =
                    await _context.RecorderHardDrives
                        .AnyAsync(h =>
                            h.RecorderId == model.RecorderId &&
                            h.SerialNumber == model.SerialNumber &&
                            h.Id != id);

                if (serialExists)
                {
                    ModelState.AddModelError(
                        nameof(model.SerialNumber),
                        "This serial number already exists for this recorder.");
                }
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            hardDrive.CapacityGb = model.CapacityGb;
            hardDrive.SerialNumber = model.SerialNumber;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Hard drive updated successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    recorderId = hardDrive.RecorderId
                });
        }


        // POST: RecorderHardDrives/Delete/5
        [Authorize(Policy = "CanChangeStatus")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hardDrive = await _context.RecorderHardDrives
                .Include(h => h.Recorder)
                .FirstOrDefaultAsync(h =>
                    h.Id == id);

            if (hardDrive == null ||
                !hardDrive.Recorder.HasStorage ||
                !hardDrive.Recorder.IsActive)
            {
                return NotFound();
            }


            var recorderId =
                hardDrive.RecorderId;


            _context.RecorderHardDrives.Remove(
                hardDrive);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Hard drive removed successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    recorderId
                });
        }


        private async Task<Recorder?>
            GetManageableRecorderAsync(int id)
        {
            return await _context.Recorders
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.IsActive &&
                    r.HasStorage);
        }
    }
}