using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Identity.UI;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la connexion à la base de données
var connectionString = builder.Configuration.GetConnectionString("gestionCabinetContextConnection")
    ?? throw new InvalidOperationException("Connection string 'gestionCabinetContextConnection' not found.");

builder.Services.AddDbContext<MedicalOfficeContext>(options =>
    options.UseSqlServer(connectionString));

// Configuration Identity par défaut
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Options de connexion
    options.SignIn.RequireConfirmedAccount = true;

    // Options de mot de passe (Relaxation des règles)
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

//  FILTRE D'AUTORISATION GLOBAL (COMMENTÉ POUR TESTER) 
builder.Services.AddControllersWithViews(options =>
{
    // Le bloc suivant est temporairement commenté pour éviter la boucle de redirection
    // var policy = new AuthorizationPolicyBuilder() 
    //     .RequireAuthenticatedUser() 
    //     .Build(); 
    // options.Filters.Add(new AuthorizeFilter(policy)); 
});
// ----------------------------------------------------------------------------------

builder.Services.AddRazorPages();


var app = builder.Build();

// Configuration du pipeline HTTP
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

// Le middleware d'authentification DOIT précéder UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

// Mappage de l'endpoint pour les pages Razor d'Identity
app.MapRazorPages();

// Définition de la route par défaut.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// --- Initialiseur de Base de Données (Seeding) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedRolesAndAdminUser(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// -------------------------------------------------

app.Run();