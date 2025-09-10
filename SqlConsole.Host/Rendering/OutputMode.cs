namespace SqlConsole.Host.Rendering;

/// <summary>
/// Defines the output rendering modes
/// </summary>
public enum OutputMode
{
    /// <summary>
    /// Interactive mode with Spectre.Console styling and timing
    /// </summary>
    Interactive,
    
    /// <summary>
    /// Pure CSV export mode without styling or timing
    /// </summary>
    Csv
}