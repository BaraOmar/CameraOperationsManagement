using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace CameraOperationsManagement.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AuditService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task LogAsync(
            string action,
            string entityType,
            string entityId,
            string? description = null)
        {
            var principal =
                _httpContextAccessor.HttpContext?.User;


            ApplicationUser? user = null;

            if (principal?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager
                    .GetUserAsync(principal);
            }


            var displayName = user == null
                ? "System"
                : BuildDisplayName(user);


            var auditLog = new AuditLog
            {
                UserId =
                    user?.Id ?? "SYSTEM",

                UserDisplayName =
                    displayName,

                UserEmail =
                    user?.Email,

                Action =
                    action.Trim(),

                EntityType =
                    entityType.Trim(),

                EntityId =
                    entityId.Trim(),

                Description =
                    Normalize(description),

                PerformedAtUtc =
                    DateTime.UtcNow
            };


            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
        }


        private static string BuildDisplayName(
            ApplicationUser user)
        {
            var fullName = string.Join(
                " ",
                new[]
                {
                    user.FirstName,
                    user.SecondName,
                    user.LastName
                }
                .Where(name =>
                    !string.IsNullOrWhiteSpace(name)));


            return string.IsNullOrWhiteSpace(fullName)
                ? user.Email ?? "Unknown User"
                : fullName;
        }


        private static string? Normalize(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}