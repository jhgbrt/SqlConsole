namespace SqlConsole.Host
{
    struct Provider
    {
         public readonly string Name;

         public Provider(string name)
            : this()
        {
            Name = name;
        }

        #region Equality
        private bool Equals(Provider other) => string.Equals(Name, other.Name);
        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (obj is Provider && Equals((Provider) obj));
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
        public static bool operator ==(Provider left, Provider right) => left.Equals(right);
        public static bool operator !=(Provider left, Provider right) => !(left == right);
        #endregion

        public override string ToString() => Name ?? "default";

        public static readonly Provider Sqlserver = new Provider("sqlserver");
        public static readonly Provider SqlCompact = new Provider("sqlce");
        public static readonly Provider SqLite = new Provider("sqlite");
        public static readonly Provider Oracle = new Provider("oracle");
        public static readonly Provider IbmDB2 = new Provider("db2");
        public static readonly Provider MySql = new Provider("mysql");
        public static readonly Provider PostGreSQL = new Provider("postgres");
        public static readonly Provider Default = new Provider(null);
    }
}