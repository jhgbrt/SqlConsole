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
        public bool Equals(Provider other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Provider && Equals((Provider)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(Provider left, Provider right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Provider left, Provider right)
        {
            return !(left == right);
        }
        #endregion

        public override string ToString()
        {
            return Name ?? "Default";
        }

        public static readonly Provider Sqlserver = new Provider("System.Data.SqlClient");
        public static readonly Provider SqlCompact = new Provider("System.Data.SqlServerCe.4.0");
        public static readonly Provider SqLite = new Provider("System.Data.SqLite");
        public static readonly Provider Oracle = new Provider("Oracle.ManagedDataAccess.Client");
        public static readonly Provider IbmDB2 = new Provider("IBM.Data.DB2");
        public static readonly Provider MySql = new Provider("MySql.Data.MySqlClient");
        public static readonly Provider PostGreSQL = new Provider("Npgsql");
        public static readonly Provider Default = new Provider(null);
    }
}