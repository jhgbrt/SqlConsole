using System.Collections.Generic;

namespace SqlConsole.Host
{
    class ScalarFormatter : ITextFormatter<object>
    {
        public IEnumerable<string> Format(object item)
        {
            yield return item?.ToString() ?? string.Empty;
        }
    }
}