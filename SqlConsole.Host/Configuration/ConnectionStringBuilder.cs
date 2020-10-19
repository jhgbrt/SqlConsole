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
            CommandLine commandLine)
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

            if (!string.IsNullOrEmpty(commandLine.ConnectionString))
            {
                return commandLine.ConnectionString;
            }

            var commandLineToConnectionString = ParameterMappings[provider].ToDictionary(x => x.commandLine, x => x.connectionString);

            var csb = new DbConnectionStringBuilder();
            foreach (var p in commandLine.Where(kv => commandLineToConnectionString.ContainsKey(kv.Key)))
            {
                var parameterName = commandLineToConnectionString[p.Key];
                csb[parameterName.Name] = p.Value;
            }
            return csb.ConnectionString;
        }


        private static readonly ILookup<Provider,(Provider provider, CommandLineParam commandLine, ConnectionStringParam connectionString)> ParameterMappings = new []
        {
            (Provider.SqlServer, server, DataSource),
            (Provider.SqlServer, database, InitialCatalog),
            (Provider.SqlServer, user, UserId),
            (Provider.SqlServer, password, Password),
            (Provider.SqlServer, integratedSecurity, IntegratedSecurity),
            (Provider.SqlServer, file, Attachdbfilename),
            (Provider.Oracle, server, DataSource),
            (Provider.Oracle, database, InitialCatalog),
            (Provider.Oracle, user, UserId),
            (Provider.Oracle, password, Password),
            (Provider.Oracle, integratedSecurity, IntegratedSecurity),
            (Provider.IbmDB2, server, Server),
            (Provider.IbmDB2, port, Port),
            (Provider.IbmDB2, database, Database),
            (Provider.IbmDB2, user, Uid),
            (Provider.IbmDB2, password, Pwd),
            (Provider.MySql, server, Server),
            (Provider.MySql, port, Port),
            (Provider.MySql, database, Database),
            (Provider.MySql, user, Uid),
            (Provider.MySql, password, Pwd),
            (Provider.MySql, integratedSecurity, IntegratedSecurity),
            (Provider.PostGreSQL, port, Port),
            (Provider.PostGreSQL, server, Server),
            (Provider.PostGreSQL, database, Database),
            (Provider.PostGreSQL, user, UserId),
            (Provider.PostGreSQL, password, Password),
            (Provider.PostGreSQL, integratedSecurity, IntegratedSecurity),
            (Provider.SqLite, file, DataSource),
            (Provider.SqLite, password, Password)
        }.ToLookup(p => p.Item1);

        private static readonly RuleList Rules = new RuleList
        {
            {ProviderIsRequired, Provider.None},
            {ServerIsRequired, Provider.Oracle, Provider.MySql, Provider.PostGreSQL},
            {ServerOrFileIsRequired, Provider.SqlServer},
            {DatabaseIsRequired, Provider.SqlServer, Provider.Oracle, Provider.MySql, Provider.PostGreSQL},
            {FileIsRequired, Provider.SqLite},
            {SetIntegratedSecurityIfNoUser, Provider.SqlServer, Provider.Oracle, Provider.MySql},
            {AttachPortToServer, Provider.IbmDB2}
        };

        private static bool ProviderIsRequired(CommandLine commandLine) => !string.IsNullOrEmpty(commandLine.Provider);
        private static bool ServerIsRequired(CommandLine commandLine) => !string.IsNullOrEmpty(commandLine.Server);
        private static bool ServerOrFileIsRequired(CommandLine commandLine) => !string.IsNullOrEmpty(commandLine.Server) || !string.IsNullOrEmpty(commandLine.File);
        private static bool FileIsRequired(CommandLine commandLine) => !string.IsNullOrEmpty(commandLine.File);
        private static bool DatabaseIsRequired(CommandLine commandLine) => !string.IsNullOrEmpty(commandLine.Database);
        private static bool SetIntegratedSecurityIfNoUser(CommandLine commandLine)
        {
            if (!string.IsNullOrEmpty(commandLine.User) && string.IsNullOrEmpty(commandLine.IntegratedSecurity))
            {
                commandLine.IntegratedSecurity = "True";
            }
            return true;
        }
        private static bool AttachPortToServer(CommandLine commandLine)
        {
            if (string.IsNullOrEmpty(commandLine.Port)) return true;
            if (string.IsNullOrEmpty(commandLine.Server)) return false;
            var server = commandLine.Server;
            var port = commandLine.Port;
            commandLine.Server = $"{server}:{port}";
            return true;
        }

        #region helper classes
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

            public RuleList this[Provider provider] 
                => new RuleList(this.Where(r => r.Provider == provider));
        }

        record Rule(Provider Provider, Func<CommandLine, bool> Apply, string Description)
        {
            public Rule(Provider provider, Func<CommandLine, bool> apply) : this(provider, apply, string.Empty)
            {
                var sentence = string.Join(" ", apply.Method.Name.BookTitleToSentence());
                Description = $"{sentence} for provider {provider}";
            }

            public override string ToString() => Description;
        }
        #endregion
    }
}