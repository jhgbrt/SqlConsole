using System;

namespace SqlConsole.Host.Visualizers
{
    class NonQueryResultWriter<T> : IResultProcessor<T>
    {
        public void Process(T result)
        {
            if (!(result is int)) throw new InvalidOperationException("NonQuery result writer can only process integer results");
            int r = (int)(object)result;
            if (r >= 0)
            {
                var s = r == 1 ? "" : "s";
                Console.WriteLine("{0} row{1} affected", result, s);
            }
        }
    }

    class NonQueryResultWriter : NonQueryResultWriter<int>
    {
    }
}