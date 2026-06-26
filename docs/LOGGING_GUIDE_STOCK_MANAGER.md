# Compte rendu — Ajout des logs dans Stock Manager

## Objectif

L'objectif de cette évolution est d'ajouter une traçabilité professionnelle dans l'application **Stock Manager** sans surcharger le code et sans exposer d'informations sensibles.

Les logs ajoutés permettent de suivre :

- le démarrage de l'application et l'environnement actif ;
- la base utilisée sans afficher le mot de passe SQL ;
- les requêtes HTTP utiles ;
- les connexions, déconnexions et créations de comptes ;
- les actions d'administration sur les rôles et utilisateurs ;
- les opérations métier sur les produits, catégories, fournisseurs ;
- les entrées et sorties de stock ;
- les alertes de stock faible dans le dashboard ;
- le seeding initial des rôles, utilisateurs et données de démonstration ;
- les erreurs bloquantes et les cas métier refusés.

## Choix technique

L'intégration utilise le système natif ASP.NET Core :

```csharp
ILogger<T>
```

Ce choix est volontaire pour rester simple, robuste et compatible avec ton projet actuel sans ajouter de dépendance externe comme Serilog pour l'instant.

## Fichiers ajoutés

```txt
StockManager.Web/Middleware/RequestLoggingMiddleware.cs
```

Ce middleware trace les requêtes HTTP applicatives avec :

- méthode HTTP ;
- chemin demandé ;
- code statut ;
- durée d'exécution ;
- utilisateur connecté ou anonyme ;
- TraceId.

Les fichiers statiques comme CSS, JS et images sont exclus pour éviter des logs trop bruyants.

## Fichiers modifiés

```txt
StockManager.Web/Program.cs
StockManager.Web/appsettings.json
StockManager.Web/appsettings.Development.json
StockManager.Web/appsettings.Recette.json
StockManager.Web/appsettings.Production.json
StockManager.Web/Data/DbSeeder.cs
StockManager.Web/Controllers/AdminController.cs
StockManager.Web/Controllers/CategoriesController.cs
StockManager.Web/Controllers/DashboardController.cs
StockManager.Web/Controllers/HomeController.cs
StockManager.Web/Controllers/ProductsController.cs
StockManager.Web/Controllers/StockMovementsController.cs
StockManager.Web/Controllers/SuppliersController.cs
StockManager.Web/Services/DashboardService.cs
StockManager.Web/Services/ProductService.cs
StockManager.Web/Services/NoOpEmailSender.cs
StockManager.Web/Areas/Identity/Pages/Account/Login.cshtml.cs
StockManager.Web/Areas/Identity/Pages/Account/Register.cshtml.cs
```

## Correction de sécurité importante

Avant modification, `Program.cs` affichait la chaîne de connexion complète via `Console.WriteLine`.

Cela pouvait exposer le mot de passe SQL en Production :

```txt
Password=StockManager@2026!
```

La nouvelle version remplace cela par un log masqué :

```txt
Server=localhost\SQLEXPRESS; Database=StockManagerDb_Prod
```

## Niveaux de logs configurés

### Development

```json
"StockManager.Web": "Debug"
```

Permet de voir plus de détails pendant le développement.

### Recette et Production

```json
"StockManager.Web": "Information"
```

Permet de garder les logs métier importants sans trop de bruit.

### Entity Framework Core

```json
"Microsoft.EntityFrameworkCore.Database.Command": "Warning"
```

Évite d'afficher toutes les requêtes SQL et les paramètres sensibles.

## Exemples de logs attendus

```txt
Démarrage de Stock Manager | Environnement actif=Production
Base de données configurée | Server=localhost\SQLEXPRESS; Database=StockManagerDb_Prod
Utilisateur connecté | UserId=... | Email=manager@amadagoit.com
Produit créé | ProductId=12 | Référence=PROD-080 | Nom=Écran Dell 27 pouces | Quantité=10
Sortie stock refusée : stock insuffisant | ProductId=5 | Référence=PROD-005 | QuantitéDemandée=8 | StockDisponible=2
Alerte stock faible détectée | ProduitsStockFaible=4
HTTP GET /Products -> 200 en 84 ms | Utilisateur=manager
```

## Où voir les logs ?

### En développement Visual Studio

Les logs apparaissent dans :

```txt
Fenêtre Output / Sortie
Console de lancement ASP.NET Core
Terminal dotnet run
```

### En local avec Git Bash ou PowerShell

```bash
cd StockManager/StockManager.Web
dotnet run --environment Development
```

### En Production IIS

Pour une première phase, les logs console sont visibles si la journalisation stdout est activée dans le `web.config` publié.

Exemple :

```xml
<aspNetCore processPath="dotnet"
            arguments=".\StockManager.Web.dll"
            stdoutLogEnabled="true"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess" />
```

Créer aussi un dossier `logs` dans le dossier publié et donner les droits d'écriture au pool IIS.

## Bonnes pratiques respectées

- Aucun mot de passe n'est loggé.
- La chaîne de connexion est masquée.
- Les actions métier importantes sont tracées.
- Les erreurs et refus métier sont loggés en `Warning`.
- Les erreurs critiques de démarrage sont loggées en `Critical`.
- Les logs techniques trop bruyants sont limités.
- Les fichiers statiques ne sont pas loggés par le middleware applicatif.

## Prochaine amélioration possible

Quand le projet passera sur un VPS Windows/IIS définitif, tu pourras ajouter Serilog pour produire des fichiers journaliers propres :

```txt
logs/stockmanager-2026-06-25.log
```

Mais pour l'état actuel du projet, l'approche native `ILogger<T>` est plus simple, plus propre et suffisante pour montrer une démarche professionnelle aux recruteurs.
