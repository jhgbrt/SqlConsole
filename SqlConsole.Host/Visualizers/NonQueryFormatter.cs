using System.Collections.Generic;

namespace SqlConsole.Host
{
    class NonQueryFormatter : ITextFormatter<int>
    {
        public IEnumerable<string> Format(int result)
        {
            var r = result;
            if (r < 0) yield break;
            var s = r == 1 ? "" : "s";
            yield return $"{result} row{s} affected";
        }
    }
}