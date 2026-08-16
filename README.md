# Loan Application

[Link al video — pendiente]

## Test data

- Approved: any state except `NY`, any SSN not in the blacklist.
- Denied (state): `State = NY`.
- Denied (blacklist): `SSN = 999-99-9999` or `SSN = 888-88-8888`.
- Returning customer: submit the same SSN twice with different data — the second submission updates the existing customer and application instead of creating new ones.

## How to run locally

### Backend

```bash
cd backend
dotnet ef database update --project src/LoanApp.Infrastructure --startup-project src/LoanApp.Api
dotnet run --project src/LoanApp.Api
```

API available at `https://localhost:5298/api/applications` (swagger UI at `/swagger`).

## How to run the tests

```bash
cd backend
dotnet test
```