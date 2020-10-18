using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Options;
using static Mono.Options.OptionSet;
using static SqlConsole.Host.CommandLineParam;
using static SqlConsole.Host.ConnectionStringBuilder;

namespace SqlConsole.Host
{
    class Config
    {
        public static Config Create(string[] args) => new Config(args);

        public static void PrintUsage()
        {
            var options = All.Select(item => CreateOption(item.Prototype, item.Description, _ => { }));
            new OptionSet().AddRange(options).WriteOptionDescriptions(Console.Out);
        }

        public Provider Provider { get; }

        public string ConnectionString { get; }

        public bool Help { get;  }

        public string Output { get; }

        public string Query { get; }

        public bool Scalar { get; }

        public bool NonQuery { get; }

        public bool OutputToFile => !string.IsNullOrEmpty(Output) && !string.IsNullOrEmpty(Query);

        CommandLine ToCommandLine(IEnumerable<string> arguments)
        {
            var commandLine = new CommandLine();
            var options =
                from item in All
                select CreateOption(item.Prototype, item.Description, s => commandLine[item] = s.Trim('"'));

            commandLine.Query = new OptionSet().AddRange(options).Parse(arguments) switch
            {
                List<string> l when l.Any() && File.Exists(l[0]) => File.ReadAllText(l[0]),
                List<string> l when l.Any() => l[0],
                _ => string.Empty
            };

            return commandLine;
        }

        private Config(IEnumerable<string> args)
        {
            var commandLine = ToCommandLine(args);
            Provider = Provider.Get(commandLine[provider]);
            Output = commandLine[output];
            Scalar = commandLine[scalar] == scalar.Name;
            NonQuery = commandLine[nonquery] == nonquery.Name;
            Help = commandLine[help] == help.Name;
            Query = commandLine.Query;
            ConnectionString = GetConnectionString(Provider, commandLine);
        }
    }
}