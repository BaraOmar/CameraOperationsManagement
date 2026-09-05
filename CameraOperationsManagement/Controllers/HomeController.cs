using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.Models.Enums;
using CameraOperationsManagement.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CameraOperationsManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;


        public HomeController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(AppRoles.Viewer))
            {
                return RedirectToAction(
                    "Index",
                    "Sites");
            }

            var viewModel = new DashboardViewModel
            {
                ActiveSites =
                    await _context.Sites
                        .AsNoTracking()
                        .CountAsync(s => s.IsActive),

                ActiveCameras =
                    await _context.Cameras
                        .AsNoTracking()
                        .CountAsync(c => c.IsActive),

                ActiveWorkers =
                    await _context.Workers
                        .AsNoTracking()
                        .CountAsync(w => w.IsActive),

                TotalVisits =
                    await _context.Visits
                        .AsNoTracking()
                        .CountAsync(),


                RecentVisits =
                    await _context.Visits
                        .AsNoTracking()
                        .OrderByDescending(v =>
                            v.VisitDate)
                        .Take(8)
                        .Select(v =>
                            new RecentVisitViewModel
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
                                    v.ComponentType
                                        .ToString(),

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

                                RepairResult =
                                    v.RepairResult
                            })
                        .ToListAsync()
            };


            return View(viewModel);
        }


        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id ??
                        HttpContext.TraceIdentifier
                });
        }
    }
}