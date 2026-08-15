namespace LoanApp.Domain.Rules;

public interface IBlacklistProvider
{
    bool IsBlacklisted(string ssn);
}