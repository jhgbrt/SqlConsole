using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Configuration
{
    [TestClass]
    public class CommandLineParamTests
    {
        [TestMethod]
        public void Default_IsNull()
        {
            var value = new CommandLineParam();
            Assert.IsNull(value.Name);
        }

        [TestMethod]
        public void SameString_Equal()
        {
            var value1 = CommandLineParam.server;
            var value2 = CommandLineParam.server;
            Assert.AreEqual(value1, value2);
            Assert.IsFalse(value1 != value2);
            Assert.IsTrue(value1 == value2);
        }
        [TestMethod]
        public void SameString_Object_Equal()
        {
            object value1 = CommandLineParam.server;
            object value2 = CommandLineParam.server;
            Assert.AreEqual(value1, value2);
        }

        [TestMethod]
        public void DifferentString_NotEqual()
        {
            var value1 = CommandLineParam.user;
            var value2 = CommandLineParam.password;
            Assert.AreNotEqual(value1, value2);
            Assert.IsTrue(value1 != value2);
            Assert.IsFalse(value1 == value2);
        }
        [TestMethod]
        public void SameString_SameHashCode()
        {
            var value1 = CommandLineParam.integratedsecurity;
            var value2 = CommandLineParam.integratedsecurity;
            Assert.AreEqual(value1.GetHashCode(), value2.GetHashCode());
        }
        [TestMethod]
        public void DifferentString_DifferentHashCode()
        {
            var value1 = CommandLineParam.database;
            var value2 = CommandLineParam.server;
            Assert.AreNotEqual(value1.GetHashCode(), value2.GetHashCode());
        }

        [TestMethod]
        public void Equals_Null_ReturnsFalse()
        {
            object value1 = CommandLineParam.server;
            Assert.IsFalse(value1.Equals(null));
        }
        [TestMethod]
        public void Null_GetHashCode_ReturnsFalse()
        {
            object value1 = new CommandLineParam();
            Assert.AreEqual(0, value1.GetHashCode());
        }
    }
}