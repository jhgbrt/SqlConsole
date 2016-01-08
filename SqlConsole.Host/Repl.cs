using System;
using System.Linq;
using System.Text;

namespace SqlConsole.Host
{
    internal class Repl
    {
        public static void Enter()
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
                    case "select":
                        QueryHandlerFactory.DataTable.Execute(query);
                        break;
                    case "update":
                    case "delete":
                    case "insert":
                    case "create":
                        QueryHandlerFactory.NonQuery.Execute(query);
                        break;
                }
            }
        }

        private static string ReadQuery()
        {
            var sb = new StringBuilder();
            string readLine = Console.ReadLine();
            while (true)
            {
                if (string.IsNullOrWhiteSpace(readLine))
                    break;

                sb.AppendLine(readLine);

                if (readLine.EndsWith(";"))
                    break;
                if (readLine == "GO")
                    break;

                readLine = Console.ReadLine();
            }
            return sb.ToString();
        }
    }
}