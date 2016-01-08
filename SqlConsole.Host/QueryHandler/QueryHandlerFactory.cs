using System;
using System.Data;
using SqlConsole.Host.Infrastructure;
using SqlConsole.Host.Visualizers;

namespace SqlConsole.Host
{

    static class QueryHandlerFactory
    {
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

        public static IQueryHandler NonQuery
        {
            get
            {
                return new QueryHandler<int>(
                    (db, query) => db.Sql(query).AsNonQuery(),
                    new NonQueryResultWriter()
                    );
            }
        }
        public static IQueryHandler DataTable
        {
            get
            {
                return new QueryHandler<DataTable>(
                    (db, query) => db.Sql(query).AsDataTable(),
                    new ConsoleTableVisualizer()
                    );
            }
        }
        public static IQueryHandler Scalar
        {
            get
            {
                return new QueryHandler<object>(
                    (db, query) => db.Sql(query).AsScalar(),
                    new ConsoleWriter()
                    );
            }
        }

        public static IQueryHandler Create(Config config)
        {
            if (config.Scalar)
            {
                return new QueryHandler<object>(
                    (db, query) => db.Sql(query).AsScalar(),
                    ScalarResultProcessor(config)
                    );
            }

            if (config.NonQuery)
            {
                return new QueryHandler<int>(
                    (db, query) => db.Sql(query).AsNonQuery(),
                    new NonQueryResultWriter()
                    );
            }

            return new QueryHandler<DataTable>(
                (db, query) => db.Sql(query).AsDataTable(),
                DataTableProcessor(config)
                );
        }


    }
}