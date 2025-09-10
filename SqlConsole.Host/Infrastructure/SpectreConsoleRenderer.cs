using Spectre.Console;

namespace SqlConsole.Host;

/// <summary>
/// Console renderer that uses Spectre.Console for enhanced styling
/// </summary>
internal class SpectreConsoleRenderer : IConsoleRenderer
{
    public void WriteError(string message)
    {
        var panel = new Panel(new Text(message))
        {
            Header = new PanelHeader("[red]Error[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.Red)
        };
        
        AnsiConsole.Write(panel);
    }
    
    public void WriteConnectionStatus(string status)
    {
        AnsiConsole.MarkupLine($"[green]{status.EscapeMarkup()}[/]");
    }
    
    public void WriteTiming(TimeSpan elapsed)
    {
        var (color, description) = elapsed.TotalMilliseconds switch
        {
            < 100 => ("green", "fast"),
            < 1000 => ("yellow", "medium"), 
            _ => ("red", "slow")
        };
        
        AnsiConsole.MarkupLine($"[dim]({elapsed.TotalMilliseconds:F0}ms - [{color}]{description}[/])[/]");
    }
    
    public void WriteTimingAndRows(TimeSpan elapsed, int? rowCount)
    {
        var (color, description) = elapsed.TotalMilliseconds switch
        {
            < 100 => ("green", "fast"),
            < 1000 => ("yellow", "medium"), 
            _ => ("red", "slow")
        };
        
        var timingText = $"{elapsed.TotalMilliseconds:F0}ms - [{color}]{description}[/]";
        
        if (rowCount.HasValue && rowCount > 0)
        {
            var rowText = rowCount == 1 ? "1 row" : $"{rowCount} rows";
            AnsiConsole.MarkupLine($"[dim]({rowText} | {timingText})[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[dim]({timingText})[/]");
        }
    }
}