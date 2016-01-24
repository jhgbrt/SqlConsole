using System;
using System.IO;

namespace SqlConsole.Host
{
    class NonQueryResultWriter<T> : IResultProcessor<T>
    {
        private readonly TextWriter _textWriter = Console.Out;

        public void Process(T result)
        {
            if (!(result is int)) throw new InvalidOperationException("NonQuery result writer can only process integer results");
            var r = (int)(object)result;
            if (r < 0) return;
            var s = r == 1 ? "" : "s";
            _textWriter.WriteLine($"{result} row{s} affected");
        }
    }

    class NonQueryResultWriter : NonQueryResultWriter<int>
    {
    }
}