using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockManager.Web.Data;
using StockManager.Web.Models;

namespace StockManager.Web.Controllers;

[Authorize(Roles = "Admin,Manager,Employee")]
public class StockMovementsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StockMovementsController> _logger;

    public StockMovementsController(
        ApplicationDbContext context,
        ILogger<StockMovementsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var movements = await _context.StockMovements
            .Include(m => m.Product)
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync();

        _logger.LogInformation(
            "Liste mouvements de stock consultée | Résultats={MovementCount} | Utilisateur={UserName}",
            movements.Count,
            User.Identity?.Name);

        return View(movements);
    }

    public async Task<IActionResult> CreateEntry()
    {
        _logger.LogDebug("Ouverture formulaire entrée stock | Utilisateur={UserName}", User.Identity?.Name);
        await LoadProductsAsync();
        return View(new StockMovement { Type = StockMovementType.Entry });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEntry(StockMovement movement)
    {
        movement.Type = StockMovementType.Entry;

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Entrée stock invalide | ProductId={ProductId} | Quantité={Quantity} | Utilisateur={UserName}",
                movement.ProductId,
                movement.Quantity,
                User.Identity?.Name);

            await LoadProductsAsync();
            return View(movement);
        }

        var product = await _context.Products.FindAsync(movement.ProductId);

        if (product == null)
        {
            _logger.LogWarning(
                "Entrée stock impossible : produit introuvable | ProductId={ProductId} | Quantité={Quantity} | Utilisateur={UserName}",
                movement.ProductId,
                movement.Quantity,
                User.Identity?.Name);
            return NotFound();
        }

        var previousQuantity = product.Quantity;
        product.Quantity += movement.Quantity;
        movement.MovementDate = DateTime.Now;

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Entrée stock créée | MovementId={MovementId} | ProductId={ProductId} | Référence={Reference} | QuantitéAjoutée={MovementQuantity} | AncienneQuantité={PreviousQuantity} | NouvelleQuantité={NewQuantity} | Motif={Reason} | CrééePar={UserName}",
            movement.Id,
            product.Id,
            product.Reference,
            movement.Quantity,
            previousQuantity,
            product.Quantity,
            movement.Reason,
            User.Identity?.Name);

        TempData["SuccessMessage"] = "Entrée de stock enregistrée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> CreateExit()
    {
        _logger.LogDebug("Ouverture formulaire sortie stock | Utilisateur={UserName}", User.Identity?.Name);
        await LoadProductsAsync();
        return View(new StockMovement { Type = StockMovementType.Exit });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExit(StockMovement movement)
    {
        movement.Type = StockMovementType.Exit;

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Sortie stock invalide | ProductId={ProductId} | Quantité={Quantity} | Utilisateur={UserName}",
                movement.ProductId,
                movement.Quantity,
                User.Identity?.Name);

            await LoadProductsAsync();
            return View(movement);
        }

        var product = await _context.Products.FindAsync(movement.ProductId);

        if (product == null)
        {
            _logger.LogWarning(
                "Sortie stock impossible : produit introuvable | ProductId={ProductId} | Quantité={Quantity} | Utilisateur={UserName}",
                movement.ProductId,
                movement.Quantity,
                User.Identity?.Name);
            return NotFound();
        }

        if (product.Quantity < movement.Quantity)
        {
            _logger.LogWarning(
                "Sortie stock refusée : stock insuffisant | ProductId={ProductId} | Référence={Reference} | QuantitéDemandée={RequestedQuantity} | StockDisponible={AvailableQuantity} | Utilisateur={UserName}",
                product.Id,
                product.Reference,
                movement.Quantity,
                product.Quantity,
                User.Identity?.Name);

            ModelState.AddModelError("Quantity", "Stock insuffisant pour effectuer cette sortie.");
            await LoadProductsAsync();
            return View(movement);
        }

        var previousQuantity = product.Quantity;
        product.Quantity -= movement.Quantity;
        movement.MovementDate = DateTime.Now;

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Sortie stock créée | MovementId={MovementId} | ProductId={ProductId} | Référence={Reference} | QuantitéRetirée={MovementQuantity} | AncienneQuantité={PreviousQuantity} | NouvelleQuantité={NewQuantity} | Motif={Reason} | CrééePar={UserName}",
            movement.Id,
            product.Id,
            product.Reference,
            movement.Quantity,
            previousQuantity,
            product.Quantity,
            movement.Reason,
            User.Identity?.Name);

        TempData["SuccessMessage"] = "Sortie de stock enregistrée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadProductsAsync()
    {
        ViewBag.Products = new SelectList(
            await _context.Products.OrderBy(p => p.Name).ToListAsync(),
            "Id",
            "Name");
    }
}
