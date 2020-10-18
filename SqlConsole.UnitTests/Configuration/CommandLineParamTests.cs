using SqlConsole.Host;
using Xunit;
using static SqlConsole.Host.CommandLineParam;

namespace SqlConsole.UnitTests.Configuration
{
    public class CommandLineParamTests
    {

        [Fact]
        public void Prototype_Boolean()
        {
            var clp = new CommandLineParam("test", string.Empty, CommandLineParamType.Boolean);
            Assert.Equal("test", clp.Prototype);
        }
        [Fact]
        public void Prototype_String()
        {
            var clp = new CommandLineParam("test", string.Empty, CommandLineParamType.String);
            Assert.Equal("test=", clp.Prototype);
        }
    }
}