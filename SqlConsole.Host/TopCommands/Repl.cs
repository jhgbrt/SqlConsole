
using static SqlConsole.Host.CommandFactory;
using static System.ConsoleKey;

namespace SqlConsole.Host;

internal class Repl : ICommand
{

    private readonly IReplConsole _console;

    public Repl() : this(new ReplConsole()) { }
    public Repl(IReplConsole console) => _console = console;

    public void Execute(IQueryHandler queryHandler, QueryOptions options)
    {
        queryHandler.Connect();

        _console.WriteLine(queryHandler.ConnectionStatus);

        foreach (var command in ReadCommands())
        {
            if (command.IsAbort)
                break;

            switch (command)
            {
                case Connect:
                    queryHandler.Connect();
                    _console.WriteLine(queryHandler.ConnectionStatus);
                    break;
                case Disconnect:
                    queryHandler.Disconnect();
                    _console.WriteLine(queryHandler.ConnectionStatus);
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

                            _console.ForegroundColor = ConsoleColor.Red;
                            _console.Error.WriteLine(e.Message);
                            _console.ResetColor();
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

    string ReadLine()
    {
        var sb = new StringBuilder();
        int left = _console.CursorLeft;
        int x = 0;
        int y = 0;
        bool insertMode = true;
        string prefix = string.Empty;

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
                        => (sb.Clear(), string.Empty, 0, 0),
                    { IsBackspace: true } when x > 0
                        => (sb.Remove(x - 1, 1), string.Empty, x - 1, y),
                    { IsDelete: true } when x < sb.Length
                        => (sb.Remove(x, 1), string.Empty, x, y),
                    { IsLeftArrow: true } when x > 0
                        => (sb, string.Empty, x - 1, y),
                    { IsRightArrow: true } when x < sb.Length
                        => (sb, string.Empty, x + 1, y),
                    { IsUpArrow: true } when y < _history.Count
                        => (sb.Clear().Append(_history[y]), string.Empty, sb.Length, y + 1),
                    { IsDownArrow: true } when y > 1
                        => (sb.Clear().Append(_history[y - 2]), string.Empty, sb.Length, y - 1),
                    { IsDownArrow: true } when y == 0 && _history.Any()
                        => (sb.Clear().Append(_history[0]), string.Empty, sb.Length, 1),
                    { IsTab: true }
                        => FindInHistory(sb, prefix),
                    { IsHome: true }
                        => (sb, prefix, 0, y),
                    { IsEnd: true }
                        => (sb, prefix, sb.Length, y),
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

        (StringBuilder, string, int, int) InsertOrAppend(StringBuilder sb, bool insertMode, int x, char c)
        {
            if (x == sb.Length)
                sb = sb.Append(c);
            else if (insertMode)
                sb = sb.Insert(x, c);
            else
                sb = sb.Replace(sb[x], c, x, 1);
            return (sb, string.Empty, x + 1, y);
        }
        (StringBuilder, string, int, int) FindInHistory(StringBuilder sb, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                prefix = sb.ToString();
            for (int i = y; i < _history.Count; i++)
                if (_history[i].StartsWith(prefix))
                    return (sb.Clear().Append(_history[i]), prefix, sb.Length, i + 1);
            for (int i = 0; i < y; i++)
                if (_history[i].StartsWith(prefix))
                    return (sb.Clear().Append(_history[i]), prefix, sb.Length, i + 1);
            return (sb, prefix, sb.Length, y);
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
