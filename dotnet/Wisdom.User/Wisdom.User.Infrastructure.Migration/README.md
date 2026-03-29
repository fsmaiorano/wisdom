# Wisdom.User.Infrastructure.Migration

This project holds all EF Core migrations for the `UserDbContext`.  
It must be used together with `Wisdom.User.Api` as the startup project.

---

## Prerequisites

```bash
dotnet tool install --global dotnet-ef
```

> Verify with: `dotnet ef --version`

---

## Creating a new migration

```bash
dotnet ef migrations add <MigrationName> \
  --project Wisdom.User.Infrastructure.Migration \
  --startup-project Wisdom.User.Api \
  --namespace Wisdom.User.Infrastructure.Migrations
```

> **Tip:** Use the helper script — it runs automatically from the correct directory
> to avoid build artifacts appearing at the solution root:

```bash
./migrate.sh add AddUserTable
```

---

## Applying migrations to the database

```bash
dotnet ef database update \
  --project Wisdom.User.Infrastructure.Migration \
  --startup-project Wisdom.User.Api
```

Or:

```bash
./migrate.sh update
```

---

## Rolling back to a specific migration

```bash
dotnet ef database update <MigrationName> \
  --project Wisdom.User.Infrastructure.Migration \
  --startup-project Wisdom.User.Api
```

Or:

```bash
./migrate.sh update <MigrationName>
```

---

## Listing existing migrations

```bash
dotnet ef migrations list \
  --project Wisdom.User.Infrastructure.Migration \
  --startup-project Wisdom.User.Api
```

Or:

```bash
./migrate.sh list
```

---

## Removing the last migration (if not yet applied)

```bash
dotnet ef migrations remove \
  --project Wisdom.User.Infrastructure.Migration \
  --startup-project Wisdom.User.Api
```

Or:

```bash
./migrate.sh remove
```

---

## Connection string for design-time

The `DesignTimeDbContextFactory` resolves the connection string in this order:

1. Environment variable: `ConnectionStrings__DefaultConnection`
2. Hardcoded fallback: `Host=localhost;Port=5401;Database=UserDb;Username=postgres;Password=postgres`

To override (e.g. in CI):

```bash
export ConnectionStrings__DefaultConnection="Host=myhost;Port=5432;Database=UserDb;Username=user;Password=pass"
./migrate.sh update
```
