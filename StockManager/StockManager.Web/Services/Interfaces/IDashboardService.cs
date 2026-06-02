using StockManager.Web.Services.ViewModels;

namespace StockManager.Web.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}