namespace SqlConsole.Host;

/// <summary>
/// Abstraction for console output with styling and timing capabilities
/// </summary>
internal interface IConsoleRenderer
{
    /// <summary>
    /// Write an error message with appropriate styling
    /// </summary>
    /// <param name="message">The error message to display</param>
    void WriteError(string message);
    
    /// <summary>
    /// Write a connection status message with appropriate styling
    /// </summary>
    /// <param name="status">The connection status to display</param>
    void WriteConnectionStatus(string status);
    
    /// <summary>
    /// Write timing information with color thresholds
    /// </summary>
    /// <param name="elapsed">The elapsed time</param>
    void WriteTiming(TimeSpan elapsed);
    
    /// <summary>
    /// Write timing and row statistics information with color thresholds
    /// </summary>
    /// <param name="elapsed">The elapsed time</param>
    /// <param name="rowCount">Number of rows affected or returned (null if unknown)</param>
    void WriteTimingAndRows(TimeSpan elapsed, int? rowCount);
}