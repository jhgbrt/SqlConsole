using NSubstitute;
using NSubstitute.Core;

using SqlConsole.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static SqlConsole.Host.CommandFactory;

namespace SqlConsole.UnitTests.TopCommands
{
    public class ReplTests
    {
        [Theory]
        [InlineData("exit\n")]
        [InlineData("quit\n")]
        public void ExitOnly_ConnectsAndDoesNothingElse(string command)
        {
            var queryHandler = Substitute.For<IQueryHandler>();
            var console = Substitute.For<IReplConsole>();
            console.ReadKey().ReturnsSequence(command);

            var repl = new Repl(console);
            repl.Execute(queryHandler, new QueryOptions());

            queryHandler.DidNotReceive().Execute(Arg.Any<string>());
        }

        [Theory]
        [InlineData("connect\nexit\n")]
        public void Connect_ConnectsAndDoesNothingElse(string command)
        {
            var queryHandler = Substitute.For<IQueryHandler>();
            var console = Substitute.For<IReplConsole>();
            console.ReadKey().ReturnsSequence(command);

            var repl = new Repl(console);
            repl.Execute(queryHandler, new QueryOptions());

            queryHandler.Received().Connect();
        }

        [Theory]
        [InlineData("disconnect\nexit\n")]
        public void Disconnect_ConnectsAndDoesNothingElse(string command)
        {
            var queryHandler = Substitute.For<IQueryHandler>();
            var console = Substitute.For<IReplConsole>();
            console.ReadKey().ReturnsSequence(command);

            var repl = new Repl(console);
            repl.Execute(queryHandler, new QueryOptions());

            queryHandler.Received().Disconnect();
        }


        [Theory]
        [InlineData("SELECT 1;\nexit\n", "SELECT 1;\r\n")]
        [InlineData("SED←LECT 1;\nexit\n", "SELECT 1;\r\n")]
        [InlineData("<Insert>SED←<Delete>LECT 1;\nexit\n", "SELECT 1;\r\n")]
        public void DoQuery_AndExit_ConnectsAndDoesNothingElse(string input, string expected)
        {
            var queryHandler = Substitute.For<IQueryHandler>();
            var console = Substitute.For<IReplConsole>();
            console.ReadKey().ReturnsSequence(input);

            var repl = new Repl(console);
            repl.Execute(queryHandler, new QueryOptions());

            queryHandler.Received().Execute(expected);
        }

        [Theory]
        [InlineData("SELECT 1;\nS\t\nexit\n", "SELECT 1;\r\n")]
        public void Tab_CreatesCompletion(string input, string expected)
        {
            var queryHandler = Substitute.For<IQueryHandler>();
            var console = Substitute.For<IReplConsole>();
            console.ReadKey().ReturnsSequence(input);

            var repl = new Repl(console);
            repl.Execute(queryHandler, new QueryOptions());

            queryHandler.Received(2).Execute(expected);
        }

        [Theory]
        [InlineData("SELECT 1;\n↑\nexit\n", "SELECT 1;\r\n")]
        public void UpArrow_RunsPreviousCommand(string input, string expected)
        {
            var queryHandler = Substitute.For<IQueryHandler>();
            var console = Substitute.For<IReplConsole>();
            console.ReadKey().ReturnsSequence(input);

            var repl = new Repl(console);
            repl.Execute(queryHandler, new QueryOptions());

            queryHandler.Received(2).Execute(expected);
        }

        [Theory]
        [InlineData("SELECT 1;\nSELECT 2;\n↑↑↓\nexit\n", "SELECT 1;\r\n", "SELECT 2;\r\n", "SELECT 2;\r\n")]
        public void UpAndDownArrow_RunsSelectedQueries(string input, params string[] expected)
        {
            var queryHandler = Substitute.For<IQueryHandler>();
            var console = Substitute.For<IReplConsole>();
            console.ReadKey().ReturnsSequence(input);

            var repl = new Repl(console);
            repl.Execute(queryHandler, new QueryOptions());

            foreach (var s in expected)
                queryHandler.Received().Execute(s);
        }

    }

    static class Extensions
    {
        public static ConfiguredCall ReturnsSequence(this ConsoleKeyInfo value, string sequence)
        {
            var keys = sequence.ToConsoleKeyInfo().ToList();
            return value.Returns(keys[0], keys.Skip(1).ToArray());
        }

        enum State
        {
            SpecialKey,
            NormalKey
        }
        private static IEnumerable<ConsoleKeyInfo> ToConsoleKeyInfo(this IEnumerable<char> chars)
        {
            var state = State.NormalKey;

            var sb = new StringBuilder();
            foreach (var c in chars)
            {
                if (state == State.NormalKey)
                {
                    if (c == '<')
                    {
                        state = State.SpecialKey;
                        continue;
                    }

                    var key = c switch
                    {
                        '\n' => new ConsoleKeyInfo(c, ConsoleKey.Enter, false, false, false),
                        ' ' => new ConsoleKeyInfo(c, ConsoleKey.Spacebar, false, false, false),
                        '\t' => new ConsoleKeyInfo(c, ConsoleKey.Tab, false, false, false),
                        '←' => new ConsoleKeyInfo(c, ConsoleKey.LeftArrow, false, false, false),
                        '→' => new ConsoleKeyInfo(c, ConsoleKey.RightArrow, false, false, false),
                        '↑' => new ConsoleKeyInfo(c, ConsoleKey.UpArrow, false, false, false),
                        '↓' => new ConsoleKeyInfo(c, ConsoleKey.DownArrow, false, false, false),
                        '.' => new ConsoleKeyInfo(c, ConsoleKey.OemPeriod, false, false, false),
                        ',' => new ConsoleKeyInfo(c, ConsoleKey.OemComma, false, false, false),
                        '+' => new ConsoleKeyInfo(c, ConsoleKey.Add, false, false, false),
                        '-' => new ConsoleKeyInfo(c, ConsoleKey.Subtract, false, false, false),
                        '/' => new ConsoleKeyInfo(c, ConsoleKey.Divide, false, false, false),
                        '*' => new ConsoleKeyInfo(c, ConsoleKey.Multiply, false, false, false),
                        ';' => new ConsoleKeyInfo(c, ConsoleKey.Oem1, false, false, false),
                        //'/' => new ConsoleKeyInfo(c, ConsoleKey.Oem2, false, false, false),
                        '`' => new ConsoleKeyInfo(c, ConsoleKey.Oem3, false, false, false),
                        '[' => new ConsoleKeyInfo(c, ConsoleKey.Oem4, false, false, false),
                        '\\' => new ConsoleKeyInfo(c, ConsoleKey.Oem5, false, false, false),
                        ']' => new ConsoleKeyInfo(c, ConsoleKey.Oem6, false, false, false),
                        '\'' => new ConsoleKeyInfo(c, ConsoleKey.Oem7, false, false, false),
                        ':' => new ConsoleKeyInfo(c, ConsoleKey.Oem1, true, false, false),
                        '?' => new ConsoleKeyInfo(c, ConsoleKey.Oem2, true, false, false),
                        '~' => new ConsoleKeyInfo(c, ConsoleKey.Oem3, true, false, false),
                        '{' => new ConsoleKeyInfo(c, ConsoleKey.Oem4, true, false, false),
                        '|' => new ConsoleKeyInfo(c, ConsoleKey.Oem5, false, false, false),
                        '}' => new ConsoleKeyInfo(c, ConsoleKey.Oem6, true, false, false),
                        '"' => new ConsoleKeyInfo(c, ConsoleKey.Oem7, true, false, false),
                        '!' => new ConsoleKeyInfo(c, ConsoleKey.D1, true, false, false),
                        '@' => new ConsoleKeyInfo(c, ConsoleKey.D2, true, false, false),
                        '#' => new ConsoleKeyInfo(c, ConsoleKey.D3, true, false, false),
                        '$' => new ConsoleKeyInfo(c, ConsoleKey.D4, true, false, false),
                        '%' => new ConsoleKeyInfo(c, ConsoleKey.D5, true, false, false),
                        '^' => new ConsoleKeyInfo(c, ConsoleKey.D6, true, false, false),
                        '&' => new ConsoleKeyInfo(c, ConsoleKey.D7, true, false, false),
                        '(' => new ConsoleKeyInfo(c, ConsoleKey.D9, true, false, false),
                        ')' => new ConsoleKeyInfo(c, ConsoleKey.D0, true, false, false),
                        _ when char.IsLetter(c) => new ConsoleKeyInfo(c, Enum.Parse<ConsoleKey>($"{c}".ToUpper()), char.IsUpper(c), false, false),
                        _ when char.IsDigit(c) => new ConsoleKeyInfo(c, Enum.Parse<ConsoleKey>($"D{c}"), char.IsUpper(c), false, false),
                        _ => throw new Exception($"unsupported key character: {c}")
                    };
                    yield return key;
                }
                if (state == State.SpecialKey)
                {
                    ConsoleKeyInfo key;
                    (state, key, sb) = c switch
                    {
                        '>' => (State.NormalKey, new ConsoleKeyInfo('\0', Enum.Parse<ConsoleKey>(sb.ToString()), false, false, false), sb.Clear()),
                        _ => (state, default, sb.Append(c))
                    };
                    if (state != State.SpecialKey)
                        yield return key;
                }


            }
        }

    }
}
