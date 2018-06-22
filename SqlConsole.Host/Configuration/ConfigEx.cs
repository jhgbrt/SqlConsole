using System.Data.Common;
using System.Data.SqlClient;
using IBM.Data.DB2.Core;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Net.Code.ADONet;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace SqlConsole.Host
{
    static class ConfigEx
    {
        public static DbProviderFactory GetFactory(this Config config)
        {
            var providerName = config.ProviderName;
            switch (providerName)
            {
                case "postgres": return NpgsqlFactory.Instance;
                case "mysql": return MySqlClientFactory.Instance;
                case "sqlserver": return SqlClientFactory.Instance;
                case "db2": return DB2Factory.Instance;
                case "oracle": return OracleClientFactory.Instance;
                case "sqlite": return SqliteFactory.Instance;
                default: return SqlClientFactory.Instance;
            }
        }

        public static DbConfig GetDbConfig(this Config config)
        {
            var providerName = config.ProviderName;
            switch (providerName)
            {
                case "postgres": return DbConfig.PostGreSQL(providerName);
                case "oracle": return DbConfig.Oracle(providerName);
                default: return DbConfig.Create(providerName);
            }
        }
    }
}