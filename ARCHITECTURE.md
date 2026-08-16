# Architecture

## Project structure

backend/
src/
LoanApp.Domain/ Entities, deny rules, rule engine contracts. No external dependencies.
LoanApp.Application/ Orchestration: RuleEngine, LoanApplicationService, repository/unit-of-work contracts.
LoanApp.Infrastructure/ EF Core (SQL Server), repositories, outbox processor, external HTTP client.
LoanApp.Api/ Thin controllers, DI wiring.
tests/
LoanApp.Tests/ Unit tests (rules, rule engine) + integration tests (SQLite in-memory).
external-service/ Mock HTTP service simulating the partner API.
frontend/ Next.js + TypeScript + Tailwind.

Dependencies point inward: `Api → Application → Domain`, with `Infrastructure` implementing interfaces defined in `Application`. `Api` never references `Domain` directly.

## Rule engine

Each deny rule implements `IDenyRule.Evaluate(RuleContext) -> RuleResult` in `Domain`. `RuleEngine` (in `Application`) holds an ordered `IEnumerable<IDenyRule>` injected via DI and returns the first denial found, or approval if none match.

**Adding a rule**: create a class implementing `IDenyRule`, register it in `Program.cs` with `AddScoped<IDenyRule, YourNewRule>()`. No existing rule is touched. Rule order is determined by DI registration order — currently: state check, then blacklist check.

Current rules: `NyStateDenyRule` (state == "NY"), `BlacklistedSsnDenyRule` (SSN in `IBlacklistProvider`, backed by `appsettings.json`).

## Transaction & outbox

`Customer`, `LoanApplication`, and `OutboxEvent` are all added to the same EF Core `DbContext` and persisted in a single `SaveChangesAsync()` call, which EF Core wraps in an implicit transaction. If any write fails (constraint violation, connection loss), all three are rolled back together — no orphaned customer, no orphaned application, no outbox event for work that didn't happen.

The outbox event is written to the database in the same transaction, not sent over HTTP synchronously. A background processor (in progress — see next section) polls `OutboxEvents` for `Pending`/`Failed` rows and delivers them to the external service, decoupling "data saved" from "external call succeeded."

**Returning customer**: identified by SSN (unique index on `Customers.Ssn`). If found, the existing `Customer` and its `LoanApplication` (unique index on `Applications.CustomerId`, enforcing one application per customer) are updated in place — no new rows. The outbox event records whether the operation was `Create` or `Update` so the background processor knows which HTTP verb to use against the external service.

## Trade-offs so far

- **Guid over int identity**: avoids needing a round-trip to the database to get a generated key before building the outbox payload.
- **SQL Server for the app, SQLite in-memory for integration tests**: keeps tests portable (no external DB dependency to run `dotnet test`) and isolated between runs, at the cost of SQLite and SQL Server not being 100% behaviorally identical — acceptable here because the logic under test is EF Core/application logic, not SQL Server–specific behavior.
- **No generic repository**: `ICustomerRepository` and `IOutboxRepository` expose only the methods this flow actually needs, not a generic `IRepository<T>`.
- **Rollback verified with a dedicated test** (`SaveChangesAsync_ConstraintViolationMidTransaction_RollsBackEverything`): forces a unique-constraint violation mid-`SaveChangesAsync` and asserts that no entity in that unit of work — not even the ones that were individually valid — was persisted.