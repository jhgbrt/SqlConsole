namespace SqlConsole.Host.ResultModel;

/// <summary>
/// Represents the result of a non-query operation (rows affected)
/// </summary>
public class NonQueryView : IResultView
{
    public ResultViewType Type => ResultViewType.NonQuery;
    
    /// <summary>
    /// Number of rows affected by the operation
    /// </summary>
    public int RowsAffected { get; }
    
    public NonQueryView(int rowsAffected)
    {
        RowsAffected = rowsAffected;
    }
}