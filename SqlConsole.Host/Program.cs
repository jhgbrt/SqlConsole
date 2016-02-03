using System;

namespace SqlConsole.Host
{

    static class Program
    {
        static void Main(string[] args)
        {
            Config config;
            try
            {
                config = Config.Create(args);
            }
            catch (ConnectionConfigException re)
            {
                var fg = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(re.Message);
                Console.ForegroundColor = fg;
                return;
            }
            if (config.Help)
            {
                PrintUsage(config);
                return;
            }


            try
            {
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine();
                config.PrintUsage();
            }
        }

        private static void PrintUsage(Config config)
        {
            Console.WriteLine("usage: sql [query or file] [options]");
            Console.WriteLine("");
            Console.WriteLine("Executes the given query, or a sql statement in a file and reports the results.");
            Console.WriteLine("");
            Console.WriteLine("Arguments:");
            Console.WriteLine("");
            Console.WriteLine("      query or file: an inline SQL query, or the path to a file containing such query");
            Console.WriteLine("");
            config.PrintUsage();
        }
    }
}
