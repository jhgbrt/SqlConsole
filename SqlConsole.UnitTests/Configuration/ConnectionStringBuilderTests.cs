using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Configuration
{
    [TestClass]
    public class ConnectionStringBuilderTests
    {
        [TestMethod]
        [ExpectedException(typeof(ConnectionConfigException))]
        public void DefaultProvider_EmptyCommandLine_Throws()
        {
            var provider = Provider.Default;
            var commandLine = new CommandLine();
            var connectionString = new ConnectionString();
            var result = ConnectionStringBuilder.GetConnectionString(provider, commandLine, connectionString);
        }
        [TestMethod]
        [ExpectedException(typeof(ConnectionConfigException))]
        public void DefaultProvider_invalidCommandLine_Throws()
        {
            var provider = Provider.Default;
            var commandLine = new CommandLine {[CommandLineParam.user] = Value.From("user")};
            var connectionString = new ConnectionString();
            var result = ConnectionStringBuilder.GetConnectionString(provider, commandLine, connectionString);
        }

        [TestMethod]
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
            Assert.AreEqual("server", cb["Data Source"]);
            Assert.AreEqual("database", cb["Initial Catalog"]);
            Assert.AreEqual("user", cb["User Id"]);
            Assert.AreEqual("password", cb["Password"]);
        }

        [TestMethod]
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
            Assert.AreEqual("server", cb["Data Source"]);
            Assert.AreEqual("database", cb["Initial Catalog"]);
            Assert.AreEqual("user", cb["User Id"]);
            Assert.AreEqual("password", cb["Password"]);
        }

        [TestMethod]
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
            Assert.AreEqual("server", cb["Data Source"]);
            Assert.AreEqual("database", cb["Initial Catalog"]);
            Assert.AreEqual("user", cb["User Id"]);
            Assert.AreEqual("password", cb["Password"]);
        }
        [TestMethod]
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
            Assert.AreEqual("server:1234", cb["Server"]);
            Assert.AreEqual("database", cb["Database"]);
            Assert.AreEqual("user", cb["Uid"]);
            Assert.AreEqual("password", cb["Pwd"]);
        }
        [TestMethod]
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
            Assert.AreEqual("server", cb["Server"]);
            Assert.AreEqual("database", cb["Database"]);
            Assert.AreEqual("user", cb["Uid"]);
            Assert.AreEqual("password", cb["Pwd"]);
        }

        [TestMethod]
        public void SqLiteProvider()
        {
            var provider = Provider.SqLite;
            var commandLine = new CommandLine
            {
                [CommandLineParam.file] = Value.From("file"),
                [CommandLineParam.password] = Value.From("password"),
            };
            var cb = GetConnectionStringBuilder(provider, commandLine);
            Assert.AreEqual("file", cb["Data Source"]);
            Assert.AreEqual("password", cb["Password"]);
        }

        [TestMethod]
        public void SqCompactProvider()
        {
            var provider = Provider.SqlCompact;
            var commandLine = new CommandLine
            {
                [CommandLineParam.file] = Value.From("file"),
                [CommandLineParam.password] = Value.From("password"),
            };
            var cb = GetConnectionStringBuilder(provider, commandLine);
            Assert.AreEqual("file", cb["Data Source"]);
            Assert.AreEqual("password", cb["Password"]);
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

        [TestMethod]
        public void CommandLineToConnectionString_Default()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.Default).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.server, ConnectionStringParam.DataSource},
                {CommandLineParam.database, ConnectionStringParam.InitialCatalog},
                {CommandLineParam.user, ConnectionStringParam.UserId},
                {CommandLineParam.password, ConnectionStringParam.Password},
                {CommandLineParam.integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            }.ToList();
            CollectionAssert.AreEquivalent(expected, result);
        }
        [TestMethod]
        public void CommandLineToConnectionString_SqlServer()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.Sqlserver).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.server, ConnectionStringParam.DataSource},
                {CommandLineParam.database, ConnectionStringParam.InitialCatalog},
                {CommandLineParam.user, ConnectionStringParam.UserId},
                {CommandLineParam.password, ConnectionStringParam.Password},
                {CommandLineParam.integratedsecurity, ConnectionStringParam.IntegratedSecurity},
                {CommandLineParam.file, ConnectionStringParam.Attachdbfilename},
            }.ToList();
            CollectionAssert.AreEquivalent(expected, result);
        }
        [TestMethod]
        public void CommandLineToConnectionString_Oracle()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.Oracle).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.server, ConnectionStringParam.DataSource},
                {CommandLineParam.database, ConnectionStringParam.InitialCatalog},
                {CommandLineParam.user, ConnectionStringParam.UserId},
                {CommandLineParam.password, ConnectionStringParam.Password},
                {CommandLineParam.integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            }.ToList();
            CollectionAssert.AreEquivalent(expected, result);
        }
        [TestMethod]
        public void CommandLineToConnectionString_Sqlite()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.SqLite).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.file, ConnectionStringParam.DataSource},
                {CommandLineParam.password, ConnectionStringParam.Password},
            }.ToList();
            CollectionAssert.AreEquivalent(expected, result);
        }
        [TestMethod]
        public void CommandLineToConnectionString_SqlCompact()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.SqlCompact).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.file, ConnectionStringParam.DataSource},
                {CommandLineParam.password, ConnectionStringParam.Password}
            }.ToList();
            CollectionAssert.AreEquivalent(expected, result);
        }
        [TestMethod]
        public void CommandLineToConnectionString_IBMDB2()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.IbmDB2).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.server, ConnectionStringParam.Server},
                {CommandLineParam.port, ConnectionStringParam.Port},
                {CommandLineParam.database, ConnectionStringParam.Database},
                {CommandLineParam.user, ConnectionStringParam.Uid},
                {CommandLineParam.password, ConnectionStringParam.Pwd},
            }.ToList();
            CollectionAssert.AreEquivalent(expected, result);
        }
        [TestMethod]
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
            CollectionAssert.AreEquivalent(expected, result);
        }
        [TestMethod]
        public void CommandLineToConnectionString_PostGres()
        {
            var result = ConnectionStringBuilder.CommandLineToConnectionString(Provider.PostGreSQL).ToList();
            var expected = new Dictionary<CommandLineParam, ConnectionStringParam>
            {
                {CommandLineParam.server, ConnectionStringParam.Server},
                {CommandLineParam.port, ConnectionStringParam.Port},
                {CommandLineParam.database, ConnectionStringParam.Database},
                {CommandLineParam.user, ConnectionStringParam.UserId},
                {CommandLineParam.password, ConnectionStringParam.Password},
                {CommandLineParam.integratedsecurity, ConnectionStringParam.IntegratedSecurity},
            }.ToList();
            CollectionAssert.AreEquivalent(expected, result);
        }
    }
}