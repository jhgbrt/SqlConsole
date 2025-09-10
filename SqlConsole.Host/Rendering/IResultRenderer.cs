using SqlConsole.Host.ResultModel;

namespace SqlConsole.Host.Rendering;

/// <summary>
/// Interface for rendering semantic result views
/// </summary>
public interface IResultRenderer
{
    /// <summary>
    /// Render a result view to the appropriate output
    /// </summary>
    /// <param name="resultView">The semantic result to render</param>
    void Render(IResultView resultView);
}