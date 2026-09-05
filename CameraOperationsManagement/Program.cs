using CameraOperationsManagement.Data;
using CameraOperationsManagement.Models;
using CameraOperationsManagement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "CanManageUsers",
        policy =>
            policy.RequireRole(
                AppRoles.Admin));


    options.AddPolicy(
        "CanManageInfrastructure",
        policy =>
            policy.RequireRole(
                AppRoles.Admin,
                AppRoles.InfrastructureManager));


    options.AddPolicy(
        "CanViewInfrastructure",
        policy =>
            policy.RequireRole(
                AppRoles.Admin,
                AppRoles.InfrastructureManager,
                AppRoles.Editor,
                AppRoles.InfrastructureViewer));


    options.AddPolicy(
        "CanViewSites",
        policy =>
            policy.RequireRole(
                AppRoles.Admin,
                AppRoles.InfrastructureManager,
                AppRoles.Editor,
                AppRoles.InfrastructureViewer,
                AppRoles.Viewer));


    options.AddPolicy(
        "CanManageWorkers",
        policy =>
            policy.RequireRole(
                AppRoles.Admin,
                AppRoles.InfrastructureManager));


    options.AddPolicy(
        "CanViewWorkers",
        policy =>
            policy.RequireRole(
                AppRoles.Admin,
                AppRoles.InfrastructureManager,
                AppRoles.Editor));


    options.AddPolicy(
        "CanManageVisits",
        policy =>
            policy.RequireRole(
                AppRoles.Admin,
                AppRoles.Editor));


    options.AddPolicy(
        "CanChangeStatus",
        policy =>
            policy.RequireRole(
                AppRoles.Admin));
    options.AddPolicy(
    "CanViewVisits",
    policy =>
        policy.RequireRole(
            AppRoles.Admin,
            AppRoles.InfrastructureManager,
            AppRoles.Editor,
            AppRoles.InfrastructureViewer));


    options.AddPolicy(
        "CanViewVisitReport",
        policy =>
            policy.RequireRole(
                AppRoles.Admin,
                AppRoles.InfrastructureManager,
                AppRoles.Editor,
                AppRoles.InfrastructureViewer,
                AppRoles.Viewer));
});
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "DisablePublicRegistration",
        policy =>
            policy.RequireAssertion(_ => false));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaPage(
        "Identity",
        "/Account/Register",
        "DisablePublicRegistration");
});
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    // Automatically apply pending EF Core migrations
    await dbContext.Database.MigrateAsync();

    // Seed roles, users, and other initial data
    await DbInitializer.SeedAsync(
        services,
        app.Configuration);
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
