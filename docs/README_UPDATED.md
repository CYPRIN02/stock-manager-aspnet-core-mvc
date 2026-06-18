# 🚀 Stock Manager — ASP.NET Core MVC

<div align="center">

## Inventory Management System for Electronic Devices

Application web professionnelle de gestion de stock développée avec **ASP.NET Core MVC**, **Entity Framework Core**, **ASP.NET Core Identity**, **SQL Server 2022 Express** et **IIS**.

**Designed and developed by Princy Rasoloarivony — AmadagoIT**

[🌐 Portfolio](https://flask-portfolio-thic.onrender.com/) • [📂 Repository](https://github.com/CYPRIN02/stock-manager-aspnet-core-mvc) • [🔗 Linktree](https://linktr.ee/princy02)

</div>

---

## 📖 Overview

**Stock Manager** est une application ASP.NET Core MVC conçue pour gérer un stock de produits électroniques de manière claire, sécurisée et professionnelle.

Le projet met en avant une architecture MVC propre, une base de données SQL Server, des migrations Entity Framework Core, une authentification ASP.NET Core Identity, une gestion des rôles et un déploiement local via **IIS + SQL Server Express**.

Objectif principal : construire un projet portfolio réaliste, proche d’un contexte entreprise, présentable à des recruteurs, formateurs et équipes techniques.

---

## 🎯 Project Objectives

- Construire une application web professionnelle en **ASP.NET Core MVC**.
- Gérer les produits, catégories, fournisseurs et mouvements de stock.
- Mettre en place une authentification avec **ASP.NET Core Identity**.
- Sécuriser les accès avec des rôles utilisateurs.
- Utiliser **Entity Framework Core Code First** et les migrations SQL Server.
- Préparer un projet propre pour GitHub, portfolio et démonstration technique.
- Déployer l’application localement sous **IIS** avec un environnement **Production**.

---

## 🛠 Technology Stack

### Backend

- C#
- ASP.NET Core MVC
- ASP.NET Core Identity
- Entity Framework Core
- LINQ
- Dependency Injection

### Frontend

- Razor Views
- HTML5
- CSS3
- Bootstrap 5
- JavaScript
- Responsive layout

### Database

- SQL Server 2022 Express
- SQL Server Management Studio
- Entity Framework Core Migrations
- Multi-environment connection strings

### Deployment

- IIS — Internet Information Services
- ASP.NET Core Hosting Bundle
- Application Pool dédié
- SQL Server Express instance `localhost\\SQLEXPRESS`

### Development Tools

- Visual Studio 2022
- Git / GitHub
- Package Manager Console
- PowerShell
- SSMS

---

## 🏗 Architecture

The application follows the **MVC — Model View Controller** pattern.

```text
StockManager.Web
│
├── Controllers
│   ├── ProductsController.cs
│   ├── CategoriesController.cs
│   ├── SuppliersController.cs
│   └── StockMovementsController.cs
│
├── Data
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs
│
├── Models
│   ├── Product.cs
│   ├── Category.cs
│   ├── Supplier.cs
│   └── StockMovement.cs
│
├── Views
│   ├── Products
│   ├── Categories
│   ├── Suppliers
│   ├── StockMovements
│   └── Shared
│
├── Migrations
├── wwwroot
└── appsettings.*.json
```

### Principles Applied

- MVC Architecture
- Separation of Concerns
- Entity Framework Code First
- Dependency Injection
- Role-Based Access Control
- Clean and maintainable structure

---

## ✨ Implemented Features

### 📦 Product Management

- Create, edit, delete and view product details.
- Product reference uniqueness validation.
- Duplicate reference prevention.
- Product search.
- Product filtering by reference, category, supplier, quantity and status.
- Product sorting.
- Pagination.
- Improved product list navigation.

### 🗂 Category Management

- Create categories.
- Edit categories.
- Delete categories.
- View category details.
- Integration with product classification.

### 🏭 Supplier Management

- Create suppliers.
- Edit suppliers.
- Delete suppliers.
- View supplier details.
- Link products to suppliers.

### 🔄 Stock Movements

- Track stock input and output movements.
- Associate movements with products.
- Support inventory history tracking.

### 🔐 Authentication & Authorization

- ASP.NET Core Identity integration.
- User registration and login.
- Role-based access control.
- Seeded roles and users.
- Access protection by role.

Current role model:

```text
ADMIN
MANAGER
EMPLOYEE
VISITOR
```

### 🎨 User Experience

- Modern AmadagoIT branding.
- Improved home page.
- Responsive navigation.
- Professional layout.
- Clean forms and validation messages.
- User-friendly CRUD pages.

---

## 🗄 Database Design

### Main Business Tables

```text
Categories
Products
Suppliers
StockMovements
```

### ASP.NET Core Identity Tables

```text
AspNetUsers
AspNetRoles
AspNetUserRoles
AspNetUserClaims
AspNetRoleClaims
AspNetUserLogins
AspNetUserTokens
```

### Relationships

```text
Category 1 ---- * Product
Supplier 1 ---- * Product
Product  1 ---- * StockMovement
User     * ---- * Role
```

---

## ⚙️ Environment Configuration

The project supports multiple environments:

```text
Development
Recette
Production
```

Configuration files:

```text
appsettings.json
appsettings.Development.json
appsettings.Recette.json
appsettings.Production.json
```

Production connection example for IIS local deployment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=StockManagerDb_Prod;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  },
  "Application": {
    "Environment": "Production"
  }
}
```

> Do not expose real production secrets in a public repository. Use environment variables, user secrets or secure deployment settings for sensitive values.

---

## 🚀 Getting Started — Development

### Prerequisites

- Visual Studio 2022
- .NET SDK compatible with the project target framework
- SQL Server 2022 Express or SQL Server Developer
- SQL Server Management Studio

### Clone Repository

```bash
git clone https://github.com/CYPRIN02/stock-manager-aspnet-core-mvc.git
cd stock-manager-aspnet-core-mvc
```

### Restore Packages

```bash
dotnet restore
```

### Apply Database Migrations

Using Package Manager Console:

```powershell
Update-Database -Context ApplicationDbContext
```

Or using CLI if `dotnet-ef` is installed:

```bash
dotnet ef database update --context ApplicationDbContext
```

### Run Application

```bash
dotnet run --project StockManager.Web
```

---

## 🌐 Local IIS Deployment

The application has been successfully deployed locally with:

```text
IIS Application: Default Web Site / gestionstockdeploy
URL: http://localhost/gestionstockdeploy
Environment: Production
Database: StockManagerDb_Prod
SQL Instance: localhost\SQLEXPRESS
Application Pool: StockManagerPool
```

### Required IIS Configuration

Application Pool:

```text
.NET CLR Version: No Managed Code
Managed Pipeline Mode: Integrated
Identity: ApplicationPoolIdentity
```

Required server component:

```text
ASP.NET Core Hosting Bundle
AspNetCoreModuleV2
```

### Production Migration Command

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
Update-Database -Context ApplicationDbContext -Verbose
```

### SQL Permission for IIS App Pool

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

---

## 🧪 Test URLs

```text
http://localhost/gestionstockdeploy
http://localhost/gestionstockdeploy/Products
http://localhost/gestionstockdeploy/Categories
http://localhost/gestionstockdeploy/Suppliers
http://localhost/gestionstockdeploy/StockMovements
http://localhost/gestionstockdeploy/Identity/Account/Login
```

---

## ✅ Current Project Status

Implemented:

- ASP.NET Core MVC project foundation.
- Multi-environment configuration.
- SQL Server database integration.
- Entity Framework Core migrations.
- Product CRUD with filtering, sorting and pagination.
- Category and supplier management.
- Stock movements module.
- ASP.NET Core Identity authentication.
- Role-based access control.
- User administration foundation.
- Modern UI and AmadagoIT branding.
- IIS local Production deployment with SQL Server Express.

Planned improvements:

- REST API project for external frontend integration.
- Angular frontend module.
- Advanced dashboard statistics.
- Excel/PDF exports.
- Docker support.
- GitHub Actions CI/CD.
- Azure or VPS deployment.
- Automated tests.

---

## 🧾 Deployment Notes

A dedicated deployment report is available in:

```text
docs/DEPLOYMENT_REPORT_IIS_PRODUCTION.md
```

It summarizes the IIS deployment steps, SQL Server Express migration, errors encountered and final configuration.

---

## 👨‍💻 About The Developer

## Princy Rasoloarivony

Full Stack Developer specialized in .NET and Python.

🎓 Master's Degree in Artificial Intelligence & Big Data — ESGI Paris

💼 Former .NET Developer Apprentice at BPCE Solutions Informatiques

Experienced with:

- ASP.NET MVC / ASP.NET Core MVC
- C#
- SQL Server
- Entity Framework Core
- REST APIs
- Authentication / Authorization
- Deployment pipelines
- Agile methodologies

Passionate about:

- Software Engineering
- Artificial Intelligence
- Cloud Technologies
- Data Engineering
- Business Applications

---

## 🌐 Links

- Portfolio: https://flask-portfolio-thic.onrender.com/
- GitHub: https://github.com/CYPRIN02
- Repository: https://github.com/CYPRIN02/stock-manager-aspnet-core-mvc
- Linktree: https://linktr.ee/princy02

---

## 🏢 AmadagoIT

Stock Manager is developed under the **AmadagoIT** initiative, focused on creating professional software solutions, web applications and business tools.

---

## ⭐ Support

If you find this project interesting:

- Give it a star ⭐
- Follow my GitHub profile
- Connect with me through Linktree
- Visit my portfolio

Thank you for visiting this project!

---

© 2026 Princy Rasoloarivony — AmadagoIT
