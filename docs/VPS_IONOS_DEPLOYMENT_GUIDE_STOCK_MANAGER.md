# Guide de déploiement externe — Stock Manager ASP.NET Core MVC

Projet : **Stock Manager .NET — Amadago IT**  
Domaine prévu : **amadago-it.com**  
Hébergement prévu : **VPS Windows IONOS**  
Architecture cible : **Windows Server + IIS + SQL Server Express + ASP.NET Core Hosting Bundle**

---

## 1. Objectif du déploiement

L’objectif est de déployer l’application **StockManager.Web** sur un serveur externe accessible depuis Internet avec le domaine :

```txt
https://amadago-it.com
https://www.amadago-it.com
```

L’application doit utiliser :

```txt
Environnement ASP.NET Core : Production
Serveur Web : IIS
Base de données : SQL Server Express 2022
Instance SQL : localhost\SQLEXPRESS
Base : StockManagerDb_Prod
Application Pool IIS : StockManagerPool
```

---

## 2. Architecture finale

```txt
Utilisateur Internet
        |
        v
Nom de domaine amadago-it.com
        |
        v
DNS : A record vers l’adresse IP publique du VPS
        |
        v
VPS Windows IONOS
        |
        v
IIS / StockManagerPool
        |
        v
Application ASP.NET Core MVC StockManager.Web
        |
        v
SQL Server Express localhost\SQLEXPRESS
        |
        v
StockManagerDb_Prod
```

---

## 3. Préparation côté IONOS

### 3.1 Choisir le VPS

Pour ton projet portfolio, un VPS Windows d’entrée de gamme peut suffire au début, mais il faut vérifier les ressources.

Configuration minimale conseillée :

```txt
OS : Windows Server 2022
RAM : 2 Go minimum
RAM recommandée : 4 Go
CPU : 1 à 2 vCPU
Stockage : 40 Go minimum
Accès : RDP activé
Adresse IPv4 publique : obligatoire
```

Le pack à environ 5 €/mois peut convenir pour un site de démonstration, mais si Windows Server + SQL Server Express + IIS deviennent lourds, il faudra envisager plus de RAM.

### 3.2 Informations à récupérer après achat

Depuis l’espace client IONOS, récupérer :

```txt
Adresse IPv4 publique du VPS
Utilisateur administrateur Windows
Mot de passe administrateur
Nom du serveur
Accès RDP
```

Note ces informations dans un coffre sécurisé, pas dans GitHub.

---

## 4. Première connexion au VPS

Depuis Windows local :

```txt
Menu Démarrer
→ Connexion Bureau à distance
→ Adresse : IP_PUBLIQUE_DU_VPS
→ Utilisateur : Administrator ou utilisateur fourni par IONOS
→ Mot de passe : fourni par IONOS
```

Après connexion :

```txt
1. Changer le mot de passe administrateur si nécessaire
2. Vérifier les mises à jour Windows
3. Redémarrer le serveur après mises à jour
4. Renommer le serveur éventuellement : STOCKMANAGER-SRV
```

---

## 5. Installer les composants nécessaires sur le VPS

### 5.1 Activer IIS

Sur le VPS Windows Server :

```txt
Gestionnaire de serveur
→ Ajouter des rôles et fonctionnalités
→ Installation basée sur un rôle ou une fonctionnalité
→ Sélectionner le serveur
→ Cocher : Web Server (IIS)
```

Activer au minimum :

```txt
Web Server / IIS
├── Common HTTP Features
│   ├── Default Document
│   ├── Static Content
│   ├── HTTP Errors
│   └── HTTP Redirection
├── Health and Diagnostics
│   ├── HTTP Logging
│   └── Request Monitor
├── Security
│   ├── Request Filtering
│   └── Windows Authentication si besoin
├── Application Development
│   ├── .NET Extensibility 4.8
│   ├── ASP.NET 4.8
│   ├── ISAPI Extensions
│   ├── ISAPI Filters
│   └── WebSocket Protocol
└── Management Tools
    └── IIS Management Console
```

### 5.2 Installer .NET Hosting Bundle

Télécharger et installer le **.NET 10 Hosting Bundle** sur le VPS.

Après installation, lancer PowerShell en administrateur :

```powershell
iisreset
Import-Module WebAdministration
Get-WebGlobalModule | Where-Object { $_.Name -like "*AspNetCore*" }
```

Résultat attendu :

```txt
AspNetCoreModuleV2
```

Si ce module n’apparaît pas, IIS ne pourra pas lancer l’application ASP.NET Core.

### 5.3 Installer SQL Server Express 2022

Installer SQL Server Express avec :

```txt
Nom d’instance : SQLEXPRESS
Mode : Basic ou Custom
Administrateur SQL : utilisateur Windows administrateur du VPS
```

Chaîne locale attendue :

```txt
Server=localhost\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;
```

### 5.4 Installer SQL Server Management Studio

Installer SSMS pour gérer les bases SQL.

Connexion attendue :

```txt
Server name : localhost\SQLEXPRESS
Authentication : Windows Authentication
```

---

## 6. Préparer la configuration Production de Stock Manager

Dans `appsettings.Production.json`, utiliser :

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=StockManagerDb_Prod;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  },

  "Application": {
    "Environment": "Production"
  }
}
```

Pour un serveur externe, il est plus propre de ne pas mettre de mot de passe SQL dans GitHub. Comme cette configuration utilise `Trusted_Connection=True`, IIS se connectera avec l’identité du pool :

```txt
IIS APPPOOL\StockManagerPool
```

---

## 7. Publier l’application depuis Visual Studio

Dans Visual Studio :

```txt
Clic droit sur StockManager.Web
→ Publier
→ Dossier
```

Paramètres conseillés :

```txt
Configuration : Release
Framework cible : net10.0
Mode de déploiement : Dépendant du framework
Runtime cible : Portable
Supprimer tous les fichiers existants : true
```

Dans la partie base de données :

```txt
☐ Utiliser cette chaîne de connexion au moment de l’exécution
☐ Appliquer cette migration lors de la publication
```

Pourquoi décocher ? Parce qu’on préfère contrôler la base avec `appsettings.Production.json` et appliquer les migrations séparément.

Dossier de publication local possible :

```txt
C:\Users\asus\Desktop\gestionstockdeploy
```

Après publication, compresser le contenu du dossier en ZIP :

```txt
stockmanager-publish.zip
```

Ne pas compresser le dossier parent, mais bien le contenu publié directement.

---

## 8. Copier les fichiers sur le VPS

Sur le VPS, créer :

```txt
C:\inetpub\wwwroot\stockmanager
```

Copier le contenu publié dans ce dossier.

Le dossier doit contenir notamment :

```txt
StockManager.Web.dll
web.config
appsettings.json
appsettings.Production.json
wwwroot
Views
```

---

## 9. Créer le pool IIS

Dans IIS Manager :

```txt
Pools d’applications
→ Ajouter un pool d’applications
```

Configuration :

```txt
Nom : StockManagerPool
Version CLR .NET : Aucun code managé
Mode pipeline : Intégré
Identité : ApplicationPoolIdentity
```

Important : pour ASP.NET Core, il faut choisir :

```txt
Aucun code managé
```

---

## 10. Créer le site IIS

Dans IIS Manager :

```txt
Sites
→ Ajouter un site web
```

Configuration :

```txt
Nom du site : StockManager
Pool d’applications : StockManagerPool
Chemin physique : C:\inetpub\wwwroot\stockmanager
Type : http
Adresse IP : Toutes non attribuées ou IP du VPS
Port : 80
Nom d’hôte : amadago-it.com
```

Ajouter ensuite un second binding HTTP :

```txt
Type : http
Port : 80
Nom d’hôte : www.amadago-it.com
```

Après SSL, ajouter les bindings HTTPS.

---

## 11. Donner les droits Windows au dossier

Sur le dossier :

```txt
C:\inetpub\wwwroot\stockmanager
```

Ajouter l’utilisateur :

```txt
IIS AppPool\StockManagerPool
```

Droits nécessaires :

```txt
Lecture
Lecture et exécution
Affichage du contenu du dossier
```

Si les logs temporaires sont activés, créer :

```txt
C:\inetpub\wwwroot\stockmanager\logs
```

Et donner aussi :

```txt
Modification
Écriture
```

Quand l’application fonctionne, désactiver les logs stdout dans `web.config`.

---

## 12. Configurer l’environnement Production dans IIS

Vérifier que `web.config` contient bien `Production`.

Exemple recommandé :

```xml
<aspNetCore processPath="dotnet"
            arguments=".\StockManager.Web.dll"
            stdoutLogEnabled="false"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  </environmentVariables>
</aspNetCore>
```

Si tu dois diagnostiquer une erreur 500.30, activer temporairement :

```xml
stdoutLogEnabled="true"
```

Puis créer le dossier :

```txt
logs
```

---

## 13. Créer la base de données Production sur le VPS

Méthode recommandée : générer un script SQL idempotent depuis Visual Studio.

Dans Package Manager Console :

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
Script-Migration -Context ApplicationDbContext -Idempotent -Output migration-production.sql
```

Copier `migration-production.sql` sur le VPS.

Dans SSMS sur le VPS :

```txt
Connexion : localhost\SQLEXPRESS
Nouvelle requête
Ouvrir migration-production.sql
Exécuter
```

Cette méthode est plus professionnelle, car tu contrôles exactement ce qui est exécuté sur le serveur.

Alternative : exécuter `Update-Database` directement sur le VPS si le projet source et les outils EF sont installés dessus.

---

## 14. Donner les droits SQL au pool IIS

Après création de la base `StockManagerDb_Prod`, exécuter dans SSMS :

```sql
USE master;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.server_principals
    WHERE name = 'IIS APPPOOL\StockManagerPool'
)
BEGIN
    CREATE LOGIN [IIS APPPOOL\StockManagerPool] FROM WINDOWS;
END
GO

USE StockManagerDb_Prod;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.database_principals
    WHERE name = 'IIS APPPOOL\StockManagerPool'
)
BEGIN
    CREATE USER [IIS APPPOOL\StockManagerPool] FOR LOGIN [IIS APPPOOL\StockManagerPool];
END
GO

ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\StockManagerPool];
ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\StockManagerPool];
GO
```

Pendant la première installation uniquement, si l’application doit appliquer des migrations automatiquement, ajouter temporairement :

```sql
ALTER ROLE db_ddladmin ADD MEMBER [IIS APPPOOL\StockManagerPool];
GO
```

Après installation, retirer ce droit :

```sql
ALTER ROLE db_ddladmin DROP MEMBER [IIS APPPOOL\StockManagerPool];
GO
```

---

## 15. Vérifier les tables attendues

Dans SSMS :

```txt
localhost\SQLEXPRESS
→ Databases
→ StockManagerDb_Prod
→ Tables
```

Tables attendues :

```txt
dbo.__EFMigrationsHistory
dbo.AspNetRoles
dbo.AspNetUsers
dbo.AspNetUserRoles
dbo.AspNetUserClaims
dbo.AspNetRoleClaims
dbo.AspNetUserLogins
dbo.AspNetUserTokens
dbo.Categories
dbo.Products
dbo.Suppliers
dbo.StockMovements
```

---

## 16. Configurer le DNS du domaine amadago-it.com

Objectif : faire pointer le domaine vers l’adresse IP publique du VPS IONOS.

Chez le fournisseur DNS du domaine, créer ou modifier :

### Enregistrement A racine

```txt
Type : A
Nom / Host : @
Valeur / Points to : IP_PUBLIQUE_DU_VPS
TTL : 3600 ou automatique
```

### Enregistrement www

Option recommandée :

```txt
Type : CNAME
Nom / Host : www
Valeur / Points to : amadago-it.com
TTL : 3600 ou automatique
```

Option alternative :

```txt
Type : A
Nom / Host : www
Valeur / Points to : IP_PUBLIQUE_DU_VPS
TTL : 3600 ou automatique
```

Pour le domaine racine `amadago-it.com`, utiliser un **A record**, pas un CNAME.

Propagation DNS : quelques minutes à 24 h.

Vérification depuis ton PC :

```powershell
nslookup amadago-it.com
nslookup www.amadago-it.com
```

Résultat attendu :

```txt
IP_PUBLIQUE_DU_VPS
```

---

## 17. Ouvrir les ports réseau

Sur le firewall Windows du VPS :

```txt
Autoriser le trafic entrant TCP 80
Autoriser le trafic entrant TCP 443
```

Dans l’espace client IONOS, vérifier aussi si un pare-feu réseau existe et autoriser :

```txt
HTTP 80
HTTPS 443
RDP 3389 uniquement depuis ton IP si possible
```

Éviter d’exposer SQL Server 1433 publiquement.

---

## 18. Installer HTTPS avec Let’s Encrypt et win-acme

Pour HTTPS gratuit sur IIS, utiliser **win-acme**.

Étapes générales :

```txt
1. Télécharger win-acme sur le VPS
2. Extraire dans C:\tools\win-acme
3. Lancer wacs.exe en administrateur
4. Choisir : Create certificate
5. Choisir : IIS site binding
6. Sélectionner le site StockManager
7. Inclure amadago-it.com et www.amadago-it.com
8. Choisir validation HTTP
9. Installer le certificat dans IIS
10. Activer la tâche automatique de renouvellement
```

Après installation, vérifier dans IIS :

```txt
Site StockManager
→ Bindings
→ https
→ amadago-it.com
→ certificat Let’s Encrypt
```

Tester :

```txt
https://amadago-it.com
https://www.amadago-it.com
```

---

## 19. Redirection HTTP vers HTTPS

Dans IIS, installer ou activer le module URL Rewrite si nécessaire.

Exemple de règle dans `web.config` :

```xml
<rewrite>
  <rules>
    <rule name="Redirect to HTTPS" stopProcessing="true">
      <match url="(.*)" />
      <conditions>
        <add input="{HTTPS}" pattern="off" ignoreCase="true" />
      </conditions>
      <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
    </rule>
  </rules>
</rewrite>
```

À ajouter uniquement si le module URL Rewrite est installé. Sinon IIS affichera une erreur 500.19.

---

## 20. Tester l’application

Depuis le VPS :

```txt
http://localhost
```

Depuis ton PC :

```txt
http://IP_DU_VPS
http://amadago-it.com
https://amadago-it.com
```

Pages à vérifier :

```txt
/
/Products
/Categories
/Suppliers
/StockMovements
/Identity/Account/Login
```

Tester aussi les comptes seedés et les rôles :

```txt
ADMIN
MANAGER
EMPLOYEE
VISITOR
```

---

## 21. Checklist finale de sécurité

```txt
☐ Le site répond sur https://amadago-it.com
☐ Le site répond sur https://www.amadago-it.com
☐ HTTP redirige vers HTTPS
☐ appsettings.Production.json ne contient pas de mot de passe sensible
☐ stdoutLogEnabled=false
☐ Le dossier logs ne contient pas d’informations sensibles
☐ SQL Server n’est pas exposé publiquement
☐ RDP est protégé par un mot de passe fort
☐ Windows Update est activé
☐ Le pool IIS utilise ApplicationPoolIdentity
☐ Les droits SQL du pool sont limités à db_datareader/db_datawriter
☐ Les comptes admin seedés ont un mot de passe changé
☐ Products, Categories, Suppliers, Movements fonctionnent
☐ Login/Logout fonctionne
☐ Les rôles ADMIN, MANAGER, EMPLOYEE, VISITOR fonctionnent
```

---

## 22. Commandes utiles

### Redémarrer IIS

```powershell
iisreset
```

### Vérifier le module ASP.NET Core IIS

```powershell
Import-Module WebAdministration
Get-WebGlobalModule | Where-Object { $_.Name -like "*AspNetCore*" }
```

### Tester l’application hors IIS

```powershell
cd C:\inetpub\wwwroot\stockmanager
dotnet .\StockManager.Web.dll
```

### Voir les services SQL

```powershell
Get-Service | Where-Object { $_.Name -like "MSSQL*" }
```

### Générer un script SQL de migration

```powershell
Script-Migration -Context ApplicationDbContext -Idempotent -Output migration-production.sql
```

---

## 23. Erreurs fréquentes et solutions

### HTTP 500.19

Cause probable :

```txt
web.config non lisible
AspNetCoreModuleV2 absent
URL Rewrite utilisé mais module non installé
```

Solution :

```txt
Installer .NET Hosting Bundle
Vérifier AspNetCoreModuleV2
Installer URL Rewrite si une règle rewrite est présente
```

### HTTP 500.30

Cause probable :

```txt
Application ASP.NET Core démarre puis plante
Erreur de base SQL
Migrations non appliquées
appsettings.Production.json incorrect
```

Solution :

```txt
Activer stdoutLogEnabled=true temporairement
Lire le fichier logs/stdout_*.log
Tester dotnet .\StockManager.Web.dll
Vérifier la chaîne SQL
Vérifier les tables AspNetRoles et AspNetUsers
```

### Cannot create an automatic instance

Cause :

```txt
Utilisation de (localdb)\MSSQLLocalDB avec IIS
```

Solution :

```txt
Remplacer LocalDB par SQL Server Express : localhost\SQLEXPRESS
```

### Invalid object name AspNetRoles

Cause :

```txt
Tables ASP.NET Identity non créées
```

Solution :

```txt
Appliquer les migrations EF Core ou exécuter le script SQL de migration
```

---

## 24. Stratégie de mise à jour après première mise en ligne

Pour chaque nouvelle version :

```txt
1. Tester en local Development
2. Tester en Production local IIS si possible
3. Publier en Release dans un dossier
4. Sauvegarder le dossier actuel du VPS
5. Sauvegarder la base SQL
6. Copier les nouveaux fichiers sur le VPS
7. Exécuter les migrations si nécessaire
8. Redémarrer IIS
9. Tester les pages principales
10. Vérifier les logs
```

---

## 25. Résumé professionnel à valoriser

```txt
J’ai préparé et validé un déploiement Production d’une application ASP.NET Core MVC
sur IIS avec SQL Server Express, configuration multi-environnements, migrations
Entity Framework Core, ASP.NET Identity, rôles utilisateurs, bindings domaine,
HTTPS et configuration serveur Windows VPS.
```

Commande commit recommandée :

```bash
git add docs/VPS_IONOS_DEPLOYMENT_GUIDE_STOCK_MANAGER.md
git commit -m "docs(deploy): add IONOS VPS deployment guide"
git push origin main
```

---

© 2026 Princy Rasoloarivony — Amadago IT
