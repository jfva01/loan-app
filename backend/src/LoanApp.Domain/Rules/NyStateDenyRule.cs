namespace LoanApp.Domain.Rules;

public class NyStateDenyRule : IDenyRule
{
    public RuleResult Evaluate(RuleContext context)
    {
        return context.State.Equals("NY", StringComparison.OrdinalIgnoreCase)
            ? RuleResult.Deny("State NY is not elegible.") 
            : RuleResult.Approve();
    }
}