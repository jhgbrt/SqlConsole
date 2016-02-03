using System;

namespace SqlConsole.Host
{
    interface IQueryHandler : IDisposable
    {
        void Execute(string query);
    }
}