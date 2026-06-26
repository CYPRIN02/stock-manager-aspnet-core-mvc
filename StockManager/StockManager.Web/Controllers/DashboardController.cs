using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockManager.Web.Services.Interfaces;

namespace StockManager.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogDebug("Chargement du tableau de bord | Utilisateur={UserName}", User.Identity?.Name);

        var dashboard = await _dashboardService.GetDashboardAsync();

        _logger.LogInformation(
            "Tableau de bord consulté | TotalProduits={TotalProducts} | ProduitsStockFaible={LowStockProductsCount} | ValeurStock={TotalStockValue} | Utilisateur={UserName}",
            dashboard.TotalProducts,
            dashboard.LowStockProductsCount,
            dashboard.TotalStockValue,
            User.Identity?.Name);

        return View(dashboard);
    }
}
