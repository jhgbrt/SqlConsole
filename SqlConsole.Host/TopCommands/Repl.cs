
using static SqlConsole.Host.CommandFactory;
using static System.ConsoleKey;

namespace SqlConsole.Host;

internal class Repl : ICommand
{

    private readonly IReplConsole _console;

    // SQL Keywords and Functions for Tab completion
    private static readonly string[] SqlKeywords = new[]
    {
        // Common SQL Keywords
        "SELECT", "FROM", "WHERE", "INSERT", "UPDATE", "DELETE", "CREATE", "ALTER", "DROP",
        "TABLE", "INDEX", "VIEW", "DATABASE", "SCHEMA", "PROCEDURE", "FUNCTION", "TRIGGER",
        "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "OUTER", "CROSS", "UNION", "ALL", "DISTINCT",
        "GROUP", "BY", "ORDER", "HAVING", "LIMIT", "OFFSET", "TOP", "AS", "AND", "OR", "NOT",
        "NULL", "IS", "IN", "LIKE", "BETWEEN", "EXISTS", "CASE", "WHEN", "THEN", "ELSE", "END",
        "IF", "WHILE", "BEGIN", "COMMIT", "ROLLBACK", "TRANSACTION", "DECLARE", "SET", "EXEC",
        
        // Data Types
        "INT", "INTEGER", "VARCHAR", "CHAR", "TEXT", "DATE", "DATETIME", "TIME", "TIMESTAMP",
        "DECIMAL", "NUMERIC", "FLOAT", "REAL", "DOUBLE", "BOOLEAN", "BIT", "BINARY", "VARBINARY",
        
        // Common Functions
        "COUNT", "SUM", "AVG", "MIN", "MAX", "LEN", "LENGTH", "UPPER", "LOWER", "SUBSTRING",
        "TRIM", "LTRIM", "RTRIM", "REPLACE", "CONCAT", "COALESCE", "ISNULL", "NULLIF",
        "CAST", "CONVERT", "GETDATE", "NOW", "DATEADD", "DATEDIFF", "YEAR", "MONTH", "DAY"
    };

    public Repl() : this(new ReplConsole()) { }
    public Repl(IReplConsole console) => _console = console;

    public void Execute(IQueryHandler queryHandler, QueryOptions options, IConsoleRenderer renderer)
    {
        queryHandler.Connect();

        renderer.WriteConnectionStatus(queryHandler.ConnectionStatus);

        foreach (var command in ReadCommands())
        {
            if (command.IsAbort)
                break;

            switch (command)
            {
                case Connect:
                    queryHandler.Connect();
                    renderer.WriteConnectionStatus(queryHandler.ConnectionStatus);
                    break;
                case Disconnect:
                    queryHandler.Disconnect();
                    renderer.WriteConnectionStatus(queryHandler.ConnectionStatus);
                    break;
                case Clear:
                    _console.Clear();
                    break;
                case SqlQuery q:
                    try
                    {
                        queryHandler.Execute(q.Sql);
                    }
                    catch (DbException)
                    {
                        try
                        {

                            queryHandler.Disconnect();
                            queryHandler.Connect();
                            queryHandler.Execute(q.Sql);
                        }
                        catch (DbException e)
                        {

                            renderer.WriteError(e.Message);
                        }
                    }
                    break;
                case Help:
                    _console.WriteLine("Type your SQL query in the console. A query terminated by a ';' is immediately executed. " +
                        "A GO statement, a forward slash ('/') on a single line or an empty line will also run the query.");
                    _console.WriteLine();
                    _console.WriteLine("commands:");
                    _console.WriteLine("    help            Show this help message");
                    _console.WriteLine("    exit, quit      Exit the console");
                    _console.WriteLine("    connect         Connect to the underlying database");
                    _console.WriteLine("    disconnect      Disconnect from the database");
                    _console.WriteLine("    cls             Clear the screen");
                    _console.WriteLine("");
                    break;
                case Continue:
                    break;
            }
        }
    }

    record BaseCommand(bool IsAbort) { }
    record SqlQuery(string Sql) : BaseCommand(false) { }
    record StopExecution() : BaseCommand(true) { }
    record Connect() : BaseCommand(false) { }
    record Disconnect() : BaseCommand(false) { }
    record Help() : BaseCommand(false) { }
    record Continue() : BaseCommand(false) { }
    record Clear() : BaseCommand(false) { }

    static readonly Dictionary<string, BaseCommand> commands = new()
    {
        ["exit"] = new StopExecution(),
        ["quit"] = new StopExecution(),
        ["connect"] = new Connect(),
        ["disconnect"] = new Disconnect(),
        ["help"] = new Help(),
        ["cls"] = new Clear(),
        ["clear"] = new Clear(),
    };
    IEnumerable<BaseCommand> ReadCommands()
    {
        while (true)
        {
            var query = ReadCommand();
            yield return query;
        }
    }

    class CommandBuilder
    {
        StringBuilder _query = new();

        public CommandBuilder AppendLine(string line)
        {
            (IsComplete, _query, Command) = line switch
            {
                "GO" or "/" => (true, _query, new SqlQuery(_query.ToString())),
                "" when _query.Length > 0 => (true, _query, new SqlQuery(_query.ToString())),
                "" when _query.Length == 0 => (true, _query.Append(line), new Continue()),
                string s when _query.Length == 0 && commands.ContainsKey(s) => (true, _query.Append(s), commands[s]),
                string s when s.EndsWith(";") => (true, _query, new SqlQuery(_query.AppendLine(s).ToString())),
                string s => (false, _query.AppendLine(s), null)
            };

            return this;
        }

        public bool IsComplete { get; set; }

        internal BaseCommand? Command { get; private set; }
    }



    BaseCommand ReadCommand()
    {
        var cb = new CommandBuilder();
        _console.Write("> ");
        while (true)
        {
            cb.AppendLine(ReadLine());
            if (cb.IsComplete) break;
            _console.Write("| ");
        }
        return cb.Command!;
    }

    readonly List<string> _history = new();

    static bool IsIdentChar(char ch)
        => char.IsLetterOrDigit(ch) || ch == '_' || ch == '$';

    string ReadLine()
    {
        var sb = new StringBuilder();
        int left = _console.CursorLeft;
        int x = 0;
        int y = 0;
        bool insertMode = true;
        string prefix = string.Empty;
        int lastTokenStart = -1;
        int lastTokenEnd = -1;

        while (true)
        {
            var key = new MyConsoleKey(_console.ReadKey());
            switch (key)
            {
                case { Key: Enter }:
                    _console.WriteLine();
                    var returnValue = sb.ToString();
                    if (_history.FirstOrDefault() != returnValue)
                        _history.Insert(0, returnValue);
                    if (_history.Count > 100)
                        _history.RemoveAt(100);
                    return returnValue;
                case { Key: Insert }:
                    insertMode = !insertMode;
                    _console.CursorSize = insertMode ? 25 : 100;
                    continue;
            }


            try
            {
                _console.CursorVisible = false;
                _console.CursorLeft = left;
                for (int i = 0; i < sb.Length; i++) _console.Write(' ');
                (sb, prefix, x, y) = key switch
                {
                    { IsEscape: true }
                        => (sb.Clear(), string.Empty, 0, ResetState()),
                    { IsBackspace: true } when x > 0
                        => (sb.Remove(x - 1, 1), string.Empty, x - 1, ResetState()),
                    { IsDelete: true } when x < sb.Length
                        => (sb.Remove(x, 1), string.Empty, x, ResetState()),
                    { IsLeftArrow: true } when x > 0
                        => (sb, string.Empty, x - 1, ResetState()),
                    { IsRightArrow: true } when x < sb.Length
                        => (sb, string.Empty, x + 1, ResetState()),
                    { IsUpArrow: true } when y < _history.Count
                        => (sb.Clear().Append(_history[y]), string.Empty, sb.Length, y + 1),
                    { IsDownArrow: true } when y > 1
                        => (sb.Clear().Append(_history[y - 2]), string.Empty, sb.Length, y - 1),
                    { IsDownArrow: true } when y == 0 && _history.Any()
                        => (sb.Clear().Append(_history[0]), string.Empty, sb.Length, 1),
                    { IsTab: true }
                        => FindCompletion(sb, prefix),
                    { IsHome: true }
                        => (sb, prefix, 0, ResetState()),
                    { IsEnd: true }
                        => (sb, prefix, sb.Length, ResetState()),
                    { IsControl: false }
                        => InsertOrAppend(sb, insertMode, x, key.KeyChar),
                    _ => (sb, string.Empty, x, y)
                };
            }
            finally
            {
                _console.CursorLeft = left;
                for (int i = 0; i < sb.Length; i++) _console.Write(sb[i]);
                _console.CursorLeft = Math.Min(left + x, left + sb.Length);
                _console.CursorVisible = true;
            }
        }

        int ResetState()
        {
            lastTokenStart = -1;
            lastTokenEnd = -1;
            return 0;
        }

        (StringBuilder, string, int, int) InsertOrAppend(StringBuilder sb, bool insertMode, int x, char c)
        {
            if (x == sb.Length)
                sb = sb.Append(c);
            else if (insertMode)
                sb = sb.Insert(x, c);
            else
                sb = sb.Replace(sb[x], c, x, 1);
            return (sb, string.Empty, x + 1, ResetState());
        }
        (StringBuilder, string, int, int) FindCompletion(StringBuilder sb, string prefix)
        {
            // Compute token boundaries at cursor position
            int start = x;
            while (start > 0 && IsIdentChar(sb[start - 1])) start--;
            int end = x;
            while (end < sb.Length && IsIdentChar(sb[end])) end++;
            string token = sb.ToString(start, end - start);
            
            // Use existing prefix if we're in the middle of cycling, otherwise use current token
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = token;
                lastTokenStart = start;
                lastTokenEnd = end;
            }
            
            // If token is empty and we don't have a prefix, no completion
            if (string.IsNullOrEmpty(prefix))
                return (sb, string.Empty, x, 0);
            
            // First, search through history using prefix (whole-line replacement)
            for (int i = y; i < _history.Count; i++)
                if (_history[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return (sb.Clear().Append(_history[i]), prefix, sb.Length, i + 1);
            for (int i = 0; i < Math.Min(y, _history.Count); i++)
                if (_history[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return (sb.Clear().Append(_history[i]), prefix, sb.Length, i + 1);
                    
            // After exhausting history, search through keywords (token-only replacement)
            var matchingKeywords = SqlKeywords
                .Where(keyword => keyword.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToArray();
                
            if (matchingKeywords.Length > 0)
            {
                int keywordIndex = y - _history.Count;
                if (keywordIndex >= 0 && keywordIndex < matchingKeywords.Length)
                {
                    var selectedKeyword = matchingKeywords[keywordIndex];
                    // Replace only the token range, preserve surrounding text  
                    // Use the original token boundaries for replacement
                    sb.Remove(lastTokenStart, lastTokenEnd - lastTokenStart);
                    sb.Insert(lastTokenStart, selectedKeyword);
                    int newX = lastTokenStart + selectedKeyword.Length;
                    return (sb, prefix, newX, _history.Count + keywordIndex + 1);
                }
                // Wrap around to first keyword when cycling beyond end
                else if (keywordIndex >= matchingKeywords.Length)
                {
                    var selectedKeyword = matchingKeywords[0];
                    sb.Remove(lastTokenStart, lastTokenEnd - lastTokenStart);
                    sb.Insert(lastTokenStart, selectedKeyword);
                    int newX = lastTokenStart + selectedKeyword.Length;
                    return (sb, prefix, newX, _history.Count + 1);
                }
            }
            
            return (sb, prefix, x, y);
        }

    }
    struct MyConsoleKey
    {
        public MyConsoleKey(ConsoleKeyInfo c) { KeyInfo = c; }
        public ConsoleKeyInfo KeyInfo { get; init; }
        public ConsoleKey Key => KeyInfo.Key;
        public char KeyChar => KeyInfo.KeyChar;
        public bool IsControl => char.IsControl(KeyChar);
        public bool IsEscape => Key == Escape;
        public bool IsBackspace => Key == Backspace;
        public bool IsDelete => Key == Delete;
        public bool IsLeftArrow => Key == LeftArrow;
        public bool IsRightArrow => Key == RightArrow;
        public bool IsUpArrow => Key == UpArrow;
        public bool IsDownArrow => Key == DownArrow;
        public bool IsTab => Key == Tab;
        public bool IsHome => Key == Home;
        public bool IsEnd => Key == End;
    }
}
