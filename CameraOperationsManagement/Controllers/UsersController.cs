using CameraOperationsManagement.Models;
using CameraOperationsManagement.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CameraOperationsManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly string[] AllowedRoles =
        {
            "Admin",
            "Editor",
            "Viewer"
        };


        public UsersController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        // GET: Users
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToList();

            var result = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles =
                    await _userManager.GetRolesAsync(user);

                result.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    SecondName = user.SecondName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? string.Empty
                });
            }

            return View(result);
        }


        // GET: Users/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = AllowedRoles;

            return View(new CreateUserViewModel());
        }


        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateUserViewModel model)
        {
            ViewBag.Roles = AllowedRoles;

            if (!AllowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Invalid role.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser =
                await _userManager.FindByEmailAsync(
                    model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "A user with this email already exists.");

                return View(model);
            }

            var user = new ApplicationUser
            {
                FirstName = model.FirstName.Trim(),
                SecondName = model.SecondName.Trim(),
                LastName = model.LastName.Trim(),

                Email = model.Email.Trim(),
                UserName = model.Email.Trim(),

                EmailConfirmed = true
            };

            var createResult =
                await _userManager.CreateAsync(
                    user,
                    model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    model.Role);

            if (!roleResult.Succeeded)
            {
                // Prevent leaving a user without the selected role.
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            TempData["SuccessMessage"] =
                "User created successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}