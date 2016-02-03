using System.IO;

namespace SqlConsole.Host
{
    class NonQueryResultWriter : IResultProcessor<int>
    {
        private readonly TextWriter _textWriter;

        public NonQueryResultWriter(TextWriter textWriter)
        {
            _textWriter = textWriter;
        }

        public void Process(int result)
        {
            var r = result;
            if (r < 0) return;
            var s = r == 1 ? "" : "s";
            _textWriter.WriteLine($"{result} row{s} affected");
        }
    }

}