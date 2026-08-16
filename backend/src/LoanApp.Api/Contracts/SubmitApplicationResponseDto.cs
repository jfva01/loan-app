namespace LoanApp.Api.Contracts;

public record SubmitApplicationResponseDto(
    bool Approved,
    string? DenialReason,
    Guid? ApplicationId
);