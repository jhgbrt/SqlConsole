using Xunit;
using SqlConsole.Host;
using static SqlConsole.Host.ConnectionStringParam;

namespace SqlConsole.UnitTests.Configuration
{
    public class ConnectionStringParamTests
    {
        [Fact]
        public void Default_IsNull()
        {
            var value = new ConnectionStringParam();
            Assert.Null(value.Name);
        }

        [Fact]
        public void SameString_Equal()
        {
            var value1 = DataSource;
            var value2 = DataSource;
            Assert.Equal(value1, value2);
            Assert.False(value1 != value2);
            Assert.True(value1 == value2);
        }
        [Fact]
        public void SameString_Object_Equal()
        {
            object value1 = DataSource;
            object value2 = DataSource;
            Assert.Equal(value1, value2);
        }

        [Fact]
        public void DifferentString_NotEqual()
        {
            var value1 = UserId;
            var value2 = Password;
            Assert.NotEqual(value1, value2);
            Assert.True(value1 != value2);
            Assert.False(value1 == value2);
        }
        [Fact]
        public void SameString_SameHashCode()
        {
            var value1 = IntegratedSecurity;
            var value2 = IntegratedSecurity;
            Assert.Equal(value1.GetHashCode(), value2.GetHashCode());
        }
        [Fact]
        public void DifferentString_DifferentHashCode()
        {
            var value1 = DataSource;
            var value2 = Server;
            Assert.NotEqual(value1.GetHashCode(), value2.GetHashCode());
        }

        [Fact]
        public void Equals_Null_ReturnsFalse()
        {
            object value1 = Server;
            Assert.False(value1.Equals(null));
        }
        [Fact]
        public void Null_GetHashCode_ReturnsFalse()
        {
            object value1 = new ConnectionStringParam();
            Assert.Equal(0, value1.GetHashCode());
        }
    }
}