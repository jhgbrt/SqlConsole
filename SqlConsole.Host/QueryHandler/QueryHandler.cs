using System;
using System.IO;
using Net.Code.ADONet;

namespace SqlConsole.Host
{



    class QueryHandler<TQueryResult> : IQueryHandler
    {
        private readonly Func<CommandBuilder, TQueryResult> _do;
        private readonly IResultProcessor<TQueryResult> _resultProcessor;
        private readonly IDb _db;
        private readonly TextWriter _writer;

        public QueryHandler(IDb db, TextWriter writer, Func<CommandBuilder, TQueryResult> @do, IResultProcessor<TQueryResult> resultProcessor)
        {
            _db = db;
            _writer = writer;
            _resultProcessor = resultProcessor;
            _do = @do;
        }

        public void Execute(string query)
        {
            foreach (var script in query.SplitOnGo())
            {
                var cb = _db.Sql(script);
                var result = _do(cb);
                _resultProcessor.Process(result);
            }
        }

        public void Dispose()
        {
            _db.Dispose();
            _writer.Dispose();
        }
    }
}

