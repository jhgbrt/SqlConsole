using System;
using System.Data.Common;

namespace SqlConsole.Host
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var config = Config.Create(args);

                if (config.Help)
                {
                    PrintUsage();
                    return;
                }

                Console.WriteLine(config.ConnectionString);

                var csb = new DbConnectionStringBuilder {ConnectionString = config.ConnectionString}.WithoutSensitiveInformation();
                Console.WriteLine(csb.ConnectionString);

                using (var queryHandler = new QueryHandlerFactory(config).Create())
                {
                    if (!string.IsNullOrEmpty(config.Query))
                    {
                        queryHandler.Execute(config.Query);
                    }
                    else
                    {
                        new Repl(queryHandler).Enter();
                    }
                }
            }
            catch (ConnectionConfigException re)
            {
                WriteLine(re.Message, ConsoleColor.Red);
                PrintUsage();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine();
                PrintUsage();
            }
        }

        static DbConnectionStringBuilder WithoutSensitiveInformation(this DbConnectionStringBuilder b)
        {
            foreach (var v in new[] { "password", "Password", "PWD", "Pwd", "pwd" })
            {
                if (b.ContainsKey(v)) b[v] = "******";
            }
            return b;
        }

        static void WriteLine(string message, ConsoleColor color)
        {
            var fg = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = fg;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("usage: sql [query or file] [options]");
            Console.WriteLine("");
            Console.WriteLine("Executes the given query, or a sql statement in a file and reports the results.");
            Console.WriteLine("");
            Console.WriteLine("Arguments:");
            Console.WriteLine("");
            Console.WriteLine("      query or file: an inline SQL query, or the path to a file containing such query");
            Console.WriteLine("");
            Config.PrintUsage();
        }
    }
}
