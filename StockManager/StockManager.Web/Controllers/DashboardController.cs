using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockManager.Web.Services.Interfaces;

namespace StockManager.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]

public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        return View(dashboard);
    }
}