using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedicalOfficeManagement.Data.Seeders;
using MedicalOfficeManagement.Models.Email;
using MedicalOfficeManagement.Services.Email;


var builder = WebApplication.CreateBuilder(args);

// 1. Configuration de la base de données
var dataProvider = builder.Configuration["DataProvider"] ?? "SqlServer";
var connectionString = dataProvider == "Sqlite"
    ? builder.Configuration.GetConnectionString("MedicalOfficeDb")
        ?? throw new InvalidOperationException("SQLite connection string not found.")
    : builder.Configuration.GetConnectionString("gestionCabinetContextConnection")
        ?? throw new InvalidOperationException("SQL Server connection string not found.");

builder.Services.AddDbContext<MedicalOfficeContext>(options =>
{
    if (dataProvider == "Sqlite")
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});




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
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IDashboardMetricsService, DashboardMetricsService>();

// Configure SMTP settings
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<AppointmentEmailService>();


var app = builder.Build();

// Initialize database asynchronously
await InitializeDatabaseAsync(app);

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MedicalOfficeContext>();
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

    if (pendingMigrations.Any())
    {
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex) when (app.Environment.IsDevelopment() && (ex.Message.Contains("already exists") || ex.Message.Contains("table") && ex.Message.Contains("exists")))
        {
            // In development, drop and recreate database if there's a migration conflict
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    if (app.Environment.IsDevelopment())
    {
        await DatabaseSeeder.SeedAsync(services, context);
    }
}

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
        var returnPath = context.Request.Path.HasValue && context.Request.Path != "/" ? context.Request.Path.Value : "/Home/Index";
        var returnUrl = string.Concat(returnPath, context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty);
        context.Response.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl!)}");
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

app.Run();
