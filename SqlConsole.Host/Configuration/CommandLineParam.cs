using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlConsole.Host
{
    struct CommandLineParam : IEquatable<CommandLineParam>
    {
        enum CommandLineParamType
        {
            Boolean,
            String
        }

        public readonly string Name;
        public readonly string Description;
        readonly CommandLineParamType _type;
        public string Prototype => _type == CommandLineParamType.Boolean ? Name : Name + "=";

        private CommandLineParam(string name, string description, CommandLineParamType type)
            : this()
        {
            _type = type;
            Name = name;
            Description = description;
        }

        #region Equality
        public bool Equals(CommandLineParam other) => string.Equals(Name, other.Name);

        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (obj is CommandLineParam && Equals((CommandLineParam) obj));

        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        public static bool operator ==(CommandLineParam left, CommandLineParam right) => left.Equals(right);

        public static bool operator !=(CommandLineParam left, CommandLineParam right) => !(left == right);
        #endregion

        public override string ToString() => Name;

        // ReSharper disable InconsistentNaming
        public static readonly CommandLineParam server = String(nameof(server), "The server or data source to connect to.");
        public static readonly CommandLineParam port = String(nameof(port), "Server TCP port. Optional, only relevant for some providers.");
        public static readonly CommandLineParam database = String(nameof(database), "The database instance");
        public static readonly CommandLineParam user = String(nameof(user), "User name (in case no integrated security is used)");
        public static readonly CommandLineParam password = String(nameof(password), "Password (in case no integrated security is used)");
        public static readonly CommandLineParam integratedsecurity = String(nameof(integratedsecurity), "use windows integrated security (or not)");
        public static readonly CommandLineParam connectionString = String(nameof(connectionString), "a full-blown connection string. Other parameters are ignored.");
        public static readonly CommandLineParam file = String(nameof(file), "The db File, for providers that attach directly to file");
        public static readonly CommandLineParam output = String(nameof(output), "Path to output File. If none specified, output is written to the console.");
        public static readonly CommandLineParam scalar = Boolean(nameof(scalar), "Interpret the query as a scalar result, i.e. a single value (of any type)");
        public static readonly CommandLineParam nonquery = Boolean(nameof(nonquery), "Run the query as a 'non-select' statement, i.e. INSERT, UPDATE, DELETE or a DDL statement. Outputs the number of affected records of the last statement.");
        public static readonly CommandLineParam providerName = String(nameof(providerName), "The db provider name.");
        public static readonly CommandLineParam help = Boolean(nameof(help), "Print help text");
        public static readonly CommandLineParam query = String(nameof(query), "The query");
        // ReSharper restore InconsistentNaming

        public static IEnumerable<CommandLineParam> All => 
            typeof(CommandLineParam).GetFields(BindingFlags.Static|BindingFlags.Public)
            .Where(f => f.FieldType == typeof(CommandLineParam))
            .Select(f => (CommandLineParam)f.GetValue(null));

        private static CommandLineParam Boolean(string name, string description) => new CommandLineParam(name, description, CommandLineParamType.Boolean);
        private static CommandLineParam String(string name, string description) => new CommandLineParam(name, description, CommandLineParamType.String);
    }
}