using System;
using System.IO;
using System.Linq;
using SqlConsole.Host.Infrastructure;

namespace SqlConsole.Host
{
    class Config
    {
        private readonly OptionSet _optionSet;

        static OptionSet CreateOptionSet(Config config)
        {
            return new OptionSet
            {
                {"output=", "Path to output file. If none specified, output is written to the console.", s => config.Output = s},
                {"scalar", "Interpret the query as a scalar result, i.e. a single value (of any type)", s => config.Scalar = true},
                {"nonquery", "Run the query as a 'non-select' statement, i.e. INSERT, UPDATE, DELETE or a DDL statement. Outputs the number of affected records of the last statement.", s => config.NonQuery = true},
                {"help", "Print this text.", s => config.Help = true}
            };
        }

        public bool Help { get; private set; }

        public Config Parse(string[] args)
        {
            var remaining = _optionSet.Parse(args);
            if (remaining.Any())
            {
                var queryOrFileName = remaining.First();
                Query = File.Exists(queryOrFileName) ? File.ReadAllText(queryOrFileName) : queryOrFileName;
            }
            return this;
        }

        private Config()
        {
            _optionSet = CreateOptionSet(this);
        }

        public string Output { get; private set; }

        public string Query { get; private set; }

        public bool Scalar { get; private set; }

        public bool NonQuery { get; private set; }

        public static Config Create(string[] args)
        {
            var result = new Config().Parse(args);
            return result;
        }

        public static void PrintUsage()
        {
            CreateOptionSet(new Config()).WriteOptionDescriptions(Console.Out);
        }
    }
}