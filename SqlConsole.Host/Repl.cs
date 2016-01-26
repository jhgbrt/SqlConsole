using System;
using System.IO;
using System.Text;

namespace SqlConsole.Host
{
    internal class Repl
    {
        private readonly Config _config;
        private readonly TextWriter _textWriter = Console.Out;
        private readonly TextReader _textReader = Console.In;

        public Repl(Config config)
        {
            _config = config;
        }

        public void Enter()
        {
            while (true)
            {
                var query = ReadQuery();

                if (string.IsNullOrEmpty(query))
                    continue;

                var firstWord = query.FirstWord().ToLowerInvariant();

                switch (firstWord)
                {
                    case "quit":
                    case "exit":
                        return;
                    default:
                        _textWriter.WriteLine();
                        QueryHandlerFactory.DataTable(_config, new ConsoleTableVisualizer()).Execute(query);
                        break;
                }
                _textWriter.WriteLine();
            }
        }

        private string ReadQuery()
        {
            var sb = new StringBuilder();
            var readLine = _textReader.ReadLine();
            while (true)
            {
                if (string.IsNullOrWhiteSpace(readLine))
                    break;

                if (readLine.EndsWith(";"))
                {
                    sb.AppendLine(readLine);
                    break;
                }
                if (readLine == "GO")
                {
                    break;
                }
                if (readLine == "/")
                {
                    break;
                }

                sb.AppendLine(readLine);
                readLine = _textReader.ReadLine();
            }
            return sb.ToString();
        }
    }
}