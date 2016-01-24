using System;
using System.IO;

namespace SqlConsole.Host
{
    class ConsoleWriter<T> : IResultProcessor<T>
    {
        private readonly TextWriter _textWriter = Console.Out;

        public void Process(T result)
        {
            _textWriter.WriteLine(result);
        }
    }

    class ConsoleWriter : ConsoleWriter<object>
    {
    }
}