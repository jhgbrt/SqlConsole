using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

using static System.Console;
using static System.ConsoleKey;
using static SqlConsole.Host.CommandFactory;

namespace SqlConsole.Host
{
    internal interface ICommand
    {
        public void Execute(IQueryHandler queryHandler, QueryOptions options);
    }
    internal class SingleQuery : ICommand
    {
        public void Execute(IQueryHandler queryHandler, QueryOptions option)
        {
            var query = option.GetQuery();
            try
            {
                queryHandler.Execute(query);
            }
            catch (DbException e)
            {
                ForegroundColor = ConsoleColor.Red;
                Error.WriteLine(e.Message);
                ResetColor();
            }
        }

    }
    internal class Repl : ICommand
    {
        public void Execute(IQueryHandler queryHandler, QueryOptions options)
        {
            queryHandler.Connect();
            WriteLine(queryHandler.ConnectionStatus);
            foreach (var command in ReadCommands())
            {
                if (command.IsAbort) 
                    break;

                switch (command)
                {
                   case Connect:
                        queryHandler.Connect();
                        WriteLine(queryHandler.ConnectionStatus);
                        break;
                    case Disconnect:
                        queryHandler.Disconnect();
                        WriteLine(queryHandler.ConnectionStatus);
                        break;
                    case Clear:
                        Console.Clear();
                        break;
                    case SqlQuery q:
                        try
                        {
                            queryHandler.Execute(q.Sql);
                        }
                        catch (DbException e)
                        {
                            ForegroundColor = ConsoleColor.Red;
                            Error.WriteLine(e.Message);
                            ResetColor();
                        }
                        break;
                    case Help:
                        WriteLine("Type your SQL query in the console. A query terminated by a ';' is immediately executed. " +
                            "A GO statement, a forward slash ('/') on a single line or an empty line will also run the query.");
                        WriteLine();
                        WriteLine("commands:");
                        WriteLine("    help            Show this help message");
                        WriteLine("    exit, quit      Exit the console");
                        WriteLine("    connect         Connect to the underlying database");
                        WriteLine("    disconnect      Disconnect from the database");
                        WriteLine("    cls             Clear the screen");
                        WriteLine("");
                        break;
                    case Continue:
                        break;
                }
            }
        }

        record BaseCommand(bool IsAbort) { }
        record SqlQuery(string Sql): BaseCommand(false) { }
        record StopExecution() : BaseCommand(true) { }
        record Connect() : BaseCommand(false) { }
        record Disconnect() : BaseCommand(false) { }
        record Help() : BaseCommand(false) { }
        record Continue() : BaseCommand(false) { }
        record Clear() : BaseCommand(false) { }
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
            StringBuilder _query = new StringBuilder();

            public CommandBuilder AppendLine(string line)
            {
                (IsComplete, _query, Command) = line switch
                {
                    "GO" or "/" => (true, _query, new SqlQuery(_query.ToString()) as BaseCommand),
                    "exit" or "quit" when _query.Length == 0 => (true, _query.Append(line), new StopExecution()),
                    "disconnect" when _query.Length == 0 => (true, _query.Append(line), new Disconnect()),
                    "connect" when _query.Length == 0 => (true, _query.Append(line), new Connect()),
                    "help" when _query.Length == 0 => (true, _query.Append(line), new Help()),
                    "cls" or "clear" when _query.Length == 0 => (true, _query.Append(line), new Clear()),
                    "" when _query.Length == 0 => (true, _query.Append(line), new Continue()),
                    "" => (true, _query, new SqlQuery(_query.ToString())),
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
            Write("> ");
            while (true)
            {
                var line = ReadLine();
                cb.AppendLine(line);
                if (cb.IsComplete) break;
                Write("| ");
            }
            return cb.Command!;
        }

        List<string> _history = new List<string>();

        string ReadLine()
        {
            var sb = new StringBuilder();
            int min = CursorLeft;
            int x = 0;
            int y = 0;
            bool insertMode = false;

 
            while (true)
            {
                var key = new MyConsoleKey(ReadKey(true));
                switch (key)
                {
                    case { Key: Enter }:
                        Console.WriteLine();
                        var returnValue = sb.ToString();
                        if (_history.FirstOrDefault() != returnValue)
                            _history.Insert(0, returnValue);
                        if (_history.Count > 100)
                            _history.RemoveAt(100);
                        return returnValue;
                    case { Key: Insert }:
                        insertMode = !insertMode;
                        continue;
                }

                ClearLine();
                (sb, x, y) = key switch
                {
                    { IsEscape: true }
                        => (sb.Clear(), 0, y),
                    { IsBackspace: true } when x > 0
                        => (sb.Remove(x - 1, 1), x - 1, y),
                    { IsDelete: true } when x < sb.Length
                        => (sb.Remove(x, 1), x, y),
                    { IsLeftArrow: true } when x > 0 => (sb, x - 1, y),
                    { IsRightArrow: true } when x < sb.Length
                        => (sb, x + 1, y),
                    { IsUpArrow: true } when y < _history.Count
                        => (sb.Clear().Append(_history[y]), sb.Length, y + 1),
                    { IsDownArrow: true } when y > 1
                        => (sb.Clear().Append(_history[y - 2]), sb.Length, y - 1),
                    { IsTab: true }
                        => FindInHistory(sb),
                    { IsControl: false }
                        => InsertOrAppend(sb, insertMode, key.KeyChar),
                    _ => (sb, x, y)
                };
                WriteLine();
            }

            void ClearLine()
            {
                CursorLeft = min;
                for (int i = 0; i < sb.Length; i++) Write(' ');
                CursorLeft = min;
            }
            void WriteLine()
            {
                CursorLeft = min;
                for (int i = 0; i < sb.Length; i++) Write(sb[i]);
                CursorLeft = Math.Min(min + x, min + sb.Length);
            }

            (StringBuilder, int, int) InsertOrAppend(StringBuilder sb, bool insertMode, char c)
            {
                StringBuilder returnValue;
                if (x == sb.Length)
                    returnValue = sb.Append(c);
                else if (insertMode)
                    returnValue = sb.Insert(x, c);
                else
                    returnValue = sb.Replace(sb[x], c, x, 1);
                return (returnValue, x + 1, y);
            }
            (StringBuilder, int, int) FindInHistory(StringBuilder sb)
            {
                if (_history.Any() && sb.ToString() == _history[y])
                {
                    y = y >= _history.Count ? 0 : y + 1;
                }
                for (int i = y; i < _history.Count; i++)
                    if (_history[i].StartsWith(sb.ToString()))
                        return (sb.Clear().Append(_history[i]), sb.Length, i);
                for (int i = 0; i < y; i++)
                    if (_history[i].StartsWith(sb.ToString()))
                        return (sb.Clear().Append(_history[i]), sb.Length, i);
                return (sb, sb.Length, y);
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
        }
    }

}