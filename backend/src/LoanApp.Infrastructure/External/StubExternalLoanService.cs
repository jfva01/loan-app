using LoanApp.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace LoanApp.Infrastructure.External;

public class StubExteranlLoanService : IExternalLoanService
{
    private readonly ILogger<StubExteranlLoanService> __logger;

    public StubExteranlLoanService(ILogger<StubExteranlLoanService> logger)
    {
        __logger = logger;
    }

    public Task CreateAsync(string payload, CancellationToken ct = default)
    {
        __logger.LogInformation("STUB create -> {Payload}", payload);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(string payload, CancellationToken ct = default)
    {
        __logger.LogInformation("STUB update -> {Payload}", payload);
        return Task.CompletedTask;
    }
}