using System;

namespace SqlConsole.Host.Visualizers
{
    class ConsoleWriter<T> : IResultProcessor<T>
    {
        public void Process(T result)
        {
            Console.WriteLine(result);
        }
    }

    class ConsoleWriter : ConsoleWriter<object>
    {
    }
}