using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockManager.Web.Models;

namespace StockManager.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogDebug("Page d'accueil consultée | TraceId={TraceId}", HttpContext.TraceIdentifier);
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.LogDebug("Page confidentialité consultée | TraceId={TraceId}", HttpContext.TraceIdentifier);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            _logger.LogError(
                "Page erreur affichée | RequestId={RequestId} | TraceId={TraceId}",
                requestId,
                HttpContext.TraceIdentifier);

            return View(new ErrorViewModel { RequestId = requestId });
        }
    }
}
