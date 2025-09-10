using System.CommandLine.IO;

namespace SqlConsole.Host.Rendering;

/// <summary>
/// Factory for creating appropriate result renderers based on output mode
/// </summary>
internal static class ResultRendererFactory
{
    public static IResultRenderer Create(OutputMode mode, IStandardStreamWriter writer, IConsoleRenderer consoleRenderer)
    {
        return mode switch
        {
            OutputMode.Csv => new CsvResultRenderer(writer),
            OutputMode.Interactive when consoleRenderer is SpectreConsoleRenderer => 
                new SpectreInteractiveRenderer(consoleRenderer, writer),
            OutputMode.Interactive => 
                new NoColorInteractiveRenderer(writer, consoleRenderer),
            _ => throw new ArgumentException($"Unsupported output mode: {mode}", nameof(mode))
        };
    }
}