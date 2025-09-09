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
}