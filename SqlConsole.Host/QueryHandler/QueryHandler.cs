using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using SqlConsole.Host.Infrastructure;
using SqlConsole.Host.Visualizers;

namespace SqlConsole.Host
{

    class QueryHandler<TQueryResult> : IQueryHandler
    {
        readonly Func<IDb, string, TQueryResult> _runQuery;
        private readonly IResultProcessor<TQueryResult> _resultProcessor;

        public QueryHandler(Func<IDb, string, TQueryResult> runQuery, IResultProcessor<TQueryResult> resultProcessor)
        {
            _runQuery = runQuery;
            _resultProcessor = resultProcessor;
        }

        public void Execute(string query)
        {
            var splitter = new ScriptSplitter(query);
            try
            {
                using (var db = Db.FromConfig("Default"))
                {
                    foreach (var script in splitter)
                    {
                        var result = _runQuery(db, script);
                        _resultProcessor.Process(result);
                    }
                }
            }
            catch (DbException e)
            {
                Console.WriteLine(e.Message);
                //Console.WriteLine("Line number: {0}", e.LineNumber);
                //if (!string.IsNullOrEmpty(e.Procedure))
                //    Console.WriteLine("Procedure  : {0}", e.Procedure);
                //Console.WriteLine("Severity   : {0}", e.Class);
                //Console.WriteLine("State      : {0}", e.State);
            }

        }
    }
}
