using SqlConsole.Host;
using Xunit;

namespace SqlConsole.UnitTests.Configuration
{
    public class CommandLineParamTests
    {
        [Fact]
        public void Default_IsNull()
        {
            var value = new CommandLineParam();
            Assert.Null(value.Name);
        }

        [Fact]
        public void SameString_Equal()
        {
            var value1 = CommandLineParam.server;
            var value2 = CommandLineParam.server;
            Assert.Equal(value1, value2);
            Assert.False(value1 != value2);
            Assert.True(value1 == value2);
        }
        [Fact]
        public void SameString_Object_Equal()
        {
            object value1 = CommandLineParam.server;
            object value2 = CommandLineParam.server;
            Assert.Equal(value1, value2);
        }

        [Fact]
        public void DifferentString_NotEqual()
        {
            var value1 = CommandLineParam.user;
            var value2 = CommandLineParam.password;
            Assert.NotEqual(value1, value2);
            Assert.True(value1 != value2);
            Assert.False(value1 == value2);
        }
        [Fact]
        public void SameString_SameHashCode()
        {
            var value1 = CommandLineParam.integratedsecurity;
            var value2 = CommandLineParam.integratedsecurity;
            Assert.Equal(value1.GetHashCode(), value2.GetHashCode());
        }
        [Fact]
        public void DifferentString_DifferentHashCode()
        {
            var value1 = CommandLineParam.database;
            var value2 = CommandLineParam.server;
            Assert.NotEqual(value1.GetHashCode(), value2.GetHashCode());
        }

        [Fact]
        public void Equals_Null_ReturnsFalse()
        {
            object value1 = CommandLineParam.server;
            Assert.False(value1.Equals(null));
        }
        [Fact]
        public void Null_GetHashCode_ReturnsFalse()
        {
            object value1 = new CommandLineParam();
            Assert.Equal(0, value1.GetHashCode());
        }
    }
}