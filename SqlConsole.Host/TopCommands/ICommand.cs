using static SqlConsole.Host.CommandFactory;

namespace SqlConsole.Host;

internal interface ICommand
{
    public void Execute(IQueryHandler queryHandler, QueryOptions options, IConsoleRenderer renderer);
}
