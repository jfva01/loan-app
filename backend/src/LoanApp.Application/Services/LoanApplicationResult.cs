namespace LoanApp.Application.Services;

public record LoanApplicationResult(bool Approved, string? DenialReason, Guid? ApplicationId, bool IsReturningCustomer);