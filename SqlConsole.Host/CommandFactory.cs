using Net.Code.ADONet;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SqlConsole.Host
{
    static partial class CommandFactory
    {
        public static Command CreateCommand()
        {
            var console = CreateProviderCommands((queryHandler, options) => new Repl(queryHandler).Enter())
                .Aggregate(
                    new Command("console", "Run an interactive SQL console. Run the help command for a specific provider (console [provider] -h) for more info."), 
                    (parent, child) => parent.WithChildCommand(child)
                    );

            var query = CreateProviderCommands((queryHandler, options) => queryHandler.Execute(options.GetQuery()))
                .Aggregate(
                    new Command("query", $"Run an SQL query inline or from a file. Run the help command for a specific provider (console [provider] -h) for more info."), 
                    (parent, child) => parent.WithChildCommand(child.WithArgument(new Argument("query"))
                                             .WithOptions(typeof(QueryOptions).GetOptions()))
                );

            return new RootCommand
            {
                Description = $"A generic SQL utility tool for running SQL queries, either directly from the commandline or in an interactive console. " +
                    $"Supports the following providers: {string.Join(",", Provider.All.Select(p => p.Name))}. " +
                    $"Use the help function of each command for info on how to connect."
            }.WithChildCommands(console, query);
        }
        static IEnumerable<Command> CreateProviderCommands(Action<IQueryHandler, QueryOptions> execute) 
            => (
                from dynamic p in Provider.All
                select CreateCommand(p, execute)
                )
                .OfType<Command>();

        static Command CreateCommand<TConnectionStringBuilder>(
            Provider<TConnectionStringBuilder> provider, Action<IQueryHandler, QueryOptions> execute)
            where TConnectionStringBuilder : DbConnectionStringBuilder, new()
        {
            var options = provider.ConnectionConfigurationProperties().ToOptions();
            var command = new Command(provider.Name)
            {
                Handler = CommandHandler.Create((TConnectionStringBuilder builder, QueryOptions options, IConsole console) =>
                {
                    using var queryHandler = CreateQueryHandler(provider, builder, options, console);
                    execute(queryHandler, options);
                })
            }.WithOptions(options);

            return command;
        }

        // builder, options and console are injected by System.CommandLine
        private static IQueryHandler CreateQueryHandler(Provider provider, DbConnectionStringBuilder builder, QueryOptions options, IConsole console)
        {
            var connectionString = builder.ConnectionString;
            //console.Out.WriteLine(builder.WithoutSensitiveInformation().ConnectionString);

            var db = new Db(connectionString, provider.DbConfig, provider.Factory);
            db.Connect();

            return options switch
            {
                { AsScalar: true } => new QueryHandler<object>(db, console.Out, cb => cb.AsScalar(), new ScalarFormatter()),
                { AsNonquery: true } => new QueryHandler<int>(db, console.Out, cb => cb.AsNonQuery(), new NonQueryFormatter()),
                _ when options.Output != null => new QueryHandler<DataTable>(db, options.GetWriter(console), cb => cb.AsDataTable(), new CsvFormatter()),
                _ => new QueryHandler<DataTable>(db, console.Out, cb => cb.AsDataTable(), new ConsoleTableFormatter(GetWindowWidth(), " | "))
            };
            
            static int GetWindowWidth()
            {
                try { return Console.WindowWidth; } catch { return 120; }
            }
        }

      
    }

    
}