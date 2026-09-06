using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.Models.Enums;
using CameraOperationsManagement.ViewModels.Common;
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
        public async Task<IActionResult> Index(
            string? search,
            string? siteId,
            string? status,
            int page = 1)
        {
            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }


            var query = _context.NetworkSwitches
                .AsNoTracking()
                .AsQueryable();


            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(s =>
                    s.Name.Contains(search));
            }


            // SITE
            if (!string.IsNullOrWhiteSpace(siteId))
            {
                query = query.Where(s =>
                    s.SiteId == siteId);
            }


            // STATUS
            if (status == "active")
            {
                query = query.Where(s =>
                    s.IsActive);
            }
            else if (status == "inactive")
            {
                query = query.Where(s =>
                    !s.IsActive);
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


            var switches = await query
                .OrderBy(s => s.Site.Name)
                .ThenBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s =>
                    new NetworkSwitchListItemViewModel
                    {
                        Id = s.Id,

                        Name = s.Name,

                        SiteId = s.SiteId,

                        SiteName = s.Site.Name,

                        IsActive = s.IsActive
                    })
                .ToListAsync();


            var model =
                new PagedResult<NetworkSwitchListItemViewModel>
                {
                    Items = switches,

                    Page = page,

                    PageSize = pageSize,

                    TotalItems = totalItems
                };


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


            ViewBag.Search = search;

            ViewBag.Status = status;


            if (Request.Headers["X-Requested-With"] ==
                "XMLHttpRequest")
            {
                return PartialView(
                    "_NetworkSwitchList",
                    model);
            }


            return View(model);
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
            model.Name =
                (model.Name ?? string.Empty).Trim();


            // Validate selected Site
            var siteExists =
                await _context.Sites
                    .AnyAsync(s =>
                        s.Id == model.SiteId &&
                        s.IsActive);

            if (!siteExists)
            {
                ModelState.AddModelError(
                    nameof(model.SiteId),
                    "Please select a valid active site.");
            }


            // Validate number of ports
            var allowedPortCounts =
                new[]
                {
            4,
            8,
            16,
            32
                };

            if (!model.NumberOfPorts.HasValue ||
                !allowedPortCounts.Contains(
                    model.NumberOfPorts.Value))
            {
                ModelState.AddModelError(
                    nameof(model.NumberOfPorts),
                    "Please select 4, 8, 16, or 32 ports.");
            }


            // Switch Name must be unique within the selected Site
            var nameExists =
                await _context.NetworkSwitches
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
                await LoadSitesAsync(
                    model.SiteId);

                return View(model);
            }


            var networkSwitch =
                new NetworkSwitch
                {
                    Name =
                        model.Name,

                    SiteId =
                        model.SiteId,

                    NumberOfPorts =
                        model.NumberOfPorts!.Value,

                    IsActive =
                        true
                };


            // Automatically create all physical ports.
            for (var portNumber = 1;
                 portNumber <= model.NumberOfPorts.Value;
                 portNumber++)
            {
                networkSwitch.Ports.Add(
                    new NetworkSwitchPort
                    {
                        PortNumber =
                            portNumber,

                        Status =
                            SwitchPortStatus.Available
                    });
            }


            _context.NetworkSwitches.Add(
                networkSwitch);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Network switch created successfully.";


            return RedirectToAction(
                nameof(Index));
        }

        // GET: NetworkSwitches/Edit/5
        [Authorize(Policy = "CanManageInfrastructure")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var networkSwitch =
                await _context.NetworkSwitches
                    .AsNoTracking()
                    .Include(s => s.Ports)
                        .ThenInclude(p => p.Camera)
                    .FirstOrDefaultAsync(s => s.Id == id);

            if (networkSwitch == null)
            {
                return NotFound();
            }


            var model = new NetworkSwitchFormViewModel
            {
                Id = networkSwitch.Id,

                Name = networkSwitch.Name,

                SiteId = networkSwitch.SiteId,

                NumberOfPorts =
                    networkSwitch.NumberOfPorts == 0
                        ? null
                        : networkSwitch.NumberOfPorts,

                Ports = networkSwitch.Ports
                    .OrderBy(p => p.PortNumber)
                    .Select(p => new NetworkSwitchPortEditViewModel
                    {
                        Id = p.Id,

                        PortNumber = p.PortNumber,

                        Status = p.Status,

                        CameraName =
                            p.Camera != null
                                ? p.Camera.Name
                                : null
                    })
                    .ToList()
            };


            // Old switches with 0 ports may choose the count once.
            ViewBag.CanChangePortCount =
                networkSwitch.NumberOfPorts == 0;


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


            var networkSwitch =
                await _context.NetworkSwitches
                    .Include(s => s.Ports)
                        .ThenInclude(p => p.Camera)
                    .FirstOrDefaultAsync(s => s.Id == id);

            if (networkSwitch == null)
            {
                return NotFound();
            }


            model.Name =
                (model.Name ?? string.Empty).Trim();


            var isOldSwitch =
                networkSwitch.NumberOfPorts == 0;


            // Existing configured switches cannot change port count.
            if (!isOldSwitch)
            {
                model.NumberOfPorts =
                    networkSwitch.NumberOfPorts;

                ModelState.Remove(
                    nameof(model.NumberOfPorts));
            }
            else
            {
                var allowedPortCounts =
                    new[]
                    {
                4,
                8,
                16,
                32
                    };

                if (!model.NumberOfPorts.HasValue ||
                    !allowedPortCounts.Contains(
                        model.NumberOfPorts.Value))
                {
                    ModelState.AddModelError(
                        nameof(model.NumberOfPorts),
                        "Please select 4, 8, 16, or 32 ports.");
                }
            }


            // Validate selected site.
            var siteExists =
                await _context.Sites
                    .AnyAsync(s =>
                        s.Id == model.SiteId &&
                        s.IsActive);

            if (!siteExists)
            {
                ModelState.AddModelError(
                    nameof(model.SiteId),
                    "Please select a valid active site.");
            }


            // Unique switch name inside the site.
            var nameExists =
                await _context.NetworkSwitches
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


            // Validate requested port states.
            foreach (var port in networkSwitch.Ports)
            {
                // Used is automatic and cannot be manually changed.
                if (port.CameraId.HasValue)
                {
                    port.Status =
                        SwitchPortStatus.Used;

                    continue;
                }


                var postedPort =
                    model.Ports.FirstOrDefault(p =>
                        p.Id == port.Id);

                if (postedPort == null)
                {
                    continue;
                }


                if (postedPort.Status !=
                        SwitchPortStatus.Available &&
                    postedPort.Status !=
                        SwitchPortStatus.OutOfService)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Port {port.PortNumber} has an invalid state.");
                }
            }


            if (!ModelState.IsValid)
            {
                model.Ports =
                    networkSwitch.Ports
                        .OrderBy(p => p.PortNumber)
                        .Select(p =>
                            new NetworkSwitchPortEditViewModel
                            {
                                Id = p.Id,

                                PortNumber =
                                    p.PortNumber,

                                Status =
                                    p.Status,

                                CameraName =
                                    p.Camera != null
                                        ? p.Camera.Name
                                        : null
                            })
                        .ToList();


                ViewBag.CanChangePortCount =
                    isOldSwitch;


                await LoadSitesAsync(
                    model.SiteId);


                return View(model);
            }


            networkSwitch.Name =
                model.Name;

            networkSwitch.SiteId =
                model.SiteId;


            // Existing old switch:
            // configure its real physical ports once.
            if (isOldSwitch)
            {
                networkSwitch.NumberOfPorts =
                    model.NumberOfPorts!.Value;


                for (var portNumber = 1;
                     portNumber <=
                        networkSwitch.NumberOfPorts;
                     portNumber++)
                {
                    networkSwitch.Ports.Add(
                        new NetworkSwitchPort
                        {
                            PortNumber =
                                portNumber,

                            Status =
                                SwitchPortStatus.Available
                        });
                }
            }
            else
            {
                // Update only unused port states.
                foreach (var port in
                         networkSwitch.Ports)
                {
                    if (port.CameraId.HasValue)
                    {
                        port.Status =
                            SwitchPortStatus.Used;

                        continue;
                    }


                    var postedPort =
                        model.Ports.FirstOrDefault(p =>
                            p.Id == port.Id);

                    if (postedPort == null)
                    {
                        continue;
                    }


                    port.Status =
                        postedPort.Status;
                }
            }


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Network switch updated successfully.";


            return RedirectToAction(
                nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var networkSwitch =
                await _context.NetworkSwitches
                    .AsNoTracking()
                    .Where(s => s.Id == id)
                    .Select(s =>
                        new NetworkSwitchDetailsViewModel
                        {
                            Id = s.Id,

                            Name = s.Name,

                            SiteId = s.SiteId,

                            SiteName = s.Site.Name,

                            NumberOfPorts = s.NumberOfPorts,

                            IsActive = s.IsActive,

                            AvailablePorts =
                                s.Ports.Count(p =>
                                    p.Status ==
                                    SwitchPortStatus.Available),

                            UsedPorts =
                                s.Ports.Count(p =>
                                    p.Status ==
                                    SwitchPortStatus.Used),

                            OutOfServicePorts =
                                s.Ports.Count(p =>
                                    p.Status ==
                                    SwitchPortStatus.OutOfService),

                            Ports =
    s.Ports
        .OrderBy(p => p.PortNumber)
        .Select(p =>
            new NetworkSwitchPortDetailsViewModel
            {
                Id =
                    p.Id,

                PortNumber =
                    p.PortNumber,

                Status =
                    p.Status,

                CameraId =
                    p.CameraId,

                CameraName =
                    p.Camera != null
                        ? p.Camera.Name
                        : null,

                CameraIpAddress =
                    p.Camera != null
                        ? p.Camera.IpAddress
                        : null,

                CameraInstallationLocation =
                    p.Camera != null
                        ? p.Camera.InstallationLocation
                        : null,

                CameraDescription =
                    p.Camera != null
                        ? p.Camera.Notes
                        : null
            })
        .ToList(),

                            Recorders =
                                _context.Recorders
                                    .Where(r =>
                                        r.NetworkSwitchId == s.Id)
                                    .OrderBy(r => r.Name)
                                    .Select(r =>
                                        new NetworkSwitchRecorderDetailsViewModel
                                        {
                                            Id = r.Id,

                                            Name = r.Name,

                                            Type =
                                                r.Type.ToString(),

                                            IsActive =
                                                r.IsActive
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