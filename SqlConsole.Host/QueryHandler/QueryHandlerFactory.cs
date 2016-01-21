using System.Data;

namespace SqlConsole.Host
{

    static class QueryHandlerFactory
    {
        public static IQueryHandler Create(Config config)
        {
            if (config.Scalar)
            {
                return Scalar(config);
            }

            if (config.NonQuery)
            {
                return NonQuery(config);
            }

            return DataTable(config);
        }

        public static IQueryHandler Scalar(Config config, IResultProcessor<object> resultProcessor = null)
        {
            return new QueryHandler<object>(
                config,
                (db, query) => db.Sql(query).AsScalar(),
                resultProcessor ?? ScalarResultProcessor(config)
                );
        }

        public static IQueryHandler NonQuery(Config config, IResultProcessor<int> resultProcessor = null)
        {
            return new QueryHandler<int>(
                config,
                (db, query) => db.Sql(query).AsNonQuery(),
                resultProcessor ?? new NonQueryResultWriter()
                );
        }

        public static IQueryHandler DataTable(Config config, IResultProcessor<DataTable> resultProcessor = null)
        {
            return new QueryHandler<DataTable>(
                config,
                (db, query) => db.Sql(query).AsDataTable(),
                resultProcessor ?? DataTableProcessor(config)
                );
        }

        static IResultProcessor<object> ScalarResultProcessor(Config config)
        {
            return !string.IsNullOrEmpty(config.Output)
                    ? (IResultProcessor<object>)new FileWriter(config.Output)
                    : new ConsoleWriter();
        }

        static IResultProcessor<DataTable> DataTableProcessor(Config config)
        {
            return !string.IsNullOrEmpty(config.Output)
                    ? new CsvWriter(config.Output)
                    : (IResultProcessor<DataTable>)new ConsoleTableVisualizer();
        }


    }
}