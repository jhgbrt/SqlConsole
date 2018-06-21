using System.Collections.Generic;
using System.Diagnostics;

namespace SqlConsole.Host
{
    class CommandLine : Dictionary<CommandLineParam, Value>
    {
        public Value? Server
        {
            get { return TryGet(CommandLineParam.server); }
            set { SetValue(CommandLineParam.server, value); }
        }
        public Value? Database => TryGet(CommandLineParam.database);
        public Value? File => TryGet(CommandLineParam.file);

        public Value? Port => TryGet(CommandLineParam.port);

        public Value? User => TryGet(CommandLineParam.user);

        public Value? ConnectionString => TryGet(CommandLineParam.connectionString);

        public Value? IntegratedSecurity
        {
            get { return TryGet(CommandLineParam.integratedsecurity); }
            set { SetValue(CommandLineParam.integratedsecurity, value); }
        }

        private void SetValue(CommandLineParam key, Value? value)
        {
            Debug.Assert(value != null, "value != null");
            this[key] = value.Value;
        }

        private Value? TryGet(CommandLineParam commandLineParam)
        {
            Value v;
            if (TryGetValue(commandLineParam, out v)) return v;
            return null;

        }
        public override string ToString() => string.Join(",", this);
    }
}