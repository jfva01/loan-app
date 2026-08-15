using LoanApp.Domain.Rules;
using Xunit;

namespace LoanApp.Tests.Domain.Rules;

public class FakeBlacklistProvider : IBlacklistProvider
{
    private readonly HashSet<string> _blacklisted;

    public FakeBlacklistProvider(params string[] blacklisted)
    {
        _blacklisted = new HashSet<string>(blacklisted);
    }

    public bool IsBlacklisted(string ssn) => _blacklisted.Contains(ssn);  

}

public class BlacklistedSsnDenyRuleTests
{
    [Fact]
    public void Evaluate_SsnIsBlacklisted_ReturnsDenied()
    {
        // Arrange
        var rule = new BlacklistedSsnDenyRule(new FakeBlacklistProvider("999-99-9999"));

        // Act
        var result = rule.Evaluate(new RuleContext("CA", "999-99-9999"));

        // Assert
        Assert.True(result.IsDenied);
        Assert.Equal("SSN is blacklisted.", result.Reason);
    }

    [Fact]
    public void Evaluate_SsnIsNotBlacklisted_ReturnsAllowed()
    {
        // Arrange
        var rule = new BlacklistedSsnDenyRule(new FakeBlacklistProvider("999-99-9999"));
        
        // Act
        var result = rule.Evaluate(new RuleContext("CA", "123-45-6789"));

        // Assert
        Assert.False(result.IsDenied);
    }
}
