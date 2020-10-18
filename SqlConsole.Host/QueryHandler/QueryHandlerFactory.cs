using System;
using System.Configuration;
using System.Data;
using System.IO;
using Net.Code.ADONet;

namespace SqlConsole.Host
{
  
    class QueryHandlerFactory
    {
        private readonly Config _config;

        public QueryHandlerFactory(Config config)
        {
            _config = config;
        }

        public IQueryHandler Create()
        {
            var provider = _config.Provider;
            var dbconfig = provider.DbConfig;
            var factory = provider.Factory;
            var writer = _config.OutputToFile ? new StreamWriter(_config.Output) : Console.Out;
            
            var db = new Db(_config.ConnectionString, dbconfig, factory);
            db.Connect();

            if (_config.Scalar)
            {
                return new QueryHandler<object>(db, writer, cb => cb.AsScalar(), new ScalarFormatter());
            }

            if (_config.NonQuery)
            {
                return new QueryHandler<int>(db, writer, cb => cb.AsNonQuery(), new NonQueryFormatter());
            }

            var formatter = _config.OutputToFile
                ? (ITextFormatter<DataTable>) new CsvFormatter()
                : new ConsoleTableFormatter(GetWindowWidth(), " | ");

            return new QueryHandler<DataTable>(db, writer, cb => cb.AsDataTable(), formatter);
        }
        private static int GetWindowWidth()
        {
            return Try(
                () => Console.WindowWidth,
                () => int.Parse(ConfigurationManager.AppSettings["WindowWidth"])
                ) ?? 120;
        }
        private static T? Try<T>(params Func<T>[] functions) where T : struct
        {
            foreach (var f in functions)
            {
                // ReSharper disable once EmptyGeneralCatchClause
                try { return f(); } catch { }
            }
            return null;
        }
    }
}