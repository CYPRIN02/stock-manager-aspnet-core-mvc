using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace StockManager.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AdminController> _logger;
    public AdminController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }


    public async Task<IActionResult> Index()
    {
        _logger.LogDebug("Chargement de la page d'administration des utilisateurs par {UserName}.", User.Identity?.Name);

        var users = _userManager.Users.ToList();
        var model = new List<UserRoleViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            model.Add(new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "Aucun rôle"
            });
        }

        _logger.LogInformation(
            "Page administration chargée | NombreUtilisateurs={UserCount} | DemandéPar={UserName}",
            model.Count,
            User.Identity?.Name);
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> ChangeRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning(
                "Changement de rôle impossible : utilisateur introuvable | UserId={UserId} | DemandéPar={UserName}",
                userId,
                User.Identity?.Name); 
            return NotFound();
        }

        var roleExists = string.IsNullOrWhiteSpace(role) || await _roleManager.RoleExistsAsync(role);
        if (!roleExists)
        {
            _logger.LogWarning(
                "Changement de rôle impossible : rôle introuvable | UserId={UserId} | Role={Role} | DemandéPar={UserName}",
                userId,
                role,
                User.Identity?.Name);
            return BadRequest("Rôle introuvable.");
        }
        
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!string.IsNullOrWhiteSpace(role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        _logger.LogInformation(
            "Rôle utilisateur modifié | UserId={UserId} | Email={Email} | AncienRole={OldRoles} | NouveauRole={NewRole} | ModifiéPar={UserName}",
            user.Id,
            user.Email,
            string.Join(",", currentRoles),
            string.IsNullOrWhiteSpace(role) ? "Aucun rôle" : role,
            User.Identity?.Name);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning(
                "Suppression utilisateur impossible : utilisateur introuvable | UserId={UserId} | DemandéPar={UserName}",
                userId,
                User.Identity?.Name);
            return NotFound();
        }

        var deletedEmail = user.Email;
        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            _logger.LogError(
                "Suppression utilisateur échouée | UserId={UserId} | Email={Email} | Erreurs={Errors} | DemandéPar={UserName}",
                userId,
                deletedEmail,
                string.Join(", ", result.Errors.Select(e => e.Description)),
                User.Identity?.Name);

            TempData["ErrorMessage"] = "Impossible de supprimer cet utilisateur.";
            return RedirectToAction(nameof(Index));
        }

        _logger.LogInformation(
            "Utilisateur supprimé | UserId={UserId} | Email={Email} | SuppriméPar={UserName}",
            userId,
            deletedEmail,
            User.Identity?.Name);

        return RedirectToAction(nameof(Index));
    }
}

public class UserRoleViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
}