using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Mono.Options;

namespace SqlConsole.Host
{
    class Config
    {
        private readonly OptionSet _optionSet;
        
        static OptionSet CreateOptionSet(Config config, Provider provider)
        {
            var fixedOptions = new OptionSet
            {
                {
                    "output=",
                    "Path to output File. If none specified, output is written to the console.",
                    s => config.Output = s
                },
                {
                    "scalar",
                    "Interpret the query as a scalar result, i.e. a single value (of any type)",
                    s => config.Scalar = true
                },
                {
                    "nonquery",
                    "Run the query as a 'non-select' statement, i.e. INSERT, UPDATE, DELETE or a DDL statement. " +
                    "Outputs the number of affected records of the last statement.",
                    s => config.NonQuery = true
                },
                {
                    "providerName=",
                    "The db provider name.",
                    s => config.Provider = new Provider(s)
                },
                {
                    "help",
                    "Print this text.",
                    s => config.Help = true
                }
            };


            var variableOptions =
                from kv in ParameterMapper.GetParameterMappings(provider)
                let cli = kv.Key
                let description = string.Format("{0} (maps to {1})", cli.Description, string.Join("/", kv.Select(x => x.ConnectionStringParam).Distinct()))
                select OptionSet.CreateOption(cli.Prototype, description, s => config.CommandLine[cli] = Value.From(s));


            var optionSet = new OptionSet()
                .OnUnknownOption((name, value) =>
                {
                    var connectionString = new ConnectionStringParam(name);
                    config.ConnectionStringParameters[connectionString] = Value.From(value);
                    return true;
                })
                .AddRange(variableOptions)
                .AddRange(fixedOptions);

            return optionSet;
        }

        public Provider Provider { get; private set; }

        private IDictionary<ConnectionStringParam, Value> ConnectionStringParameters { get; set; }
        private CommandLine CommandLine { get; set; }
        public string ConnectionString { get; private set; }

        public bool Help { get; private set; }

        private Config Parse(string[] args)
        {
            var remaining = _optionSet.Parse(args);
            if (remaining.Any())
            {
                var queryOrFileName = remaining.First();
                Query = File.Exists(queryOrFileName) ? File.ReadAllText(queryOrFileName) : queryOrFileName;
            }

            if (CommandLine.Any() || ConnectionStringParameters.Any())
            {
                var csb = ParameterMapper.CreateConnectionStringBuilder(
                    Provider,
                    CommandLine,
                    ConnectionStringParameters);

                ConnectionString = csb.ConnectionString;
            }
            else
            {
                var settings = ConfigurationManager.ConnectionStrings["Default"];
                if (settings != null)
                {
                    ConnectionString = settings.ConnectionString;
                    Provider = new Provider(settings.ProviderName ?? string.Empty);
                } 
            }

            return this;
        }

        private Config(string[] args)
        {
            Provider? provider = null;
            new OptionSet {{"providerName=", "", s => provider = new Provider(s)}}.Parse(args);
            _optionSet = CreateOptionSet(this, provider ?? Provider.Default);
            Provider = Provider.Default;
            CommandLine = new CommandLine();
            ConnectionStringParameters = new Dictionary<ConnectionStringParam, Value>();
        }

        public string Output { get; private set; }

        public string Query { get; private set; }

        public bool Scalar { get; private set; }

        public bool NonQuery { get; private set; }

        public static Config Create(string[] args)
        {
            var result = new Config(args).Parse(args);
            return result;
        }

        public void PrintUsage()
        {
            _optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}