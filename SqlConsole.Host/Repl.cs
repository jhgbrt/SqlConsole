using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using static System.StringComparison;

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
            foreach (var query in ReadQueries())
            {
                if (string.IsNullOrEmpty(query) 
                    || query.StartsWith("quit", OrdinalIgnoreCase) 
                    || query.StartsWith("exit", OrdinalIgnoreCase))
                    return;

                try
                {
                    Console.WriteLine();
                    _queryHandler.Execute(query);
                }
                catch (DbException e)
                {
                    _textWriter.WriteLine(e.Message);
                }
            }
        }

        IEnumerable<string> ReadQueries()
        {
            while (true)
            {
                var query = ReadQuery();
                yield return query;
            }
        }

        string ReadQuery()
        {
            var sb = new StringBuilder();
            Console.WriteLine();
            Console.Write("> ");
            var readLine = _textReader.ReadLine();
            while (true)
            {
                if (string.IsNullOrWhiteSpace(readLine))
                    break;
                if (readLine == "GO")
                    break;
                if (readLine == "/")
                    break;

                sb.AppendLine(readLine);

                if (readLine.EndsWith(";"))
                    break;

                Console.Write("| ");
                readLine = _textReader.ReadLine();
            }
            return sb.ToString();
        }
    }

}