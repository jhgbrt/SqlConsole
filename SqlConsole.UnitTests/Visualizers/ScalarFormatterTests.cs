using System.Linq;
using Xunit;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Visualizers
{
    public class ScalarFormatterTests
    {
        [Fact]
        public void Format()
        {
            object input = 123;

            var formatter = new ScalarFormatter();
            var result = formatter.Format(input).Single();
            var expected = "123";

            Assert.Equal(expected, result);
        }
    }
}