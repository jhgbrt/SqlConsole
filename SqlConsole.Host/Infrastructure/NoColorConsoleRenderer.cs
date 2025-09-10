namespace SqlConsole.Host;

/// <summary>
/// Console renderer that provides plain text output without colors
/// </summary>
internal class NoColorConsoleRenderer : IConsoleRenderer
{
    public void WriteError(string message)
    {
        Console.Error.WriteLine($"Error: {message}");
    }
    
    public void WriteConnectionStatus(string status)
    {
        Console.WriteLine(status);
    }
    
    public void WriteTiming(TimeSpan elapsed)
    {
        var description = elapsed.TotalMilliseconds switch
        {
            < 100 => "fast",
            < 1000 => "medium", 
            _ => "slow"
        };
        
        Console.WriteLine($"({elapsed.TotalMilliseconds:F0}ms - {description})");
    }
    
    public void WriteTimingAndRows(TimeSpan elapsed, int? rowCount)
    {
        var description = elapsed.TotalMilliseconds switch
        {
            < 100 => "fast",
            < 1000 => "medium", 
            _ => "slow"
        };
        
        var timingText = $"{elapsed.TotalMilliseconds:F0}ms - {description}";
        
        if (rowCount.HasValue && rowCount > 0)
        {
            var rowText = rowCount == 1 ? "1 row" : $"{rowCount} rows";
            Console.WriteLine($"({rowText} | {timingText})");
        }
        else
        {
            Console.WriteLine($"({timingText})");
        }
    }
}