using System.Diagnostics;
using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

                TotalCameraVisits =
                    await _context.CameraVisits
                        .AsNoTracking()
                        .CountAsync(),


                RecentCameraVisits =
                    await _context.CameraVisits
                        .AsNoTracking()
                        .OrderByDescending(v => v.VisitDate)
                        .Take(5)
                        .Select(v =>
                            new RecentCameraVisitViewModel
                            {
                                VisitId = v.Id,

                                CameraName =
                                    v.Camera.Name,

                                SiteName =
                                    v.Camera
                                        .Recorder
                                        .Site
                                        .Name,

                                VisitDate =
                                    v.VisitDate,

                                Purpose =
                                    v.Purpose,

                                MalfunctionType =
                                    v.MalfunctionType,

                                RepairResult =
                                    v.RepairResult
                            })
                        .ToListAsync(),


                RecentSiteVisits =
                    await _context.SiteVisits
                        .AsNoTracking()
                        .OrderByDescending(v => v.VisitDate)
                        .Take(5)
                        .Select(v =>
                            new RecentSiteVisitViewModel
                            {
                                VisitId =
                                    v.Id,

                                SiteId =
                                    v.SiteId,

                                SiteName =
                                    v.Site.Name,

                                VisitDate =
                                    v.VisitDate,

                                Purpose =
                                    v.Purpose
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