using Net.Code.ADONet;
using SqlConsole.Host.QueryHandler;
using SqlConsole.Host.Rendering;
using SqlConsole.Host.ResultModel;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace SqlConsole.Host;

static partial class CommandFactory
{
    public static Command CreateCommand()
    {
        var console = CreateProviderCommands<Repl>(isRepl: true)
            .Aggregate(
                new Command("console",
                "Run an interactive SQL console. Run the help command for a specific " +
                "provider (console [provider] -h) for more info."),
                (parent, child) => parent.WithChildCommand(child)
                );

        var query = CreateProviderCommands<SingleQuery>(isRepl: false)
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
    static IEnumerable<Command> CreateProviderCommands<T>(bool isRepl) where T : ICommand, new()
        => (
            from dynamic p in Provider.All
            select CreateCommand(p, new T(), isRepl)
            )
            .OfType<Command>();

    static Command CreateCommand<TConnectionStringBuilder, TCommand>(
        Provider<TConnectionStringBuilder> provider, TCommand commandHandler, bool isRepl)
        where TConnectionStringBuilder : DbConnectionStringBuilder
        where TCommand : ICommand
    {
        var options = provider.ConnectionConfigurationProperties().ToOptions();
        return new Command(provider.Name)
        {
            Handler = CommandHandler.Create((TConnectionStringBuilder builder, QueryOptions options, IConsole console) =>
            {
                var renderer = ConsoleRendererFactory.Create(options);
                using var queryHandler = CreateQueryHandler(provider, builder, options, console, isRepl);
                commandHandler.Execute(queryHandler, options, renderer);
            })
        }.WithOptions(options);
    }

    // builder, options and console are injected by System.CommandLine
    private static IQueryHandler CreateQueryHandler(Provider provider, DbConnectionStringBuilder builder, QueryOptions options, IConsole console, bool isRepl = false)
    {
        var consoleRenderer = ConsoleRendererFactory.Create(options);
        var outputMode = options.GetOutputMode();
        
        IStandardStreamWriter writer;
        IDisposable? disposableWriter = null;
        
        if (options.Output != null)
        {
            var fileWriter = new MyTextWriter(new StreamWriter(options.Output.OpenWrite(), Encoding.UTF8));
            writer = fileWriter;
            disposableWriter = fileWriter;
        }
        else
        {
            writer = console.Out;
        }
        
        var resultRenderer = ResultRendererFactory.Create(outputMode, writer, consoleRenderer);
        var connectionString = builder.ConnectionString;
        
        return options switch
        {
            { AsScalar: true } => CreateSemanticQueryHandler(provider, connectionString, 
                cb => new ScalarView(cb.AsScalar()), resultRenderer, disposableWriter),
            { AsNonquery: true } => CreateSemanticQueryHandler(provider, connectionString, 
                cb => new NonQueryView(cb.AsNonQuery()), resultRenderer, disposableWriter),
            _ => CreateSemanticQueryHandler(provider, connectionString, 
                cb => TableView.FromDataTable(cb.AsDataTable()), resultRenderer, disposableWriter)
        };
    }
    
    private static IQueryHandler CreateSemanticQueryHandler(
        Provider provider, 
        string connectionString, 
        Func<CommandBuilder, IResultView> execute,
        IResultRenderer renderer,
        IDisposable? disposableWriter = null)
    {
        return new SemanticQueryHandler(provider, connectionString, execute, renderer, disposableWriter);
    }

}
