using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Code.ADONet;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Visualizers
{
    class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    [TestClass]
    public class CsvFormatterTests
    {
        [TestMethod]
        public void Format_CreatesCsv()
        {
            var dataTable = new[]
            {
                new Person {Id = 1, FirstName = "John", LastName = "Doe"}
            }.ToDataTable();

            var formatter = new CsvFormatter();
            var result = formatter.Format(dataTable).ToList();

            CollectionAssert.AreEqual(new[] { "\"Id\";\"FirstName\";\"LastName\"", "\"1\";\"John\";\"Doe\"" }, result);
        }
        [TestMethod]
        public void EmptyDataTable_YieldsEmpty()
        {
            var dataTable = new DataTable();

            var formatter = new CsvFormatter();
            var result = formatter.Format(dataTable).ToList();

            CollectionAssert.AreEqual(new string[] { "" }, result);
        }

        [TestMethod]
        public void SingleResult_YieldsThatResult()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Result", typeof(string));
            dataTable.Rows.Add("Some Value");

            var formatter = new CsvFormatter();
            var result = formatter.Format(dataTable).ToList();
            Assert.AreEqual("Some Value", result.Single());
        }
    }
}
