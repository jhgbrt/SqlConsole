using Net.Code.ADONet;
using SqlConsole.Host.Infrastructure;

using System.CommandLine;
using System.CommandLine.Invocation;

namespace SqlConsole.Host;

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

        var completion = CompletionCommand.Create();

        return new RootCommand
        {
            Description =
                "A generic SQL utility tool for running SQL queries, either directly " +
                "from the commandline or in an interactive console.\n\n" +
                HelpFormatter.FormatProviderList() + ".\n\n" +
                "Use the help function of each command for info on how to connect.\n\n" +
                HelpFormatter.Colorize("Quick start:", ConsoleColor.Magenta) + "\n" +
                "sqlc query sqlite --data-source \":memory:\" \"SELECT 'Hello World'\"\n" +
                "sqlc completion --shell bash  # Generate completion script"
        }.WithChildCommands(console, query, completion);
    }
    static IEnumerable<Command> CreateProviderCommands<T>() where T : ICommand, new()
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
        var baseDescription = $"Connect to {provider.Name} database";
        var enhancedDescription = HelpFormatter.CreateProviderDescription(provider.Name, baseDescription);
        
        return new Command(provider.Name)
        {
            Description = enhancedDescription,
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
            { AsScalar: true } => ScalarQueryHandler(provider, builder, options, console),
            { AsNonquery: true } => NonQueryQueryHandler(provider, builder, options, console),
            _ when options.Output != null => OutputToFileQueryHandler(provider, builder, options, console),
            _ => InteractiveQueryHandler(provider, builder, options, console)
        };


        static IQueryHandler ScalarQueryHandler(Provider provider, DbConnectionStringBuilder builder, QueryOptions options, IConsole console)
        {
            var formatter = new ScalarFormatter();
            var connectionString = builder.ConnectionString;
            var writer = console.Out;
            static object @do(CommandBuilder cb) => cb.AsScalar();
            return new QueryHandler<object>(provider, connectionString, writer, @do, formatter);
        }

        static IQueryHandler NonQueryQueryHandler(Provider provider, DbConnectionStringBuilder builder, QueryOptions options, IConsole console)
        {
            var formatter = new NonQueryFormatter();
            var connectionString = builder.ConnectionString;
            var writer = console.Out;
            static int @do(CommandBuilder cb) => cb.AsNonQuery();
            return new QueryHandler<int>(provider, connectionString, writer, @do, formatter);
        }

        static IQueryHandler OutputToFileQueryHandler(Provider provider, DbConnectionStringBuilder builder, QueryOptions options, IConsole console)
        {
            var formatter = new CsvFormatter();
            var connectionString = builder.ConnectionString;
            var writer = new MyTextWriter(new StreamWriter(options.Output!.OpenWrite(), Encoding.UTF8));
            static DataTable @do(CommandBuilder cb) => cb.AsDataTable();
            return new QueryHandler<DataTable>(provider, connectionString, writer, @do, formatter);
        }
        static IQueryHandler InteractiveQueryHandler(Provider provider, DbConnectionStringBuilder builder, QueryOptions options, IConsole console)
        {
            var formatter = new ConsoleTableFormatter(GetWindowWidth(), " | ");
            var connectionString = builder.ConnectionString;
            var writer = console.Out;
            static DataTable @do(CommandBuilder cb) => cb.AsDataTable();
            return new QueryHandler<DataTable>(provider, connectionString, writer, @do, formatter);
            static int GetWindowWidth()
            {
                try { return Console.WindowWidth; } catch { return 120; }
            }

        }
    }

}
