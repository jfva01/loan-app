using LoanApp.Domain.Entities;

namespace LoanApp.Application.Abstractions;

public interface IOutboxRepository
{
    void Add(OutboxEvent outboxEvent);
    Task<List<OutboxEvent>> GetPendingAsync(int batchSize, CancellationToken ct = default);
}