using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;

namespace SqlConsole.Host
{
    internal class Repl
    {
        private readonly TextWriter _textWriter = Console.Out;
        private readonly TextReader _textReader = Console.In;
        private readonly IQueryHandler _queryHandler;

        public Repl(IQueryHandler queryHandler)
        {
            _queryHandler = queryHandler;
        }

        public void Enter()
        {
            foreach (var command in ReadCommands())
            {
                if (command is StopExecution)
                {
                    break;
                }
                else if (command is SqlQuery q)
                {
                    try
                    {
                        Console.WriteLine();
                        _queryHandler.Execute(q.Sql);
                    }
                    catch (DbException e)
                    {
                        _textWriter.WriteLine(e.Message);
                    }
                }
            }
        }

        interface IReplCommand { }
        record SqlQuery(string Sql) : IReplCommand;
        record StopExecution() : IReplCommand;

        IEnumerable<IReplCommand> ReadCommands()
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
                    "GO" or "/" => (true, _query, new SqlQuery(_query.ToString()) as IReplCommand),
                    "exit" or "quit" => (true, _query.Append(line), new StopExecution()),
                    string s when s.EndsWith(";") => (true, _query, new SqlQuery(_query.AppendLine(s).ToString())),
                    string s => (false, _query.AppendLine(s), null)
                };

                return this;
            }

            public bool IsComplete { get; set; }

            internal IReplCommand? Command { get; private set; }
        }

        IReplCommand ReadCommand()
        {
            var cb = new CommandBuilder();
            Console.WriteLine();
            Console.Write("> ");
            while (true)
            {
                cb.AppendLine(_textReader.ReadLine()!);
                if (cb.IsComplete) break;
                Console.Write("| ");
            }
            return cb.Command!;
        }
    }

}