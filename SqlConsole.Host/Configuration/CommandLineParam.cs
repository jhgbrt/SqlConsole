using System;

namespace SqlConsole.Host
{
    struct CommandLineParam : IEquatable<CommandLineParam>
    {
        public readonly string Name;
        public readonly string Description;
        public string Prototype => Name + "=";

        private CommandLineParam(string name, string description)
            : this()
        {
            Name = name;
            Description = description;
        }

        #region Equality
        public bool Equals(CommandLineParam other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CommandLineParam && Equals((CommandLineParam) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
        
        public static bool operator ==(CommandLineParam left, CommandLineParam right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CommandLineParam left, CommandLineParam right)
        {
            return !(left == right);
        }
        #endregion

        public override string ToString()
        {
            return Name;
        }

        // ReSharper disable InconsistentNaming
        public static readonly CommandLineParam server = new CommandLineParam("server", "The server or data source to connect to.");
        public static readonly CommandLineParam port = new CommandLineParam("port", "Server TCP port. Optional, only relevant for some providers.");
        public static readonly CommandLineParam database = new CommandLineParam("database", "The database instance");
        public static readonly CommandLineParam user = new CommandLineParam("user", "User name (in case no integrated security is used)");
        public static readonly CommandLineParam password = new CommandLineParam("password", "Password (in case no integrated security is used)");
        public static readonly CommandLineParam integratedsecurity = new CommandLineParam("integratedSecurity", "use windows integrated security (or not)");
        public static readonly CommandLineParam file = new CommandLineParam("file", "The db File, for providers that attach directly to file");
        // ReSharper restore InconsistentNaming
    }
}