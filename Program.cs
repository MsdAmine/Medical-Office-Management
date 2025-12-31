using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedicalOfficeManagement.Data;
using MedicalOfficeManagement.Data.Repositories;
using MedicalOfficeManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration de la base de données
var connectionString = builder.Configuration.GetConnectionString("gestionCabinetContextConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<MedicalOfficeContext>(options =>
    options.UseSqlServer(connectionString));

// Read-only operational data context
var operationalProvider = builder.Configuration.GetValue<string>("DataProvider") ?? "Sqlite";
var operationalConnection = builder.Configuration.GetConnectionString("MedicalOfficeDb") ?? "Data Source=medicaloffice.db";
builder.Services.AddDbContext<MedicalOfficeDbContext>(options =>
{
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    if (string.Equals(operationalProvider, "InMemory", StringComparison.OrdinalIgnoreCase))
    {
        options.UseInMemoryDatabase("MedicalOfficeDb");
    }
    else
    {
        options.UseSqlite(operationalConnection);
    }
});

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();

// 2. Configuration Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<MedicalOfficeContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// --- LOGIQUE DE DÉMARRAGE ---
app.MapGet("/", context =>
{
    if (!context.User.Identity?.IsAuthenticated ?? true)
    {
        context.Response.Redirect("/Identity/Account/Login");
    }
    else
    {
        context.Response.Redirect("/Home/Index");
    }
    return Task.CompletedTask;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try { await DbInitializer.SeedRolesAndAdminUser(services); }
    catch (Exception ex) { /* Log error */ }

    try
    {
        var dataContext = services.GetRequiredService<MedicalOfficeDbContext>();
        await SeedData.EnsureSeedDataAsync(dataContext);
    }
    catch (Exception ex)
    {
        /* Log error */
    }
}

app.Run();
