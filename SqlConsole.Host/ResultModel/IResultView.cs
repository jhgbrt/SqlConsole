namespace SqlConsole.Host.ResultModel;

/// <summary>
/// Represents a semantic result view that describes "what to show" 
/// in a renderer-agnostic way
/// </summary>
public interface IResultView
{
    /// <summary>
    /// The type of result view for renderer selection
    /// </summary>
    ResultViewType Type { get; }
}

/// <summary>
/// Types of result views supported by the system
/// </summary>
public enum ResultViewType
{
    /// <summary>
    /// Tabular data with headers and rows
    /// </summary>
    Table,
    
    /// <summary>
    /// Single scalar value
    /// </summary>
    Scalar,
    
    /// <summary>
    /// Result from non-query operations (rows affected)
    /// </summary>
    NonQuery
}