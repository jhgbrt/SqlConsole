using System.Diagnostics;
using Net.Code.ADONet;
using SqlConsole.Host.Rendering;
using SqlConsole.Host.ResultModel;

namespace SqlConsole.Host.QueryHandler;

/// <summary>
/// Query handler that produces semantic result views for renderer consumption
/// </summary>
internal class SemanticQueryHandler : IQueryHandler
{
    private readonly Func<CommandBuilder, IResultView> _execute;
    private readonly IResultRenderer _renderer;
    private readonly IDb _db;
    private readonly Provider _provider;
    private readonly IDisposable? _disposableWriter;

    public SemanticQueryHandler(
        Provider provider, 
        string connectionString, 
        Func<CommandBuilder, IResultView> execute,
        IResultRenderer renderer,
        IDisposable? disposableWriter = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _disposableWriter = disposableWriter;
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
        var stopwatch = Stopwatch.StartNew();
        
        foreach (var script in query.SplitOnGo())
        {
            var cb = _db.Sql(script);
            var result = _execute(cb);
            _renderer.Render(result);
        }
        
        stopwatch.Stop();
        
        // Only write timing for interactive renderers
        if (_renderer is SpectreInteractiveRenderer spectreRenderer)
        {
            spectreRenderer.WriteTiming(stopwatch.Elapsed);
        }
        else if (_renderer is NoColorInteractiveRenderer noColorRenderer)
        {
            noColorRenderer.WriteTiming(stopwatch.Elapsed);
        }
        // CSV renderer doesn't get timing information
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
        _disposableWriter?.Dispose();
    }

    /// <summary>
    /// Gets the underlying database connection for schema operations
    /// </summary>
    internal IDb GetDb() => _db;
}