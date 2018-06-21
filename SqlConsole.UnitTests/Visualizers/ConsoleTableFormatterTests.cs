using System;
using System.Data;
using System.Linq;
using Xunit;
using Net.Code.ADONet;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Visualizers
{
    public class ConsoleTableFormatterTests
    {
        [Fact]
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

            Assert.Equal(expected, result);
        }
        [Fact]
        public void Format_WhenWindowWidthIsNotWideEnough()
        {
            var dataTable = new[]
            {
                new Person {Id = 1, FirstName = "John", LastName = "Doe"}
            }.ToDataTable();

            var formatter = new ConsoleTableFormatter(20, " | ");
            var result = string.Join(Environment.NewLine, formatter.Format(dataTable));

            var expected = "Id | FirstName | La\r\n" +
                           "---|-----------|---\r\n" +
                           "1  | John      | Do";

            Assert.Equal(expected, result);
        }
        [Fact]
        public void EmptyDataTable_YieldsEmpty()
        {
            var dataTable = new DataTable();

            var formatter = new ConsoleTableFormatter(120, " | ");

            var result = string.Join(Environment.NewLine, formatter.Format(dataTable));

            var expected = "";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void SingleResult_YieldsThatResult()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Result", typeof(string));
            dataTable.Rows.Add("Some Value");

            var formatter = new ConsoleTableFormatter(120, " | ");

            var result = formatter.Format(dataTable).ToList();
            Assert.Equal("Some Value", result.Single());
        }
    }
}