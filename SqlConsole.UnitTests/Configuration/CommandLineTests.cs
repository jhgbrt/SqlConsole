using Xunit;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Configuration
{
    public class CommandLineTests
    {
        private string _valueStr = "value";
        private Value _value;

        public CommandLineTests()
        {
            _value = Value.From(_valueStr);
        }

        [Fact]
        public void ToStringTest()
        {
            var commandLine = new CommandLine
            {
                [CommandLineParam.server] = Value.From("server"),
                [CommandLineParam.database] = Value.From("database"),
                [CommandLineParam.user] = Value.From("user"),
            };
            Assert.NotNull(commandLine.ToString());

        }
        [Fact]
        public void Server_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.server);
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
            var commandLine = Test(_valueStr, CommandLineParam.database);
        }
        [Fact]
        public void File_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.file);
        }
        [Fact]
        public void IntegratedSecurity_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.integratedsecurity);
            Assert.Equal(_value, commandLine.IntegratedSecurity);
        }
        [Fact]
        public void Password_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.password);
        }

        [Fact]
        public void User_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.user);
            Assert.Equal(_value, commandLine.User);
        }

        [Fact]
        public void Port_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.port);
            Assert.Equal(_value, commandLine.Port);
        }

        private static CommandLine Test(string commandLineValue, CommandLineParam param)
        {
            var commandLine = new CommandLine {[param] = Value.From(commandLineValue)};
            Assert.Equal(commandLineValue, commandLine[param].Get());
            return commandLine;
        }
    }
}