using System;
using System.CommandLine.IO;
using System.IO;
using Net.Code.ADONet;

namespace SqlConsole.Host
{



    class QueryHandler<TQueryResult> : IQueryHandler
    {
        private readonly Func<CommandBuilder, TQueryResult> _do;
        private readonly ITextFormatter<TQueryResult> _formatter;
        private readonly IDb _db;
        private readonly TextWriter _writer;

        public QueryHandler(IDb db, TextWriter writer, Func<CommandBuilder, TQueryResult> @do, ITextFormatter<TQueryResult> formatter)
        {
            _db = db;
            _writer = writer;
            _do = @do;
            _formatter = formatter;
        }

        public void Execute(string query)
        {
            foreach (var script in query.SplitOnGo())
            {
                var cb = _db.Sql(script);
                var result = _do(cb);
                foreach (var s in _formatter.Format(result))
                {
                    _writer.WriteLine(s);
                }
            }
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}

