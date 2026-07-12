# Enhanzer Purchase Bill Assignment

Two-page Angular and ASP.NET Core application for authenticated purchase bill entry.

## Architecture Decisions

- **PrimeNG over custom UI primitives:** PrimeNG provides accessible, tested form controls, tables, toast messages, dropdowns, and loading states. Tailwind CSS is layered on top for spacing, responsive layout, and the modern SaaS visual style instead of rebuilding controls from scratch.
- **Standalone Angular features:** Login and Purchase Bill are lazy-loaded standalone components. Core concerns such as guards, interceptors, models, and API services live under `frontend/src/app/core`.
- **Signals for local UI state:** Purchase rows, summaries, loading state, and calculated previews use Angular Signals so the UI updates predictably when rows are added or removed.
- **Backend API boundary:** The Angular app only sends `{ email, password }` to the local backend. The backend owns the external POS request shape, JWT creation, SQL persistence, and error handling.
- **Repository-based persistence:** Location sync and purchase bill save logic are isolated behind repositories to keep controllers thin and database access focused.
- **Server-side recalculation:** Purchase bill totals are recalculated by the API before saving so persisted values do not rely on client-side totals.
- **Serilog logging:** ASP.NET Core logging is configured through Serilog for structured console logs and cleaner production-ready logging configuration.

## Tech Stack

- Angular standalone components, Reactive Forms, Signals, RxJS
- PrimeNG, PrimeIcons, Tailwind CSS
- ASP.NET Core 8 Web API
- Entity Framework Core with SQL Server
- JWT bearer authentication
- Serilog

## Structure

```text
backend/Enhanzer.Api
  Controllers/      Auth, location, and purchase bill endpoints
  Data/             EF Core database context
  DTOs/             API contracts
  Entities/         Database entities
  Helpers/          JWT options
  Middleware/       Central API error handling
  Models/           External API payload models
  Repositories/     SQL persistence
  Services/         Authentication and external POS integration

frontend/src/app
  core/             Guards, interceptors, models, services
  features/auth     Login component and template
  features/purchase Purchase bill component and template
  layout/           Shared application shell
  shared/           Reusable validation helpers
```

## Database Setup

1. Start SQL Server.
2. Run `database.sql` against the server.
3. Update `backend/Enhanzer.Api/appsettings.json` if your SQL Server connection differs.

The script creates:

- `Location_Details` for synced login locations.
- `Purchase_Bills` for saved bill headers.
- `Purchase_Bill_Items` for saved bill line items.

## Backend Setup

```bash
cd backend/Enhanzer.Api
dotnet restore
dotnet run
```

The API exposes:

- `POST /api/auth/login`
- `GET /api/locations`
- `POST /api/purchase-bills`

Login calls the Enhanzer staging POS API, persists returned `User_Locations`, generates a JWT, and returns saved locations to the client.

## Frontend Setup

```bash
cd frontend
npm install
npm start
```

The Angular app runs on `http://localhost:4200` and uses `http://localhost:5092/api` in development.

## Implemented Requirements

- Login validates email and password, displays API errors, and protects the purchase route with a guard.
- Backend sends the external login request using `Company_Code`, `Username`, and `Pw` from user input.
- Returned `User_Locations` are saved to SQL Server and used as Purchase Bill batches.
- Purchase Bill supports item autocomplete, batch dropdown, numeric validation, calculated totals, add/remove rows, live summary, and save-to-database.
- The page is responsive with mobile-friendly form stacking and horizontally scrollable data table.
- JWT is stored in `sessionStorage` and attached to protected API calls through an HTTP interceptor.
