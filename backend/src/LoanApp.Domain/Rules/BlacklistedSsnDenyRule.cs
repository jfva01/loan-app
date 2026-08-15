namespace LoanApp.Domain.Rules;

public class BlacklistedSsnDenyRule : IDenyRule
{
    private readonly IBlacklistProvider _blacklistProvider;

    public BlacklistedSsnDenyRule(IBlacklistProvider blacklistProvider)
    {
        _blacklistProvider = blacklistProvider;
    }

    public RuleResult Evaluate(RuleContext context)
    {
        return _blacklistProvider.IsBlacklisted(context.Ssn)
            ? RuleResult.Deny("SSN is blacklisted.")
            : RuleResult.Approve();
    }
}