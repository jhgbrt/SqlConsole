
using static System.Console;
using static SqlConsole.Host.CommandFactory;

namespace SqlConsole.Host;

internal class SingleQuery : ICommand
{
    public void Execute(IQueryHandler queryHandler, QueryOptions options, IConsoleRenderer renderer)
    {
        var query = options.GetQuery();
        try
        {
            queryHandler.Execute(query);
        }
        catch (DbException e)
        {
            renderer.WriteError(e.Message);
        }
    }

}
