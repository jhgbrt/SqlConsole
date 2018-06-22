using System.Collections;
using System.Collections.Generic;
using static SqlConsole.Host.CommandLineParam;

namespace SqlConsole.Host
{
    class CommandLine : IEnumerable<KeyValuePair<CommandLineParam,string>>
    {
        private readonly Dictionary<CommandLineParam, string> _dictionary = new Dictionary<CommandLineParam, string>();
        public string Server
        {
            get => Get(server);
            set => Set(server, value);
        }

        public string Database => Get(database);
        public string File => Get(file);
        public string Port => Get(port);
        public string User => Get(user);
        public string ConnectionString => Get(connectionString);

        public bool Contains(CommandLineParam key) => _dictionary.ContainsKey(key);

        public string IntegratedSecurity
        {
            get => Get(integratedsecurity);
            set => Set(integratedsecurity, value);
        }

        public string Query
        {
            get => Get(query);
            set => Set(query, value);
        }

        public string this[CommandLineParam key]
        {
            get => Get(key);
            set => Set(key, value);
        } 

        private void Set(CommandLineParam key, string value) => _dictionary[key] = value;

        private string Get(CommandLineParam commandLineParam) => _dictionary.TryGetValue(commandLineParam, out var v) ? v : null;

        public IEnumerator<KeyValuePair<CommandLineParam, string>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => string.Join(",", this);
    }
}
