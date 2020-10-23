using System;
using System.Data;
using System.Linq;
using Xunit;
using Net.Code.ADONet;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Visualizers
{
    public class NonQueryFormatterTests
    {
        [Theory]
        [InlineData(123, new[] { "123 rows affected" })]
        [InlineData(0, new[] { "0 rows affected" })]
        [InlineData(1, new[] { "1 row affected" })]
        [InlineData(-1, new string[0])]
        public void Format(int input, string[] expected)
        {
            var formatter = new NonQueryFormatter();
            var result = formatter.Format(input);
            Assert.Equal(expected, result);
        }
    }
}