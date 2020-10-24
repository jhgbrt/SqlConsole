using IBM.Data.DB2.Core;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Net.Code.ADONet;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace SqlConsole.Host
{

    record Provider(string Name, DbProviderFactory Factory, DbConfig DbConfig)
    {
        public static readonly Provider[] All = new Provider[] 
        {
            new Provider<SqlConnectionStringBuilder   >("sqlserver", SqlClientFactory.Instance   , DbConfig.Default   ),
            new Provider<SqliteConnectionStringBuilder>("sqlite"   , SqliteFactory.Instance      , DbConfig.Default   ),
            new Provider<OracleConnectionStringBuilder>("oracle"   , OracleClientFactory.Instance, DbConfig.Oracle    ),
            new Provider<DB2ConnectionStringBuilder   >("db2"      , DB2Factory.Instance         , DbConfig.Default   ),
            new Provider<MySqlConnectionStringBuilder >("mysql"    , MySqlClientFactory.Instance , DbConfig.Default   ),
            new Provider<NpgsqlConnectionStringBuilder>("postgres" , NpgsqlFactory.Instance      , DbConfig.PostGreSQL)
        };

        public override string ToString() => Name;
    }

    record Provider<TConnectionStringBuilder>(string Name, DbProviderFactory Factory, DbConfig DbConfig)
        : Provider(Name, Factory, DbConfig)
    {
        public Provider(string name) : this(name, SqlClientFactory.Instance, DbConfig.Default)
        {

        }
    }
}