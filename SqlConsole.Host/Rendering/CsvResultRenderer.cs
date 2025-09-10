using System.CommandLine.IO;
using SqlConsole.Host.ResultModel;

namespace SqlConsole.Host.Rendering;

/// <summary>
/// Renders results as pure CSV without any styling or timing information
/// </summary>
internal class CsvResultRenderer : IResultRenderer
{
    private readonly IStandardStreamWriter _writer;
    
    public CsvResultRenderer(IStandardStreamWriter writer)
    {
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
        // Write headers
        if (table.Headers.Length > 0)
        {
            var headers = table.Headers.Select(EscapeCsvField);
            _writer.WriteLine(string.Join(";", headers));
        }
        
        // Write rows
        foreach (var row in table.Rows)
        {
            var values = row.Select(EscapeCsvField);
            _writer.WriteLine(string.Join(";", values));
        }
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
    
    private static string EscapeCsvField(string field)
    {
        var escaped = field?.Replace("\"", "\"\"") ?? string.Empty;
        return $"\"{escaped}\"";
    }
}