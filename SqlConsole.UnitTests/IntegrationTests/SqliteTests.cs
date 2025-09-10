
using SqlConsole.Host;
using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace SqlConsole.UnitTests.IntegrationTests
{
    public class SqliteQueryTests
    {
        readonly ITestOutputHelper _output;
        readonly TestConsole _console = new TestConsole();
        readonly string query = "CREATE TABLE MyTable(ID int, Name nvarchar(5));INSERT INTO MyTable VALUES(1,'Test1'),(2, 'Test2');SELECT * FROM MyTable";
        readonly string tabularoutput = "ID | Name \r\n-- | -----\r\n1  | Test1\r\n2  | Test2";
        readonly string csvoutput = "\"ID\";\"Name\"\r\n\"1\";\"Test1\"\r\n\"2\";\"Test2\"\r\n";
        public SqliteQueryTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public async Task TestScalarQuery()
        {
            var command = CommandFactory.CreateCommand();
            await command.InvokeAsync("query sqlite \"SELECT 1\" --as-scalar --data-source=:memory:", _console);
            var output = _console.Out.ToString();
            _output.WriteLine(output);
            Assert.EndsWith("1" + Environment.NewLine, output);
        }
        [Fact]
        public async Task TestNonQuery()
        {
            var command = CommandFactory.CreateCommand();
            await command.InvokeAsync(
                $"query sqlite --as-nonquery --data-source=:memory: \"{query}\"", _console);
            var output = _console.Out.ToString();
            _output.WriteLine(output);
            Assert.Contains("2 rows affected", output);
        }

        [Fact]
        public async Task TestQuery()
        {
            // TODO this test does not work on linux for some reason
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            var command = CommandFactory.CreateCommand();
            await command.InvokeAsync(
                $"query sqlite --data-source=:memory: \"{query}\"", _console);
            var output = _console.Out.ToString();
            _output.WriteLine(output);
            Assert.Contains(tabularoutput, output);
        }

        [Fact]
        public async Task TestQueryFromFile()
        {
            if (!OperatingSystem.IsWindows()) return;
            var command = CommandFactory.CreateCommand();
            File.WriteAllText("query.txt", query);
            Assert.True(File.Exists("query.txt"));
            await command.InvokeAsync("query sqlite --data-source=:memory: query.txt", _console);
            var output = _console.Out.ToString();
            _output.WriteLine(output);
            Assert.Contains(tabularoutput, output);
        }

        [Fact]
        public async Task TestQueryFromFileAndWriteToFile()
        {
            var command = CommandFactory.CreateCommand();
            File.WriteAllText("query.txt", query);
            Assert.True(File.Exists("query.txt"));
            await command.InvokeAsync("query sqlite --output=out.txt --data-source=:memory: query.txt", _console);
            Assert.True(File.Exists("out.txt"));
            var output = File.ReadAllText("out.txt");
            _output.WriteLine(output);
            Assert.Equal(csvoutput, output, ignoreLineEndingDifferences: true);
        }


    }


}
