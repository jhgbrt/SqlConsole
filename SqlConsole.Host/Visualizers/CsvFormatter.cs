using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlConsole.Host
{
    class CsvFormatter : ITextFormatter<DataTable>
    {
        public IEnumerable<string> Format(DataTable dt)
        {
            var columnNames = dt.Columns.OfType<DataColumn>().Select(column => "\"" + column.ColumnName.Replace("\"", "\"\"") + "\"");

            var rowValues =
                from row in dt.Rows.OfType<DataRow>()
                select (
                    from field in row.ItemArray
                    let representation = field?.ToString()?.Replace("\"", "\"\"") ?? string.Empty
                    select $"\"{representation}\""
                    );

            var query = from itemArray in new[] { columnNames }.Concat(rowValues)
                        select string.Join(";", itemArray);

            foreach (var line in query)
            {
                yield return line;
            }
        }
    }
}