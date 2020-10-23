using IBM.Data.DB2.Core;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Net.Code.ADONet;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using SqlConsole.Host.Configuration;
using System.Linq;

namespace SqlConsole.Host
{

    record Provider(string Name, DbProviderFactory Factory, DbConfig DbConfig)
    {
        public static readonly Provider[] All = new Provider[] 
        {
            new Provider<SqlConnectionStringBuilder   , SqlSettings   >("sqlserver", SqlClientFactory.Instance   , DbConfig.Default   ),
            new Provider<SqliteConnectionStringBuilder, SqliteSettings>("sqlite"   , SqliteFactory.Instance      , DbConfig.Default   ),
            new Provider<OracleConnectionStringBuilder, OracleSettings>("oracle"   , OracleClientFactory.Instance, DbConfig.Oracle    ),
            new Provider<DB2ConnectionStringBuilder   , DB2Settings   >("db2"      , DB2Factory.Instance         , DbConfig.Default   ),
            new Provider<MySqlConnectionStringBuilder , MySqlSettings >("mysql"    , MySqlClientFactory.Instance , DbConfig.Default   ),
            new Provider<NpgsqlConnectionStringBuilder, NpgsqlSettings>("postgres" , NpgsqlFactory.Instance      , DbConfig.PostGreSQL)
        };

        public override string ToString() => Name;

        public static Provider<TConnectionStringBuilder, TSettings> Get<TConnectionStringBuilder, TSettings>() 
            => All.OfType<Provider<TConnectionStringBuilder, TSettings>>().Single();
    }

    record Provider<TConnectionStringBuilder, TSettings>(string Name, DbProviderFactory Factory, DbConfig DbConfig)
        : Provider(Name, Factory, DbConfig)
    {
    }
}