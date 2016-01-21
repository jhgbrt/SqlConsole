using System;

namespace SqlConsole.Host
{
    class NonQueryResultWriter<T> : IResultProcessor<T>
    {
        public void Process(T result)
        {
            if (!(result is int)) throw new InvalidOperationException("NonQuery result writer can only process integer results");
            var r = (int)(object)result;
            if (r < 0) return;
            var s = r == 1 ? "" : "s";
            Console.WriteLine($"{result} row{s} affected");
        }
    }

    class NonQueryResultWriter : NonQueryResultWriter<int>
    {
    }
}