using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlConsole.Host
{
    class ConsoleTableFormatter : ITextFormatter<DataTable>
    {
        private readonly int _windowWidth;
        private readonly string _separator;

        public ConsoleTableFormatter(int windowWidth, string separator)
        {
            _windowWidth = windowWidth;
            _separator = separator;
        }

        public IEnumerable<string> Format(DataTable result)
        {
            if (result.Rows.Count == 0)
            {
                yield break;
            }
            if (result.Rows.Count == 1 && result.Columns.Count == 1)
            {
                yield return result.Rows[0][0].ToString();
            }
            else
            {
                var maxLength = _windowWidth - 1;
                var columnLengths = result.ColumnLengths(maxLength, _separator.Length);
                var columnNames = result.Columns.OfType<DataColumn>().Select(c => c.ColumnName.OfLength(columnLengths[c])).ToList();
                var joinedColumnNames = string.Join(_separator, columnNames);
                var line = string.Join("-|-", columnNames.Select(c => new string(Enumerable.Repeat('-', c.Length).ToArray())));

                yield return joinedColumnNames.SafeSubstring(0, maxLength);
                yield return line.SafeSubstring(0, maxLength);

                foreach (var item in result.Rows.OfType<DataRow>())
                {
                    var row = item;
                    var values = result.Columns.OfType<DataColumn>().Select(c => row[c].ToString().OfLength(columnLengths[c]));
                    var rowStr = string.Join(_separator, values);
                    yield return rowStr.SafeSubstring(0, maxLength);
                }
            }
        }
    }
}