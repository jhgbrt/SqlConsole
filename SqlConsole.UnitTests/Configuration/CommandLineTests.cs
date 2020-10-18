using Xunit;
using SqlConsole.Host;
using static SqlConsole.Host.CommandLineParam;

namespace SqlConsole.UnitTests.Configuration
{
    public class CommandLineTests
    {
        private string _value = "value";


        [Fact]
        public void TestToString()
        {
            var commandLine = new CommandLine
            {
                [server] = "server",
                [database] = "database",
                [user] = "user",
            };
            Assert.NotNull(commandLine.ToString());

        }
        [Fact]
        public void Server_WhenSet_SetsValue()
        {
            var commandLine = Test(_value, server);
            Assert.Equal(_value, commandLine.Server);
        }
        [Fact]
        public void Server_WhenSetViaProperty_SetsValue()
        {
            var commandLine = new CommandLine {Server = _value};
            Assert.Equal(_value, commandLine.Server);
        }
        [Fact]
        public void Database_WhenSet_SetsValue()
        {
            var commandLine = Test(_value, database);
        }
        [Fact]
        public void File_WhenSet_SetsValue()
        {
            var commandLine = Test(_value, file);
        }
        [Fact]
        public void IntegratedSecurity_WhenSet_SetsValue()
        {
            var commandLine = Test(_value, integratedSecurity);
            Assert.Equal(_value, commandLine.IntegratedSecurity);
        }
        [Fact]
        public void Password_WhenSet_SetsValue()
        {
            var commandLine = Test(_value, password);
        }

        [Fact]
        public void User_WhenSet_SetsValue()
        {
            var commandLine = Test(_value, user);
            Assert.Equal(_value, commandLine.User);
        }

        [Fact]
        public void Port_WhenSet_SetsValue()
        {
            var commandLine = Test(_value, port);
            Assert.Equal(_value, commandLine.Port);
        }

        private static CommandLine Test(string commandLineValue, CommandLineParam param)
        {
            var commandLine = new CommandLine {[param] = commandLineValue};
            Assert.Equal(commandLineValue, commandLine[param]);
            return commandLine;
        }
    }
}