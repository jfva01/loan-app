namespace LoanApp.Domain.Rules;

public record RuleResult
{
    public bool IsDenied { get; }
    public string? Reason { get; }

    private RuleResult(bool isDenied, string? reason)
    {
        IsDenied = isDenied;
        Reason = reason;
    }

    public static RuleResult Approve() => new RuleResult(false, null);
    public static RuleResult Deny(string reason) => new RuleResult(true, reason);
}