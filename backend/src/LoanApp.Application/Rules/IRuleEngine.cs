using LoanApp.Domain.Rules;

namespace LoanApp.Application.Rules;

public interface IRuleEngine
{
    RuleResult Evaluate(RuleContext context);
}