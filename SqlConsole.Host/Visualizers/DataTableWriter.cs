using System.Data;
using System.IO;

namespace SqlConsole.Host
{
    class DataTableWriter : IResultProcessor<DataTable>
    {
        private readonly TextWriter _writer;
        private readonly ITextFormatter<DataTable> _formatter; 

        public DataTableWriter(TextWriter writer, ITextFormatter<DataTable> formatter)
        {
            _writer = writer;
            _formatter = formatter;
        }

        public void Process(DataTable result)
        {
            foreach (var line in _formatter.Format(result))
            {
                _writer.WriteLine(line);
                _writer.Flush();
            }
        }

    }
}