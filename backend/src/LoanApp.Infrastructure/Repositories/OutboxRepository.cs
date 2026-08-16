using LoanApp.Application.Abstractions;
using LoanApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanApp.Infrastructure.Persistence.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly AppDbContext _context;

    public OutboxRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(OutboxEvent outboxEvent) => _context.OutboxEvents.Add(outboxEvent);

    public Task<List<OutboxEvent>> GetPendingAsync(int batchSize, CancellationToken ct = default) =>
        _context.OutboxEvents
            .Where(e => e.Status == OutboxEventStatus.Pending || e.Status == OutboxEventStatus.Failed)
            .OrderBy(e => e.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);
}