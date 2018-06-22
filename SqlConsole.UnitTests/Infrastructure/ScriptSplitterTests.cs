using System.Collections.Generic;
using System.Linq;
using SqlConsole.Host.Infrastructure;
using Xunit;

namespace SqlConsole.UnitTests.Infrastructure
{
    static class Script
    {
        public static List<string> ParseScripts(string script)
        {
            return ScriptSplitter.Split(script).ToList();
        }
    }

    /// <summary>
    /// Summary description for ScriptHelperTests.
    /// </summary>
    public class ScriptHelperTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("GO\r\n")]
        [InlineData("GO\r\nGO")]
        [InlineData("// abc\r\n", "// abc\r\n")]
        [InlineData("-- abc", "-- abc")]
        [InlineData("/* /* abc\r\n */\r\ngo\r\n -- def*/", "/* /* abc\r\n */\r\n", " -- def*/")]
        [InlineData("foo\r\ngo\r\nbar", "foo\r\n", "bar")]
        [InlineData("'foo'", "'foo'")]
        [InlineData("'foo --bar'\r\nGO", "'foo --bar'\r\n")]
        [InlineData("'foo ''bar'''\r\nGO", "'foo ''bar'''\r\n")]
        [InlineData("\"foo \"\"bar\"\"\"\r\nGO", "\"foo \"\"bar\"\"\"\r\n")]
        [InlineData("'foo //bar'\r\nGO", "'foo //bar'\r\n")]
        [InlineData("'foo /*bar*/'\r\nGO", "'foo /*bar*/'\r\n")]
        [InlineData("foo\r\ngo", "foo\r\n")]
        [InlineData("foo\r\ngo\r\n", "foo\r\n")]
        [InlineData("foo\r\ngo\r\nGO\r\n", "foo\r\n")]
        [InlineData("foo\r\ngo\r\nGO\r\nbar", "foo\r\n", "bar")]
        [InlineData("foo\r\nGO\r\nfoo_go_bar\r\nGO\r\n", "foo\r\n", "foo_go_bar\r\n")]
        public void Tests(string input, params string[] expected)
        {
            var result = Script.ParseScripts(input).ToArray();
            Assert.Equal(expected, result);
        }
    }
}