using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    [TestClass]
    public class ConsoleTableFormatterTests
    {
        [TestMethod]
        public void Format_WhenWindowWidthIsWideEnough()
        {
            var dataTable = new[]
            {
                new Person {Id = 1, FirstName = "John", LastName = "Doe"}
            }.ToDataTable();

            var formatter = new ConsoleTableFormatter(120, " | ");
            var result = string.Join(Environment.NewLine, formatter.Format(dataTable));

            var expected = "Id | FirstName | LastName\r\n" +
                           "---|-----------|---------\r\n" +
                           "1  | John      | Doe     ";

            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        public void Format_WhenWindowWidthIsNotWideEnough()
        {
            var dataTable = new[]
            {
                new Person {Id = 1, FirstName = "John", LastName = "Doe"}
            }.ToDataTable();

            var formatter = new ConsoleTableFormatter(20, " | ");
            var result = string.Join(Environment.NewLine, formatter.Format(dataTable));

            var expected = "Id | FirstNa | Last\r\n" +
                           "---|---------|-----\r\n" +
                           "1  | John    | Doe ";

            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        public void EmptyDataTable_YieldsEmpty()
        {
            var dataTable = new DataTable();

            var formatter = new ConsoleTableFormatter(120, " | ");

            var result = string.Join(Environment.NewLine, formatter.Format(dataTable));

            var expected = "";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SingleResult_YieldsThatResult()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Result", typeof(string));
            dataTable.Rows.Add("Some Value");

            var formatter = new ConsoleTableFormatter(120, " | ");

            var result = formatter.Format(dataTable).ToList();
            Assert.AreEqual("Some Value", result.Single());
        }
    }
}
