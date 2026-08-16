# Loan Application

[Link al video — pendiente]

## Test data

- Approved: any state except `NY`, any SSN not in the blacklist.
- Denied (state): `State = NY`.
- Denied (blacklist): `SSN = 999-99-9999` or `SSN = 888-88-8888`.
- Returning customer: submit the same SSN twice with different data — the second submission updates the existing customer and application instead of creating new ones.

## How to run locally

[pendiente — se completa cuando el resto de servicios esté armado]

## How to run the tests

```bash
cd backend
dotnet test
```