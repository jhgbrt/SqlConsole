using System;
using System.Data.Common;

using static System.Console;
using static SqlConsole.Host.CommandFactory;

namespace SqlConsole.Host
{
    internal class SingleQuery : ICommand
    {
        public void Execute(IQueryHandler queryHandler, QueryOptions option)
        {
            var query = option.GetQuery();
            try
            {
                queryHandler.Execute(query);
            }
            catch (DbException e)
            {
                ForegroundColor = ConsoleColor.Red;
                Error.WriteLine(e.Message);
                ResetColor();
            }
        }

    }

}