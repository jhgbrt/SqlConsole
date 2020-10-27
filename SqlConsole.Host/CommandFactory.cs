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
            var console = CreateProviderCommands<Repl>()
                .Aggregate(
                    new Command("console", 
                    "Run an interactive SQL console. Run the help command for a specific " +
                    "provider (console [provider] -h) for more info."), 
                    (parent, child) => parent.WithChildCommand(child)
                    );

            var query = CreateProviderCommands<SingleQuery>()
                .Aggregate(
                    new Command("query", 
                    "Run a SQL query inline or from a file. Run the help command for a specific " +
                    "provider (query [provider] -h) for more info."), 
                    (parent, child) => parent.WithChildCommand(child.WithArgument(new Argument("query"))
                                             .WithOptions(typeof(QueryOptions).GetOptions()))
                );

            return new RootCommand
            {
                Description = 
                    "A generic SQL utility tool for running SQL queries, either directly " +
                    "from the commandline or in an interactive console. " +
                    $"Supports the following providers: {string.Join(", ", Provider.All.Select(p => p.Name))}. " +
                    "Use the help function of each command for info on how to connect."
            }.WithChildCommands(console, query);
        }
        static IEnumerable<Command> CreateProviderCommands<T>() where T: ICommand, new()
            => (
                from dynamic p in Provider.All
                select CreateCommand(p, new T())
                )
                .OfType<Command>();

        static Command CreateCommand<TConnectionStringBuilder, TCommand>(
            Provider<TConnectionStringBuilder> provider, TCommand commandHandler)
            where TConnectionStringBuilder : DbConnectionStringBuilder
            where TCommand : ICommand
        {
            var options = provider.ConnectionConfigurationProperties().ToOptions();
            return new Command(provider.Name)
            {
                Handler = CommandHandler.Create((TConnectionStringBuilder builder, QueryOptions options, IConsole console) =>
                {
                    using var queryHandler = CreateQueryHandler(provider, builder, options, console);
                    commandHandler.Execute(queryHandler, options);
                })
            }.WithOptions(options);
        }

        // builder, options and console are injected by System.CommandLine
        private static IQueryHandler CreateQueryHandler(Provider provider, DbConnectionStringBuilder builder, QueryOptions options, IConsole console)
        {
            return options switch
            {
                { AsScalar: true } => new QueryHandler<object>(provider, builder.ConnectionString, console.Out, cb => cb.AsScalar(), new ScalarFormatter()),
                { AsNonquery: true } => new QueryHandler<int>(provider, builder.ConnectionString, console.Out, cb => cb.AsNonQuery(), new NonQueryFormatter()),
                _ when options.Output != null => new QueryHandler<DataTable>(provider, builder.ConnectionString, options.GetWriter(console), cb => cb.AsDataTable(), new CsvFormatter()),
                _ => new QueryHandler<DataTable>(provider, builder.ConnectionString, console.Out, cb => cb.AsDataTable(), new ConsoleTableFormatter(GetWindowWidth(), " | "))
            };
            
            static int GetWindowWidth()
            {
                try { return Console.WindowWidth; } catch { return 120; }
            }
        }

      
    }

    
}