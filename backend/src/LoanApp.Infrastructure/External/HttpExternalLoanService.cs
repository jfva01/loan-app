using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LoanApp.Application.Abstractions;

namespace LoanApp.Infrastructure.External;

public class HttpExternalLoanService : IExternalLoanService
{
    private readonly HttpClient _httpClient;

    public HttpExternalLoanService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task CreateAsync(string payload, CancellationToken ct = default)
    {
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/loanrecords", content, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(string payload, CancellationToken ct = default)
    {
        // El payload ya trae CustomerId, o extendemos para armar 
        // la ruta que el mock espera /api/loanrecords/{customerId}
        using var doc = JsonDocument.Parse(payload);
        var customerId = doc.RootElement.GetProperty("customerId").GetGuid();

        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"api/loanrecords/{customerId}", content, ct);
        response.EnsureSuccessStatusCode();
    }
}