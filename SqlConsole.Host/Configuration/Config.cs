using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Options;
using static System.Configuration.ConfigurationManager;
using static SqlConsole.Host.ConnectionStringBuilder;

namespace SqlConsole.Host
{
    class Config
    {
        private readonly Provider _provider;
        private readonly MyOptionSet _optionSet;

        class MyOptionSet : OptionSet
        {
            public MyOptionSet(
                IDictionary<CommandLineParam, ConnectionStringParam> dictionary, Action<CommandLineParam, string> action,
                Func<string, string, bool> onUnknownOption)
            {
                var variableOptions =
                    from kv in dictionary
                    let cli = kv.Key
                    let description = $"{cli.Description} (maps to {kv.Value})"
                    select CreateOption(cli.Prototype, description, s => action(cli, s));
                AddRange(variableOptions);
                OnUnknownOption(onUnknownOption);
            }
        }

        public string ProviderName => _provider.Name;

        public string ConnectionString { get; }

        public bool Help { get;  }

        private Config(string[] args)
        {
            Provider? p = null;
            new OptionSet { { "providerName=", "", s => p = new Provider(s) } }.Parse(args);

            Provider provider = p ?? Provider.Default;

            string output = null;
            bool scalar = false, nonquery = false, help = false;
            var commandLine = new CommandLine();
            var connectionString = new ConnectionString();

            var options = new MyOptionSet(
                CommandLineToConnectionString(provider), 
                (cl, s) => commandLine[cl] = Value.From(s),
                (name, value) =>
                {
                    connectionString[new ConnectionStringParam(name)] = Value.From(value);
                    return true;
                })
            {
                {
                    "output=",
                    "Path to output File. If none specified, output is written to the console.",
                    s => output = s
                },
                {
                    "scalar",
                    "Interpret the query as a scalar result, i.e. a single value (of any type)",
                    s => scalar = true
                },
                {
                    "nonquery",
                    "Run the query as a 'non-select' statement, i.e. INSERT, UPDATE, DELETE or a DDL statement. " +
                    "Outputs the number of affected records of the last statement.",
                    s => nonquery = true
                },
                {
                    "providerName=",
                    "The db provider name.",
                    s => provider = new Provider(s)
                },
                {
                    "help",
                    "Print this text.",
                    s => help = true
                }
            };


            var remaining = options.Parse(args);

            Output = output;
            Scalar = scalar;
            NonQuery = nonquery;
            Help = help;
            _provider = provider;
            _optionSet = options;

            if (remaining.Any())
            {
                var queryOrFileName = remaining.First();
                Query = File.Exists(queryOrFileName) ? File.ReadAllText(queryOrFileName) : queryOrFileName;
            }

            if (commandLine.Any() || connectionString.Any())
            {
                ConnectionString = GetConnectionString(_provider, commandLine, connectionString);
            }
            else
            {
                var settings = ConnectionStrings["Default"];
                if (settings != null)
                {
                    ConnectionString = settings.ConnectionString;
                    _provider = new Provider(settings.ProviderName);
                }
            }
        }

        public string Output { get;  }

        public string Query { get; }

        public bool Scalar { get; }

        public bool NonQuery { get; }

        public static Config Create(string[] args) => new Config(args);

        public void PrintUsage()
        {
            _optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}