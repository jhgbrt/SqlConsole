using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Configuration
{
    [TestClass]
    public class ValueTests
    {
        [TestMethod]
        public void Default_IsNull()
        {
            var value = new Value();
            Assert.IsNull(value.Get());
        }
        [TestMethod]
        public void Empty()
        {
            var value = Value.From(string.Empty);
            Assert.AreEqual(string.Empty, value.Get());
        }
        [TestMethod]
        public void SomeString()
        {
            var value = Value.From("SomeString");
            Assert.AreEqual("SomeString", value.Get());
        }

        [TestMethod]
        public void SameString_Equal()
        {
            var value1 = Value.From("SomeString");
            var value2 = Value.From("SomeString");
            Assert.AreEqual(value1, value2);
            Assert.IsFalse(value1 != value2);
            Assert.IsTrue(value1 == value2);
        }

        [TestMethod]
        public void DifferentString_NotEqual()
        {
            var value1 = Value.From("SomeString1");
            var value2 = Value.From("SomeString2");
            Assert.AreNotEqual(value1, value2);
            Assert.IsTrue(value1 != value2);
            Assert.IsFalse(value1 == value2);
        }
        [TestMethod]
        public void SameString_SameHashCode()
        {
            var value1 = Value.From("SomeString");
            var value2 = Value.From("SomeString");
            Assert.AreEqual(value1.GetHashCode(), value2.GetHashCode());
        }
        [TestMethod]
        public void DifferentString_DifferentHashCode()
        {
            var value1 = Value.From("SomeString1");
            var value2 = Value.From("SomeString2");
            Assert.AreNotEqual(value1.GetHashCode(), value2.GetHashCode());
        }
    }
}