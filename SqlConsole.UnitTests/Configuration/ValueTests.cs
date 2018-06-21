using Xunit;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Configuration
{
    public class ValueTests
    {
        [Fact]
        public void Default_IsNull()
        {
            var value = new Value();
            Assert.Null(value.Get());
        }
        [Fact]
        public void Empty()
        {
            var value = Value.From(string.Empty);
            Assert.Equal(string.Empty, value.Get());
        }
        [Fact]
        public void SomeString()
        {
            var value = Value.From("SomeString");
            Assert.Equal("SomeString", value.Get());
        }

        [Fact]
        public void SameString_Equal()
        {
            var value1 = Value.From("SomeString");
            var value2 = Value.From("SomeString");
            Assert.Equal(value1, value2);
            Assert.False(value1 != value2);
            Assert.True(value1 == value2);
        }

        [Fact]
        public void DifferentString_NotEqual()
        {
            var value1 = Value.From("SomeString1");
            var value2 = Value.From("SomeString2");
            Assert.NotEqual(value1, value2);
            Assert.True(value1 != value2);
            Assert.False(value1 == value2);
        }
        [Fact]
        public void SameString_SameHashCode()
        {
            var value1 = Value.From("SomeString");
            var value2 = Value.From("SomeString");
            Assert.Equal(value1.GetHashCode(), value2.GetHashCode());
        }
        [Fact]
        public void DifferentString_DifferentHashCode()
        {
            var value1 = Value.From("SomeString1");
            var value2 = Value.From("SomeString2");
            Assert.NotEqual(value1.GetHashCode(), value2.GetHashCode());
        }
    }
}