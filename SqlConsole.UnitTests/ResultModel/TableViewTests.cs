using System.Data;
using SqlConsole.Host.ResultModel;
using Xunit;

namespace SqlConsole.UnitTests.ResultModel;

public class TableViewTests
{
    [Fact]
    public void FromDataTable_CreatesCorrectTableView()
    {
        // Arrange
        var dataTable = new DataTable();
        dataTable.Columns.Add("Id", typeof(int));
        dataTable.Columns.Add("Name", typeof(string));
        dataTable.Rows.Add(1, "Test");
        dataTable.Rows.Add(2, "Another");

        // Act
        var tableView = TableView.FromDataTable(dataTable);

        // Assert
        Assert.Equal(ResultViewType.Table, tableView.Type);
        Assert.Equal(new[] { "Id", "Name" }, tableView.Headers);
        
        var rows = tableView.Rows.ToArray();
        Assert.Equal(2, rows.Length);
        Assert.Equal(new[] { "1", "Test" }, rows[0]);
        Assert.Equal(new[] { "2", "Another" }, rows[1]);
    }

    [Fact]
    public void FromDataTable_EmptyTable_CreatesEmptyTableView()
    {
        // Arrange
        var dataTable = new DataTable();
        dataTable.Columns.Add("Id", typeof(int));

        // Act
        var tableView = TableView.FromDataTable(dataTable);

        // Assert
        Assert.Equal(ResultViewType.Table, tableView.Type);
        Assert.Equal(new[] { "Id" }, tableView.Headers);
        Assert.Empty(tableView.Rows);
    }

    [Fact]
    public void FromDataTable_NullValues_HandledCorrectly()
    {
        // Arrange
        var dataTable = new DataTable();
        dataTable.Columns.Add("Value", typeof(string));
        dataTable.Rows.Add(DBNull.Value);
        dataTable.Rows.Add((object?)null);

        // Act
        var tableView = TableView.FromDataTable(dataTable);

        // Assert
        var rows = tableView.Rows.ToArray();
        Assert.Equal("", rows[0][0]);
        Assert.Equal("", rows[1][0]);
    }
}