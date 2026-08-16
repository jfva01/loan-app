using System.Text.Json;
using LoanApp.Application.Abstractions;
using Xunit;

namespace LoanApp.Tests.Application.Services;

public class LoanApplicationPayloadSerializationTests
{
    [Fact]
    public void Serialize_WithCamelCasePolicy_ProducesLowercaseFirstLetterKeys()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var payload = new LoanApplicationPayload(
            Guid.NewGuid(), "Juan", "Díaz", "123 Main St", "CA", "Acme",
            Guid.NewGuid(), 5000m);
        // Act
        var json = JsonSerializer.Serialize(payload, options);
        // Assert
        Assert.Contains("\"customerId\"", json);
        Assert.DoesNotContain("\"CustomerId\"", json);
    }
}