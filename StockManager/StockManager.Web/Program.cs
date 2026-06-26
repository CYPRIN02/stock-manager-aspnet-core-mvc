using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using StockManager.Web.Data;
using StockManager.Web.Middleware;
using StockManager.Web.Repositories;
using StockManager.Web.Repositories.Interfaces;
using StockManager.Web.Services;
using StockManager.Web.Services.Interfaces;


using NoOpEmailSender = StockManager.Web.Services.NoOpEmailSender;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();

builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddTransient<IEmailSender, NoOpEmailSender>();

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation(
    "Démarrage de Stock Manager | Environnement actif={EnvironmentName}",
    app.Environment.EnvironmentName);
startupLogger.LogInformation(
    "Base de données configurée | {ConnectionInfo}",
    GetSafeConnectionInfo(builder.Configuration.GetConnectionString("DefaultConnection")));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Initialisation des rôles, utilisateurs et données de démonstration.");
        await DbSeeder.SeedAsync(services);
        logger.LogInformation("Initialisation terminée avec succès.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Erreur critique pendant l'initialisation de la base de données.");
        throw;
    }
}


app.Run();

static string GetSafeConnectionInfo(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return "ConnectionString non configurée";
    }

    try
    {
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        var server = builder.ContainsKey("Server")
            ? builder["Server"]
            : builder.ContainsKey("Data Source")
                ? builder["Data Source"]
                : "Non renseigné";

        var database = builder.ContainsKey("Database")
            ? builder["Database"]
            : builder.ContainsKey("Initial Catalog")
                ? builder["Initial Catalog"]
                : "Non renseignée";

        return $"Server={server}; Database={database}";
    }
    catch
    {
        return "ConnectionString masquée";
    }
}

