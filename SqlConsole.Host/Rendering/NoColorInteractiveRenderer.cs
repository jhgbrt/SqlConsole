using System.CommandLine.IO;
using SqlConsole.Host.ResultModel;

namespace SqlConsole.Host.Rendering;

/// <summary>
/// Renders results for interactive mode without color/styling
/// </summary>
internal class NoColorInteractiveRenderer : IResultRenderer
{
    private readonly IStandardStreamWriter _writer;
    private readonly IConsoleRenderer _consoleRenderer;
    
    public NoColorInteractiveRenderer(IStandardStreamWriter writer, IConsoleRenderer consoleRenderer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _consoleRenderer = consoleRenderer ?? throw new ArgumentNullException(nameof(consoleRenderer));
    }
    
    public void Render(IResultView resultView)
    {
        switch (resultView)
        {
            case TableView table:
                RenderTable(table);
                break;
            case ScalarView scalar:
                RenderScalar(scalar);
                break;
            case NonQueryView nonQuery:
                RenderNonQuery(nonQuery);
                break;
            default:
                throw new ArgumentException($"Unsupported result view type: {resultView.GetType()}", nameof(resultView));
        }
    }
    
    private void RenderTable(TableView table)
    {
        if (!table.Headers.Any() || !table.Rows.Any())
        {
            return;
        }
        
        // Check for single value result (1 column, 1 row)
        if (table.Headers.Length == 1 && table.Rows.Count() == 1)
        {
            var singleValue = table.Rows.First()[0];
            _writer.WriteLine(singleValue);
            return;
        }
        
        // Use the existing ConsoleTableFormatter approach
        var maxLength = GetWindowWidth() - 1;
        var separator = " | ";
        
        // Calculate column lengths (using extension from existing code)
        var columnLengths = CalculateColumnLengths(table, maxLength, separator.Length);
        
        _writer.WriteLine(string.Empty);
        
        // Write headers
        var formattedHeaders = table.Headers.Select((header, i) => header.OfLength(columnLengths[i])).ToList();
        var headerLine = string.Join(separator, formattedHeaders);
        _writer.WriteLine(headerLine.SafeSubstring(0, maxLength));
        
        // Write separator line
        var separatorLine = string.Join(separator, formattedHeaders.Select(h => new string('-', h.Length)));
        _writer.WriteLine(separatorLine.SafeSubstring(0, maxLength));
        
        // Write rows
        foreach (var row in table.Rows)
        {
            var formattedRow = row.Select((cell, i) => cell.OfLength(columnLengths[i]));
            var rowLine = string.Join(separator, formattedRow);
            _writer.WriteLine(rowLine.SafeSubstring(0, maxLength));
        }
        
        _writer.WriteLine(string.Empty);
    }
    
    private void RenderScalar(ScalarView scalar)
    {
        _writer.WriteLine(scalar.Value);
    }
    
    private void RenderNonQuery(NonQueryView nonQuery)
    {
        if (nonQuery.RowsAffected < 0)
        {
            return;
        }
        
        var message = nonQuery.RowsAffected switch
        {
            1 => "1 row affected",
            _ => $"{nonQuery.RowsAffected} rows affected"
        };
        
        _writer.WriteLine(message);
    }
    
    private int[] CalculateColumnLengths(TableView table, int maxLength, int separatorLength)
    {
        var columnCount = table.Headers.Length;
        var columnLengths = new int[columnCount];
        
        // Start with header lengths
        for (int i = 0; i < columnCount; i++)
        {
            columnLengths[i] = table.Headers[i].Length;
        }
        
        // Check row data lengths
        foreach (var row in table.Rows)
        {
            for (int i = 0; i < Math.Min(columnCount, row.Length); i++)
            {
                columnLengths[i] = Math.Max(columnLengths[i], row[i].Length);
            }
        }
        
        // Adjust for available space (simplified version of existing logic)
        var totalSeparatorLength = (columnCount - 1) * separatorLength;
        var availableLength = maxLength - totalSeparatorLength;
        var totalColumnLength = columnLengths.Sum();
        
        if (totalColumnLength > availableLength)
        {
            // Proportionally reduce column widths if needed
            var ratio = (double)availableLength / totalColumnLength;
            for (int i = 0; i < columnCount; i++)
            {
                columnLengths[i] = Math.Max(1, (int)(columnLengths[i] * ratio));
            }
        }
        
        return columnLengths;
    }
    
    private static int GetWindowWidth()
    {
        try { return Console.WindowWidth; } catch { return 120; }
    }
    
    /// <summary>
    /// Write timing information (delegated to console renderer)
    /// </summary>
    public void WriteTiming(TimeSpan elapsed)
    {
        _consoleRenderer.WriteTiming(elapsed);
    }
    
    /// <summary>
    /// Write connection status (delegated to console renderer)
    /// </summary>
    public void WriteConnectionStatus(string status)
    {
        _consoleRenderer.WriteConnectionStatus(status);
    }
    
    /// <summary>
    /// Write error message (delegated to console renderer)
    /// </summary>
    public void WriteError(string message)
    {
        _consoleRenderer.WriteError(message);
    }
}