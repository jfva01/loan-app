using LoanApp.Domain.Rules;

namespace LoanApp.Application.Rules;

public class RuleEngine : IRuleEngine
{
    private readonly IEnumerable<IDenyRule> _rules;

    public RuleEngine(IEnumerable<IDenyRule> rules)
    {
        _rules = rules;
    }

    public RuleResult Evaluate(RuleContext context)
    {
        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(context);
            if (result.IsDenied)
                return result; // corta en la primera regla que deniega
        }

        return RuleResult.Approve();
    }
}