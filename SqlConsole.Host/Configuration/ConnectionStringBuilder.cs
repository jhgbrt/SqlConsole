using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using static SqlConsole.Host.CommandLineParam;
using static SqlConsole.Host.ConnectionStringParam;

namespace SqlConsole.Host
{
    internal class ConnectionConfigException : Exception
    {
        public ConnectionConfigException(string message):base(message) {}
        
    }
    internal static class ConnectionStringBuilder
    {
        public static string GetConnectionString(
            Provider provider,
            CommandLine commandLine,
            ConnectionString connectionStringParameters
            )
        {
            var errors = (
                from rule in Rules[provider]
                where !rule.Apply(commandLine)
                select rule.Description
                ).ToList();

            if (errors.Any())
            {
                var message = string.Join(Environment.NewLine, errors);
                throw new ConnectionConfigException(message);
            }

            var csb = new DbConnectionStringBuilder();

            if (commandLine.ConnectionString.HasValue)
            {
                return commandLine.ConnectionString.Value.Get();
            }

            var commandLineToConnectionString = CommandLineToConnectionString(provider);

            foreach (var p in commandLine)
            {
                var parameterName = commandLineToConnectionString[p.Key];
                csb[parameterName.Name] = p.Value.Get();
            }
            foreach (var p in connectionStringParameters)
            {
                csb[p.Key.Name] = p.Value.Get();
            }
            return csb.ConnectionString;
        }

        public static IDictionary<CommandLineParam, ConnectionStringParam> CommandLineToConnectionString(Provider provider)
            => ParameterMappings[provider].ToDictionary(x => x.CommandLineParam, x => x.ConnectionStringParam);

        private static readonly ILookup<Provider,Parameter> ParameterMappings = new ParameterList
        {
            {Provider.Default, server, DataSource},
            {Provider.Default, database, InitialCatalog},
            {Provider.Default, user, UserId},
            {Provider.Default, integratedsecurity, IntegratedSecurity},
            {Provider.Default, password, Password},
            {Provider.Sqlserver, server, DataSource},
            {Provider.Sqlserver, database, InitialCatalog},
            {Provider.Sqlserver, user, UserId},
            {Provider.Sqlserver, password, Password},
            {Provider.Sqlserver, integratedsecurity, IntegratedSecurity},
            {Provider.Sqlserver, file, Attachdbfilename},
            {Provider.Oracle, server, DataSource},
            {Provider.Oracle, database, InitialCatalog},
            {Provider.Oracle, user, UserId},
            {Provider.Oracle, password, Password},
            {Provider.Oracle, integratedsecurity, IntegratedSecurity},
            {Provider.IbmDB2, server, Server},
            {Provider.IbmDB2, port, Port},
            {Provider.IbmDB2, database, Database},
            {Provider.IbmDB2, user, Uid},
            {Provider.IbmDB2, password, Pwd},
            {Provider.MySql, server, Server},
            {Provider.MySql, port, Port},
            {Provider.MySql, database, Database},
            {Provider.MySql, user, Uid},
            {Provider.MySql, password, Pwd},
            {Provider.MySql, integratedsecurity, IntegratedSecurity},
            {Provider.PostGreSQL, port, Port},
            {Provider.PostGreSQL, server, Server},
            {Provider.PostGreSQL, database, Database},
            {Provider.PostGreSQL, user, UserId},
            {Provider.PostGreSQL, password, Password},
            {Provider.PostGreSQL, integratedsecurity, IntegratedSecurity},
            {Provider.SqlCompact, file, DataSource},
            {Provider.SqlCompact, password, Password},
            {Provider.SqLite, file, DataSource},
            {Provider.SqLite, password, Password}
        }.ToLookup(p => p.Provider);

        private static readonly RuleList Rules = new RuleList
        {
            {ServerIsRequired, Provider.Default, Provider.Oracle, Provider.MySql, Provider.PostGreSQL},
            {ServerOrFileIsRequired, Provider.Sqlserver},
            {DatabaseIsRequired, Provider.Default, Provider.Sqlserver, Provider.Oracle,Provider.MySql, Provider.PostGreSQL},
            {SetIntegratedSecurityIfNoUser, Provider.Sqlserver, Provider.Oracle, Provider.MySql, Provider.Default},
            {AttachPortToServer, Provider.IbmDB2}
        };

        private static bool ServerIsRequired(CommandLine commandLine) => commandLine.Server.HasValue;
        private static bool ServerOrFileIsRequired(CommandLine commandLine) => commandLine.Server.HasValue || commandLine.File.HasValue;
        private static bool DatabaseIsRequired(CommandLine commandLine) => commandLine.Database.HasValue;
        private static bool SetIntegratedSecurityIfNoUser(CommandLine commandLine)
        {
            if (commandLine.Any() && !commandLine.User.HasValue && !commandLine.IntegratedSecurity.HasValue)
            {
                commandLine.IntegratedSecurity = Value.From("True");
            }
            return true;
        }
        private static bool AttachPortToServer(CommandLine commandLine)
        {
            if (!commandLine.Port.HasValue) return true;
            if (!commandLine.Server.HasValue) return false;
            var server = commandLine.Server;
            var port = commandLine.Port;
            commandLine.Server = Value.From($"{server}:{port}");
            return true;
        }

        #region helper classes
        struct Parameter
        {
            public readonly CommandLineParam CommandLineParam;
            public readonly ConnectionStringParam ConnectionStringParam;
            public readonly Provider Provider;

            public Parameter(
                CommandLineParam commandLineParam,
                ConnectionStringParam connectionStringParam,
                Provider provider)
                : this()
            {
                CommandLineParam = commandLineParam;
                ConnectionStringParam = connectionStringParam;
                Provider = provider;
            }
        }

        class ParameterList : List<Parameter>
        {
            public void Add(Provider providerName, CommandLineParam commandLineParam, ConnectionStringParam connectionStringParam)
            {
                Add(new Parameter(commandLineParam, connectionStringParam, providerName));
            }
        }
        class RuleList : List<Rule>
        {
            public RuleList() { }

            private RuleList(IEnumerable<Rule> rules)
            {
                AddRange(rules);
            }
            public void Add(Func<CommandLine, bool> ruleFunc, params Provider[] providers) 
                => AddRange(providers.Select(p => new Rule(p, ruleFunc)));

            public override string ToString() => string.Join(Environment.NewLine, this);

            public RuleList this[Provider provider] => new RuleList(this.Where(r => r.Provider == provider));
        }

        class Rule
        {
            public Rule(Provider provider, Func<CommandLine, bool> apply)
            {
                var sentence = string.Join(" ", apply.Method.Name.BookTitleToSentence());
                Description = $"{sentence} for provider {provider}";
                Provider = provider;
                Apply = apply;
            }

            public Provider Provider { get; }
            public Func<CommandLine, bool> Apply { get; }
            public string Description { get; }
            public override string ToString() => Description;
        }
        #endregion
    }
}