using System.IO;

namespace SqlConsole.Host
{
    class ScalarResultWriter : IResultProcessor<object>
    {
        private readonly TextWriter _textWriter;

        public ScalarResultWriter(TextWriter textWriter)
        {
            _textWriter = textWriter;
        }

        public void Process(object result)
        {
            _textWriter.WriteLine(result);
            _textWriter.Flush();
        }
    }
}