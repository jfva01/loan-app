namespace LoanApp.Application.Abstractions;

// Contrato hacia el servicio externo ("mock").

public interface IExternalLoanService
{
    Task CreateAsync(string payload, CancellationToken ct = default);
    Task UpdateAsync(string payload, CancellationToken ct = default);
}