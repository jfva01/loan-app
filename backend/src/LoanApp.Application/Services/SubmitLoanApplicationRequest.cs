namespace LoanApp.Application.Services;

public record SubmitLoanApplicationRequest(
    string FirstName,
    string LastName,
    string Address,
    string State,
    string CompanyName,
    string Ssn,
    decimal RequestedAmount
);