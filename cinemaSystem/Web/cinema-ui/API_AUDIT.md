# Cinema UI API Audit

Audit date: 2026-03-06

## Scope
- Backend inspected: `D:\DACS4\cinemaSystem\Api`
- Frontend inspected: `D:\DACS4\cinemaSystem\Web\cinema-ui`
- Backend source of truth used for endpoint inventory: `Api/obj/Debug/net8.0/ApiEndpoints.json`

## Backend Inventory Summary
- Total controller endpoints: 134
- Total controllers: 35

Main route groups:
- `api/auth`
- `api/identity`
- `api/profile`
- `api/movies`, `api/showtimes`, `api/cinemas`, `api/bookings`
- `api/admin/*`
- `api/manager/*`
- `api/pos/*`
- `api/promotions`, `api/pricingtiers`, `api/timeslots`, `api/genres`, `api/seattypes`

## UI Endpoint Contract File
- `Web/cinema-ui/src/shared/api/endpoints.ts`

## Critical UI/API Mismatches

1. Duplicate key in endpoint config (build blocker)
- File: `src/shared/api/endpoints.ts`
- Issue: `MANAGER_BOOKINGS` is declared twice in the same object.
- Impact: TypeScript build error `TS1117`.

2. Admin bookings page uses endpoint factory as string (build/runtime blocker)
- File: `src/pages/admin/bookings.tsx`
- Code currently calls `api.get(Endpoints.MANAGER_BOOKINGS.BASE)`.
- But `BASE` is a function requiring `cinemaId`.
- Impact: Type error + broken request URL.

3. Admin showtimes page requests wrong manager endpoint
- File: `src/pages/admin/showtimes.tsx`
- Code currently calls `api.get(Endpoints.MANAGER_SHOWTIMES.BASE)`.
- Backend GET route is `GET /api/manager/showtimes/{cinemaId}` (cinemaId required), not base path.

4. Admin movies list uses missing backend endpoint
- File: `src/features/admin/movies/api/adminMoviesApi.ts`
- Current call: `GET /api/admin/movies`.
- Backend has no `GET /api/admin/movies`; only `POST`, `PUT {id}`, `DELETE {id}`.

5. Admin cinemas list uses missing backend endpoint
- File: `src/pages/admin/cinemas.tsx`
- Current call: `GET /api/admin/cinemas`.
- Backend has no list/get endpoint at this path; only create/update/delete plus screen/seat operations.

6. Public cinemas detail endpoint expected by UI does not exist
- File: `src/features/cinemas/api/cinemasApi.ts`
- Current call: `GET /api/cinemas/{id}`.
- Backend `CinemasController` exposes only `GET /api/cinemas` plus seat block/link/unlink actions.

7. Showtimes base endpoint expected by UI does not exist
- File: `src/features/showtime/api/showtimesApi.ts`
- Current call: `GET /api/showtimes` (base list).
- Backend `ShowtimesController` exposes:
  - `GET /api/showtimes/movie/{movieId}`
  - `GET /api/showtimes/cinema/{cinemaId}`
  - `GET /api/showtimes/{id}/seating-plan`

8. Admin users lock/unlock HTTP verb mismatch
- File: `src/pages/admin/users.tsx`
- Current calls use `POST` to lock/unlock.
- Backend expects `PUT /api/admin/users/{userId}/lock` and `PUT /unlock`.

## Recommended Next Actions

1. Fix endpoint config first
- Remove duplicate `MANAGER_BOOKINGS`.
- Keep only one canonical definition.

2. Align all admin listing pages to real backend queries
- Movies: use `GET /api/movies` with filters for list views.
- Cinemas: add backend `GET /api/admin/cinemas` or switch UI to `GET /api/cinemas` if acceptable.
- Showtimes/bookings: always pass required `cinemaId` for manager routes.

3. Fix HTTP method mismatches
- Update admin users lock/unlock calls from `POST` to `PUT`.

4. Decide contract ownership
- Option A: update backend to match UI endpoint assumptions.
- Option B: update UI endpoint constants and API clients to match backend.
- Recommendation: Option B first for faster stabilization.

## Notes
- Some backend routes appear with PascalCase in generated metadata (e.g. `api/Movies`). ASP.NET Core routing is case-insensitive by default, so lowercase UI routes still resolve.
- This audit is based on current source code and generated endpoint metadata in local workspace.
