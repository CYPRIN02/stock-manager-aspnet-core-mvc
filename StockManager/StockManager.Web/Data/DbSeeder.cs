using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockManager.Web.Models;

namespace StockManager.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("StockManager.Web.Data.DbSeeder");

        logger.LogInformation("Démarrage du seeding de la base de données.");

        await SeedRolesAsync(roleManager, logger);
        await SeedAdminAsync(userManager, logger);

        await CreateUserAsync(
        userManager,
            "manager@amadagoit.com",
            "Manager123!",
            "Manager",
            logger);

        await CreateUserAsync(
            userManager,
            "employee@amadagoit.com",
            "Employee123!",
            "Employee",
            logger);

        await CreateUserAsync(
            userManager,
            "visitor@amadagoit.com",
            "Visitor123!",
            "Visitor",
            logger);

        await SeedDataAsync(context, logger);

        logger.LogInformation("Seeding de la base de données terminé.");
    }

    private static async Task CreateUserAsync(
        UserManager<IdentityUser> userManager,
        string email,
        string password,
        string role,
        ILogger logger)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = email.Split('@')[0],
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                logger.LogInformation("Compte seed créé | Email={Email} | Role={Role}", email, role);
                await AddUserToRoleAsync(userManager, user, role, logger);
            }
            else
            {
                logger.LogError(
                    "Création compte seed échouée | Email={Email} | Erreurs={Errors}",
                    email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else if (!await userManager.IsInRoleAsync(user, role))
        {
            await AddUserToRoleAsync(userManager, user, role, logger);
        }
        else
        {
            logger.LogDebug("Compte seed déjà présent avec le bon rôle | Email={Email} | Role={Role}", email, role);
        }
    }

    private static async Task AddUserToRoleAsync(
        UserManager<IdentityUser> userManager,
        IdentityUser user,
        string role,
        ILogger logger)
    {
        var result = await userManager.AddToRoleAsync(user, role);

        if (result.Succeeded)
        {
            logger.LogInformation("Rôle seed affecté | Email={Email} | Role={Role}", user.Email, role);
        }
        else
        {
            logger.LogError(
                "Affectation rôle seed échouée | Email={Email} | Role={Role} | Erreurs={Errors}",
                user.Email,
                role,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        string[] roles = { "Admin", "Manager", "Employee","Visitor" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));

                if (result.Succeeded)
                {
                    logger.LogInformation("Rôle seed créé | Role={Role}", role);
                }
                else
                {
                    logger.LogError(
                        "Création rôle seed échouée | Role={Role} | Erreurs={Errors}",
                        role,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogDebug("Rôle seed déjà présent | Role={Role}", role);
            }
        }
    }

    private static async Task SeedAdminAsync(UserManager<IdentityUser> userManager, ILogger logger)
    {
        const string adminEmail = "admin@amadagoit.com";
        const string adminPassword = "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail.Split('@')[0],
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (result.Succeeded)
            {
                logger.LogInformation("Compte administrateur seed créé | Email={Email}", adminEmail);
                await AddUserToRoleAsync(userManager, admin, "Admin", logger);
            }
            else
            {
                logger.LogError(
                    "Création administrateur seed échouée | Email={Email} | Erreurs={Errors}",
                    adminEmail,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await AddUserToRoleAsync(userManager, admin, "Admin", logger);
        }
        else
        {
            logger.LogDebug("Compte administrateur seed déjà présent avec le rôle Admin.");
        }
    }

    private static async Task SeedDataAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Categories.AnyAsync() ||
            await context.Suppliers.AnyAsync() ||
            await context.Products.AnyAsync() ||
            await context.StockMovements.AnyAsync())
        {
            logger.LogInformation("Données métier déjà présentes : seeding métier ignoré.");
            return;
        }

        var categories = new List<Category>
        {
            new() { Name = "Informatique" },
            new() { Name = "Bureautique" },
            new() { Name = "Réseau" },
            new() { Name = "Accessoires" },
            new() { Name = "Stockage" }
        };

        await context.Categories.AddRangeAsync(categories);

        var suppliers = new List<Supplier>
        {
            new() { Name = "Dell France", Email = "contact@dell.fr", Phone = "01 44 55 66 77" },
            new() { Name = "HP Store", Email = "support@hp.fr", Phone = "01 22 33 44 55" },
            new() { Name = "Logitech Partner", Email = "sales@logitech.fr", Phone = "01 87 65 43 21" },
            new() { Name = "Cisco Network", Email = "contact@cisco.fr", Phone = "01 98 76 54 32" }
        };

        await context.Suppliers.AddRangeAsync(suppliers);
        await context.SaveChangesAsync();

        logger.LogInformation("Données seed catégories/fournisseurs ajoutées | Categories={CategoryCount} | Fournisseurs={SupplierCount}", categories.Count, suppliers.Count);
        
        var products = new List<Product>
        {
            new()
            {
                Name = "PC Portable Dell Latitude",
                Reference = "PROD-001",
                Quantity = 15,
                UnitPrice = 899.99m,
                AlertQuantity = 5,
                CategoryId = categories[0].Id,
                SupplierId = suppliers[0].Id
            },
            new()
            {
                Name = "Clavier Logitech K120",
                Reference = "PROD-002",
                Quantity = 4,
                UnitPrice = 19.99m,
                AlertQuantity = 10,
                CategoryId = categories[3].Id,
                SupplierId = suppliers[2].Id
            },
            new()
            {
                Name = "Souris Logitech M185",
                Reference = "PROD-003",
                Quantity = 25,
                UnitPrice = 14.99m,
                AlertQuantity = 8,
                CategoryId = categories[3].Id,
                SupplierId = suppliers[2].Id
            },
            new()
            {
                Name = "Écran HP 24 pouces",
                Reference = "PROD-004",
                Quantity = 8,
                UnitPrice = 159.99m,
                AlertQuantity = 5,
                CategoryId = categories[0].Id,
                SupplierId = suppliers[1].Id
            },
            new()
            {
                Name = "Switch Cisco 24 ports",
                Reference = "PROD-005",
                Quantity = 2,
                UnitPrice = 349.99m,
                AlertQuantity = 3,
                CategoryId = categories[2].Id,
                SupplierId = suppliers[3].Id
            },
            new()
            {
                Name = "Disque SSD Samsung 1To",
                Reference = "PROD-006",
                Quantity = 12,
                UnitPrice = 89.99m,
                AlertQuantity = 5,
                CategoryId = categories[4].Id,
                SupplierId = suppliers[0].Id
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        logger.LogInformation("Données seed produits ajoutées | Produits={ProductCount}", products.Count);
        
        var movements = new List<StockMovement>
        {
            new()
            {
                ProductId = products[0].Id,
                Type = StockMovementType.Entry,
                Quantity = 15,
                Reason = "Stock initial",
                MovementDate = DateTime.Now.AddDays(-5)
            },
            new()
            {
                ProductId = products[1].Id,
                Type = StockMovementType.Entry,
                Quantity = 10,
                Reason = "Réapprovisionnement",
                MovementDate = DateTime.Now.AddDays(-4)
            },
            new()
            {
                ProductId = products[1].Id,
                Type = StockMovementType.Exit,
                Quantity = 6,
                Reason = "Sortie matériel interne",
                MovementDate = DateTime.Now.AddDays(-2)
            },
            new()
            {
                ProductId = products[4].Id,
                Type = StockMovementType.Entry,
                Quantity = 2,
                Reason = "Stock initial réseau",
                MovementDate = DateTime.Now.AddDays(-1)
            }
        };

        await context.StockMovements.AddRangeAsync(movements);
        await context.SaveChangesAsync();

        logger.LogInformation("Données seed mouvements de stock ajoutées | Mouvements={MovementCount}", movements.Count);
    }
}