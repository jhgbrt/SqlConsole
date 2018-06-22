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
        private readonly Provider _provider;

        public static Config Create(string[] args) => new Config(args);

        public static void PrintUsage()
        {
            var options = All.Select(item => CreateOption(item.Prototype, item.Description, _ => { }));
            new OptionSet().AddRange(options).WriteOptionDescriptions(Console.Out);
        }

        public string ProviderName => _provider.Name;

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
            void SetCommandLineValue(CommandLineParam cl, string s) => commandLine[cl] = s.Trim('"');

            var options =
                from item in All
                select CreateOption(item.Prototype, item.Description, s => SetCommandLineValue(item, s));

            var remaining = new OptionSet().AddRange(options).Parse(arguments);

            if (remaining.Any() && File.Exists(remaining[0]))
                commandLine.Query = File.ReadAllText(remaining[0]);
            else if (remaining.Any())
                commandLine.Query = remaining[0];

            return commandLine;
        }

        private Config(IEnumerable<string> args)
        {
            var commandLine = ToCommandLine(args);
            var provider = commandLine.Contains(providerName) ? new Provider(commandLine[providerName]) : Provider.Default;
            Output = commandLine[output];
            Scalar = commandLine[scalar] == scalar.Name;
            NonQuery = commandLine[nonquery] == nonquery.Name;
            Help = commandLine[help] == help.Name;
            Query = commandLine.Query;
            _provider = provider;
            ConnectionString = GetConnectionString(provider, commandLine);
        }
    }
}