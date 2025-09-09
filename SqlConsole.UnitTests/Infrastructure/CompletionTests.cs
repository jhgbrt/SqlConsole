using SqlConsole.Host;
using SqlConsole.Host.Infrastructure;
using System.CommandLine;
using System.CommandLine.IO;
using Xunit;

namespace SqlConsole.UnitTests.Infrastructure;

public class CompletionTests
{
    [Theory]
    [InlineData("bash")]
    [InlineData("zsh")]
    [InlineData("powershell")]
    [InlineData("fish")]
    public async Task CompletionCommand_GeneratesScriptForShell(string shell)
    {
        // Arrange
        var command = CommandFactory.CreateCommand();
        var console = new TestConsole();
        
        // Act
        var result = await command.InvokeAsync($"completion --shell {shell}", console);
        
        // Assert
        Assert.Equal(0, result);
        var output = console.Out.ToString();
        Assert.NotEmpty(output);
        
        // Each shell should have its characteristic markers
        switch (shell)
        {
            case "bash":
                Assert.Contains("_sqlc_completion", output);
                Assert.Contains("complete -F", output);
                break;
            case "zsh":
                Assert.Contains("#compdef sqlc", output);
                Assert.Contains("_sqlc()", output);
                break;
            case "powershell":
                Assert.Contains("Register-ArgumentCompleter", output);
                Assert.Contains("-CommandName sqlc", output);
                break;
            case "fish":
                Assert.Contains("complete -c sqlc", output);
                Assert.Contains("__fish_use_subcommand", output);
                break;
        }
    }

    [Fact]
    public async Task CompletionCommand_ContainsAllProviders()
    {
        // Arrange
        var command = CommandFactory.CreateCommand();
        var console = new TestConsole();
        
        // Act
        var result = await command.InvokeAsync("completion --shell bash", console);
        
        // Assert
        Assert.Equal(0, result);
        var output = console.Out.ToString();
        
        // Should contain all provider names
        foreach (var provider in Provider.All)
        {
            Assert.Contains(provider.Name, output);
        }
    }

    [Fact]
    public async Task CompletionCommand_ShowsHelpWhenRequested()
    {
        // Arrange
        var command = CommandFactory.CreateCommand();
        var console = new TestConsole();
        
        // Act
        var result = await command.InvokeAsync("completion --help", console);
        
        // Assert
        Assert.Equal(0, result);
        var output = console.Out.ToString();
        Assert.Contains("Generate shell completion scripts", output);
        Assert.Contains("Usage examples:", output);
        Assert.Contains("bash", output);
        Assert.Contains("zsh", output);
        Assert.Contains("powershell", output);
        Assert.Contains("fish", output);
    }

    [Fact]
    public async Task CompletionCommand_FailsWithInvalidShell()
    {
        // Arrange
        var command = CommandFactory.CreateCommand();
        var console = new TestConsole();
        
        // Act
        var result = await command.InvokeAsync("completion --shell invalid", console);
        
        // Assert
        Assert.NotEqual(0, result);
    }

    [Fact]
    public async Task RootCommand_ShowsCompletionInHelp()
    {
        // Arrange
        var command = CommandFactory.CreateCommand();
        var console = new TestConsole();
        
        // Act
        var result = await command.InvokeAsync("--help", console);
        
        // Assert
        Assert.Equal(0, result);
        var output = console.Out.ToString();
        Assert.Contains("completion", output);
        Assert.Contains("Generate shell completion scripts", output);
    }
}