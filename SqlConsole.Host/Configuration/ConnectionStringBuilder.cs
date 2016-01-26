using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using static SqlConsole.Host.CommandLineParam;
using static SqlConsole.Host.ConnectionStringParam;
using static SqlConsole.Host.Provider;

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
            {Default, server, DataSource},
            {Default, database, InitialCatalog},
            {Default, user, UserId},
            {Default, integratedsecurity, IntegratedSecurity},
            {Default, password, Password},
            {Sqlserver, server, DataSource},
            {Sqlserver, database, InitialCatalog},
            {Sqlserver, user, UserId},
            {Sqlserver, password, Password},
            {Sqlserver, integratedsecurity, IntegratedSecurity},
            {Sqlserver, file, Attachdbfilename},
            {Oracle, server, DataSource},
            {Oracle, database, InitialCatalog},
            {Oracle, user, UserId},
            {Oracle, password, Password},
            {Oracle, integratedsecurity, IntegratedSecurity},
            {IbmDB2, server, Server},
            {IbmDB2, port, Port},
            {IbmDB2, database, Database},
            {IbmDB2, user, Uid},
            {IbmDB2, password, Pwd},
            {MySql, server, Server},
            {MySql, port, Port},
            {MySql, database, Database},
            {MySql, user, Uid},
            {MySql, password, Pwd},
            {MySql, integratedsecurity, IntegratedSecurity},
            {PostGreSQL, port, Port},
            {PostGreSQL, server, Server},
            {PostGreSQL, database, Database},
            {PostGreSQL, user, UserId},
            {PostGreSQL, password, Password},
            {PostGreSQL, integratedsecurity, IntegratedSecurity},
            {SqlCompact, file, DataSource},
            {SqlCompact, password, Password},
            {SqLite, file, DataSource},
            {SqLite, password, Password}
        }.ToLookup(p => p.Provider);

        private static readonly RuleList Rules = new RuleList
        {
            {ServerIsRequired, Default, Oracle, MySql, PostGreSQL},
            {ServerOrFileIsRequired, Sqlserver},
            {DatabaseIsRequired, Default, Sqlserver, Oracle, MySql, PostGreSQL},
            {SetIntegratedSecurityIfNoUser, Sqlserver, Oracle, MySql, Default},
            {AttachPortToServer, IbmDB2}
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