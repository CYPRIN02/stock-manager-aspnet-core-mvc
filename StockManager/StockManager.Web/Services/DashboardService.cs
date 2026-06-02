using StockManager.Web.Repositories.Interfaces;
using StockManager.Web.Services.Interfaces;
using StockManager.Web.Services.ViewModels;

namespace StockManager.Web.Services;

public class DashboardService : IDashboardService
{
    private readonly IProductRepository _productRepository;
    private readonly IStockMovementRepository _stockMovementRepository;

    public DashboardService(
        IProductRepository productRepository,
        IStockMovementRepository stockMovementRepository)
    {
        _productRepository = productRepository;
        _stockMovementRepository = stockMovementRepository;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        return new DashboardViewModel
        {
            TotalProducts = await _productRepository.CountAsync(),
            TotalStockValue = await _productRepository.GetTotalStockValueAsync(),
            LowStockProductsCount = await _productRepository.CountLowStockAsync(),
            LowStockProducts = await _productRepository.GetLowStockProductsAsync(),
            LatestMovements = await _stockMovementRepository.GetLatestMovementsAsync(5)
        };
    }
}