using System;
using System.Configuration;
using System.Data;
using System.Linq;

namespace SqlConsole.Host.Visualizers
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
            var columnLengths = result.ColumnLengths(_windowWidth, _separator.Length);
            var columnNames = result.Columns.OfType<DataColumn>().Select(c => c.ColumnName.OfLength(columnLengths[c])).ToList();
            Console.WriteLine(string.Join(_separator, columnNames));
            Console.WriteLine(string.Join("-|-", columnNames.Select(c => new string(Enumerable.Repeat('-', c.Length).ToArray()))));
            foreach (var item in result.Rows.OfType<DataRow>())
            {
                var row = item;
                var values = result.Columns.OfType<DataColumn>().Select(c => row[c].ToString().OfLength(columnLengths[c]));
                Console.WriteLine(string.Join(_separator, values));
            }
        }

        static T? Try<T>(params Func<T>[] functions) where T : struct
        {
            foreach (var f in functions)
            {
                try
                {
                    return f();
                }
                catch
                {
                    continue;
                }
            }
            return null;
        }
    }
}