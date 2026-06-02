using StockManager.Web.Models;

namespace StockManager.Web.Services.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }

    public decimal TotalStockValue { get; set; }

    public int LowStockProductsCount { get; set; }

    public List<Product> LowStockProducts { get; set; } = new();

    public List<StockMovement> LatestMovements { get; set; } = new();
}