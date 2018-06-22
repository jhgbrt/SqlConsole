using SqlConsole.Host;
using Xunit;
using static SqlConsole.Host.CommandLineParam;

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
            var value1 = server;
            var value2 = server;
            Assert.Equal(value1, value2);
            Assert.False(value1 != value2);
            Assert.True(value1 == value2);
        }
        [Fact]
        public void SameString_Object_Equal()
        {
            object value1 = server;
            object value2 = server;
            Assert.Equal(value1, value2);
        }

        [Fact]
        public void DifferentString_NotEqual()
        {
            var value1 = user;
            var value2 = password;
            Assert.NotEqual(value1, value2);
            Assert.True(value1 != value2);
            Assert.False(value1 == value2);
        }
        [Fact]
        public void SameString_SameHashCode()
        {
            var value1 = integratedsecurity;
            var value2 = integratedsecurity;
            Assert.Equal(value1.GetHashCode(), value2.GetHashCode());
        }
        [Fact]
        public void DifferentString_DifferentHashCode()
        {
            var value1 = database;
            var value2 = server;
            Assert.NotEqual(value1.GetHashCode(), value2.GetHashCode());
        }

        [Fact]
        public void Equals_Null_ReturnsFalse()
        {
            object value1 = server;
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