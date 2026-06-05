<div align="center">

# 🎬 MoviesMinimalAPI

**A production-ready RESTful API for managing movies, built with ASP.NET Core 9 Minimal APIs, backed by Azure SQL, and deployed via a fully automated CI/CD pipeline.**

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat-square&logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Azure](https://img.shields.io/badge/Azure-App%20Service-0078D4?style=flat-square&logo=microsoftazure)](https://azure.microsoft.com/)
[![Azure SQL](https://img.shields.io/badge/Azure%20SQL-Database-CC2927?style=flat-square&logo=microsoftazure)](https://azure.microsoft.com/en-us/products/azure-sql/)
[![CI/CD](https://img.shields.io/badge/CI%2FCD-Azure%20Pipelines-2560E0?style=flat-square&logo=azurepipelines)](https://azure.microsoft.com/en-us/products/devops/pipelines/)

[🚀 Live Demo (Swagger UI)](https://moviesminimalapi.azurewebsites.net/swagger/index.html) · [📁 Source Code](https://github.com/beqa21beraia/MoviesMinimalAPI) · [🐛 Report an Issue](https://github.com/beqa21beraia/MoviesMinimalAPI/issues)

</div>

---

## 📖 Overview

MoviesMinimalAPI demonstrates the power and simplicity of **ASP.NET Core 9 Minimal APIs** — a lightweight, high-performance approach to building HTTP APIs with minimal boilerplate. The project features full CRUD operations, a dedicated SQL Server Database project (DACPAC), and a three-stage Azure Pipelines CI/CD workflow that goes from a `git push` to a live deployment automatically.

### Key Highlights

- **Minimal API pattern** — Clean, concise endpoint definitions without controller ceremony
- **Dedicated DB project** — Schema managed as code via `.sqlproj` / DACPAC, enabling reproducible deployments
- **Three-stage CI/CD** — Build → Deploy Database → Deploy API, all automated on push to `main`
- **Live on Azure** — API and database hosted on Azure App Service and Azure SQL

---

## 🗂 Project Structure

```
MoviesMinimalAPI/
│
├── MinimalAPIsMovies/              # Main API project (.NET 9)
│   ├── Program.cs                  # Entry point: service registration & endpoint mapping
│   ├── MinimalAPIsMovies.csproj    # Project file & NuGet dependencies
│   └── ...                         # Models, DTOs, Repositories, Endpoint handlers
│
├── DB/                             # SQL Server Database project
│   └── DB.sqlproj                  # Schema definitions compiled to a DACPAC artifact
│
├── .github/                        # GitHub configuration
├── azure-pipelines.yml             # CI/CD pipeline (3-stage)
├── MinimalAPIsMovies.sln           # Solution file
└── .gitignore
```

---

## 🛠 Tech Stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 9 — Minimal APIs |
| Language | C# 12 |
| Database | Azure SQL Server (T-SQL) |
| Schema Management | SQL Server Database Project (DACPAC) |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| CI/CD | Azure Pipelines |
| Hosting | Azure App Service (`moviesminimalapi`) |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (local) or an Azure SQL Database instance
- Visual Studio 2022+ **or** VS Code with the C# extension

### Clone & Run Locally

```bash
# Clone the repository
git clone https://github.com/beqa21beraia/MoviesMinimalAPI.git
cd MoviesMinimalAPI
```

Update the connection string in `MinimalAPIsMovies/appsettings.json` (see [Configuration](#-configuration)), then:

```bash
cd MinimalAPIsMovies
dotnet restore
dotnet run
```

The API will start at `https://localhost:<port>`. Navigate to `/swagger` to explore and test all endpoints interactively.

---

## 🗄 Database Setup

Schema is managed via a **SQL Server Database Project** (`.sqlproj`) that compiles to a **DACPAC** — a portable, version-controlled snapshot of the database schema. In production, the pipeline deploys this automatically.

**For local development:**

1. Open `DB/DB.sqlproj` in Visual Studio.
2. Right-click the project → **Publish** → target your local SQL Server instance.
3. Update `appsettings.json` with the resulting connection string.

**Production target:**

| | Value |
|---|---|
| Server | `moviesdb-server.database.windows.net` |
| Database | `MoviesDB` |

---

## 📡 API Endpoints

Full interactive documentation available at the **[live Swagger UI](https://moviesminimalapi.azurewebsites.net/swagger/index.html)**.

### Movies

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/movies` | Retrieve all movies |
| `GET` | `/api/movies/{id}` | Retrieve a single movie by ID |
| `POST` | `/api/movies` | Create a new movie |
| `PUT` | `/api/movies/{id}` | Update an existing movie |
| `DELETE` | `/api/movies/{id}` | Delete a movie |

### Example: Create a Movie

**Request**
```http
POST /api/movies
Content-Type: application/json

{
  "title": "Inception",
  "genre": "Sci-Fi",
  "releaseYear": 2010,
  "director": "Christopher Nolan",
  "rating": 8.8
}
```

**Response** `201 Created`
```json
{
  "id": 42,
  "title": "Inception",
  "genre": "Sci-Fi",
  "releaseYear": 2010,
  "director": "Christopher Nolan",
  "rating": 8.8
}
```

> **Note:** For the complete and up-to-date schema including all fields and validation rules, see the live Swagger UI.

---

## 🔄 CI/CD Pipeline

Every push to `main` triggers a three-stage Azure Pipeline:

```
              push to main
                   │
                   ▼
┌─────────────────────────────────────────┐
│             Stage 1: Build              │
│                                         │
│  • Install .NET 9 SDK                   │
│  • Restore NuGet packages               │
│  • Compile solution (Release)           │
│  • Build DB project → DACPAC artifact   │
│  • Publish API → ZIP artifact           │
└──────────────────┬──────────────────────┘
                   │  depends on Build
                   ▼
┌─────────────────────────────────────────┐
│          Stage 2: Deploy Database       │
│                                         │
│  • Download build artifacts             │
│  • Deploy DACPAC to Azure SQL           │
│    (moviesdb-server.database.windows    │
│     .net → MoviesDB)                    │
└──────────────────┬──────────────────────┘
                   │  depends on DeployDB
                   ▼
┌─────────────────────────────────────────┐
│            Stage 3: Deploy API          │
│                                         │
│  • Download build artifacts             │
│  • Deploy ZIP to Azure App Service      │
│    (moviesminimalapi.azurewebsites.net) │
└─────────────────────────────────────────┘
```

Database credentials (`DB_USERNAME`, `DB_PASSWORD`) are stored as **secure pipeline variables** and are never committed to source control.

---

## ⚙️ Configuration

The API reads its settings from `appsettings.json`. Copy the template below and fill in your values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=<your-server>;Database=MoviesDB;User Id=<user>;Password=<password>;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> ⚠️ **Never commit secrets to source control.** Use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development and Azure App Service **Application Settings** for production.

---

## 🤝 Contributing

Contributions are welcome! To get started:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-new-feature`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature/my-new-feature`
5. Open a Pull Request

---

## 📄 License

This project is open source. Feel free to fork, learn from, and build on it.

---

<div align="center">
Made with ❤️ using ASP.NET Core 9 Minimal APIs
</div>
