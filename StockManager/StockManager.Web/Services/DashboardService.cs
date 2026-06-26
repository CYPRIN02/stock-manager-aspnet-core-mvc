using StockManager.Web.Repositories.Interfaces;
using StockManager.Web.Services.Interfaces;
using StockManager.Web.Services.ViewModels;

namespace StockManager.Web.Services;

public class DashboardService : IDashboardService
{
    private readonly IProductRepository _productRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IProductRepository productRepository,
        IStockMovementRepository stockMovementRepository,
        ILogger<DashboardService> logger)
    {
        _productRepository = productRepository;
        _stockMovementRepository = stockMovementRepository;
        _logger = logger;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        _logger.LogDebug("Calcul des indicateurs du tableau de bord démarré.");

        var dashboard = new DashboardViewModel
        {
            TotalProducts = await _productRepository.CountAsync(),
            TotalStockValue = await _productRepository.GetTotalStockValueAsync(),
            LowStockProductsCount = await _productRepository.CountLowStockAsync(),
            LowStockProducts = await _productRepository.GetLowStockProductsAsync(),
            LatestMovements = await _stockMovementRepository.GetLatestMovementsAsync(5)
        };

        if (dashboard.LowStockProductsCount > 0)
        {
            _logger.LogWarning(
                "Alerte stock faible détectée | ProduitsStockFaible={LowStockProductsCount}",
                dashboard.LowStockProductsCount);
        }

        _logger.LogInformation(
            "Indicateurs tableau de bord calculés | TotalProduits={TotalProducts} | ValeurStock={TotalStockValue} | ProduitsStockFaible={LowStockProductsCount} | DerniersMouvements={LatestMovementsCount}",
            dashboard.TotalProducts,
            dashboard.TotalStockValue,
            dashboard.LowStockProductsCount,
            dashboard.LatestMovements.Count);

        return dashboard;
    }
}
