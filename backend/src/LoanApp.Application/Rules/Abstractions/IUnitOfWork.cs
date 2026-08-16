namespace LoanApp.Application.Abstractions;

// Envuelve la trasacción real. Guardar Customer, LoanApplication
// y OutboxEvent; es una operación atómica - usamos un solo SaveChangesAsync
// generando una solo transacción.

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}