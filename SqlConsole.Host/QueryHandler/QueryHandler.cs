using System.CommandLine.IO;
using System.Diagnostics;

using Net.Code.ADONet;

namespace SqlConsole.Host;

class QueryHandler<TQueryResult> : IQueryHandler
{
    private readonly Func<CommandBuilder, TQueryResult> _do;
    private readonly ITextFormatter<TQueryResult> _formatter;
    private readonly IDb _db;
    private readonly IStandardStreamWriter _writer;
    private readonly Provider _provider;
    private readonly IConsoleRenderer _renderer;
    private readonly bool _showTiming;

    public QueryHandler(Provider provider, string connectionString, IStandardStreamWriter writer, Func<CommandBuilder, TQueryResult> @do, ITextFormatter<TQueryResult> formatter, IConsoleRenderer renderer, bool showTiming = true)
    {
        _provider = provider;
        _writer = writer;
        _do = @do;
        _formatter = formatter;
        _renderer = renderer;
        _db = new Db(connectionString, provider.DbConfig, provider.Factory);
        _showTiming = showTiming;
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
        var stopwatch = Stopwatch.StartNew();
        int? totalRowCount = null;
        
        foreach (var script in query.SplitOnGo())
        {
            var cb = _db.Sql(script);
            var result = _do(cb);
            
            // Extract row count based on result type
            var rowCount = ExtractRowCount(result);
            if (rowCount.HasValue)
            {
                totalRowCount = (totalRowCount ?? 0) + rowCount.Value;
            }
            
            foreach (var s in _formatter.Format(result))
            {
                _writer.WriteLine(s);
            }
        }
        
        stopwatch.Stop();
        
        if (_showTiming)
        {
            _renderer.WriteTimingAndRows(stopwatch.Elapsed, totalRowCount);
        }
    }

    public void Execute(string query, bool showTiming)
    {
        _db.Connect();
        var stopwatch = Stopwatch.StartNew();
        int? totalRowCount = null;
        
        foreach (var script in query.SplitOnGo())
        {
            var cb = _db.Sql(script);
            var result = _do(cb);
            
            // Extract row count based on result type
            var rowCount = ExtractRowCount(result);
            if (rowCount.HasValue)
            {
                totalRowCount = (totalRowCount ?? 0) + rowCount.Value;
            }
            
            foreach (var s in _formatter.Format(result))
            {
                _writer.WriteLine(s);
            }
        }
        
        stopwatch.Stop();
        
        if (showTiming)
        {
            _renderer.WriteTimingAndRows(stopwatch.Elapsed, totalRowCount);
        }
    }

    private int? ExtractRowCount(TQueryResult result)
    {
        return result switch
        {
            DataTable dt => dt.Rows.Count,
            int rowsAffected => rowsAffected >= 0 ? rowsAffected : null,
            _ => null
        };
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

