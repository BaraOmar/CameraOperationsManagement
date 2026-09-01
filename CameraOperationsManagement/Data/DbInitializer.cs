using CameraOperationsManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace CameraOperationsManagement.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var roleManager =
                serviceProvider.GetRequiredService<
                    RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<
                    UserManager<ApplicationUser>>();

            string[] roles =
            {
                "Admin",
                "Editor",
                "Viewer"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            var email =
                configuration["SeedAdmin:Email"];

            var password =
                configuration["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var admin =
                await userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    FirstName =
                        configuration["SeedAdmin:FirstName"]
                        ?? string.Empty,

                    SecondName =
                        configuration["SeedAdmin:SecondName"]
                        ?? string.Empty,

                    LastName =
                        configuration["SeedAdmin:LastName"]
                        ?? string.Empty,

                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result =
                    await userManager.CreateAsync(
                        admin,
                        password);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(
                            Environment.NewLine,
                            result.Errors.Select(
                                e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(
                    admin,
                    "Admin"))
            {
                await userManager.AddToRoleAsync(
                    admin,
                    "Admin");
            }
        }
    }
}