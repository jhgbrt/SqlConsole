using IBM.Data.DB2.Core;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Net.Code.ADONet;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace SqlConsole.Host
{
    record Provider(string Name, DbProviderFactory Factory, DbConfig DbConfig)
    {

        public override string ToString() => Name;
        public static readonly Provider None = new Provider(string.Empty, null, null);
        public static readonly Provider SqlServer = new Provider("sqlserver", SqlClientFactory.Instance, DbConfig.Default);
        public static readonly Provider SqLite = new Provider("sqlite", SqliteFactory.Instance, DbConfig.Default);
        public static readonly Provider Oracle = new Provider("oracle", OracleClientFactory.Instance, DbConfig.Oracle);
        public static readonly Provider IbmDB2 = new Provider("db2", DB2Factory.Instance, DbConfig.Default);
        public static readonly Provider MySql = new Provider("mysql", MySqlClientFactory.Instance, DbConfig.Default);
        public static readonly Provider PostGreSQL = new Provider("postgres",  NpgsqlFactory.Instance, DbConfig.PostGreSQL);

        static readonly IDictionary<string, Provider> All = typeof(Provider).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(f => f.FieldType == typeof(Provider))
            .Select(f => (Provider)f.GetValue(null)!)
            .ToDictionary(p => p.Name);

        internal static Provider Get(string name) => All[name];
    }
}