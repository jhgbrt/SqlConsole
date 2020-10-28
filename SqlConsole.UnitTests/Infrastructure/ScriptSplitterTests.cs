using SqlConsole.Host.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using Xunit.Abstractions;

namespace SqlConsole.UnitTests.Infrastructure
{
    static class Script
    {
        public static List<string> ParseScripts(string script, Action<string> log)
        {
            return new ScriptSplitter(script, log).Split().ToList();
        }
    }

    /// <summary>
    /// Summary description for ScriptHelperTests.
    /// </summary>
    public class ScriptSplittingTests
    {
        ITestOutputHelper _output;
        public ScriptSplittingTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Theory]
        [InlineData("")]
        [InlineData("GO\r\n")]
        [InlineData("GX", "GX")]
        [InlineData("GX test", "GX test")]
        [InlineData("GO\r\nGO")]
        [InlineData("// abc\r\n", "// abc\r\n")]
        [InlineData("/ abc\r\n", "/ abc\r\n")]
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
        public void SplitTest(string input, params string[] expected)
        {
            var result = Script.ParseScripts(input, s => _output.WriteLine(s)).ToArray();
            Assert.Equal(expected, result);
        }
    }
}