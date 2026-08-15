using LoanApp.Application.Rules;
using LoanApp.Domain.Rules;
using LoanApp.Tests.Domain.Rules;
using Xunit;

namespace LoanApp.Tests.Application.Rules;

public class RuleEngineTests
{
    private static RuleEngine BuildEngine(params string[] blacklisted)
    {
        var rules = new IDenyRule[]
        {
            new NyStateDenyRule(),
            new BlacklistedSsnDenyRule(new FakeBlacklistProvider(blacklisted))
        };

        return new RuleEngine(rules);
    }

    [Fact]
    public void Evaluate_NoRuleMatches_ReturnsApproved()
    {
        // Arrange
        var engine = BuildEngine("999-99-9999");

        // Act
        var result = engine.Evaluate(new RuleContext("CA", "123-45-6789"));

        // Assert
        Assert.False(result.IsDenied);
    }

    [Fact]
    public void Evaluate_StateNy_ReturnsDenied_BeforeCheckingBlacklist()
    {
        // Arrange
        var engine = BuildEngine("999-99-9999");

        // Act
        var result = engine.Evaluate(new RuleContext("NY", "123-45-6789"));

        // Assert
        Assert.True(result.IsDenied);
        Assert.Equal("State NY is not elegible.", result.Reason);
    }

    [Fact]
    public void Evaluate_SsnBlacklisted_ReturnsDenied()
    {
        // Arrange
        var engine = BuildEngine("999-99-9999");

        // Act
        var result = engine.Evaluate(new RuleContext("CA", "999-99-9999"));

        // Assert
        Assert.True(result.IsDenied);
        Assert.Equal("SSN is blacklisted.", result.Reason);
    }
}