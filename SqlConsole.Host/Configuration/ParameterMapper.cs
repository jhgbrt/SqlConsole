using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace SqlConsole.Host
{
    internal static class ParameterMapper
    {
        class ParameterMappingSet : IEnumerable<ParameterMapping>
        {
            private readonly List<ParameterMapping> _parameterMappings = new List<ParameterMapping>();

            public void Add(Provider providerName, CommandLineParam commandLineParam, ConnectionStringParam connectionStringParam)
            {
                _parameterMappings.Add(new ParameterMapping(commandLineParam, connectionStringParam, providerName));
            }

            public IEnumerator<ParameterMapping> GetEnumerator()
            {
                return _parameterMappings.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private static readonly ParameterMappingSet ParameterMappings = new ParameterMappingSet
        {
            {Provider.Default, CommandLineParam.Server, ConnectionStringParam.DataSource},
            {Provider.Default, CommandLineParam.Database, ConnectionStringParam.InitialCatalog},
            {Provider.Default, CommandLineParam.User, ConnectionStringParam.UserId},
            {Provider.Default, CommandLineParam.Integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            {Provider.Default, CommandLineParam.Password, ConnectionStringParam.Password},
            {Provider.Sqlserver, CommandLineParam.Server, ConnectionStringParam.DataSource},
            {Provider.Sqlserver, CommandLineParam.Database, ConnectionStringParam.InitialCatalog},
            {Provider.Sqlserver, CommandLineParam.User, ConnectionStringParam.UserId},
            {Provider.Sqlserver, CommandLineParam.Password, ConnectionStringParam.Password},
            {Provider.Sqlserver, CommandLineParam.Integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            {Provider.Sqlserver, CommandLineParam.File, ConnectionStringParam.Attachdbfilename},
            {Provider.Oracle, CommandLineParam.Server, ConnectionStringParam.DataSource},
            {Provider.Oracle, CommandLineParam.Database, ConnectionStringParam.InitialCatalog},
            {Provider.Oracle, CommandLineParam.User, ConnectionStringParam.UserId},
            {Provider.Oracle, CommandLineParam.Password, ConnectionStringParam.Password},
            {Provider.Oracle, CommandLineParam.Integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            {Provider.IbmDB2, CommandLineParam.Server, ConnectionStringParam.Server},
            {Provider.IbmDB2, CommandLineParam.Port, ConnectionStringParam.Port},
            {Provider.IbmDB2, CommandLineParam.Database, ConnectionStringParam.Database},
            {Provider.IbmDB2, CommandLineParam.User, ConnectionStringParam.Uid},
            {Provider.IbmDB2, CommandLineParam.Password, ConnectionStringParam.Pwd},
            {Provider.MySql, CommandLineParam.Server, ConnectionStringParam.Server},
            {Provider.MySql, CommandLineParam.Port, ConnectionStringParam.Port},
            {Provider.MySql, CommandLineParam.Database, ConnectionStringParam.Database},
            {Provider.MySql, CommandLineParam.User, ConnectionStringParam.Uid},
            {Provider.MySql, CommandLineParam.Password, ConnectionStringParam.Pwd},
            {Provider.MySql, CommandLineParam.Integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            {Provider.PostGreSQL, CommandLineParam.Port, ConnectionStringParam.Port},
            {Provider.PostGreSQL, CommandLineParam.Server, ConnectionStringParam.Server},
            {Provider.PostGreSQL, CommandLineParam.Database, ConnectionStringParam.Database},
            {Provider.PostGreSQL, CommandLineParam.User, ConnectionStringParam.UserId},
            {Provider.PostGreSQL, CommandLineParam.Password, ConnectionStringParam.Password},
            {Provider.PostGreSQL, CommandLineParam.Integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            {Provider.SqlCompact, CommandLineParam.File, ConnectionStringParam.DataSource},
            {Provider.SqlCompact, CommandLineParam.File, ConnectionStringParam.Password},
            {Provider.SqLite, CommandLineParam.File, ConnectionStringParam.DataSource},
            {Provider.SqLite, CommandLineParam.Password, ConnectionStringParam.Password}
        };

        private static bool ServerIsRequired(CommandLine commandLine)
        {
            return commandLine.Server.HasValue;
        }

        private static bool SetIntegratedSecurityIfNoUser(CommandLine commandLine)
        {
            if (!commandLine.User.HasValue && !commandLine.IntegratedSecurity.HasValue)
            {
                Console.WriteLine("Using Integrated Security");
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
            commandLine.Server = Value.From(string.Format("{0}:{1}", server, port));
            return true;
        }

        class RuleSet : IEnumerable<Rule>
        {
            private readonly List<Rule> _rules = new List<Rule>();

            public IEnumerator<Rule> GetEnumerator()
            {
                return _rules.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            public RuleBuilder Add(Func<CommandLine, bool> ruleFunc)
            {
                return new RuleBuilder(this, ruleFunc);
            }
            public class RuleBuilder
            {
                private readonly RuleSet _parent;
                private readonly Func<CommandLine, bool> _ruleFunc;

                public RuleBuilder(RuleSet parent, Func<CommandLine, bool> ruleFunc)
                {
                    _parent = parent;
                    _ruleFunc = ruleFunc;
                }
                public RuleSet AppliesTo(params Provider[] providerNames)
                {
                    foreach (var p in providerNames)
                    {
                        _parent._rules.Add(new Rule(p, _ruleFunc));
                    }
                    return _parent;
                }
            }

            public override string ToString()
            {
                return string.Join(Environment.NewLine, _rules);
            }

            public RuleSet this[Provider provider]
            {
                get
                {
                    var result = new RuleSet();
                    result._rules.AddRange(_rules.Where(r => r.Provider == provider));
                    return result;
                }
            }
        }

        class Rule
        {
            public Rule(Provider provider, Func<CommandLine, bool> apply)
            {
                Description = string.Format("{0} for provider {1}", string.Join(" ", apply.Method.Name.BookTitleToSentence()), provider);
                Provider = provider;
                Apply = apply;
            }

            public Provider Provider { get; private set; }
            public Func<CommandLine, bool> Apply { get; private set; }
            public string Description { get; private set; }

            public override string ToString()
            {
                return Description;
            }
        }

        private static readonly RuleSet RulesSet = new RuleSet()
            .Add(ServerIsRequired).AppliesTo(Provider.Sqlserver, Provider.Oracle, Provider.MySql)
            .Add(SetIntegratedSecurityIfNoUser).AppliesTo(Provider.Sqlserver, Provider.Oracle, Provider.MySql, Provider.Default)
            .Add(AttachPortToServer).AppliesTo(Provider.IbmDB2);

        public static DbConnectionStringBuilder CreateConnectionStringBuilder(
            Provider provider, 
            CommandLine commandLine,
            IDictionary<ConnectionStringParam, Value> connectionStringParameters
            )
        {
            var errors = (
                from rule in RulesSet[provider]
                where !rule.Apply(commandLine)
                let parameterStr = string.Join(";", commandLine.Select(kv => string.Format("{0}={1}", kv.Key, kv.Value)))
                let format = "Error while mapping parameter values to connection string for provider '{0}'. Parameters: '{1}', error: {2}"
                select string.Format(format, provider, parameterStr, rule.Description)
            ).ToList();

            if (errors.Any())
            {
                var message = string.Join(Environment.NewLine, errors);
                throw new Exception(message);
            }
            
            var csb = new DbConnectionStringBuilder();

            var commandLineToConnectionString = (
                from pm in ParameterMappings
                where pm.Provider == provider
                select pm
                ).ToDictionary(x => x.CommandLineParam, x => x.ConnectionStringParam);

            foreach (var p in commandLine)
            {
                var parameterName = commandLineToConnectionString[p.Key];
                csb[parameterName.Name] = p.Value.Get();
            }
            foreach (var p in connectionStringParameters)
            {
                csb[p.Key.Name] = p.Value.Get();
            }
            return csb;
        }
        public static ILookup<CommandLineParam, ParameterMapping> GetParameterMappings(Provider provider)
        {
            return
                (from pm in ParameterMappings
                where pm.Provider == provider
                select pm).ToLookup(item => item.CommandLineParam);
        }
    }
}