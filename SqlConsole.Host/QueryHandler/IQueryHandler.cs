namespace SqlConsole.Host;

interface IQueryHandler : IDisposable
{
    public string ConnectionStatus { get; }
    public void Disconnect();
    public void Connect();
    void Execute(string query);
    /// <summary>
    /// Gets the execution time of the last query in milliseconds
    /// </summary>
    public TimeSpan LastExecutionTime { get; }
}
