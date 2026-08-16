using System.Net.Http.Json;
using LoanApp.Api.Contracts;
using LoanApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LoanApp.Tests.Api;

public class ApplicationsControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly HttpClient _client;

    public ApplicationsControllerTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Saca el registro real de AppDbContext (SQL Server) y lo reemplaza
                // por SQLite en memoria, igual que en los tests de LoanApplicationService.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
            });
        });

        _client = customizedFactory.CreateClient();

        using var scope = customizedFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Submit_ApprovedApplication_Returns200WithApprovedTrue()
    {
        var request = new SubmitApplicationRequestDto(
            "Carla", "Núñez", "789 Pine Rd", "FL", "Delta Co", "222-22-2222", 4000m);

        var response = await _client.PostAsJsonAsync("/api/applications", request);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<SubmitApplicationResponseDto>();

        Assert.NotNull(body);
        Assert.True(body!.Approved);
        Assert.NotNull(body.ApplicationId);
        Assert.Null(body.DenialReason);
    }

    [Fact]
    public async Task Submit_StateNy_Returns200WithApprovedFalseAndReason()
    {
        var request = new SubmitApplicationRequestDto(
            "Carla", "Núñez", "789 Pine Rd", "NY", "Delta Co", "333-33-3333", 4000m);

        var response = await _client.PostAsJsonAsync("/api/applications", request);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<SubmitApplicationResponseDto>();

        Assert.NotNull(body);
        Assert.False(body!.Approved);
        Assert.Equal("State NY is not eligible.", body.DenialReason);
        Assert.Null(body.ApplicationId);
    }

    public void Dispose() => _connection.Dispose();
}