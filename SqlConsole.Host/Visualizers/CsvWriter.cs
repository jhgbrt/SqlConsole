using System;
using System.Data;
using System.IO;
using System.Linq;

namespace SqlConsole.Host.Visualizers
{
    class CsvWriter : IResultProcessor<DataTable>
    {
        private string _outputFile;

        public CsvWriter(string outputFile)
        {
            _outputFile = outputFile;
        }

        public void Process(DataTable result)
        {
            WriteToFile(result, _outputFile);
        }

        private static void WriteToFile(DataTable dt, string fileName)
        {
            using (var sw = new StreamWriter(fileName))
            {
                WriteToFile(dt, sw);
            }
        }

        private static void WriteToFile(DataTable dt, StreamWriter writer)
        {
            if (dt == null || writer == null) return;

            string[] columnNames = dt.Columns.OfType<DataColumn>().Select(column => "\"" + column.ColumnName.Replace("\"", "\"\"") + "\"").ToArray();
            writer.WriteLine(String.Join(",", columnNames));
            foreach (DataRow row in dt.Rows)
            {
                string[] fields = row.ItemArray.Select(field => "\"" + field.ToString().Replace("\"", "\"\"") + "\"").ToArray();
                writer.WriteLine(String.Join(";", fields));
                writer.Flush();
            }
        }
    }
}