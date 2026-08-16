using LoanApp.Application.Abstractions;

namespace LoanApp.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    // SavechangesAsync de EF Core ya envuelve las escrituras pendientes
    // en una transacción implícita cuendo hay múltiples inserts/updates
    // en el mismo ChangeTracker. Customer + LoadApplcation + OutboxEvent
    // se agregan al contexto en LoadApplicationService y se confirman 
    // aquí en un solo commit.

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct); 
}