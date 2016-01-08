using System.Data;

namespace SqlConsole.Host
{

    static class QueryHandlerFactory
    {
        public static IQueryHandler Create(Config config)
        {
            if (config.Scalar)
            {
                return Scalar(ScalarResultProcessor(config));
            }

            if (config.NonQuery)
            {
                return NonQuery();
            }

            return DataTable(DataTableProcessor(config));
        }

        public static IQueryHandler Scalar(IResultProcessor<object> resultProcessor)
        {
            return new QueryHandler<object>(
                (db, query) => db.Sql(query).AsScalar(),
                resultProcessor ?? new ConsoleWriter()
                );
        }

        public static IQueryHandler NonQuery()
        {
            return new QueryHandler<int>(
                (db, query) => db.Sql(query).AsNonQuery(),
                new NonQueryResultWriter()
                );
        }

        public static IQueryHandler DataTable(IResultProcessor<DataTable> resultProcessor = null)
        {
            return new QueryHandler<DataTable>(
                (db, query) => db.Sql(query).AsDataTable(),
                resultProcessor ?? new ConsoleTableVisualizer()
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