# Architecture — DeliverooApp

## 1. Overview

DeliverooApp is a server-rendered MVC web application built on ASP.NET Core (.NET 10.0). It follows a classic three-layer structure: controller, data access layer, and MySQL database, with Razor views for HTML generation.

There is no separate API layer — all communication between client and server happens through form submissions and MVC actions. The only exception is a handful of JSON endpoints used by JavaScript for dynamic cart updates.

---

## 2. Repository structure

```
DELIVEROO_repo/
├── DeliverooApp/          # ASP.NET Core MVC project
├── database/              # SQL migration scripts and demo seed
├── docs/                  # Documentation
│   ├── PRD.md
│   ├── ER.md
│   └── ARCHITECTURE.md
├── DeliverooApp.sln       # Solution file
└── .gitignore
```

---

## 3. Project structure

```
DeliverooApp/
├── Controllers/
│   └── HomeController.cs          # Single controller (~60 actions)
├── Filters/
│   └── OnlyAdminAttribute.cs      # Admin authorization filter
├── Models/
│   ├── Domain/                    # Pure domain classes (POCO)
│   │   ├── Articolo.cs
│   │   ├── Associazione.cs
│   │   ├── Carta.cs
│   │   ├── Orari.cs
│   │   ├── Ordine.cs              # includes Note property
│   │   ├── Recensione.cs
│   │   ├── RigaDettaglio.cs
│   │   ├── Sconto.cs
│   │   └── Utente.cs              # includes DomandaSicurezza, RispostaSicurezza
│   ├── Data/                      # Data access layer
│   │   ├── GestioneDati.cs        # All MySQL queries (parametrized)
│   │   └── GestioneCarrello.cs    # Session-based cart (anonymous users)
│   └── ErrorViewModel.cs
├── Views/
│   ├── Home/                      # 19 Razor views (customer + admin)
│   └── Shared/
│       ├── _Layout.cshtml         # Customer layout
│       ├── _LayoutAdmin.cshtml    # Admin layout (sidebar)
│       └── ...
├── wwwroot/
│   ├── css/site.css
│   ├── js/site.js
│   ├── img/
│   └── lib/                       # Bootstrap, jQuery
├── Properties/
│   └── launchSettings.json        # Dev server config (port 5213)
├── Program.cs                     # App startup and DI
├── appsettings.Example.json       # Config template (no credentials)
└── DeliverooApp.csproj
```

All model classes share the namespace `DeliverooApp.Models` regardless of which subfolder they live in. The `Domain/` and `Data/` folders are a physical organization convention only.

---

## 4. Request lifecycle

```
Browser
  │
  ▼ HTTP request
HomeController (action method)
  │
  ├── reads Session (cart, logged-in user)
  │
  ├── instantiates GestioneDati (IDisposable, opens MySQL connection)
  │   └── executes parametrized query → returns POCO / scalar
  │
  ├── puts data in ViewBag or typed model
  │
  └── returns View(model) → Razor template → HTML response to browser
```

For cart operations the controller also calls `GestioneCarrello` (session-based) or `GestioneDati` DB methods depending on whether the user is authenticated.

---

## 5. Authentication and authorization

### User login
- Credentials stored in `utente` table; password hashed with BCrypt (workFactor 11)
- On successful login, `idUtente`, `username`, and `ruolo` are written to `HttpContext.Session`
- Session timeout: 30 minutes of inactivity

### Admin login
- Separate login flow; credentials (username + BCrypt hash) stored in `appsettings.json` under `AdminCredentials`
- On success, `isAdmin = true` is written to the session

### Authorization filter
`OnlyAdminAttribute` (implements `IActionFilter`) reads `isAdmin` from the session before every admin POST action. Non-admin requests are redirected to `/Home/Login`.

---

## 6. Data access layer (GestioneDati)

`GestioneDati` is a single class (~1000 lines) that encapsulates all database interaction.

- Implements `IDisposable`; instantiated per-request in the controller
- Opens a `MySqlConnection` on construction, closes it on `Dispose()`
- Every method uses `MySqlCommand` with named parameters (`@paramName`) — no string interpolation in SQL
- Returns POCO objects or scalar values; no ORM

Categories of methods:

| Category | Examples |
|---|---|
| Users | `RecuperaUtenteConNome`, `InserisciUtente`, `AggiornaRuoloUtente` |
| Password reset | `RecuperaDomandaSicurezza`, `VerificaRispostaSicurezza`, `AggiornaPassword` |
| Startup migrations | `MigraColonnaNote`, `MigraUtentiSenzaRisposta` |
| Products | `RecuperaTuttiGliArticoli`, `ModificaArticolo`, `EliminaArticolo` |
| Orders | `InserisciOrdineERestituisciId`, `AggiornaStatoOrdine` |
| Cart (DB) | `AggiungiAlCarrelloDB`, `RimuoviUnoCarrelloDB`, `SvuotaCarrelloDB` |
| Favorites | `TogglePreferito`, `RecuperaPreferiti`, `RecuperaArticoliPreferiti` |
| Associations | `AggiornaOInserisciAssociazione`, `RecuperaAssociazioniByArticolo` |
| Discounts | `RecuperaScontoAttivoByArticolo`, `RecuperaScontiAttiviPerArticoli` |
| Reviews | `SalvaRecensione`, `MediaVotiArticolo`, `HaOrdinatoArticolo` |
| Analytics | `IncassoTotale`, `IncassoPerCategoria`, `OrdiniPerGiorno`, `Top5Articoli` |
| Settings | `RecuperaOrari`, `AggiornaOrari` |

---

## 7. Cart management

The cart uses dual persistence depending on authentication state:

| State | Storage | Class |
|---|---|---|
| Anonymous user | `HttpContext.Session` (JSON-serialized `Dictionary<int, int>`) | `GestioneCarrello` |
| Logged-in user | `carrello` table in MySQL | `GestioneDati` (DB methods) |

On login, the session cart is merged into the database cart. On logout, the session is cleared.

---

## 8. Layouts

Two separate Razor layouts:

| Layout | Used by | Key features |
|---|---|---|
| `_Layout.cshtml` | Customer pages | Navbar, cart icon with badge, login/profile link |
| `_LayoutAdmin.cshtml` | Admin pages | Sidebar navigation, admin-only links |

The correct layout is selected in each view's `@{ Layout = "..." }` directive, or inherited from `_ViewStart.cshtml`.

---

## 9. Static assets

Served from `wwwroot/` via `MapStaticAssets()` (ASP.NET Core static file middleware).

| Path | Content |
|---|---|
| `wwwroot/css/site.css` | Custom styles (Syne font for headings, DM Sans for body) |
| `wwwroot/js/site.js` | Vanilla JS (cart modal, form interactions) |
| `wwwroot/img/` | Product images |
| `wwwroot/lib/` | Bootstrap 5, jQuery 3.x, jQuery Validation |

CSS isolation bundles (`DeliverooApp.styles.css`) are auto-generated per `.cshtml` component.

---

## 10. Configuration

`appsettings.json` (excluded from git) holds:

```json
{
  "ConnectionStrings": {
    "Default": "database=...;host=localhost;port=3306;user=...;password=..."
  },
  "AdminCredentials": {
    "Username": "...",
    "Password": "..."
  }
}
```

`appsettings.Example.json` is the committed template with placeholder values. `appsettings.Development.json` is also excluded from git.

---

## 11. Startup (Program.cs)

```
Culture        → en-US (decimal separators, date formatting)
Services       → AddControllersWithViews, AddHttpContextAccessor, AddSession
Middleware     → UseHttpsRedirection, UseStatusCodePagesWithReExecute,
                 UseRouting, UseSession, UseAuthorization, MapStaticAssets
Route          → {controller=Home}/{action=Index}/{id?}
```

---

## 12. Key design decisions

| Decision | Reason |
|---|---|
| Single controller | School-project scope; all routes in one place simplifies navigation |
| Manual DAL (no ORM) | Explicit SQL gives full control and is educational for understanding database interaction |
| Dual cart persistence | Allows anonymous browsing; cart survives login |
| BCrypt for passwords | Industry-standard; avoids plain-text or MD5 storage |
| Session-based auth | Simpler than JWT for a server-rendered app with no mobile client |
| Price snapshot in `riga_dettaglio.prezzo` | Decouples order history from future price changes |
| `associazioni` table | Enables product recommendations without an ML model — co-purchase frequency is sufficient for this domain |
