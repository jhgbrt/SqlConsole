using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlConsole.Host
{
    class CsvFormatter : ITextFormatter<DataTable>
    {
        public IEnumerable<string> Format(DataTable dt)
        {
            if (dt.Rows.Count == 1 && dt.Columns.Count == 1)
            {
                yield return dt.Rows[0][0].ToString();
            }
            else
            {
                string[] columnNames = dt.Columns.OfType<DataColumn>().Select(column => "\"" + column.ColumnName.Replace("\"", "\"\"") + "\"").ToArray();
                yield return string.Join(";", columnNames);
                foreach (DataRow row in dt.Rows)
                {
                    string[] fields = row.ItemArray.Select(field => "\"" + field.ToString().Replace("\"", "\"\"") + "\"").ToArray();
                    yield return string.Join(";", fields);
                }
            }
        }
    }
}