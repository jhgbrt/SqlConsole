using System.CommandLine.IO;

using Net.Code.ADONet;

namespace SqlConsole.Host;

class QueryHandler<TQueryResult> : IQueryHandler
{
    private readonly Func<CommandBuilder, TQueryResult> _do;
    private readonly ITextFormatter<TQueryResult> _formatter;
    private readonly IDb _db;
    private readonly IStandardStreamWriter _writer;
    private readonly Provider _provider;

    public QueryHandler(Provider provider, string connectionString, IStandardStreamWriter writer, Func<CommandBuilder, TQueryResult> @do, ITextFormatter<TQueryResult> formatter)
    {
        _provider = provider;
        _writer = writer;
        _do = @do;
        _formatter = formatter;
        _db = new Db(connectionString, provider.DbConfig, provider.Factory);
    }

    public string ConnectionStatus => $"[{_provider.Name} - {DbConnectionStatus}])";
    private string DbConnectionStatus => _db.Connection.State switch
    {
        ConnectionState.Open => "connected",
        _ => "disconnected"
    };


    public void Execute(string query)
    {
        _db.Connect();
        foreach (var script in query.SplitOnGo())
        {
            var cb = _db.Sql(script);
            var result = _do(cb);
            foreach (var s in _formatter.Format(result))
            {
                _writer.WriteLine(s);
            }
        }
    }
    public void Connect()
    {
        _db.Connect();
    }
    public void Disconnect()
    {
        _db.Disconnect();
    }

    public void Dispose()
    {
        _db.Dispose();
        if (_writer is IDisposable d) d.Dispose();
    }
}

