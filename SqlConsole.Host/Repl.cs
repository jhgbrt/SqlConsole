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
            foreach (var query in ReadQueries())
            {
                if (string.IsNullOrEmpty(query))
                    continue;

                var firstWord = query.FirstWord().ToLowerInvariant();

                switch (firstWord)
                {
                    case "quit":
                    case "exit":
                        return;
                    default:
                        try
                        {
                            _queryHandler.Execute(query);
                        }
                        catch (DbException e)
                        {
                            _textWriter.WriteLine(e.Message);
                        }
                        break;
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

                readLine = _textReader.ReadLine();
            }
            return sb.ToString();
        }
    }
}