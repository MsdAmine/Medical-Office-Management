using MedicalOfficeManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MedicalOfficeManagement
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // AJOUT: Nécessaire pour l'interface utilisateur Razor Pages d'Identity
            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<MedicalOfficeContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("gestionCabinetContextConnection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Options de connexion
                options.SignIn.RequireConfirmedAccount = true;

                // Options de mot de passe (Relaxation des règles que nous avions ajoutées)
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 4;
            })
            .AddEntityFrameworkStores<MedicalOfficeContext>()
            .AddDefaultTokenProviders();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapGet("/Identity/Account/Register", context => Task.FromCanceled(context.RequestAborted));

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // AJOUT: Mappage de l'endpoint pour les pages Razor d'Identity
            app.MapRazorPages();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Appelle la méthode d'initialisation pour créer les rôles et l'Admin
                    await DbInitializer.SeedRolesAndAdminUser(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            app.Run();
        }
    }
}