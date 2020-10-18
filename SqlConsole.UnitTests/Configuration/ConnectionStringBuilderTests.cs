using System.Data.Common;
using SqlConsole.Host;
using Xunit;
using static SqlConsole.Host.CommandLineParam;
using static SqlConsole.Host.ConnectionStringBuilder;

namespace SqlConsole.UnitTests.Configuration
{
    public class ConnectionStringBuilderTests
    {
        [Fact]
        public void DefaultProvider_EmptyCommandLine_Throws()
        {
            var provider = Provider.SqlServer;
            var commandLine = new CommandLine();
            Assert.Throws<ConnectionConfigException>(() => GetConnectionString(provider, commandLine));
        }
        [Fact]
        public void DefaultProvider_invalidCommandLine_Throws()
        {
            var provider = Provider.SqlServer;
            var commandLine = new CommandLine {[user] = "user"};
            Assert.Throws<ConnectionConfigException>(() => GetConnectionString(provider, commandLine));
        }

        [Fact]
        public void SqlServerProvider()
        {
            var commandLine = new CommandLine
            {
                [provider] = "sqlserver",
                [server] = "server",
                [database] = "database",
                [user] = "user",
                [password] = "password",
            };
            var cb = GetConnectionStringBuilder(Provider.SqlServer, commandLine);
            Assert.Equal("server", cb["Data Source"]);
            Assert.Equal("database", cb["Initial Catalog"]);
            Assert.Equal("user", cb["User Id"]);
            Assert.Equal("password", cb["Password"]);
        }

        [Fact]
        public void OracleProvider()
        {
            var commandLine = new CommandLine
            {
                [provider] = "oracle",
                [server] = "server",
                [database] = "database",
                [user] = "user",
                [password] = "password",
            };
            var cb = GetConnectionStringBuilder(Provider.Oracle, commandLine);
            Assert.Equal("server", cb["Data Source"]);
            Assert.Equal("database", cb["Initial Catalog"]);
            Assert.Equal("user", cb["User Id"]);
            Assert.Equal("password", cb["Password"]);
        }
        [Fact]
        public void DB2Provider()
        {
            var commandLine = new CommandLine
            {
                [provider] = "db2",
                [server] = "server",
                [port] = "1234",
                [database] = "database",
                [user] = "user",
                [password] = "password",
            };
            var cb = GetConnectionStringBuilder(Provider.IbmDB2, commandLine);
            Assert.Equal("server:1234", cb["Server"]);
            Assert.Equal("database", cb["Database"]);
            Assert.Equal("user", cb["Uid"]);
            Assert.Equal("password", cb["Pwd"]);
        }
        [Fact]
        public void MySqlProvider()
        {
            var commandLine = new CommandLine
            {
                [provider] = "mysql",
                [server] = "server",
                [database] = "database",
                [user] = "user",
                [password] = "password",
            };
            var cb = GetConnectionStringBuilder(Provider.MySql, commandLine);
            Assert.Equal("server", cb["Server"]);
            Assert.Equal("database", cb["Database"]);
            Assert.Equal("user", cb["Uid"]);
            Assert.Equal("password", cb["Pwd"]);
        }

        [Fact]
        public void SqLiteProvider()
        {
            var commandLine = new CommandLine
            {
                [provider] = "sqlite",
                [file] = "file",
                [password] = "password",
            };
            var cb = GetConnectionStringBuilder(Provider.SqLite, commandLine);
            Assert.Equal("file", cb["Data Source"]);
            Assert.Equal("password", cb["Password"]);
        }

        private static DbConnectionStringBuilder GetConnectionStringBuilder(Provider provider, CommandLine commandLine)
        {
            var result = GetConnectionString(provider, commandLine);
            var cb = new DbConnectionStringBuilder
            {
                ConnectionString = result
            };
            return cb;
        }
    }
}