using System;
using System.Data.Common;
using Subtext.Scripting;

namespace SqlConsole.Host
{

    class QueryHandler<TQueryResult> : IQueryHandler
    {
        private readonly Config _config;
        readonly Func<IDb, string, TQueryResult> _runQuery;
        private readonly IResultProcessor<TQueryResult> _resultProcessor;

        public QueryHandler(Config config, Func<IDb, string, TQueryResult> runQuery, IResultProcessor<TQueryResult> resultProcessor)
        {
            _config = config;
            _runQuery = runQuery;
            _resultProcessor = resultProcessor;
        }

        public void Execute(string query)
        {
            var splitter = new ScriptSplitter(query);
            try
            {
                var providerName = _config.Provider.Name;
                using (var db = new Db(_config.ConnectionString, _config.Provider == Provider.Default ? null : providerName))
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
            }

        }
    }
}
