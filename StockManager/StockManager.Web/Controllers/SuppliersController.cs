using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManager.Web.Data;
using StockManager.Web.Models;

namespace StockManager.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(
        ApplicationDbContext context,
        ILogger<SuppliersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? search, string sortOrder = "name_asc", int pageSize = 25)
    {
        var query = _context.Suppliers
            .Include(s => s.Products)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.Name.Contains(search) ||
                (s.Email != null && s.Email.Contains(search)) ||
                (s.Phone != null && s.Phone.Contains(search)));
        }

        query = sortOrder switch
        {
            "name_desc" => query.OrderByDescending(s => s.Name),
            "products_asc" => query.OrderBy(s => s.Products.Count),
            "products_desc" => query.OrderByDescending(s => s.Products.Count),
            _ => query.OrderBy(s => s.Name)
        };

        pageSize = pageSize == 50 ? 50 : 25;

        ViewBag.Search = search;
        ViewBag.SortOrder = sortOrder;
        ViewBag.PageSize = pageSize;

        var suppliers = await query.Take(pageSize).ToListAsync();

        _logger.LogInformation(
            "Liste fournisseurs consultée | Recherche={Search} | Tri={SortOrder} | PageSize={PageSize} | Résultats={ResultCount} | Utilisateur={UserName}",
            search,
            sortOrder,
            pageSize,
            suppliers.Count,
            User.Identity?.Name);

        return View(suppliers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
        {
            _logger.LogWarning(
                "Fournisseur introuvable lors de la consultation détail | SupplierId={SupplierId} | Utilisateur={UserName}",
                id,
                User.Identity?.Name);
            return NotFound();
        }

        _logger.LogInformation(
            "Détail fournisseur consulté | SupplierId={SupplierId} | Nom={SupplierName} | Produits={ProductCount} | Utilisateur={UserName}",
            supplier.Id,
            supplier.Name,
            supplier.Products.Count,
            User.Identity?.Name);

        return View(supplier);
    }

    public IActionResult Create()
    {
        _logger.LogDebug("Ouverture formulaire création fournisseur | Utilisateur={UserName}", User.Identity?.Name);
        return View(new Supplier());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        var exists = await _context.Suppliers
            .AnyAsync(s => s.Name == supplier.Name);

        if (exists)
        {
            _logger.LogWarning(
                "Création fournisseur refusée : doublon | Nom={SupplierName} | Utilisateur={UserName}",
                supplier.Name,
                User.Identity?.Name);

            ModelState.AddModelError("Name", "Ce fournisseur existe déjà.");
            TempData["ErrorMessage"] = "Impossible d'ajouter : fournisseur déjà existant.";
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Création fournisseur invalide | Nom={SupplierName} | Utilisateur={UserName}",
                supplier.Name,
                User.Identity?.Name);
            return View(supplier);
        }

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Fournisseur créé | SupplierId={SupplierId} | Nom={SupplierName} | CrééPar={UserName}",
            supplier.Id,
            supplier.Name,
            User.Identity?.Name);

        TempData["SuccessMessage"] = "Fournisseur ajouté avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier == null)
        {
            _logger.LogWarning(
                "Fournisseur introuvable lors de l'ouverture modification | SupplierId={SupplierId} | Utilisateur={UserName}",
                id,
                User.Identity?.Name);
            return NotFound();
        }

        _logger.LogDebug(
            "Ouverture formulaire modification fournisseur | SupplierId={SupplierId} | Utilisateur={UserName}",
            id,
            User.Identity?.Name);

        return View(supplier);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supplier supplier)
    {
        if (id != supplier.Id)
        {
            _logger.LogWarning(
                "Modification fournisseur refusée : identifiant incohérent | RouteId={RouteId} | ModelId={ModelId} | Utilisateur={UserName}",
                id,
                supplier.Id,
                User.Identity?.Name);
            return BadRequest();
        }

        var exists = await _context.Suppliers
            .AnyAsync(s => s.Name == supplier.Name && s.Id != supplier.Id);

        if (exists)
        {
            _logger.LogWarning(
                "Modification fournisseur refusée : doublon | SupplierId={SupplierId} | Nom={SupplierName} | Utilisateur={UserName}",
                supplier.Id,
                supplier.Name,
                User.Identity?.Name);

            ModelState.AddModelError("Name", "Ce nom de fournisseur est déjà utilisé.");
            TempData["ErrorMessage"] = "Modification impossible : fournisseur déjà existant.";
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Modification fournisseur invalide | SupplierId={SupplierId} | Nom={SupplierName} | Utilisateur={UserName}",
                supplier.Id,
                supplier.Name,
                User.Identity?.Name);
            return View(supplier);
        }

        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Fournisseur modifié | SupplierId={SupplierId} | Nom={SupplierName} | ModifiéPar={UserName}",
            supplier.Id,
            supplier.Name,
            User.Identity?.Name);

        TempData["SuccessMessage"] = "Fournisseur modifié avec succès.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
        {
            _logger.LogWarning(
                "Fournisseur introuvable lors de l'ouverture suppression | SupplierId={SupplierId} | Utilisateur={UserName}",
                id,
                User.Identity?.Name);
            return NotFound();
        }

        _logger.LogDebug(
            "Ouverture confirmation suppression fournisseur | SupplierId={SupplierId} | ProduitsLiés={ProductCount} | Utilisateur={UserName}",
            supplier.Id,
            supplier.Products.Count,
            User.Identity?.Name);

        return View(supplier);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
        {
            _logger.LogWarning(
                "Suppression fournisseur impossible : fournisseur introuvable | SupplierId={SupplierId} | Utilisateur={UserName}",
                id,
                User.Identity?.Name);
            return NotFound();
        }

        if (supplier.Products.Any())
        {
            _logger.LogWarning(
                "Suppression fournisseur bloquée : produits liés | SupplierId={SupplierId} | ProduitsLiés={ProductCount} | Utilisateur={UserName}",
                supplier.Id,
                supplier.Products.Count,
                User.Identity?.Name);

            TempData["ErrorMessage"] = "Impossible de supprimer ce fournisseur car il est lié à des produits.";
            return RedirectToAction(nameof(Index));
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Fournisseur supprimé | SupplierId={SupplierId} | Nom={SupplierName} | SuppriméPar={UserName}",
            supplier.Id,
            supplier.Name,
            User.Identity?.Name);

        TempData["SuccessMessage"] = "Fournisseur supprimé avec succès.";
        return RedirectToAction(nameof(Index));
    }
}
