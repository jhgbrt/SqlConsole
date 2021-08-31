using SqlConsole.Host;

struct ScriptSplittingState
{
    internal string Script
    {
        get
        {
            var result = _script.ToString();
            _script.Clear();
            return result;
        }
    }
    char? Quote {  get; init; }
    private StringBuilder _script = new();
    private StringBuilder _buffer = new();
    internal string State => MoveNextImpl.Method.Name;

    private ScriptSplittingState Append(char c) { _script.Append(_buffer).Append(c); _buffer.Clear(); return this; }
    private ScriptSplittingState Buffer(char c)  { _buffer = _buffer.Append(c); return this; }
    private ScriptSplittingState ClearBuffer() { _buffer.Clear(); return this; }

    private ScriptSplittingState TransitionTo(Func<ScriptSplittingState, char, char, char, (ScriptSplittingState, bool)> state) => this with { MoveNextImpl = state };
    private ScriptSplittingState TransitionToGo(char b) => ClearBuffer().TransitionTo(Go);
    private ScriptSplittingState TransitionToNoGo(char b) => Append(b).TransitionTo(NoGo);
    private ScriptSplittingState TransitionToHandleQuote(char b) => Append(b).TransitionTo(HandleQuote) with { Quote = b };
    private ScriptSplittingState TransitionToCheckingGo(char b) => Buffer(b).TransitionTo(CheckingGo);
    private ScriptSplittingState TransitionToMultilineComment(char b) => Append(b).TransitionTo(MultilineComment);
    private ScriptSplittingState TransitionToHandleScript(char b) => Append(b).TransitionTo(HandleScript);
    private ScriptSplittingState TransitionToHandleScript() => ClearBuffer().TransitionTo(HandleScript);
    private Func<ScriptSplittingState, char, char, char, (ScriptSplittingState, bool)> MoveNextImpl { get; set; } = HandleScript;
    public (ScriptSplittingState, bool) MoveNext(char a, char b, char c) => MoveNextImpl(this, a, b, c);

    private static (ScriptSplittingState, bool) HandleScript(ScriptSplittingState current, char a, char b, char c) => (a, b, c) switch
    {
        (_, '/', '*') => (current.TransitionToMultilineComment(b), false),
        (_, '\'' or '\"', _) => (current.TransitionToHandleQuote(b), false),
        (_, 'G' or 'g', 'O' or 'o') => (current.TransitionToCheckingGo(b), false),
        _ => (current.Append(b), false)
    };

    private static (ScriptSplittingState, bool) MultilineComment(ScriptSplittingState current, char a, char b, char c) => (a, b, c) switch
    {
        ('*', '/', _) => (current.TransitionToHandleScript(b), false),
        _ => (current.Append(b), false)
    };

    private static (ScriptSplittingState, bool) HandleQuote(ScriptSplittingState current, char a, char b, char c) => (a, b, c) switch
    {
        (_, char y, _) when y == current.Quote => (current.TransitionToHandleScript(b), false),
        _ => (current.Append(b), false)
    };
    private static (ScriptSplittingState, bool) CheckingGo(ScriptSplittingState current, char a, char b, char c) => (a, b, c) switch
    {
        ('G' or 'g', 'O' or 'o', '\r' or '\n') => (current.TransitionToGo(b), false),
        (_, '-', '-') or (_, '/', '/') when current._buffer.Trim().ToUpperInvariant() == "GO" => (current.TransitionToGo(b), false),
        (_, _, char n) when char.IsLetterOrDigit(n) => (current.TransitionToNoGo(b), false),
        (_, _, '\n') => (current.TransitionToGo(b), false),
        _ => (current.Buffer(b), false),
    };
    private static (ScriptSplittingState, bool) NoGo(ScriptSplittingState current, char a, char b, char c) => (a, b, c) switch
    {
        (_, not '\n', _) => (current.Append(b), false),
        _ => (current.TransitionToHandleScript(b), false)
    };

    private static (ScriptSplittingState, bool) Go(ScriptSplittingState current, char a, char b, char c) => (a, b, c) switch
    {
        (_, not '\n', _) => (current, false),
        _ => (current.TransitionToHandleScript(), true)
    };
}

class ScriptSplitter
{
    private readonly string _script;
    private Action<string> _log;

    public ScriptSplitter(string script, Action<string> log = null)
    {
        _script = script;
        _log = log;
    }

    public IEnumerable<string> Split()
    {
        var s = new ScriptSplittingState();
        for (int i = 0; i < _script.Length; i++)
        {
            var a = i > 0 ? _script[i - 1] : '\0';
            var b = _script[i];
            var c = i < _script.Length - 1 ? _script[i + 1] : '\0';
            (var next, var yield) = s.MoveNext(a, b, c);
            _log?.Invoke($"({s.State}, {a},{b},{c}) -> ({next.State}, {yield})".ToCSharpLiteral());
            _log?.Invoke($"{s}");
            s = next;
            if (yield || i == _script.Length - 1)
            {
                var result = s.Script;
                if (result.Length > 0)
                    yield return result;
            }
        }
    }
}

static class Formatting
{
    public static string Trim(this StringBuilder sb) => sb.ToString().Trim();
}