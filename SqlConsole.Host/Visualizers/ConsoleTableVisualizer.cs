using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;

namespace SqlConsole.Host
{
    class ConsoleTableVisualizer : IResultProcessor<DataTable>
    {
        private readonly int _windowWidth;
        private readonly string _separator;
        private readonly TextWriter _textWriter;

        public ConsoleTableVisualizer() : this(GetWindowWidth(), Console.Out)
        {
            
        }

        internal ConsoleTableVisualizer(int windowWidth, TextWriter textWriter)
        {
            _windowWidth = windowWidth;
            _separator = " | ";
            _textWriter = textWriter;
        }

        public void Process(DataTable result)
        {
            if (result.Rows.Count == 0)
            {
                _textWriter.WriteLine("ok");
                return;
            }

            if (result.Rows.Count == 1 && result.Columns.Count == 1)
            {
                _textWriter.WriteLine(result.Rows[0][0]);
                return;
            }

            var maxLength = _windowWidth - 1;
            var columnLengths = result.ColumnLengths(maxLength, _separator.Length);
            var columnNames = result.Columns.OfType<DataColumn>().Select(c => c.ColumnName.OfLength(columnLengths[c])).ToList();
            var joinedColumnNames = string.Join(_separator, columnNames);
            var line = string.Join("-|-", columnNames.Select(c => new string(Enumerable.Repeat('-', c.Length).ToArray())));

            _textWriter.WriteLine(joinedColumnNames.SafeSubstring(0, maxLength));
            _textWriter.WriteLine(line.SafeSubstring(0, maxLength));

            foreach (var item in result.Rows.OfType<DataRow>())
            {
                var row = item;
                var values = result.Columns.OfType<DataColumn>().Select(c => row[c].ToString().OfLength(columnLengths[c]));
                var rowStr = string.Join(_separator, values);
                _textWriter.WriteLine(rowStr.SafeSubstring(0, maxLength));
            }
        }
        private static int GetWindowWidth()
        {
            return Try(
                () => Console.WindowWidth,
                () => int.Parse(ConfigurationManager.AppSettings["WindowWidth"])
                ) ?? 120;
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