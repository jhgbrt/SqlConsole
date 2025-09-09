using SqlConsole.Host.Infrastructure;
using Xunit;

namespace SqlConsole.UnitTests.Infrastructure;

public class HelpFormatterTests
{
    [Fact]
    public void SupportsAnsiColors_ReturnsConsistentValue()
    {
        // Act
        var supports = HelpFormatter.SupportsAnsiColors;
        
        // Assert
        // The value should be consistent across calls
        Assert.Equal(supports, HelpFormatter.SupportsAnsiColors);
    }

    [Theory]
    [InlineData("test", ConsoleColor.Red)]
    [InlineData("hello", ConsoleColor.Green)]
    [InlineData("", ConsoleColor.Blue)]
    public void Colorize_WithAnyColor_DoesNotThrow(string text, ConsoleColor color)
    {
        // Act & Assert (should not throw)
        var result = HelpFormatter.Colorize(text, color);
        Assert.NotNull(result);
    }

    [Fact]
    public void FormatProviderList_ContainsAllProviders()
    {
        // Act
        var result = HelpFormatter.FormatProviderList();
        
        // Assert
        Assert.Contains("Supported providers:", result);
        foreach (var provider in SqlConsole.Host.Provider.All)
        {
            Assert.Contains(provider.Name, result);
        }
    }

    [Theory]
    [InlineData("sqlite")]
    [InlineData("sqlserver")]
    [InlineData("postgres")]
    [InlineData("mysql")]
    [InlineData("oracle")]
    [InlineData("db2")]
    public void GetProviderExamples_ReturnsExamplesForKnownProviders(string providerName)
    {
        // Act
        var examples = HelpFormatter.GetProviderExamples(providerName);
        
        // Assert
        Assert.NotEmpty(examples);
        Assert.Contains("sqlc", examples);
        Assert.Contains(providerName, examples);
    }

    [Fact]
    public void GetProviderExamples_HandlesUnknownProvider()
    {
        // Act
        var examples = HelpFormatter.GetProviderExamples("unknown");
        
        // Assert
        Assert.NotEmpty(examples);
        Assert.Contains("No specific examples available", examples);
    }

    [Fact]
    public void CreateProviderDescription_IncludesExamplesAndCompletionInfo()
    {
        // Arrange
        var baseDescription = "Test description";
        
        // Act
        var result = HelpFormatter.CreateProviderDescription("sqlite", baseDescription);
        
        // Assert
        Assert.Contains(baseDescription, result);
        Assert.Contains("Examples:", result);
        Assert.Contains("Installation of shell completion:", result);
        Assert.Contains("sqlc completion --shell", result);
    }
}