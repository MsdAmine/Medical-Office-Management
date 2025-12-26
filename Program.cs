using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration de la base de données
var connectionString = builder.Configuration.GetConnectionString("gestionCabinetContextConnection")
    ?? throw new InvalidOperationException("Connection string 'gestionCabinetContextConnection' not found.");

builder.Services.AddDbContext<MedicalOfficeContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configuration Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; 
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
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

// 3. Pipeline HTTP
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
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/", context => {
    if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
    {
        context.Response.Redirect("/Identity/Account/Login");
    }
    else
    {
        context.Response.Redirect("/Home/Index"); 
    }
    return Task.CompletedTask;
});

// --- APPEL AU DBINITIALIZER ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        // On appelle votre méthode statique
        await DbInitializer.SeedRolesAndAdminUser(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erreur lors de l'initialisation de la base de données.");
    }
}
// ------------------------------

app.Run();