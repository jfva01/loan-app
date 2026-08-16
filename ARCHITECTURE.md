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

## Background event delivery (Outbox pattern)

`OutboxEvent` rows are written in the same transaction as `Customer`/`LoanApplication` (see previous section), guaranteeing that an event exists if and only if the underlying data was actually saved.

A `BackgroundService` (`OutboxProcessor`) polls the database every 5 seconds for `Pending` or `Failed` (with attempts < 5) events, in batches of 10, and sends each to the external mock over HTTP — `POST /api/loanrecords` for new customers, `PUT /api/loanrecords/{customerId}` for returning customers. On success, the event is marked `Sent`. On failure, `Attempts` increments and the event is retried on the next polling cycle, up to 5 attempts, after which it's left `Failed` permanently.

**Why polling instead of a message queue**: no external broker (RabbitMQ, Service Broker) is justified at this scale — polling on an indexed `Status` column is the simplest mechanism that still decouples "data saved" from "external call succeeded."

**Why a fixed retry limit instead of exponential backoff**: this outbox doesn't distinguish transient failures (the mock briefly down) from permanent ones (a payload the external service always rejects) — either way, a short fixed-interval retry window is enough to recover from a transient blip without the complexity of backoff. 5 attempts at a 5-second interval (25 seconds total) is a round, low number chosen for a short retry window, not derived from a specific SLA.

## Trade-offs so far

- **Guid over int identity**: avoids needing a round-trip to the database to get a generated key before building the outbox payload.
- **SQL Server for the app, SQLite in-memory for integration tests**: keeps tests portable (no external DB dependency to run `dotnet test`) and isolated between runs, at the cost of SQLite and SQL Server not being 100% behaviorally identical — acceptable here because the logic under test is EF Core/application logic, not SQL Server–specific behavior.
- **No generic repository**: `ICustomerRepository` and `IOutboxRepository` expose only the methods this flow actually needs, not a generic `IRepository<T>`.
- **Rollback verified with a dedicated test** (`SaveChangesAsync_ConstraintViolationMidTransaction_RollsBackEverything`): forces a unique-constraint violation mid-`SaveChangesAsync` and asserts that no entity in that unit of work — not even the ones that were individually valid — was persisted.

- **200 OK for both approved and denied applications**: a denial is a valid business outcome produced by the rule engine working correctly, not a system error. 4xx/5xx are reserved for actual failures (invalid payload, unhandled exceptions). The frontend branches on the `approved` field, not on status code.