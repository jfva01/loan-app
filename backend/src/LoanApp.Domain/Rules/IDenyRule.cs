namespace LoanApp.Domain.Rules;

public interface IDenyRule
{
    RuleResult Evaluate(RuleContext context);
}