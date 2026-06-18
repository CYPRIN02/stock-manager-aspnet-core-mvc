# Compte rendu — Déploiement IIS Production local

Projet : **Stock Manager — ASP.NET Core MVC**  
Auteur : **Princy Rasoloarivony — AmadagoIT**  
Contexte : Déploiement local IIS de l’application Stock Manager en environnement Production.

---

## 1. Objectif

L’objectif de cette étape était de publier et exécuter l’application **StockManager.Web** sous **IIS** avec une base de données Production séparée.

Configuration cible :

```text
Application : StockManager.Web
Framework cible : net10.0
Serveur Web : IIS — Default Web Site / gestionstockdeploy
URL locale : http://localhost/gestionstockdeploy
Environnement : Production
Base de données : StockManagerDb_Prod
Instance SQL : localhost\SQLEXPRESS
Pool IIS : StockManagerPool
```

---

## 2. État avant intervention

L’application fonctionnait depuis Visual Studio / IIS Express, mais le déploiement IIS local rencontrait plusieurs blocages :

```text
HTTP 500.19 — IIS ne lisait pas correctement le web.config
HTTP 500.30 — L’application ASP.NET Core démarrait puis s’arrêtait
Erreur SQL — Invalid object name 'AspNetRoles'
Erreur LocalDB — IIS ne pouvait pas accéder à (localdb)\MSSQLLocalDB
Warning NuGet NU1901 bloquant pendant les migrations
```

---

## 3. Actions réalisées

### 3.1 Publication Visual Studio

Un profil de publication dossier a été utilisé pour générer les fichiers déployables.

Paramètres retenus :

```text
Configuration : Release
Mode de déploiement : Dépendant du framework
Runtime cible : Portable
Option : Supprimer les fichiers existants avant publication
```

Le contenu publié a été placé dans :

```text
C:\inetpub\wwwroot\gestionstockdeploy
```

---

### 3.2 Configuration IIS

Une application IIS a été créée sous :

```text
Default Web Site / gestionstockdeploy
```

Un pool dédié a été configuré :

```text
Nom : StockManagerPool
.NET CLR Version : Aucun code managé
Mode pipeline : Intégré
Identité : ApplicationPoolIdentity
```

Les permissions Windows ont été ajoutées sur le dossier publié pour :

```text
IIS AppPool\StockManagerPool
```

Droits accordés :

```text
Lecture
Lecture et exécution
Affichage du contenu du dossier
Écriture temporaire pour les logs stdout
```

---

### 3.3 Installation du Hosting Bundle

Le problème HTTP 500.19 indiquait que IIS ne reconnaissait pas correctement la section ASP.NET Core dans le fichier `web.config`.

Action réalisée :

```text
Installation du .NET Hosting Bundle compatible avec le framework du projet
Redémarrage IIS avec iisreset
Vérification du module AspNetCoreModuleV2
```

Résultat :

```text
Le 500.19 a disparu.
IIS a commencé à lancer l’application ASP.NET Core.
```

---

### 3.4 Activation temporaire des logs stdout

Pour diagnostiquer le HTTP 500.30, les logs stdout ont été activés temporairement dans `web.config` :

```xml
<aspNetCore processPath="dotnet"
            arguments=".\StockManager.Web.dll"
            stdoutLogEnabled="true"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess" />
```

Un dossier `logs` a été créé dans le dossier publié.

Résultat du diagnostic :

```text
L’application échouait au démarrage à cause de la connexion SQL LocalDB sous IIS.
```

À remettre après stabilisation :

```xml
stdoutLogEnabled="false"
```

---

### 3.5 Correction de la base de données Production

L’ancienne chaîne de connexion Production utilisait LocalDB :

```text
Server=(localdb)\MSSQLLocalDB;Database=StockManagerDb_Prod
```

Problème : LocalDB dépend de l’utilisateur Windows courant et n’est pas adapté à une exécution IIS via un Application Pool.

Solution : installation de **SQL Server 2022 Express** avec l’instance :

```text
SQLEXPRESS
```

Nouvelle chaîne Production :

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=StockManagerDb_Prod;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
```

---

### 3.6 Migrations Entity Framework Core

Les migrations ont été appliquées sur l’environnement Production :

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
Update-Database -Context ApplicationDbContext -Verbose
```

Résultat : la base `StockManagerDb_Prod` a été créée/migrée dans `localhost\SQLEXPRESS`.

Tables attendues :

```text
AspNetUsers
AspNetRoles
AspNetUserRoles
AspNetUserClaims
AspNetRoleClaims
AspNetUserLogins
AspNetUserTokens
Categories
Products
Suppliers
StockMovements
__EFMigrationsHistory
```

---

### 3.7 Correction du warning NuGet

Un warning NuGet empêchait l’exécution correcte de certaines commandes EF :

```text
NU1901 — NuGet.Packaging 6.12.1 présente une vulnérabilité faible connue
```

Correction temporaire appliquée dans le projet :

```xml
<NuGetAudit>false</NuGetAudit>
```

Note : cette solution est acceptable temporairement pour débloquer le déploiement local. La correction propre sera de mettre à jour ou remplacer le package concerné.

---

## 4. Résultat final

L’application est maintenant accessible via IIS :

```text
http://localhost/gestionstockdeploy
```

État final :

```text
Publication IIS réussie
Hosting Bundle installé
web.config reconnu par IIS
Application ASP.NET Core démarrée correctement
Base Production migrée vers SQL Server Express
StockManagerDb_Prod utilisée en Production
Interface Stock Manager accessible dans le navigateur
```

---

## 5. Points de vérification fonctionnelle

Pages à tester après chaque publication :

```text
http://localhost/gestionstockdeploy
http://localhost/gestionstockdeploy/Products
http://localhost/gestionstockdeploy/Categories
http://localhost/gestionstockdeploy/Suppliers
http://localhost/gestionstockdeploy/StockMovements
http://localhost/gestionstockdeploy/Identity/Account/Login
```

Comptes seedés à vérifier selon la configuration du `DbSeeder` :

```text
admin@amadagoit.com
manager@amadagoit.com
employee@amadagoit.com
visitor@amadagoit.com
```

---

## 6. Points à finaliser

- Remettre `stdoutLogEnabled="false"` dans `web.config` après diagnostic.
- Vérifier que les logs temporaires ne sont pas commités.
- Ne pas exposer de mots de passe ou secrets dans GitHub.
- Remplacer plus tard `<NuGetAudit>false</NuGetAudit>` par une vraie mise à jour de dépendance.
- Ajouter une section README dédiée au déploiement IIS.
- Préparer une prochaine étape Cloud : Azure App Service, VPS ou Docker.

---

## 7. Commandes utiles

### Redémarrer IIS

```powershell
iisreset
```

### Tester l’application publiée hors IIS

```powershell
cd C:\inetpub\wwwroot\gestionstockdeploy
dotnet .\StockManager.Web.dll
```

### Appliquer les migrations Production

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
Update-Database -Context ApplicationDbContext -Verbose
```

### Vérifier les modules ASP.NET Core IIS

```powershell
Import-Module WebAdministration
Get-WebGlobalModule | Where-Object { $_.Name -like "*AspNetCore*" }
```

---

## 8. Proposition de commit

```bash
git add .
git commit -m "chore(deploy): document IIS production deployment with SQL Server Express"
git push origin main
```

---

© 2026 Princy Rasoloarivony — AmadagoIT
