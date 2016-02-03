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
            var writer = _config.OutputToFile ? new StreamWriter(_config.Output) : Console.Out;

            var db = new Db(_config.ConnectionString, _config.ProviderName);

            if (_config.Scalar)
            {
                return new QueryHandler<object>(db, writer, cb => cb.AsScalar(), new ScalarResultWriter(writer));
            }

            if (_config.NonQuery)
            {
                return new QueryHandler<int>(db, writer, cb => cb.AsNonQuery(), new NonQueryResultWriter(writer));
            }

            var formatter = _config.OutputToFile
                ? (ITextFormatter<DataTable>) new CsvFormatter()
                : new ConsoleTableFormatter(GetWindowWidth(), " | ");

            return new QueryHandler<DataTable>(db, writer, cb => cb.AsDataTable(), new DataTableWriter(writer, formatter));
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
                try { return f(); } catch { }
            }
            return null;
        }
    }
}