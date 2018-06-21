using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xunit;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Configuration
{
    public class ConnectionStringBuilderTests
    {
        [Fact]
        public void DefaultProvider_EmptyCommandLine_Throws()
        {
            var provider = Provider.Default;
            var commandLine = new CommandLine();
            var connectionString = new ConnectionString();
            Assert.Throws<ConnectionConfigException>(() => ConnectionStringBuilder.GetConnectionString(provider, commandLine, connectionString));
        }
        [Fact]
        public void DefaultProvider_invalidCommandLine_Throws()
        {
            var provider = Provider.Default;
            var commandLine = new CommandLine { [CommandLineParam.user] = Value.From("user") };
            var connectionString = new ConnectionString();
            Assert.Throws<ConnectionConfigException>(() => ConnectionStringBuilder.GetConnectionString(provider, commandLine, connectionString));
        }

        [Fact]
        public void DefaultProvider()
        {
            var provider = Provider.Default;
            var commandLine = new CommandLine
            {
                [CommandLineParam.server] = Value.From("server"),
                [CommandLineParam.database] = Value.From("database"),
                [CommandLineParam.user] = Value.From("user"),
                [CommandLineParam.password] = Value.From("password"),
            };
            var cb = GetConnectionStringBuilder(provider, commandLine);
            Assert.Equal("server", cb["Data Source"]);
            Assert.Equal("database", cb["Initial Catalog"]);
            Assert.Equal("user", cb["User Id"]);
            Assert.Equal("password", cb["Password"]);
        }

        [Fact]
        public void SqlServerProvider()
        {
            var provider = Provider.Sqlserver;
            var commandLine = new CommandLine
            {
                [CommandLineParam.server] = Value.From("server"),
                [CommandLineParam.database] = Value.From("database"),
                [CommandLineParam.user] = Value.From("user"),
                [CommandLineParam.password] = Value.From("password"),
            };
            var cb = GetConnectionStringBuilder(provider, commandLine);
            Assert.Equal("server", cb["Data Source"]);
            Assert.Equal("database", cb["Initial Catalog"]);
            Assert.Equal("user", cb["User Id"]);
            Assert.Equal("password", cb["Password"]);
        }

        [Fact]
        public void OracleProvider()
        {
            var provider = Provider.Oracle;
            var commandLine = new CommandLine
            {
                [CommandLineParam.server] = Value.From("server"),
                [CommandLineParam.database] = Value.From("database"),
                [CommandLineParam.user] = Value.From("user"),
                [CommandLineParam.password] = Value.From("password"),
            };
            var cb = GetConnectionStringBuilder(provider, commandLine);
            Assert.Equal("server", cb["Data Source"]);
            Assert.Equal("database", cb["Initial Catalog"]);
            Assert.Equal("user", cb["User Id"]);
            Assert.Equal("password", cb["Password"]);
        }
        [Fact]
        public void DB2Provider()
        {
            var provider = Provider.IbmDB2;
            var commandLine = new CommandLine
            {
                [CommandLineParam.server] = Value.From("server"),
                [CommandLineParam.port] = Value.From("1234"),
                [CommandLineParam.database] = Value.From("database"),
                [CommandLineParam.user] = Value.From("user"),
                [CommandLineParam.password] = Value.From("password"),
            };
            var cb = GetConnectionStringBuilder(provider, commandLine);
            Assert.Equal("server:1234", cb["Server"]);
            Assert.Equal("database", cb["Database"]);
            Assert.Equal("user", cb["Uid"]);
            Assert.Equal("password", cb["Pwd"]);
        }
        [Fact]
        public void MySqlProvider()
        {
            var provider = Provider.IbmDB2;
            var commandLine = new CommandLine
            {
                [CommandLineParam.server] = Value.From("server"),
                [CommandLineParam.database] = Value.From("database"),
                [CommandLineParam.user] = Value.From("user"),
                [CommandLineParam.password] = Value.From("password"),
            };
            var cb = GetConnectionStringBuilder(provider, commandLine);
            Assert.Equal("server", cb["Server"]);
            Assert.Equal("database", cb["Database"]);
            Assert.Equal("user", cb["Uid"]);
            Assert.Equal("password", cb["Pwd"]);
        }

        [Fact]
        public void SqLiteProvider()
        {
            var provider = Provider.SqLite;
            var commandLine = new CommandLine
            {
                [CommandLineParam.file] = Value.From("file"),
                [CommandLineParam.password] = Value.From("password"),
            };
            var cb = GetConnectionStringBuilder(provider, commandLine);
            Assert.Equal("file", cb["Data Source"]);
            Assert.Equal("password", cb["Password"]);
        }

        [Fact]
        public void SqCompactProvider()
        {
            var provider = Provider.SqlCompact;
            var commandLine = new CommandLine
            {
                [CommandLineParam.file] = Value.From("file"),
                [CommandLineParam.password] = Value.From("password"),
            };
            var cb = GetConnectionStringBuilder(provider, commandLine);
            Assert.Equal("file", cb["Data Source"]);
            Assert.Equal("password", cb["Password"]);
        }

        private static DbConnectionStringBuilder GetConnectionStringBuilder(Provider provider, CommandLine commandLine)
        {
            var result = ConnectionStringBuilder.GetConnectionString(provider, commandLine, new ConnectionString());
            var cb = new DbConnectionStringBuilder()
            {
                ConnectionString = result
            };
            return cb;
        }

        [Fact]
        public void CommandLineToConnectionString_Default()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.Default)
                .OrderBy(x => x.Key.Name).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.server, ConnectionStringParam.DataSource},
                {CommandLineParam.database, ConnectionStringParam.InitialCatalog},
                {CommandLineParam.user, ConnectionStringParam.UserId},
                {CommandLineParam.password, ConnectionStringParam.Password},
                {CommandLineParam.integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            }.OrderBy(x => x.Key.Name).ToList();
            Assert.Equal(expected, result);
        }
        [Fact]
        public void CommandLineToConnectionString_SqlServer()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.Sqlserver).OrderBy(x => x.Key.Name).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.server, ConnectionStringParam.DataSource},
                {CommandLineParam.database, ConnectionStringParam.InitialCatalog},
                {CommandLineParam.user, ConnectionStringParam.UserId},
                {CommandLineParam.password, ConnectionStringParam.Password},
                {CommandLineParam.integratedsecurity, ConnectionStringParam.IntegratedSecurity},
                {CommandLineParam.file, ConnectionStringParam.Attachdbfilename},
            }.OrderBy(x => x.Key.Name).ToList();
            Assert.Equal(expected, result);
        }
        [Fact]
        public void CommandLineToConnectionString_Oracle()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.Oracle).OrderBy(x => x.Key.Name).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.server, ConnectionStringParam.DataSource},
                {CommandLineParam.database, ConnectionStringParam.InitialCatalog},
                {CommandLineParam.user, ConnectionStringParam.UserId},
                {CommandLineParam.password, ConnectionStringParam.Password},
                {CommandLineParam.integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            }.OrderBy(x => x.Key.Name).ToList();
            Assert.Equal(expected, result);
        }
        [Fact]
        public void CommandLineToConnectionString_Sqlite()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.SqLite).OrderBy(x => x.Key.Name).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.file, ConnectionStringParam.DataSource},
                {CommandLineParam.password, ConnectionStringParam.Password},
            }.OrderBy(x => x.Key.Name).ToList();
            Assert.Equal(expected, result);
        }
        [Fact]
        public void CommandLineToConnectionString_SqlCompact()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.SqlCompact).OrderBy(x => x.Key.Name).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.file, ConnectionStringParam.DataSource},
                {CommandLineParam.password, ConnectionStringParam.Password}
            }.OrderBy(x => x.Key.Name).ToList();
            Assert.Equal(expected, result);
        }
        [Fact]
        public void CommandLineToConnectionString_IBMDB2()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.IbmDB2).OrderBy(x => x.Key.Name).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.server, ConnectionStringParam.Server},
                {CommandLineParam.port, ConnectionStringParam.Port},
                {CommandLineParam.database, ConnectionStringParam.Database},
                {CommandLineParam.user, ConnectionStringParam.Uid},
                {CommandLineParam.password, ConnectionStringParam.Pwd},
            }.OrderBy(x => x.Key.Name).ToList();
            Assert.Equal(expected, result);
        }
        [Fact]
        public void CommandLineToConnectionString_MySql()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.MySql).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.server, ConnectionStringParam.Server},
                {CommandLineParam.port, ConnectionStringParam.Port},
                {CommandLineParam.database, ConnectionStringParam.Database},
                {CommandLineParam.user, ConnectionStringParam.Uid},
                {CommandLineParam.password, ConnectionStringParam.Pwd},
                {CommandLineParam.integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            }.ToList();
            Assert.Equal(expected, result);
        }
        [Fact]
        public void CommandLineToConnectionString_PostGres()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.PostGreSQL).OrderBy(x => x.Key.Name).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.server, ConnectionStringParam.Server},
                {CommandLineParam.port, ConnectionStringParam.Port},
                {CommandLineParam.database, ConnectionStringParam.Database},
                {CommandLineParam.user, ConnectionStringParam.UserId},
                {CommandLineParam.password, ConnectionStringParam.Password},
                {CommandLineParam.integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            }.OrderBy(x => x.Key.Name).ToList();
            Assert.Equal(expected, result);
        }
    }
}