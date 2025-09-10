using System.Data;

namespace SqlConsole.Host.ResultModel;

/// <summary>
/// Represents tabular data with headers and rows
/// </summary>
public class TableView : IResultView
{
    public ResultViewType Type => ResultViewType.Table;
    
    /// <summary>
    /// Column headers
    /// </summary>
    public string[] Headers { get; }
    
    /// <summary>
    /// Data rows, where each row is an array of string values
    /// </summary>
    public IEnumerable<string[]> Rows { get; }
    
    public TableView(string[] headers, IEnumerable<string[]> rows)
    {
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        Rows = rows ?? throw new ArgumentNullException(nameof(rows));
    }
    
    /// <summary>
    /// Create a TableView from a DataTable
    /// </summary>
    public static TableView FromDataTable(DataTable dataTable)
    {
        if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));
        
        var headers = dataTable.Columns.OfType<DataColumn>()
            .Select(c => c.ColumnName)
            .ToArray();
            
        var rows = dataTable.Rows.OfType<DataRow>()
            .Select(row => row.ItemArray.Select(field => field?.ToString() ?? string.Empty).ToArray());
            
        return new TableView(headers, rows);
    }
}