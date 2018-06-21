using Xunit;
using SqlConsole.Host;

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
            var value1 = ConnectionStringParam.DataSource;
            var value2 = ConnectionStringParam.DataSource;
            Assert.Equal(value1, value2);
            Assert.False(value1 != value2);
            Assert.True(value1 == value2);
        }
        [Fact]
        public void SameString_Object_Equal()
        {
            object value1 = ConnectionStringParam.DataSource;
            object value2 = ConnectionStringParam.DataSource;
            Assert.Equal(value1, value2);
        }

        [Fact]
        public void DifferentString_NotEqual()
        {
            var value1 = ConnectionStringParam.UserId;
            var value2 = ConnectionStringParam.Password;
            Assert.NotEqual(value1, value2);
            Assert.True(value1 != value2);
            Assert.False(value1 == value2);
        }
        [Fact]
        public void SameString_SameHashCode()
        {
            var value1 = ConnectionStringParam.IntegratedSecurity;
            var value2 = ConnectionStringParam.IntegratedSecurity;
            Assert.Equal(value1.GetHashCode(), value2.GetHashCode());
        }
        [Fact]
        public void DifferentString_DifferentHashCode()
        {
            var value1 = ConnectionStringParam.DataSource;
            var value2 = ConnectionStringParam.Server;
            Assert.NotEqual(value1.GetHashCode(), value2.GetHashCode());
        }

        [Fact]
        public void Equals_Null_ReturnsFalse()
        {
            object value1 = ConnectionStringParam.Server;
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