# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

`sol` is a full-stack solution with two independent projects that are built, run, and versioned separately:

- **`ANPFront/`** — Angular 22 frontend with server-side rendering (SSR).
- **`ANPBack/`** — .NET 10 ASP.NET Core Web API (`ANP.API`).

The whole solution is a single git repository rooted at `sol/`.

The two sides are wired together by an **invoice module** that doubles as an Angular v22 feature showcase (Signal Forms, `httpResource`, signals). The frontend calls the API over HTTP; the API persists to PostgreSQL via EF Core. See [Invoice module](#invoice-module-the-main-feature) below for the full picture.

## Frontend (`ANPFront/`)

All `npm`/`ng` commands run from inside `ANPFront/`. **Angular 22 requires Node `v22.22.3+`, `v24.15.0+`, or `v26.0.0+`** — older Node fails the CLI's version check.

```bash
npm start            # ng serve — dev server at http://localhost:4200, hot reload
npm run build        # ng build — production build to dist/
npm run watch        # rebuild on change (development configuration)
npm test             # ng test — runs Vitest via the Angular builder
npm run serve:ssr:ANPFront   # run the built SSR server (dist/ANPFront/server/server.mjs)
```

Tests run through the `@angular/build:unit-test` builder, which supplies the Vitest globals (`describe`, `beforeEach`, etc.) and the Angular test environment. **Do not call `vitest` directly** — globals will be undefined and every suite fails with `describe is not defined`. Run a subset via the builder instead:

```bash
ng test --no-watch --include src/app/app.spec.ts   # a single spec file
ng test --no-watch --filter "^App"                  # filter by suite/test name (regex)
```

### Key architecture facts

- **SSR is the default output**, not optional. `angular.json` sets `outputMode: "server"` with an Express SSR entry at [src/server.ts](ANPFront/src/server.ts). The build produces both a browser bundle and a Node server bundle. Render modes are per-route in [src/app/app.routes.server.ts](ANPFront/src/app/app.routes.server.ts): static routes are **prerendered** (`**`), but the API-backed invoice routes are forced to **`RenderMode.Client`** so prerendering doesn't try to reach the backend at build time. Add data-driven routes the same way. Browser-only APIs (`window`, `document`) must be guarded for the server pass.
- **Standalone components, no NgModules.** App bootstrap and DI providers are in [src/app/app.config.ts](ANPFront/src/app/app.config.ts) (browser) and [src/app/app.config.server.ts](ANPFront/src/app/app.config.server.ts) (server). `provideHttpClient(withFetch())` is registered there. Routes are in [src/app/app.routes.ts](ANPFront/src/app/app.routes.ts) (`/invoices` list + `/invoices/new` editor, lazy-loaded).
- **Signals-based** state — components use `signal()`, `computed()`, `linkedSignal()`, and `httpResource()` (see the invoice module). Component selector prefix is `app`. Components default to `ChangeDetectionStrategy.OnPush` except the root (which is `Eager`, see below).
- **v22 upgrade opt-outs** left by the `ng update` migration: `changeDetection: ChangeDetectionStrategy.Eager` on the root component and `withNoIncrementalHydration()` in `app.config.ts` preserve pre-v22 behavior. Remove them deliberately when adopting the new defaults (zoneless/eager CD and incremental hydration), not by accident.
- **Styling is Tailwind CSS v4** via PostCSS (`.postcssrc.json` registers `@tailwindcss/postcss`); global stylesheet is `src/styles.css`.
- **Formatting:** Prettier with `singleQuote: true`, `printWidth: 100`, and the Angular parser for HTML templates. Indentation is 2 spaces (`.editorconfig`). Use single quotes in TypeScript.

## Backend (`ANPBack/`)

Run from inside `ANPBack/` (the solution file is `ANPBack.slnx`).

```bash
dotnet build
dotnet run --project ANP.API              # default (http) profile -> http://localhost:5250
dotnet run --project ANP.API --launch-profile https   # https://localhost:7056 + http://localhost:5250
dotnet watch --project ANP.API run        # hot reload
dotnet test                               # no test project exists yet
```

### Key architecture facts

- **Minimal hosting model** — all startup config is in [ANP.API/Program.cs](ANPBack/ANP.API/Program.cs). Controller-based routing (`AddControllers()` / `MapControllers()`); controllers live in `ANP.API/Controllers/`.
- **EF Core + PostgreSQL** via `Npgsql.EntityFrameworkCore.PostgreSQL`. The `AppDbContext` ([ANP.API/Data/AppDbContext.cs](ANPBack/ANP.API/Data/AppDbContext.cs)) is registered against the `Default` connection string. **The connection string is in user secrets, not `appsettings`** (it carries a password). The csproj has a `<UserSecretsId>`; set the value with:
  ```bash
  dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=anpdb;Username=postgres;Password=..."
  ```
  `Program.cs` throws a clear error at startup if it's missing. There are **no migrations** — in Development, `Program.cs` calls `Database.EnsureCreatedAsync()` and then `DbSeeder` ([ANP.API/Data/DbSeeder.cs](ANPBack/ANP.API/Data/DbSeeder.cs)) seeds sample products + an invoice. If you change entities, drop the `anpdb` database (EnsureCreated doesn't migrate) or switch to real migrations.
- **CORS** policy `angular-dev` allows the Angular origins (`http://localhost:4200` and the SSR server `:4000`). `UseHttpsRedirection()` is on, but the http-only dev profile has no https port so it doesn't actually redirect — browser calls to `http://localhost:5250` work.
- Entities are mapped to DTOs in [ANP.API/Dtos/InvoiceDtos.cs](ANPBack/ANP.API/Dtos/InvoiceDtos.cs) (the API never serializes entities directly; server-computed totals live on the read DTO).
- **OpenAPI** is registered and mapped only in Development (`/openapi`).
- Nullable reference types and implicit usings are both enabled (`ANP.API.csproj`), target framework `net10.0`.
- [ANP.API/ANP.API.http](ANPBack/ANP.API/ANP.API.http) holds REST Client request samples for manual API testing.

## Invoice module (the main feature)

A full-stack invoice builder that showcases Angular v22 primitives end to end.

- **Backend**: `Invoice` → `LineItem` (cascade) plus a `Product` catalog, exposed by `InvoicesController` (CRUD) and `ProductsController` (`GET /api/products/{code}` returns 200/404 — used by the frontend async validator). All under [ANP.API/](ANPBack/ANP.API).
- **Frontend**: everything under [src/app/invoices/](ANPFront/src/app/invoices):
  - [invoice-list.ts](ANPFront/src/app/invoices/invoice-list.ts) — `httpResource()` whose URL is derived from a `status` filter `signal`, so setting the signal refetches. `computed()` for counts/totals.
  - [invoice-editor.ts](ANPFront/src/app/invoices/invoice-editor.ts) — **Signal Forms**: `form(model, schema)` with `applyEach` (line-item array), sync validators (`required`/`min`/`minLength`), conditional `required` via `when`, `validateHttp` (async product-code check), and `validateTree` (cross-field credit-limit rule). Tax uses `linkedSignal` (preset-driven, user-overridable); totals are `computed`; saving goes through `submit()`.
  - [money-input.ts](ANPFront/src/app/invoices/money-input.ts) — a custom control implementing `FormValueControl<number>` with `model()` + `transformedValue()` (string UI ⇄ numeric model with parse errors).
  - [invoice.api.ts](ANPFront/src/app/invoices/invoice.api.ts) — `HttpClient` wrapper + the `API_BASE` injection token (defaults to `http://localhost:5250/api`).
  - [invoice.models.ts](ANPFront/src/app/invoices/invoice.models.ts) — shared TS types, each annotated as the mirror of a backend DTO (`InvoiceForm`↔`InvoiceWriteDto`, `InvoiceRead`↔`InvoiceReadDto`, `InvoiceSummary`↔`InvoiceSummaryDto`, `LineItem`↔`LineItemDto`), plus the `InvoiceStatus` union + `INVOICE_STATUSES` constant and the `emptyLineItem()`/`emptyInvoiceForm()` factories. Keep these in sync when you change the API DTOs.

### Signal Forms gotchas (v22, experimental — `@angular/forms/signals`)

- **The control directive selector is `[formField]`** (bind `[formField]="someField"`), and the directive class is `FormField`. Older docs/snippets show `[control]` / a `Control` directive — that is **not** what ships in 22.0.x. Import `FormField` from `@angular/forms/signals` into the component's `imports`.
- **Don't put native `min`/`max`/`minlength`/`maxlength`/`pattern` attributes on an element that also has `[formField]`** — the validators (`min(...)`, `maxLength(...)`, etc.) set those properties on the field, and a static attribute collides: `NG8022: Setting the 'min' attribute is not allowed on nodes using the '[formField]' directive`. Express the constraint as a validator in the schema instead.
- A `FieldTree` is callable: `someField()` returns the `FieldState`, so templates read `someField().value()`, `.errors()`, `.touched()`, `.pending()`, `.valid()`. Array fields are iterable — `@for (item of form.lineItems; track $index)` yields child field trees.
- Custom errors are plain objects (`{ kind, message }`); `validateHttp` maps a 200 via `onSuccess` and non-2xx/network failures via `onError`.

## Running the full stack

1. Ensure PostgreSQL is running and set the `Default` connection string in user secrets (see Backend facts above) — db `anpdb` is created automatically on first run.
2. `dotnet run --project ANP.API` (→ `http://localhost:5250`).
3. `npm start` in `ANPFront/` (→ `http://localhost:4200`).

The Angular HTTP base URL is the `API_BASE` token in [invoice.api.ts](ANPFront/src/app/invoices/invoice.api.ts); override it via DI rather than hard-coding a new URL. CORS for the dev origins is already configured in `Program.cs`.
