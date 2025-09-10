namespace SqlConsole.Host;

/// <summary>
/// Factory for creating appropriate console renderer based on color preferences
/// </summary>
internal static class ConsoleRendererFactory
{
    public static IConsoleRenderer Create(CommandFactory.QueryOptions options)
    {
        var noColorEnvironment = Environment.GetEnvironmentVariable("NO_COLOR");
        var shouldUseColor = !options.NoColor && 
                            string.IsNullOrEmpty(noColorEnvironment) && 
                            !Console.IsOutputRedirected;
        
        return shouldUseColor 
            ? new SpectreConsoleRenderer() 
            : new NoColorConsoleRenderer();
    }
}