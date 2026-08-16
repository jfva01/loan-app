using System.Text.Json;
using LoanApp.Application.Abstractions;
using LoanApp.Application.Rules;
using LoanApp.Application.Services;
using LoanApp.Domain.Entities;
using LoanApp.Domain.Rules;
using LoanApp.Infrastructure.External;
using LoanApp.Infrastructure.Persistence;
using LoanApp.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LoanApp.Tests.Application.Services;

public class FakeBlacklistProvider : IBlacklistProvider
{
    private readonly HashSet<string> _blacklisted;
    public FakeBlacklistProvider(params string[] blacklisted) => _blacklisted = blacklisted.ToHashSet();
    public bool IsBlacklisted(string ssn) => _blacklisted.Contains(ssn);
}

public class LoanApplicationServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly LoanApplicationService _service;

    public LoanApplicationServiceTests()
    {
        // SQLite en memoria requiere mantener la conexión abierta durante el test.
        // La abrimos ahora y no se cierra hasta el fin del test.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        var rules = new IDenyRule[]
        {
            new NyStateDenyRule(),
            new BlacklistedSsnDenyRule(new FakeBlacklistProvider("999-99-9999"))
        };

        _service = new LoanApplicationService(
            new RuleEngine(rules),
            new CustomerRepository(_context),
            new OutboxRepository(_context),
            new UnitOfWork(_context));
        

    }

    [Fact]
    public async Task SubmitAsync_NewCustomer_ApprovedState_CreateCustomerApplicationAndOutboxEvent()
    {
        // Arrange
        var request = new SubmitLoanApplicationRequest(
            "Juan", "Pérez", "123 Main St", "CA", "Company Inc", "123-45-6789", 5000m);
        // Act
        var result = await _service.SubmitAsync(request);
        // Assert
        Assert.True(result.Approved);
        Assert.NotNull(result.ApplicationId);

        var customers = await _context.Customers.ToListAsync();
        var applications = await _context.Applications.ToListAsync();
        var outboxEvents = await _context.OutboxEvents.ToListAsync();

        Assert.Single(customers);
        Assert.Single(applications);
        Assert.Single(outboxEvents);
        Assert.Equal(ExternalOperation.Create, outboxEvents[0].Operation);
        Assert.Equal(5000m, applications[0].RequestedAmount);
    }

    [Fact]
    public async Task SubmitAsync_DeniedByState_DoesNotPersistAnything()
    {
        // Arrange
        var request = new SubmitLoanApplicationRequest(
            "Juan", "Pérez", "123 Main St", "NY", "Company Inc", "123-45-6789", 5000m
        );
        // Act
        var result = await _service.SubmitAsync(request);
        // Assert
        Assert.False(result.Approved);
        Assert.Equal("State NY is not eligible.", result.DenialReason);
        Assert.Empty(await _context.Customers.ToListAsync());
        Assert.Empty(await _context.Applications.ToListAsync());
        Assert.Empty(await _context.OutboxEvents.ToListAsync());
    }

    [Fact]
    public async Task SubmitAsync_ReturningCustomer_UpdatesExistingRecords_DoesNotCreateSecondCustomerOrApplication()
    {
        // Arrange
        var ssn = "123-45-6789";
        var firstSubmission = new SubmitLoanApplicationRequest(
            "Juan", "Pérez", "123 Main St", "CA", "Company Inc", "123-45-6789", 5000m
        );

        await _service.SubmitAsync(firstSubmission);

        // Mismo SSN, datos actualizados: dirección, empresa, monto y estado distintos.
        var secondSubmission = new SubmitLoanApplicationRequest(
            "Juan", "Pérez", "456 Oak Ave", "TX", "New Company LLC", ssn, 9000m
        );

        var result = await _service.SubmitAsync(secondSubmission);

        Assert.True(result.Approved);

        var customers = await _context.Customers.ToListAsync();
        var applications = await _context.Applications.ToListAsync();
        var outboxEvents = await _context.OutboxEvents.ToListAsync();

        // Un solo usuario, una sola aplicación
        Assert.Single(customers);
        Assert.Single(applications);

        Assert.Equal("456 Oak Ave", customers[0].Address);
        Assert.Equal("TX", customers[0].State);
        Assert.Equal("New Company LLC", customers[0].CompanyName);
        Assert.Equal(9000m, applications[0].RequestedAmount);

        // Dos evento en el outbox, uno Create (primer Submit), uno Update (segundo) 
        Assert.Equal(2,outboxEvents.Count());
        Assert.Equal(ExternalOperation.Create, outboxEvents[0].Operation);
        Assert.Equal(ExternalOperation.Update, outboxEvents[1].Operation);
    }

    
    [Fact]
    public async Task SubmitAsync_DeniedByBlacklistedSsn_DoesNotPersistAnything()
    {
        var request = new SubmitLoanApplicationRequest(
            "Juan", "Pérez", "123 Main St", "CA", "Company Inc", "999-99-9999", 5000m);

        var result = await _service.SubmitAsync(request);

        Assert.False(result.Approved);
        Assert.Equal("SSN is blacklisted.", result.DenialReason);
        Assert.Empty(await _context.Customers.ToListAsync());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}