using System;
using System.Configuration;
using System.Data;
using System.Linq;

namespace SqlConsole.Host
{
    class ConsoleTableVisualizer : IResultProcessor<DataTable>
    {
        private readonly int _windowWidth;
        private readonly string _separator;

        public ConsoleTableVisualizer()
        {
            _windowWidth = Try(
                () => Console.WindowWidth,
                () => int.Parse(ConfigurationManager.AppSettings["WindowWidth"])
                ) ?? 120;

            _separator = " | ";
        }

        public void Process(DataTable result)
        {
            var maxLength = _windowWidth - 1;
            var columnLengths = result.ColumnLengths(maxLength, _separator.Length);
            var columnNames = result.Columns.OfType<DataColumn>().Select(c => c.ColumnName.OfLength(columnLengths[c])).ToList();
            var joinedColumnNames = string.Join(_separator, columnNames);
            var line = string.Join("-|-", columnNames.Select(c => new string(Enumerable.Repeat('-', c.Length).ToArray())));

            Console.WriteLine(joinedColumnNames.SafeSubstring(0, maxLength));
            Console.WriteLine(line.SafeSubstring(0, maxLength));

            foreach (var item in result.Rows.OfType<DataRow>())
            {
                var row = item;
                var values = result.Columns.OfType<DataColumn>().Select(c => row[c].ToString().OfLength(columnLengths[c]));
                var rowStr = string.Join(_separator, values);
                Console.WriteLine(rowStr.SafeSubstring(0, maxLength));
            }
        }

        private static T? Try<T>(params Func<T>[] functions) where T : struct
        {
            foreach (var f in functions)
            {
                try
                {
                    return f();
                }
                catch
                {
                    // ReSharper disable once RedundantJumpStatement
                    continue;
                }
            }
            return null;
        }
    }
}