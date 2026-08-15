using LoanApp.Domain.Rules;
using Xunit;

namespace LoanApp.Tests.Domain.Rules;

public class NyStateDenyRuleTests
{
    private readonly NyStateDenyRule _rule = new();

    [Theory]
    [InlineData("NY")]
    [InlineData("ny")]

    public void Evaluate_StateIsNy_ReturnsDenied(string state)
    {
        // Act
        var result = _rule.Evaluate(new RuleContext (state, "123-45-6789"));

        // Assert
        Assert.True(result.IsDenied);
        Assert.Equal("State NY is not elegible.", result.Reason);
    }

    [Theory]
    [InlineData("CA")]
    [InlineData("TX")]
    public void Evaluate_StateIsNotNy_ReturnsApproved(string state)
    {
        // Act
        var result = _rule.Evaluate(new RuleContext (state, "123-45-6789"));

        // Assert
        Assert.False(result.IsDenied);
    }
}