# Compte rendu technique — Environnements, migrations et déploiement SQL Express/IIS

Projet : **Stock Manager ASP.NET Core MVC**  
Contexte : bascule vers **SQL Server Express** pour préparer le déploiement local IIS, avec séparation des bases **Development**, **Recette** et **Production**.

---

## 1. État actuel observé

Dans SSMS, le serveur actif pour le déploiement IIS est :

```txt
localhost\SQLEXPRESS
```

Les bases visibles sont :

```txt
StockManagerDb_Prod
StockManagerDb_Recette
```

La base `StockManagerDb_Prod` contient déjà les tables principales générées par Entity Framework :

```txt
dbo.__EFMigrationsHistory
dbo.AspNetRoleClaims
dbo.AspNetRoles
dbo.AspNetUserClaims
dbo.AspNetUserLogins
dbo.AspNetUserRoles
dbo.AspNetUsers
dbo.AspNetUserTokens
dbo.Categories
dbo.Products
dbo.StockMovements
dbo.Suppliers
```

La base `StockManagerDb_Recette` existe, mais ses tables doivent encore être créées avec les migrations EF si elle est vide.

L'ancien serveur :

```txt
(localdb)\MSSQLLocalDB
```

peut rester connecté dans SSMS, mais il ne doit plus être utilisé comme référence principale pour IIS afin d'éviter la confusion.

---

## 2. Pourquoi séparer Development, Recette et Production ?

### Development

Environnement utilisé pour coder, corriger, tester rapidement, casser/recréer des données si nécessaire.

Base conseillée :

```txt
localhost\SQLEXPRESS / StockManagerDb_Dev
```

Utilisation :

```txt
- nouvelles fonctionnalités
- corrections de bugs
- tests techniques
- données nombreuses et fictives
- migrations en premier
```

### Recette

Environnement de validation avant production.

Base conseillée :

```txt
localhost\SQLEXPRESS / StockManagerDb_Recette
```

Utilisation :

```txt
- tester comme un utilisateur final
- valider les rôles Admin / Manager / Visitor
- valider les écrans Products / Categories / Suppliers / StockMovements
- vérifier que les migrations passent avant la production
```

### Production

Environnement réellement utilisé par IIS.

Base actuelle :

```txt
localhost\SQLEXPRESS / StockManagerDb_Prod
```

Utilisation :

```txt
- application publiée sur IIS
- données propres
- accès via utilisateur SQL dédié
- pas de données de test massives
```

---

## 3. Règle importante à retenir

Une migration Entity Framework sert à faire évoluer la structure de la base : tables, colonnes, index, relations.

Un environnement sert à choisir une configuration : base de données, logs, sécurité, mode d'exécution.

Donc :

```txt
Correct : appsettings.Development.json choisit StockManagerDb_Dev
Correct : appsettings.Recette.json choisit StockManagerDb_Recette
Correct : appsettings.Production.json choisit StockManagerDb_Prod
Incorrect : créer une migration appelée AddBddDevProdRecette pour gérer les environnements
```

---

## 4. Fichiers appsettings recommandés

### appsettings.json

Fichier commun. Il peut rester simple :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=StockManagerDb_Dev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### appsettings.Recette.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=StockManagerDb_Recette;User Id=StockManagerUser;Password=__A_REMPLACER_EN_LOCAL__;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=StockManagerDb_Prod;User Id=StockManagerUser;Password=__A_REMPLACER_EN_LOCAL__;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Ne pas publier le vrai mot de passe SQL sur GitHub.

---

## 5. Program.cs attendu

Dans `Program.cs`, l'application doit lire la chaîne de connexion par son nom logique :

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

Avec cette structure, ASP.NET Core choisit automatiquement le bon fichier `appsettings.{Environment}.json` selon la valeur de :

```txt
ASPNETCORE_ENVIRONMENT
```

---

## 6. launchSettings.json ajusté

Le fichier `launchSettings.json` fourni ne contenait que des profils en `Development`.

Version recommandée :

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:64748/",
      "sslPort": 44317
    }
  },
  "profiles": {
    "StockManager.Web - Development": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7163;http://localhost:5019",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "StockManager.Web - Recette": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7164;http://localhost:5020",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Recette"
      }
    },
    "StockManager.Web - Production Local Test": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7165;http://localhost:5021",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      }
    },
    "IIS Express - Development": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

Attention : `launchSettings.json` sert surtout au lancement local avec Visual Studio. Pour un vrai site IIS publié, il faut configurer l'environnement dans IIS ou dans `web.config`.

---

## 7. Correction de la migration AddBddDevProdRecette

La migration :

```txt
20260602134815_AddBddDevProdRecette
```

essayait de recréer les tables `Categories`, `Products`, `Suppliers`, `StockMovements`, alors que ces tables avaient déjà été créées par la migration précédente `AddStockBusinessTables`.

Correction recommandée : vider `Up()` et `Down()`.

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManager.Web.Migrations.StockManagerDb
{
    public partial class AddBddDevProdRecette : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migration volontairement vide.
            // Les environnements Dev / Recette / Prod sont gérés par appsettings,
            // pas par une migration EF.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rien à annuler.
        }
    }
}
```

Ne pas modifier manuellement le fichier `.Designer.cs` sauf cas exceptionnel. Le fichier important pour empêcher la recréation des tables est le fichier `.cs` principal de la migration.

---

## 8. Commandes SQL utiles

Créer la base Dev si elle n'existe pas encore :

```sql
IF DB_ID(N'StockManagerDb_Dev') IS NULL
BEGIN
    CREATE DATABASE StockManagerDb_Dev;
END;
GO
```

Créer la base Recette si nécessaire :

```sql
IF DB_ID(N'StockManagerDb_Recette') IS NULL
BEGIN
    CREATE DATABASE StockManagerDb_Recette;
END;
GO
```

Vérifier les tables d'une base :

```sql
USE StockManagerDb_Prod;
GO

SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO
```

Vérifier l'historique EF :

```sql
USE StockManagerDb_Prod;
GO

SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
ORDER BY MigrationId;
GO
```

---

## 9. Commandes EF Core recommandées

### Appliquer les migrations sur Development

```powershell
Update-Database -Context ApplicationDbContext -Connection "Server=localhost\SQLEXPRESS;Database=StockManagerDb_Dev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" -Verbose
```

### Appliquer les migrations sur Recette

```powershell
Update-Database -Context ApplicationDbContext -Connection "Server=localhost\SQLEXPRESS;Database=StockManagerDb_Recette;User Id=StockManagerUser;Password=__A_REMPLACER_EN_LOCAL__;TrustServerCertificate=True;MultipleActiveResultSets=true" -Verbose
```

### Appliquer les migrations sur Production

```powershell
Update-Database -Context ApplicationDbContext -Connection "Server=localhost\SQLEXPRESS;Database=StockManagerDb_Prod;User Id=StockManagerUser;Password=__A_REMPLACER_EN_LOCAL__;TrustServerCertificate=True;MultipleActiveResultSets=true" -Verbose
```

Ordre professionnel :

```txt
1. Development
2. Recette
3. Production
```

---

## 10. Basculer d'un environnement à l'autre

### Dans Visual Studio

Choisir le profil :

```txt
StockManager.Web - Development
StockManager.Web - Recette
StockManager.Web - Production Local Test
```

### Dans PowerShell

Development :

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project StockManager.Web
```

Recette :

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Recette"
dotnet run --project StockManager.Web
```

Production local test :

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --project StockManager.Web
```

### Dans IIS

Pour le site publié, l'environnement doit être :

```txt
Production
```

Exemple dans `web.config` publié :

```xml
<aspNetCore processPath="dotnet" arguments=".\StockManager.Web.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  </environmentVariables>
</aspNetCore>
```

---

## 11. Routine de maintenance professionnelle

Pour continuer les corrections de l'application :

```txt
1. Travailler en Development
2. Corriger le code
3. Tester avec StockManagerDb_Dev
4. Créer une migration seulement si les Models changent
5. Appliquer la migration sur Development
6. Tester l'application
7. Appliquer la même migration sur Recette
8. Valider comme un utilisateur final
9. Sauvegarder la base Production
10. Appliquer la migration sur Production
11. Publier ou mettre à jour IIS
```

Ne jamais tester directement une correction risquée en production.

---

## 12. Principe SOLID appliqué au projet Stock Manager

### S — Single Responsibility Principle

Chaque classe doit avoir une seule responsabilité.

Exemple :

```txt
ProductsController -> reçoit les actions HTTP
ProductService -> logique métier produit
StockMovementService -> logique entrée/sortie stock
ApplicationDbContext -> accès SQL Server
Views -> affichage Razor
```

### O — Open/Closed Principle

Le code doit être ouvert à l'extension, mais fermé à la modification dangereuse.

Exemple : ajouter plus tard un export PDF sans casser l'export CSV existant.

### L — Liskov Substitution Principle

Une classe concrète doit pouvoir remplacer son interface sans casser le comportement.

Exemple : `IProductService` peut être implémenté par `ProductService`, puis plus tard par `CachedProductService`.

### I — Interface Segregation Principle

Préférer plusieurs petites interfaces plutôt qu'une énorme interface générale.

Exemple :

```txt
IProductService
ICategoryService
ISupplierService
IStockMovementService
IExportService
```

### D — Dependency Inversion Principle

Le contrôleur dépend d'une abstraction, pas directement d'une classe technique.

Exemple :

```csharp
public class ProductsController : Controller
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
}
```

Puis dans `Program.cs` :

```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```

---

## 13. Règles pour éviter la confusion à l'avenir

```txt
- Ne plus utiliser LocalDB comme référence principale pour IIS.
- Utiliser SQL Express pour Dev, Recette et Prod.
- Ne pas créer les tables manuellement si EF Core gère les migrations.
- Ne pas créer une migration pour gérer les environnements.
- Nommer les migrations selon la modification métier : AddProductStatus, AddSupplierAddress, AddStockAlertLevel.
- Appliquer les migrations dans l'ordre : Dev -> Recette -> Prod.
- Ne pas mettre le vrai mot de passe SQL dans GitHub.
- Sauvegarder Production avant chaque migration importante.
```

---

## 14. Prochaine action recommandée

1. Remplacer `20260602134815_AddBddDevProdRecette.cs` par la version corrigée.
2. Garder le fichier `.Designer.cs` tel quel.
3. Remplacer `launchSettings.json` par la version ajustée.
4. Créer `appsettings.Development.json`, `appsettings.Recette.json`, `appsettings.Production.json` avec les chaînes adaptées.
5. Relancer la migration sur `StockManagerDb_Prod`.
6. Créer et migrer `StockManagerDb_Dev` pour continuer les corrections sans toucher à la production.
