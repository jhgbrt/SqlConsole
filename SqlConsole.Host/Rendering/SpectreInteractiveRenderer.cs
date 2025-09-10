using Spectre.Console;
using SqlConsole.Host.ResultModel;
using System.CommandLine.IO;

namespace SqlConsole.Host.Rendering;

/// <summary>
/// Renders results using Spectre.Console for rich interactive terminal output
/// </summary>
internal class SpectreInteractiveRenderer : IResultRenderer
{
    private readonly IConsoleRenderer _consoleRenderer;
    private readonly IStandardStreamWriter _writer;
    
    public SpectreInteractiveRenderer(IConsoleRenderer consoleRenderer, IStandardStreamWriter writer)
    {
        _consoleRenderer = consoleRenderer ?? throw new ArgumentNullException(nameof(consoleRenderer));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
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
        
        // Create Spectre table
        var spectreTable = new Table();
        
        // Add columns with headers
        foreach (var header in table.Headers)
        {
            spectreTable.AddColumn(header.EscapeMarkup());
        }
        
        // Add rows
        foreach (var row in table.Rows)
        {
            var escapedRow = row.Select(cell => cell.EscapeMarkup()).ToArray();
            spectreTable.AddRow(escapedRow);
        }
        
        AnsiConsole.Write(spectreTable);
    }
    
    private void RenderScalar(ScalarView scalar)
    {
        _writer.WriteLine(scalar.Value);
    }
    
    private void RenderNonQuery(NonQueryView nonQuery)
    {
        if (nonQuery.RowsAffected < 0)
        {
            // Don't output anything for negative row counts
            return;
        }
        
        var message = nonQuery.RowsAffected switch
        {
            1 => "1 row affected",
            _ => $"{nonQuery.RowsAffected} rows affected"
        };
        
        _writer.WriteLine(message);
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