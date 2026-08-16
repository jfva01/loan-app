namespace LoanApp.Application.Abstractions;

// Contrato de datos hacia el servicio externo.

public record LoanApplicationPayload(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string Address,
    string State,
    string CompanyName,
    Guid ApplicationId,
    decimal RequestedAmount
);