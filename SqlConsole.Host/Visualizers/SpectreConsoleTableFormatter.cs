using Spectre.Console;

namespace SqlConsole.Host;

class SpectreConsoleTableFormatter : ITextFormatter<DataTable>
{
    public IEnumerable<string> Format(DataTable result)
    {
        if (result.Rows.Count == 0)
        {
            yield break;
        }

        // Handle single scalar result
        if (result.Rows.Count == 1 && result.Columns.Count == 1)
        {
            yield return result.Rows[0][0]?.ToString() ?? string.Empty;
            yield break;
        }

        // Use our beautiful table renderer
        yield return string.Empty;
        
        var renderer = new BeautifulTableRenderer();
        foreach (var line in renderer.Render(result))
        {
            yield return line;
        }
        
        yield return string.Empty;
    }
}

// Beautiful Unicode table renderer inspired by Spectre.Console
class BeautifulTableRenderer
{
    public IEnumerable<string> Render(DataTable dataTable)
    {
        var columnCount = dataTable.Columns.Count;
        var columnWidths = new int[columnCount];
        var headers = new string[columnCount];
        var rows = new List<string[]>();
        
        // Prepare headers
        for (int i = 0; i < columnCount; i++)
        {
            headers[i] = dataTable.Columns[i].ColumnName ?? string.Empty;
            columnWidths[i] = Math.Max(headers[i].Length, 8); // Minimum width
        }
        
        // Prepare data rows and calculate column widths
        foreach (DataRow row in dataTable.Rows)
        {
            var rowData = new string[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                rowData[i] = row[i]?.ToString() ?? string.Empty;
                columnWidths[i] = Math.Max(columnWidths[i], rowData[i].Length);
            }
            rows.Add(rowData);
        }
        
        // Limit column widths to prevent extremely wide tables
        for (int i = 0; i < columnCount; i++)
        {
            columnWidths[i] = Math.Min(columnWidths[i], 50);
        }
        
        // Render top border (rounded)
        yield return "╭" + string.Join("┬", columnWidths.Select(w => new string('─', w + 2))) + "╮";
        
        // Render header row (centered and emphasized)
        var headerParts = new string[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            var centeredHeader = CenterText(headers[i], columnWidths[i]);
            headerParts[i] = $" {centeredHeader} ";
        }
        yield return "│" + string.Join("│", headerParts) + "│";
        
        // Render separator
        yield return "├" + string.Join("┼", columnWidths.Select(w => new string('─', w + 2))) + "┤";
        
        // Render data rows
        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var rowParts = new string[columnCount];
            
            for (int i = 0; i < columnCount; i++)
            {
                var cellText = row[i];
                if (cellText.Length > columnWidths[i])
                {
                    cellText = cellText.Substring(0, columnWidths[i] - 3) + "...";
                }
                
                // Right-align numbers, left-align text
                var alignedText = IsNumeric(cellText) 
                    ? cellText.PadLeft(columnWidths[i]) 
                    : cellText.PadRight(columnWidths[i]);
                    
                rowParts[i] = $" {alignedText} ";
            }
            yield return "│" + string.Join("│", rowParts) + "│";
        }
        
        // Render bottom border (rounded)
        yield return "╰" + string.Join("┴", columnWidths.Select(w => new string('─', w + 2))) + "╯";
    }
    
    private static string CenterText(string text, int width)
    {
        if (text.Length >= width) return text.Substring(0, width);
        
        var padding = width - text.Length;
        var leftPadding = padding / 2;
        var rightPadding = padding - leftPadding;
        
        return new string(' ', leftPadding) + text + new string(' ', rightPadding);
    }
    
    private static bool IsNumeric(string text)
    {
        return decimal.TryParse(text, out _) || int.TryParse(text, out _) || double.TryParse(text, out _);
    }
}