# EF Core Interceptors Example

![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet?logo=dotnet)
![License](https://img.shields.io/github/license/ahedfi/efcore-interceptor?color=green)

A .NET 10 Web API demonstrating EF Core `SaveChangesInterceptor` patterns in a clean architecture with minimal API, repository + unit of work pattern, and PostgreSQL.

## Overview

This repo was created to illustrate the examples from the blog post [EF Core Interceptors](https://ahedfi.github.io/ef-core-interceptors). It implements two interceptor patterns:

1. **Auditing Interceptor**: Automatically stamps `CreatedDate` and `ModifiedDate` on entities implementing `IAuditable` during Add/Modify operations.
2. **Soft-Delete Interceptor**: Converts physical deletions into logical updates (`IsDeleted = true`) for entities implementing `ISoftDelete`, backed by global query filters to hide them from normal queries.

## Architecture

```
src/
├── Domain/                           # Core entities, interfaces, and abstractions
│   ├── Common/IAuditable.cs          # Auditable interface (CreatedDate, ModifiedDate)
│   ├── Common/ISoftDelete.cs         # Soft-delete interface (IsDeleted)
│   ├── Entities/Product.cs           # Auditable entity
│   ├── Entities/Category.cs          # Soft-delete entity
│   └── Repositories/                 # Repository + UoW interfaces
├── Application/                      # Business logic and services
│   ├── Products/ProductService.cs    # Product CRUD service
│   ├── Categories/CategoryService.cs # Category CRUD service (includes soft-delete view)
│   └── DependencyInjection.cs        # DI registration
├── Infrastructure/                   # EF Core, repositories, interceptors
│   ├── Interceptors/
│   │   ├── AuditInterceptor.cs       # Auditing logic
│   │   └── SoftDeleteInterceptor.cs  # Soft-delete logic
│   ├── Persistence/AppDbContext.cs   # DbContext
│   ├── Configurations/               # Entity configurations with query filters
│   ├── Repositories/                 # Generic Repository<T>, UnitOfWork
│   ├── Migrations/                   # EF Core migrations (auto-generated)
│   └── DependencyInjection.cs        # DI registration
└── Api/                              # Minimal API endpoints
    ├── Program.cs                    # Entry point, middleware config, auto-migrations
    ├── Endpoints/                    # Endpoint definitions
    ├── efcore-interceptor.http       # REST client test file
    └── appsettings.Development.json  # Postgres connection string
```

## Prerequisites

- .NET 10 SDK (installed via `dotnet --version`)
- Docker & Docker Compose (for PostgreSQL)
- REST Client (VS Code extension, Visual Studio, or Postman)

## Getting Started

### Option A: Quick start script (Windows)

```cmd
start.cmd
```

This starts PostgreSQL via Docker Compose and runs the API in one step.

### Option B: Manual steps

#### 1. Start PostgreSQL

```bash
docker compose up -d
# Verify: docker ps | grep efcore
```

#### 2. Run the API (auto-applies migrations in Development)

```bash
dotnet run --project src/Api
# Server listens on http://localhost:5080
```

### 3. Test via REST Client

Open `src/Api/efcore-interceptor.http` in VS Code or Visual Studio and execute requests in order:

1. **Create Category** (Electronics)
2. **Create Product** (Laptop) → observe `createdDate == modifiedDate` (audit on Add)
3. **Update Product** → observe `modifiedDate` advances later than `createdDate` (audit on Modify)
4. **Soft-Delete Category** → soft-delete interceptor triggers
5. **List Categories** (default) → Electronics hidden by query filter
6. **List Categories** (`/all` with `IgnoreQueryFilters()`) → Electronics still visible with `isDeleted: true`
7. **Hard-Delete Product** → Product fully removed (no soft-delete behavior)
8. **List Products** → Laptop gone (contrast with soft-delete)

## Key Implementation Details

### Auditing Interceptor (`AuditInterceptor.cs`)

Overrides `SaveChanges` and `SaveChangesAsync`:
- On `EntityState.Added`: sets both `CreatedDate` and `ModifiedDate` to `DateTime.UtcNow`
- On `EntityState.Modified`: updates only `ModifiedDate`

```csharp
foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
{
    if (entry.State == EntityState.Added)
    {
        entry.Entity.CreatedDate = now;
        entry.Entity.ModifiedDate = now;
    }
    else if (entry.State == EntityState.Modified)
    {
        entry.Entity.ModifiedDate = now;
    }
}
```

### Soft-Delete Interceptor (`SoftDeleteInterceptor.cs`)

Overrides `SaveChanges` and `SaveChangesAsync`:
- On `EntityState.Deleted`: changes to `EntityState.Modified` and sets `IsDeleted = true`
- Combined with `builder.HasQueryFilter(c => !c.IsDeleted)` in `CategoryConfiguration`

```csharp
foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
{
    if (entry.State == EntityState.Deleted)
    {
        entry.State = EntityState.Modified;
        entry.Entity.IsDeleted = true;
    }
}
```

### Global Query Filter

Applied in `CategoryConfiguration`:

```csharp
builder.HasQueryFilter(c => !c.IsDeleted);
```

This filter:
- Applies automatically to `GetAllAsync()` queries
- Includes `Include()` navigations
- Can be bypassed with `.IgnoreQueryFilters()` (used in `GetAllIncludingDeletedAsync()`)

### Known Limitations

**ExecuteDelete/ExecuteUpdate bulk operations bypass SaveChanges interceptors entirely.**  
Both interceptors only work with normal `SaveChanges`-based operations. If using bulk delete/update via LINQ queries:

```csharp
// ❌ Bypasses soft-delete interceptor
await context.Categories.Where(...).ExecuteDeleteAsync();

// ✅ Triggers soft-delete interceptor
var categories = await context.Categories.Where(...).ToListAsync();
context.RemoveRange(categories);
await context.SaveChangesAsync();
```

This is an EF Core design decision and is noted in both interceptor files.

## Project Structure Notes

- **Domain layer** defines entities (Product, Category), interfaces (IAuditable, ISoftDelete), and repository/UoW contracts.
- **Application layer** implements services (ProductService, CategoryService) with DTOs and manual mapping — no MediatR for simplicity.
- **Infrastructure layer** contains EF Core configuration, interceptors, repository implementations, and migrations.
- **Api layer** wires up minimal API endpoints, DI, and auto-applies migrations on startup in Development.

## Unit of Work Pattern

The generic `IUnitOfWork.Repository<T>()` accessor keeps the UoW interface stable as entities are added:

```csharp
var productRepo = unitOfWork.Repository<Product>();
await productRepo.AddAsync(newProduct, ct);
await unitOfWork.SaveChangesAsync(ct);
```

## Package Versions

- `Microsoft.EntityFrameworkCore` 10.0.10
- `Microsoft.EntityFrameworkCore.Design` 10.0.10
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3
- `dotnet-ef` (global tool) 10.0.10

## Cleanup

```bash
# Stop API: Ctrl+C in terminal
# Stop PostgreSQL
docker compose down

# Remove volume (deletes DB data)
docker volume rm efcore-interceptor-data
```

## References

- [EF Core Interceptors (Blog Post)](https://ahedfi.github.io/ef-core-interceptors)
- [SaveChangesInterceptor (Microsoft Docs)](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
- [Global Query Filters (Microsoft Docs)](https://learn.microsoft.com/en-us/ef/core/querying/filters)
- [Minimal APIs (.NET 10)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)