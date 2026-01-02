using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedicalOfficeManagement.Data.Seeders;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration de la base de données
var connectionString = builder.Configuration.GetConnectionString("gestionCabinetContextConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<MedicalOfficeContext>(options =>
    options.UseSqlServer(connectionString));


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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    await RoleSeeder.SeedAsync(roleManager);
    await UserSeeder.SeedAsync(userManager, roleManager);
}


app.Run();
