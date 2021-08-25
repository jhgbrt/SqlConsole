using IBM.Data.DB2.Core;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Net.Code.ADONet;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Data.SqlClient;
using System.Reflection;

namespace SqlConsole.Host;

abstract record Provider(string Name, DbProviderFactory Factory, DbConfig DbConfig)
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
    public abstract IEnumerable<PropertyInfo> ConnectionConfigurationProperties();
}

record Provider<TConnectionStringBuilder>(string Name, DbProviderFactory Factory, DbConfig DbConfig)
    : Provider(Name, Factory, DbConfig)
{
    public override IEnumerable<PropertyInfo> ConnectionConfigurationProperties()
    {
        var type = typeof(TConnectionStringBuilder);
        var baseproperties = typeof(DbConnectionStringBuilder).GetProperties().Select(p => p.Name).ToHashSet();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(p => p.PropertyType.IsSimpleType() && p.GetSetMethod() is not null && !baseproperties.Contains(p.Name));
        return properties;
    }
}
