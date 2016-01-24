using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Configuration
{
    [TestClass]
    public class ConnectionStringParamTests
    {
        [TestMethod]
        public void Default_IsNull()
        {
            var value = new ConnectionStringParam();
            Assert.IsNull(value.Name);
        }

        [TestMethod]
        public void SameString_Equal()
        {
            var value1 = ConnectionStringParam.DataSource;
            var value2 = ConnectionStringParam.DataSource;
            Assert.AreEqual(value1, value2);
            Assert.IsFalse(value1 != value2);
            Assert.IsTrue(value1 == value2);
        }
        [TestMethod]
        public void SameString_Object_Equal()
        {
            object value1 = ConnectionStringParam.DataSource;
            object value2 = ConnectionStringParam.DataSource;
            Assert.AreEqual(value1, value2);
        }

        [TestMethod]
        public void DifferentString_NotEqual()
        {
            var value1 = ConnectionStringParam.UserId;
            var value2 = ConnectionStringParam.Password;
            Assert.AreNotEqual(value1, value2);
            Assert.IsTrue(value1 != value2);
            Assert.IsFalse(value1 == value2);
        }
        [TestMethod]
        public void SameString_SameHashCode()
        {
            var value1 = ConnectionStringParam.IntegratedSecurity;
            var value2 = ConnectionStringParam.IntegratedSecurity;
            Assert.AreEqual(value1.GetHashCode(), value2.GetHashCode());
        }
        [TestMethod]
        public void DifferentString_DifferentHashCode()
        {
            var value1 = ConnectionStringParam.DataSource;
            var value2 = ConnectionStringParam.Server;
            Assert.AreNotEqual(value1.GetHashCode(), value2.GetHashCode());
        }

        [TestMethod]
        public void Equals_Null_ReturnsFalse()
        {
            object value1 = ConnectionStringParam.Server;
            Assert.IsFalse(value1.Equals(null));
        }
        [TestMethod]
        public void Null_GetHashCode_ReturnsFalse()
        {
            object value1 = new ConnectionStringParam();
            Assert.AreEqual(0, value1.GetHashCode());
        }
    }
}