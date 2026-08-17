# Loan Application

[Watch the demo video](https://www.loom.com/share/c5aac01b877d4274942c75b4c06bdd02)

## Test data

- Approved: any state except `NY`, any SSN not in the blacklist.
- Denied (state): `State = NY`.
- Denied (blacklist): `SSN = 999-99-9999` or `SSN = 888-88-8888`.
- Returning customer: submit the same SSN twice with different data — the second submission updates the existing customer and application instead of creating new ones.

## How to run locally

Three services must run simultaneously, each in its own terminal.

### 1. Backend

```bash
cd backend
dotnet ef database update --project src/LoanApp.Infrastructure --startup-project src/LoanApp.Api
dotnet run --project src/LoanApp.Api
```

API available at `http://localhost:5298/api/applications` (Swagger UI at `/swagger`). Requires a local SQL Server instance reachable with the connection string in `src/LoanApp.Api/appsettings.json` — adjust it to match your local setup (instance name, auth mode) before running the migration.

- Default instance, Windows Auth: `Server=localhost;Database=LoanAppDb;Trusted_Connection=True;TrustServerCertificate=True;`
- Named instance (e.g. SQL Express): `Server=localhost\SQLEXPRESS;Database=LoanAppDb;Trusted_Connection=True;TrustServerCertificate=True;`

### 2. External service (mock)

```bash
cd external-service
dotnet run
```

Available at `http://localhost:5262` (Swagger UI at `/swagger`). `GET /api/loanrecords` shows everything the mock has received — useful for confirming the background event was delivered.

### 3. Frontend

```bash
cd frontend
npm install
```

Create `frontend/.env.local`:

```
NEXT_PUBLIC_API_BASE_URL=http://localhost:5298
```

```bash
npm run dev
```

Available at `http://localhost:3000`.

## How to run the tests

```bash
cd backend
dotnet test
```

17 tests: rule engine (unit), returning-customer transactional flow (SQLite in-memory integration), applications endpoint (HTTP integration via `WebApplicationFactory`), payload serialization, and transaction rollback on constraint violation.