namespace SqlConsole.Host.ResultModel;

/// <summary>
/// Represents a single scalar value result
/// </summary>
public class ScalarView : IResultView
{
    public ResultViewType Type => ResultViewType.Scalar;
    
    /// <summary>
    /// The scalar value as a string
    /// </summary>
    public string Value { get; }
    
    public ScalarView(object? value)
    {
        Value = value?.ToString() ?? string.Empty;
    }
}