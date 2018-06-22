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
            var provider = Provider.Default;
            var commandLine = new CommandLine();
            Assert.Throws<ConnectionConfigException>(() => GetConnectionString(provider, commandLine));
        }
        [Fact]
        public void DefaultProvider_invalidCommandLine_Throws()
        {
            var provider = Provider.Default;
            var commandLine = new CommandLine {[user] = "user"};
            Assert.Throws<ConnectionConfigException>(() => GetConnectionString(provider, commandLine));
        }

        [Fact]
        public void DefaultProvider()
        {
            var provider = Provider.Default;
            var commandLine = new CommandLine
            {
                [server] = "server",
                [database] = "database",
                [user] = "user",
                [password] = "password",
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
                [server] = "server",
                [database] = "database",
                [user] = "user",
                [password] = "password",
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
                [server] = "server",
                [database] = "database",
                [user] = "user",
                [password] = "password",
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
                [server] = "server",
                [port] = "1234",
                [database] = "database",
                [user] = "user",
                [password] = "password",
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
                [server] = "server",
                [database] = "database",
                [user] = "user",
                [password] = "password",
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
                [file] = "file",
                [password] = "password",
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
                [file] = "file",
                [password] = "password",
            };
            var cb = GetConnectionStringBuilder(provider, commandLine);
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