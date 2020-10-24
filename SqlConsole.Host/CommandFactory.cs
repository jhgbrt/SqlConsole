using Net.Code.ADONet;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SqlConsole.Host
{
    static partial class CommandFactory
    {
        public static Command CreateCommand()
        {
            var console = CreateProviderCommands(DoRepl)
                .Aggregate(
                    new Command("console", "Run an interactive SQL console"), 
                    (parent, child) => parent.WithChildCommand(child)
                    );

            var query = CreateProviderCommands(DoQuery)
                .Aggregate(
                    new Command("query", $"Run an SQL query inline or from a file"), 
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
        static IEnumerable<Command> CreateProviderCommands(Action<Provider, DbConnectionStringBuilder, QueryOptions, IConsole> execute) 
            => (
                from dynamic p in Provider.All
                select CreateCommand(p, execute)
                )
                .OfType<Command>();

        static Command CreateCommand<TConnectionStringBuilder, TOptions>(
            Provider<TConnectionStringBuilder> provider, Action<Provider, DbConnectionStringBuilder, TOptions, IConsole> execute)
            where TConnectionStringBuilder : DbConnectionStringBuilder, new()
        {
            var options = provider.GetDbConnectionStringBuilderProperties().ToOptions();
            var command = new Command(provider.Name)
            {
                Handler = CommandHandler.Create((TConnectionStringBuilder builder, TOptions options, IConsole console) =>
                {
                    execute(provider, builder, options, console);
                })
            }.WithOptions(options);

            return command;
        }
        static void DoRepl(Provider provider, DbConnectionStringBuilder builder, QueryOptions options, IConsole console)
        {
            using var queryHandler = CreateQueryHandler(provider, builder, options, console);
            new Repl(queryHandler).Enter();
        }

        static void DoQuery(Provider provider, DbConnectionStringBuilder builder, QueryOptions options, IConsole console)
        {
            using var queryHandler = CreateQueryHandler(provider, builder, options, console);
            queryHandler.Execute(options.GetQuery());
        }

        // builder, options and console are injected by System.CommandLine
        private static IQueryHandler CreateQueryHandler(Provider provider, DbConnectionStringBuilder builder, QueryOptions options, IConsole console)
        {
            var connectionString = builder.ConnectionString;
            console.Out.WriteLine(builder.WithoutSensitiveInformation().ConnectionString);

            var db = new Db(connectionString, provider.DbConfig, provider.Factory);
            db.Connect();

            return options switch
            {
                { AsScalar: true } => new QueryHandler<object>(db, console.Out, cb => cb.AsScalar(), new ScalarFormatter()),
                { AsNonquery: true } => new QueryHandler<int>(db, console.Out, cb => cb.AsNonQuery(), new NonQueryFormatter()),
                _ when options.Output != null => new QueryHandler<DataTable>(db, options.GetWriter(console), cb => cb.AsDataTable(), new CsvFormatter()),
                _ => new QueryHandler<DataTable>(db, console.Out, cb => cb.AsDataTable(), new ConsoleTableFormatter(GetWindowWidth(), " | "))
            };
        }

        private static int GetWindowWidth()
        {
            try { return Console.WindowWidth; } catch { return 120; }
        }
    }

    
}