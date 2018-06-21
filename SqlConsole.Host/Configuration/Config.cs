using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using IBM.Data.DB2.Core;
using Microsoft.Data.Sqlite;
using Mono.Options;
using MySql.Data.MySqlClient;
using Net.Code.ADONet;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using static System.Configuration.ConfigurationManager;
using static SqlConsole.Host.ConnectionStringBuilder;

namespace SqlConsole.Host
{
    static class ConfigEx
    {
        public static DbProviderFactory GetFactory(this Config config)
        {
            var providerName = config.ProviderName;
            switch (providerName)
            {
                case "postgres": return NpgsqlFactory.Instance;
                case "mysql": return MySqlClientFactory.Instance;
                case "sqlserver": return SqlClientFactory.Instance;
                case "db2": return DB2Factory.Instance;
                case "oracle": return OracleClientFactory.Instance;
                case "sqlite": return SqliteFactory.Instance;
                default: throw new ConnectionConfigException($"Unsupported provider: '{providerName}'");
            }
        }

        public static DbConfig GetDbConfig(this Config config)
        {
            var providerName = config.ProviderName;
            switch (providerName)
            {
                case "postgres": return DbConfig.PostGreSQL(providerName);
                case "oracle": return DbConfig.Oracle(providerName);
                default: return DbConfig.Create(providerName);
            }
        }
    }

    class Config
    {
        private readonly Provider _provider;
        private readonly MyOptionSet _optionSet;

        public static Config Create(string[] args) => new Config(args);

        public void PrintUsage()
        {
            _optionSet.WriteOptionDescriptions(Console.Out);
        }

        public string ProviderName => _provider.Name;

        public string ConnectionString { get; }

        public bool Help { get;  }

        public string Output { get; }

        public string Query { get; }

        public bool Scalar { get; }

        public bool NonQuery { get; }

        public bool OutputToFile => !string.IsNullOrEmpty(Output) && !string.IsNullOrEmpty(Query);

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

        private Config(string[] args)
        {
            Provider? p = null;
            new OptionSet { { "provider=", "", s => p = new Provider(s) } }.Parse(args);

            var provider = p ?? Provider.Default;

            string output = null;
            bool scalar = false, nonquery = false, help = false;
            var commandLine = new CommandLine();
            var connectionString = new ConnectionString();

            void SetCommandLineValue(CommandLineParam cl, string s) => commandLine[cl] = Value.From(s.Trim('"'));

            bool OnUnknownOption(string name, string value)
            {
                connectionString[new ConnectionStringParam(name.Trim('"'))] = Value.From(value.Trim('"'));
                return true;
            }

            var options = new MyOptionSet(
                CommandLineToConnectionString(provider), 
                SetCommandLineValue,
                OnUnknownOption)
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
                    "provider=",
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
                if (settings == null) 
                    throw new ConnectionConfigException("No connection configuration found. Either provide command line parameters, or add a .config file with a connection string named 'Default'");
                ConnectionString = settings.ConnectionString;
                _provider = new Provider(settings.ProviderName);
            }
        }
    }
}