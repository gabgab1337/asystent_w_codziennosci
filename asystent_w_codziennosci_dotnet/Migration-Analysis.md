# Migration Analysis & Estimates

Date: 2026-08-13

This document summarizes the code analysis for the existing ASP.NET project and provides migration estimates and reasoning for two target approaches: **.NET MAUI** and **Kotlin Multiplatform**.

---

## 1. Code analysis — what this project includes (notable classes & important pieces)

- Architecture overview
  - ASP.NET MVC web application split into presentation (Razor Views + Controllers), business logic (`AssistantLogic`), and persistence (`AsystenDatabase`).
  - Business logic is well-separated in `AssistantLogic` and is therefore highly reusable for a .NET-based client migration.

- Notable projects / folders
  - `AssistantLogic/` — core business logic, services, ViewModels, triggers, validators.
  - `AsystenDatabase/` — data access layer, repositories and EF-style models (persistence).
  - `Asystent w codziennoÿci/` — web UI project with Controllers and Razor Views (user-facing pages).
  - `AssistantModel/` — an additional MVC project with shared views/layouts.

- Important classes and components (representative)
  - `AssistantLogic.InternalServices.PlanDayService` — builds the daily plan (done vs to-do), composes `PlanDayVM`/`PlanDayPreviewVM`, ties into `ITimeService`, `ITriggerService`, `IWeatherService`, and repositories.
  - `AssistantLogic.InternalServices.TaskService` — CRUD, ordering (Up/Down), conversions between `TaskDM` and `Task*VM`.
  - `AssistantLogic.InternalServices.CurrentActivityService` — creates the current activity view model used in partials.
  - `Asystent_w_codzienności.Controllers.ASDController` — example controller using `PlanDayService` and `CurrentActivityService`, session management and permission checks.
  - `AssistantLogic.ViewModel.*` — ~33 view models that map to Razor views (forms, lists, details).
  - Repositories in `AsystenDatabase.IRepositories` — single source of truth for DB reads/writes.
  - `Triggers` subsystem — a pluggable set of triggers (time, weather, etc.) applied server-side via `ITriggerService`.

- UI surface (counts)
  - Razor views / pages: ~33 (list/detail/add/edit, many simple forms and lists)
  - Controllers: ~8
  - ViewModels: ~33

- Other notable behavior
  - Session-based user model (`SessionManager`) with permissions (`asdPerson` vs `caregiver`).
  - Weather and trigger checking performed server-side (reduces client complexity if backend is reused).
  - Simple page interactions (no heavy client-side JS frameworks observed) — most logic lives server-side.

---

## 2. .NET MAUI — approach, estimates and reasoning

### Recommended approach
- Build a **MAUI client** that consumes the existing backend and reuses as much C# business logic as possible by referencing `AssistantLogic` (project reference or NuGet).
- Implement UI pages in XAML (one page per Razor view), wire ViewModels to XAML via data-binding, and use a lightweight API client for calls that must hit the server.

### Assumptions
- Reuse `AssistantLogic` and only implement UI + small adapters in MAUI.
- Authentication and session state are manageable via token or session endpoints (no complex SSO).
- No heavy offline-first or background native features required.
- Single experienced developer with interactive Claude assistance for scaffolding.

### Estimated hours (single dev, Claude-assisted)
- Client-only (reuse backend + `AssistantLogic`): **60–110 hours**
  - Project setup & wiring: 6–10 h
  - Integrate `AssistantLogic` and services: 12–20 h
  - UI pages (30 pages × avg 2–3 h each): 60–90 h (many pages are simple list/detail/forms)
  - Navigation, session handling, polish & accessibility: 8–12 h
  - QA & builds: 6–10 h

- Standalone MAUI (local DB + migration + sync): **110–180 hours** (extra 30–70h for data layer and sync logic)

### Why this estimate is modest
- Most app logic (plan generation, triggers, time calculations, validators) resides inside `AssistantLogic`, which can be reused directly by a .NET MAUI client, cutting the port work to UI and bindings.
- Views are simple Razor list/detail/forms — straightforward to implement as XAML pages.
- Using Claude to scaffold repetitive XAML/ViewModel wiring typically reduces implementation time by ~25–35%.

### Risks / unknowns
- Custom authentication flow and session management could require additional work.
- Offline sync or platform-specific triggers need extra design and effort.
- Any ASP.NET-specific helpers referenced directly by views must be decoupled.

### Pilot suggestion
- Implement a single representative page: `ASD/PlanDay` (uses `PlanDayService`, displays tasks and points). Estimated pilot time: **6–10 hours**. This validates reuse of `AssistantLogic`, determines realistic per-page velocity, and surfaces integration blockers.

---

## 3. Kotlin Multiplatform (KMP) — approach, estimates and reasoning

### Two reasonable KMP strategies
1. **Client-only KMP** that consumes the existing backend APIs (recommended if you want Kotlin clients but keep the server). Business logic remains server-side; client implements UI and shared Kotlin modules for models + client-side logic.
2. **Full KMP rewrite** (client + backend migration to Kotlin/other JVM tech) — large effort and higher risk.

### Assumptions
- Backend remains available and stable (API endpoints can be used by mobile clients).
- Use KMP for shared models + business logic if desired, but minimal server changes.
- Single experienced Kotlin developer (or full-stack Kotlin dev) using Claude for scaffolding.

### Estimated hours (single dev, Claude-assisted)
- Client-only (use backend APIs): **100–180 hours**
  - Shared Kotlin module + API client: 20–40 h
  - UI implementation (Android + iOS or Compose Multiplatform): 70–120 h (UI for ~30 pages)
  - Integration & QA: 10–20 h

- Full rewrite (including backend): **300–560+ hours** (highly variable — includes server migrations, DB changes, and testing)

### Why KMP is more expensive than MAUI (for this repo)
- The codebase is C#/.NET — MAUI can reuse `AssistantLogic` directly. KMP requires porting or duplicating logic in Kotlin (or shifting logic to the server and keeping thin clients).
- UI must be implemented for each platform or via multiplatform UI (Compose Multiplatform is improving but often still requires platform-specific polish).

### Risks / unknowns
- Porting triggers and time-weather logic from C# to Kotlin could be non-trivial if those modules are complex.
- Integration of KMP with native UI paradigms requires careful design for maintainability.

### Pilot suggestion
- Implement `ASD/PlanDay` as a small KMP client that calls the existing backend. Estimated pilot time: **12–20 hours**.

---

## Final recommendations
- For fastest, lowest-risk migration: build a **.NET MAUI** client that reuses `AssistantLogic` and keeps the current backend. Start with the `ASD/PlanDay` pilot (6–10 hours) to validate reuse and per-page velocity.
- If you are committed to Kotlin for company reasons, use **KMP** but prefer the client-only approach (consume the current backend) to reduce scope and cost; expect ~100–180 hours to reach feature parity for the client.

If you want, I will create a per-page hour estimate table and/or start the MAUI `ASD/PlanDay` prototype now.
