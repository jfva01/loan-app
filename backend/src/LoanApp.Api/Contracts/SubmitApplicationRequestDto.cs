namespace LoanApp.Api.Contracts;

public record SubmitApplicationRequestDto(
    string FirstName,
    string LastName,
    string Address,
    string State,
    string CompanyName,
    string Ssn,
    decimal RequestedAmount
);