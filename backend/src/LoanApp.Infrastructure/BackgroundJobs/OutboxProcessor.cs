using LoanApp.Application.Abstractions;
using LoanApp.Domain.Entities;
using LoanApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoanApp.Infrastructure.BackgroundJobs;

public class OutboxProcessor : BackgroundService
{
    private const int PollingIntervalSeconds = 5;
    private const int BatchSize = 10;
    private const int MaxAttempts = 5;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync(stoppingToken);
            }
            catch(Exception ex)
            {
                // No dejamos que un fallo inesperado del ciclo de polling 
                // mate el BackgroundService entero, hacemos el log 
                // e intentamos en el proximo ciclo
                _logger.LogError(ex, "Unexpected error while processing outbox events.");
            }

            await Task.Delay(TimeSpan.FromSeconds(PollingIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessPendingEventsAsync(CancellationToken ct)
    {
        // Nuevo ciclo
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var externalService = scope.ServiceProvider.GetRequiredService<IExternalLoanService>();

        var pendingEvents = await context.OutboxEvents
            .Where(e => e.Status == OutboxEventStatus.Pending ||
                (e.Status == OutboxEventStatus.Failed && e.Attempts < MaxAttempts))
            .OrderBy(e => e.CreatedAtUtc)
            .Take(BatchSize)
            .ToListAsync(ct);

        foreach(var outboxEvent in pendingEvents)
        {
            try
            {
                if (outboxEvent.Operation == ExternalOperation.Create)
                    await externalService.CreateAsync(outboxEvent.Payload, ct);
                else
                    await externalService.UpdateAsync(outboxEvent.Payload, ct);

                outboxEvent.MarkAsSent();
                _logger.LogInformation("Outbox event {Id} sent succesfully.", outboxEvent.Id);
            }
            catch(Exception ex)
            {
                outboxEvent.MarkAttemptFailed();
                _logger.LogWarning(ex,"Outbox event {Id} failed (attempt {Attemps}/{Max}).",
                    outboxEvent.Id, outboxEvent.Attempts, MaxAttempts);
            }
        }

        if (pendingEvents.Count > 0)
            await context.SaveChangesAsync(ct);
    }
}