using SqlConsole.Host;
using System.Data;
using Xunit;

namespace SqlConsole.UnitTests.Visualizers;

public class SpectreConsoleTableFormatterTests
{
    [Fact]
    public void Format_EmptyDataTable_ReturnsEmpty()
    {
        // Arrange
        var formatter = new SpectreConsoleTableFormatter();
        var emptyTable = new DataTable();

        // Act
        var result = formatter.Format(emptyTable).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Format_SingleScalarResult_ReturnsValue()
    {
        // Arrange
        var formatter = new SpectreConsoleTableFormatter();
        var table = new DataTable();
        table.Columns.Add("Count");
        table.Rows.Add(42);

        // Act
        var result = formatter.Format(table).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("42", result[0]);
    }

    [Fact]
    public void Format_RegularTable_ReturnsFormattedTable()
    {
        // Arrange
        var formatter = new SpectreConsoleTableFormatter();
        var table = new DataTable();
        table.Columns.Add("ID");
        table.Columns.Add("Name");
        table.Rows.Add(1, "John");
        table.Rows.Add(2, "Jane");

        // Act
        var result = formatter.Format(table).ToList();

        // Assert
        Assert.NotEmpty(result);
        
        // Should start and end with empty lines
        Assert.Equal(string.Empty, result.First());
        Assert.Equal(string.Empty, result.Last());
        
        // Should contain Unicode box drawing characters for beautiful formatting
        Assert.Contains(result, line => line.Contains("╭") || line.Contains("╮"));
        Assert.Contains(result, line => line.Contains("╰") || line.Contains("╯"));
        
        // Should contain the data
        Assert.Contains(result, line => line.Contains("John"));
        Assert.Contains(result, line => line.Contains("Jane"));
    }
}