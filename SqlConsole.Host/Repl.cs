using System;
using System.Linq;
using System.Text;

namespace SqlConsole.Host
{
    internal class Repl
    {
        private readonly Config _config;
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

                var firstWord = query.Words().First().ToLowerInvariant();

                switch (firstWord)
                {
                    case "quit":
                    case "exit":
                        return;
                    default:
                        Console.WriteLine();
                        QueryHandlerFactory.DataTable(_config, new ConsoleTableVisualizer()).Execute(query);
                        break;
                }
                Console.WriteLine();
            }
        }

        private static string ReadQuery()
        {
            var sb = new StringBuilder();
            var readLine = Console.ReadLine();
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
                readLine = Console.ReadLine();
            }
            return sb.ToString();
        }
    }
}