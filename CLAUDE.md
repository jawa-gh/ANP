# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

`sol` is a full-stack solution with two independent projects that are built, run, and versioned separately:

- **`ANPFront/`** — Angular 21 frontend with server-side rendering (SSR). This is its own git repository (`.git` lives here, not at the solution root).
- **`ANPBack/`** — .NET 10 ASP.NET Core Web API (`ANP.API`).

Both are currently the default framework scaffolds (Angular CLI app + ASP.NET `webapi` template). There is no wiring between them yet — the frontend has no HTTP client calling the API, and the API still contains the template `WeatherForecast` model with no controllers. Expect to build out both sides.

## Frontend (`ANPFront/`)

All `npm`/`ng` commands run from inside `ANPFront/`.

```bash
npm start            # ng serve — dev server at http://localhost:4200, hot reload
npm run build        # ng build — production build to dist/
npm run watch        # rebuild on change (development configuration)
npm test             # ng test — runs Vitest
npm run serve:ssr:ANPFront   # run the built SSR server (dist/ANPFront/server/server.mjs)
```

Run a single test file with Vitest's CLI directly:

```bash
npx vitest run src/app/app.spec.ts
npx vitest run -t "test name substring"   # filter by test name
```

### Key architecture facts

- **SSR is the default output**, not optional. `angular.json` sets `outputMode: "server"` with an Express SSR entry at [src/server.ts](ANPFront/src/server.ts). The build produces both a browser bundle and a Node server bundle. Server routes and their render modes are configured in [src/app/app.routes.server.ts](ANPFront/src/app/app.routes.server.ts) (currently prerendering everything). Browser-only APIs (`window`, `document`) must be guarded for the server pass.
- **Standalone components, no NgModules.** App bootstrap and DI providers are in [src/app/app.config.ts](ANPFront/src/app/app.config.ts) (browser) and [src/app/app.config.server.ts](ANPFront/src/app/app.config.server.ts) (server). Client routes live in [src/app/app.routes.ts](ANPFront/src/app/app.routes.ts) (empty — add routes here).
- **Signals-based** state — components use `signal()` (see [src/app/app.ts](ANPFront/src/app/app.ts)). Component selector prefix is `app`.
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

- **Minimal hosting model** — all startup config is in [ANP.API/Program.cs](ANPBack/ANP.API/Program.cs). Uses controller-based routing (`AddControllers()` / `MapControllers()`), so add controllers under `ANP.API/` (no `Controllers/` folder exists yet).
- **OpenAPI** is registered and mapped only in Development (`/openapi`).
- Nullable reference types and implicit usings are both enabled (`ANP.API.csproj`), target framework `net10.0`.
- [ANP.API/ANP.API.http](ANPBack/ANP.API/ANP.API.http) holds REST Client request samples for manual API testing.

## Running the full stack

Start the API and the Angular dev server in separate terminals. When adding frontend-to-backend calls, point the Angular HTTP client at the API's dev URL (`http://localhost:5250` or `https://localhost:7056`) — note `UseHttpsRedirection()` is enabled in `Program.cs`, and there is currently no CORS policy configured, so cross-origin browser calls will need one added.
