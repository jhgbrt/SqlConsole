using System;
using System.Data.SqlClient;
using System.Linq;

namespace SqlConsole.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = Config.Create(args);
            if (config.Help)
            {
                PrintUsage();
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(config.Query))
                {
                    QueryHandlerFactory.Create(config).Execute(config.Query);
                }
                else
                {
                    Repl.Enter();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine();
                Config.PrintUsage();
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
