# ANP

A full-stack sample solution pairing an **Angular 22** SSR frontend with a **.NET 10** Web API,
wired together by an **invoice builder** that showcases Angular v22 primitives — Signal Forms,
`httpResource`, signals, `linkedSignal`, and a custom form control — backed by PostgreSQL.

```
sol/
├─ ANPFront/   # Angular 22 frontend (SSR)
└─ ANPBack/    # .NET 10 ASP.NET Core Web API (ANP.API)
```

The two projects are built, run, and versioned independently inside a single git repository.

## Prerequisites

| Tool | Version |
| --- | --- |
| Node.js | `v22.22.3+`, `v24.15.0+`, or `v26.0.0+` (required by Angular 22) |
| .NET SDK | `10.0` |
| PostgreSQL | any recent version, reachable on `localhost:5432` |

## Quick start

### 1. Backend (`ANPBack/`)

The connection string lives in **user secrets** (it carries a password), not `appsettings`. Set it once:

```bash
cd ANPBack/ANP.API
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=anpdb;Username=postgres;Password=YOUR_PASSWORD"
```

Then run the API:

```bash
cd ANPBack
dotnet run --project ANP.API        # http://localhost:5250
```

On first start in Development the database (`anpdb`) and schema are created automatically
(`EnsureCreated`) and seeded with sample products and an invoice. There are no EF migrations —
if you change entities, drop `anpdb` and let it recreate, or switch to migrations.

### 2. Frontend (`ANPFront/`)

```bash
cd ANPFront
npm install
npm start                           # http://localhost:4200
```

The app calls the API at `http://localhost:5250/api` (the `API_BASE` injection token in
[`invoice.api.ts`](ANPFront/src/app/invoices/invoice.api.ts)). CORS for the dev origins is already
configured in the API.

## The invoice module

A full-stack invoice builder used as the Angular v22 feature showcase:

- **List page** — loaded with `httpResource()` whose URL is derived from a status-filter `signal`,
  so changing the filter refetches; counts/totals via `computed()`.
- **Editor** — **Signal Forms** (`form(model, schema)`): a dynamic line-item array (`applyEach`),
  sync validators, conditional validation, async product-code checks against the API
  (`validateHttp`), a cross-field credit-limit rule (`validateTree`), preset-driven-but-overridable
  tax via `linkedSignal`, `computed` totals, and `submit()`.
- **Custom control** — a money input implementing `FormValueControl<number>` with `model()` +
  `transformedValue()`.
- **Backend** — `Invoice` → `LineItem` (cascade) + a `Product` catalog, exposed by
  `InvoicesController` (CRUD) and `ProductsController` (code lookup for the async validator).
  See [`ANP.API.http`](ANPBack/ANP.API/ANP.API.http) for ready-to-run request samples.

## Common commands

**Frontend** (run inside `ANPFront/`):

```bash
npm start            # dev server with hot reload
npm run build        # production build (browser + SSR server bundles)
npm test             # unit tests (Vitest via the Angular builder)
```

**Backend** (run inside `ANPBack/`):

```bash
dotnet build
dotnet run --project ANP.API                 # http profile  -> http://localhost:5250
dotnet watch --project ANP.API run           # hot reload
```

## Documentation

[`CLAUDE.md`](CLAUDE.md) contains the deeper architecture notes and gotchas (SSR render modes,
Signal Forms `[formField]` usage, EF Core/seed strategy, etc.).
