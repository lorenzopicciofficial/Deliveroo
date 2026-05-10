# DeliverooApp

A web application inspired by Deliveroo, built with ASP.NET Core MVC. Simulates a complete food ordering platform with a customer-facing catalog and a full admin panel.

School project — 5th year, Computer Science.

---

## Tech stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10.0) |
| Language | C# |
| Database | MySQL 9.x |
| Password hashing | BCrypt.Net-Next 4.1.0 |
| Frontend | Bootstrap 5, jQuery, Vanilla JS |

---

## Features

**Customer**
- Browse and search the product catalog with category tabs
- Add to cart (session-based for anonymous users, DB-based after login)
- Checkout with credit card or cash on delivery
- Order tracking with estimated delivery time
- 1–5 star reviews (only for purchased products)
- Save favourite items
- User profile with saved payment defaults
- Password reset via security question
- Dark mode

**Admin**
- Full CRUD on products, users, discounts, and product associations
- Order status management
- Opening hours and closure message settings
- Analytics dashboard (revenue, top products, orders per day)

---

## Project structure

```
DELIVEROO_repo/
├── DeliverooApp/          # ASP.NET Core MVC application
│   ├── Controllers/
│   ├── Filters/
│   ├── Models/
│   │   ├── Domain/        # Domain model classes
│   │   └── Data/          # Data access layer (GestioneDati, GestioneCarrello)
│   ├── Views/
│   ├── wwwroot/
│   └── Program.cs
├── database/              # SQL scripts (schema only)
├── docs/
│   ├── PRD.md             # Product requirements
│   ├── ER.md              # Entity-relationship diagram
│   └── ARCHITECTURE.md    # Architecture overview
└── DeliverooApp.sln
```

---

## Setup

### 1. Install prerequisites

#### .NET 10.0 SDK
Download from [dot.net](https://dotnet.microsoft.com/download/dotnet/10.0) and follow the installer for your OS.

Verify:
```bash
dotnet --version   # should print 10.x.x
```

#### MySQL

**macOS (Homebrew)**
```bash
brew install mysql
brew services start mysql
mysql_secure_installation   # optional but recommended
```

**Windows**
Download and run the [MySQL Installer](https://dev.mysql.com/downloads/installer/). Choose "Server only" or "Custom" and install MySQL Server 8.x or 9.x.

---

### 2. Create the database

Log into MySQL as root (or another user with CREATE privileges):
```bash
mysql -u root -p
```

Then run:
```sql
CREATE DATABASE deliveroo CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'deliveroo_user'@'localhost' IDENTIFIED BY 'your_password';
GRANT ALL PRIVILEGES ON deliveroo.* TO 'deliveroo_user'@'localhost';
FLUSH PRIVILEGES;
EXIT;
```

---

### 3. Create the schema

```bash
mysql -u deliveroo_user -p deliveroo < database/schema.sql
```

This creates all 11 tables and inserts a default row in `impostazioni` (opening hours).
The database starts empty — no users or products are included. Register your first user through the app, then promote them to admin directly in the database if needed.

---

### 4. Configure the app

Copy the example config and fill in your values:
```bash
cp DeliverooApp/appsettings.Example.json DeliverooApp/appsettings.Development.json
```

Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Default": "database=deliveroo;host=localhost;port=3306;user=deliveroo_user;password=your_password"
  },
  "AdminCredentials": {
    "Username": "admin",
    "Password": "$2b$12$..."
  }
}
```

This file is gitignored and never committed — keep your credentials here only.

The admin password must be a **BCrypt hash** (work factor 12). To generate one, use any BCrypt tool — for example [bcrypt-generator.com](https://bcrypt-generator.com) — and paste the resulting `$2b$12$...` string as the value.

---

### 5. Run

```bash
cd DeliverooApp
dotnet run
```

The app starts on `http://localhost:5213` by default.

---

## Documentation

- [PRD](docs/PRD.md) — Full product requirements and feature specification
- [ER Diagram](docs/ER.md) — Database entity-relationship diagram (Mermaid)
- [Architecture](docs/ARCHITECTURE.md) — Request lifecycle, DAL design, auth flow
