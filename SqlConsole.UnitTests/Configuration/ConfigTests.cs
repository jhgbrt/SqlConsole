using System.IO;
using Xunit;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Configuration
{
    public class ConfigTests
    {
        [Fact]
        public void WhenNoArguments_ThenThrows()
        {
            Assert.Throws<ConnectionConfigException>(() => Config.Create(new string[] { }));
        }
        [Fact]
        public void WhenArgContainsHelp_HelpIsTrue()
        {
            var config = Config.Create(new[] { "--provider=sqlite", "--server=server", "--database=database", "--help" });
            Assert.True(config.Help);
        }
        [Fact]
        public void WhenArgContainsScalar_ThenScalarIsTrue()
        {
            var config = Config.Create(new[] { "--provider=sqlite", "--provider=sqlserver", "--server=server", "--database=database", "--scalar" });
            Assert.True(config.Scalar);
        }
        [Fact]
        public void WhenArgContainsScalar_ThenNonQueryIsTrue()
        {
            var config = Config.Create(new[] { "--provider=sqlite", "--server=server", "--database=database", "--nonquery" });
            Assert.True(config.NonQuery);
        }
        [Fact]
        public void WhenArgContainsOutput_ThenOutputIsSet()
        {
            var config = Config.Create(new[] { "--provider=sqlite", "--server=server", "--database=database", "--output=output.csv" });
            Assert.Equal("output.csv", config.Output);
        }
        [Fact]
        public void WhenArgContainsExistingFile_ThenThatFileIsTheQuery()
        {
            File.WriteAllText("query.txt", "THIS IS THE QUERY");
            var config = Config.Create(new[] { "--provider=sqlite", "--server=server", "--database=database", "query.txt" });
            Assert.Equal("THIS IS THE QUERY", config.Query);
        }
        [Fact]
        public void WhenArgContainsRandomString_ThenThatStringIsTheQuery()
        {
            var config = Config.Create(new[] { "--provider=sqlite", "--server=server", "--database=database", "THIS IS THE QUERY" });
            Assert.Equal("THIS IS THE QUERY", config.Query);
        }
        [Fact]
        public void WhenKnownCommandLineArgIsSet_ThenItIsTranslatedToConnectionStringArgument()
        {
            var config = Config.Create(new[] { "--provider=sqlserver", "--server=server", "--database=database" });
            Assert.Contains("Data Source=server", config.ConnectionString);
        }
        [Fact]
        public void WhenUnknownCommandLineArgArgIsSet_ThenItIsIgnored()
        {
            var config = Config.Create(new[] { "--provider=sqlserver", "--server=server", "--database=database", "--\"Some Name\"=\"Some Value\"" });
            Assert.DoesNotContain(config.ConnectionString, "Some Name=\"Some Value\"");
        }
        [Fact]
        public void WhenConnectionStringIsPassed_ThenOtherParamsAreIgnored()
        {
            var config = Config.Create(new[] { "--provider=sqlserver", "--server=server", "--database=database", "--connectionString=\"Some Value\"" });
            Assert.Equal("Some Value", config.ConnectionString);
        }
    }
}
