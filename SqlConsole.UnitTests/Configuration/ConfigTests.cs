using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Configuration
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        [ExpectedException(typeof(ConnectionConfigException))]
        public void WhenNoArguments_ThenThrows()
        {
            var config = Config.Create(new string[] { });
            Assert.AreEqual(Provider.Default.Name, config.ProviderName);
        }
        [TestMethod]
        public void WhenArgContainsHelp_HelpIsTrue()
        {
            var config = Config.Create(new[] { "--server=server", "--database=database", "--help" });
            Assert.IsTrue(config.Help);
        }
        [TestMethod]
        public void WhenArgContainsScalar_ThenScalarIsTrue()
        {
            var config = Config.Create(new[] { "--server=server", "--database=database", "--scalar" });
            Assert.IsTrue(config.Scalar);
        }
        [TestMethod]
        public void WhenArgContainsScalar_ThenNonQueryIsTrue()
        {
            var config = Config.Create(new[] { "--server=server", "--database=database", "--nonquery" });
            Assert.IsTrue(config.NonQuery);
        }
        [TestMethod]
        public void WhenArgContainsOutput_ThenOutputIsSet()
        {
            var config = Config.Create(new[] { "--server=server", "--database=database", "--output=output.csv" });
            Assert.AreEqual("output.csv", config.Output);
        }
        [TestMethod]
        public void WhenArgContainsExistingFile_ThenThatFileIsTheQuery()
        {
            File.WriteAllText("query.txt", "THIS IS THE QUERY");
            var config = Config.Create(new[] {"--server=server", "--database=database", "query.txt" });
            Assert.AreEqual("THIS IS THE QUERY", config.Query);
        }
        [TestMethod]
        public void WhenArgContainsRandomString_ThenThatStringIsTheQuery()
        {
            var config = Config.Create(new[] { "--server=server", "--database=database", "THIS IS THE QUERY" });
            Assert.AreEqual("THIS IS THE QUERY", config.Query);
        }
        [TestMethod]
        public void WhenKnownCommandLineArgIsSet_ThenItIsTranslatedToConnectionStringArgument()
        {
            var config = Config.Create(new[] { "--server=server", "--database=database" });
            Assert.IsTrue(config.ConnectionString.Contains("Data Source=server"));
        }
        [TestMethod]
        public void WhenUnknownCommandLineArgArgIsSet_ThenItIsTranslatedToConnectionStringArgument()
        {
            var config = Config.Create(new[] { "--server=server", "--database=database", "--\"Some Name\"=\"Some Value\"" });
            Assert.IsTrue(config.ConnectionString.Contains("Some Name=\"Some Value\""), config.ConnectionString);
        }
    }
}
