using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Configuration
{
    [TestClass]
    public class CommandLineTests
    {
        private string _valueStr = "value";
        private Value _value;

        public CommandLineTests()
        {
            _value = Value.From(_valueStr);
        }

        [TestMethod]
        public void ToString()
        {
            var commandLine = new CommandLine
            {
                [CommandLineParam.server] = Value.From("server"),
                [CommandLineParam.database] = Value.From("database"),
                [CommandLineParam.user] = Value.From("user"),
            };
            Assert.IsNotNull(commandLine.ToString());

        }
        [TestMethod]
        public void Server_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.server);
            Assert.AreEqual(_value, commandLine.Server);
        }
        [TestMethod]
        public void Server_WhenSetViaProperty_SetsValue()
        {
            var commandLine = new CommandLine {Server = _value};
            Assert.AreEqual(_value, commandLine.Server);
        }
        [TestMethod]
        public void Database_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.database);
        }
        [TestMethod]
        public void File_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.file);
        }
        [TestMethod]
        public void IntegratedSecurity_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.integratedsecurity);
            Assert.AreEqual(_value, commandLine.IntegratedSecurity);
        }
        [TestMethod]
        public void Password_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.password);
        }

        [TestMethod]
        public void User_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.user);
            Assert.AreEqual(_value, commandLine.User);
        }

        [TestMethod]
        public void Port_WhenSet_SetsValue()
        {
            var commandLine = Test(_valueStr, CommandLineParam.port);
            Assert.AreEqual(_value, commandLine.Port);
        }

        private static CommandLine Test(string commandLineValue, CommandLineParam param)
        {
            var commandLine = new CommandLine {[param] = Value.From(commandLineValue)};
            Assert.AreEqual(commandLineValue, commandLine[param].Get());
            return commandLine;
        }
    }
}