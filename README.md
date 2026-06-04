# 🚀 Stock Manager - ASP.NET Core MVC

<div align="center">

## Inventory Management System

Modern inventory management application built with ASP.NET Core MVC, Entity Framework Core and SQL Server.

**Designed and developed by Princy Rasoloarivony**

[🌐 Portfolio](https://flask-portfolio-thic.onrender.com/) • [📂 GitHub](https://github.com/CYPRIN02/stock-manager-aspnet-core-mvc) • [🔗 Linktree](https://linktr.ee/princy02)

</div>

---

# 📖 Overview

Stock Manager is a professional inventory management web application designed to help organizations efficiently manage products, categories, suppliers and inventory information.

This project was developed to demonstrate modern Full Stack .NET development skills and software engineering best practices using the Microsoft ecosystem.

The application focuses on:

- Product inventory management
- Category management
- Supplier management
- Data validation
- User experience
- Scalable architecture
- Multi-environment deployment preparation

---

# 🎯 Project Objectives

The main goals of this project are:

- Build a professional ASP.NET Core MVC application
- Apply enterprise-level software development practices
- Demonstrate backend and frontend development skills
- Implement Entity Framework Core with SQL Server
- Showcase clean architecture principles
- Create a portfolio-ready project for recruiters and technical interviews

---

# 🛠 Technology Stack

## Backend

- C#
- ASP.NET Core MVC
- Entity Framework Core
- LINQ
- Dependency Injection

## Frontend

- Razor Views
- HTML5
- CSS3
- Bootstrap 5
- JavaScript

## Database

- SQL Server 2022
- Entity Framework Core Migrations

## Development Tools

- Visual Studio 2022
- Git
- GitHub
- SQL Server Management Studio

---

# 🏗 Architecture

The application follows the MVC (Model-View-Controller) pattern.

```text
StockManager.Web
│
├── Controllers
├── Models
├── Data
├── Services
├── ViewModels
├── Views
├── Migrations
└── wwwroot
```

### Principles Applied

- MVC Architecture
- SOLID Principles
- Separation of Concerns
- Dependency Injection
- Clean Code
- Entity Framework Code First

---

# ✨ Features

## 📦 Product Management

- Create products
- Edit products
- Delete products
- Product details page
- Unique reference validation
- Duplicate reference prevention
- Product search
- Product filtering
- Product sorting
- Pagination

## 🗂 Category Management

- Create categories
- Edit categories
- Delete categories
- Category details page

## 🏭 Supplier Management

- Create suppliers
- Edit suppliers
- Delete suppliers
- Supplier details page

## 🎨 User Experience

- Responsive interface
- Bootstrap design
- Modern navigation
- Form validation
- User-friendly CRUD operations

---

# 🗄 Database Design

## Product

- Id
- Reference
- Name
- Description
- Quantity
- Price
- Status
- CategoryId
- SupplierId

## Category

- Id
- Name
- Description

## Supplier

- Id
- Name
- Email
- Phone
- Address

### Relationships

```text
Category 1 ---- * Product

Supplier 1 ---- * Product
```

---

# ⚙️ Environment Configuration

The project supports multiple environments:

```text
Development
Staging
Production
```

Configuration files:

```text
appsettings.json
appsettings.Development.json
appsettings.Staging.json
appsettings.Production.json
```

---

# 🚀 Getting Started

## Prerequisites

- .NET 8 SDK
- SQL Server
- Visual Studio 2022

## Clone Repository

```bash
git clone https://github.com/CYPRIN02/stock-manager-aspnet-core-mvc.git
```

## Restore Packages

```bash
dotnet restore
```

## Apply Database Migrations

```bash
dotnet ef database update
```

## Run Application

```bash
dotnet run
```

---

# 📈 Future Improvements

Planned features:

- Authentication & Authorization
- User Roles
- Dashboard & Statistics
- Inventory Movements
- Stock History Tracking
- Reporting
- Excel Export
- PDF Export
- REST API
- Docker Support
- GitHub Actions CI/CD
- Azure Deployment
- Advanced Analytics

---

# 👨‍💻 About The Developer

## Princy Rasoloarivony

Full Stack Developer specialized in .NET and Python.

🎓 Master's Degree in Artificial Intelligence & Big Data (ESGI Paris)

💼 Former .NET Developer Apprentice at BPCE Solutions Informatiques

Experienced with:

- ASP.NET MVC
- ASP.NET Core
- C#
- SQL Server
- Entity Framework
- REST APIs
- Deployment Pipelines
- Agile Methodologies

Passionate about:

- Software Engineering
- Artificial Intelligence
- Cloud Technologies
- Data Engineering
- Business Applications

---

# 🌐 Links

### Portfolio

https://flask-portfolio-thic.onrender.com/

### GitHub

https://github.com/CYPRIN02

### Linktree

https://linktr.ee/princy02

---

# 📫 Contact

📧 Email

prasoloarivony@gmail.com

💻 GitHub

https://github.com/CYPRIN02

🌐 Portfolio

https://flask-portfolio-thic.onrender.com/

---

# 🏢 AmadagoIT

Stock Manager is developed under the AmadagoIT initiative, focused on creating professional software solutions, web applications and business tools.

---

# ⭐ Support

If you find this project interesting:

- Give it a star ⭐
- Follow my GitHub profile
- Connect with me through Linktree
- Visit my portfolio

Thank you for visiting this project!

---

© 2026 Princy Rasoloarivony - AmadagoIT
