using System.IO;

namespace SqlConsole.Host.Visualizers
{
    class FileWriter : IResultProcessor<object>
    {
        private readonly string _outputFile;

        public FileWriter(string outputFile)
        {
            _outputFile = outputFile;
        }

        public void Process(object result)
        {
            File.WriteAllText(_outputFile, result.ToString());
        }
    }
}