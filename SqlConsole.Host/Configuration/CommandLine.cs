using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SqlConsole.Host
{
    class CommandLine : IEnumerable<KeyValuePair<CommandLineParam, Value>>
    {
        readonly IDictionary<CommandLineParam, Value> _values;

        public CommandLine()
        {
            _values = new Dictionary<CommandLineParam, Value>();
        }

        public Value? Server
        {
            get { return TryGet(CommandLineParam.Server); }
            set { SetValue(CommandLineParam.Server, value); }
        }

        public Value? Port
        {
            get { return TryGet(CommandLineParam.Port); }
        }
        public Value? User
        {
            get { return TryGet(CommandLineParam.User); }
        }
        public Value? IntegratedSecurity
        {
            get { return TryGet(CommandLineParam.Integratedsecurity); }
            set { SetValue(CommandLineParam.Integratedsecurity, value); }
        }

        private void SetValue(CommandLineParam key, Value? value)
        {
            Debug.Assert(value != null, "value != null");
            this[key] = value.Value;
        }

        public Value this[CommandLineParam key]
        {
            set { _values[key] = value; }
        }

        private Value? TryGet(CommandLineParam commandLineParam)
        {
            Value v;
            if (_values.TryGetValue(commandLineParam, out v)) return v;
            return null;

        }

        public IEnumerator<KeyValuePair<CommandLineParam, Value>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(",", this);
        }
    }
}