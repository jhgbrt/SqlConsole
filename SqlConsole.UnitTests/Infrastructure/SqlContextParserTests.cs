using SqlConsole.Host.Infrastructure;
using Xunit;

namespace SqlConsole.UnitTests.Infrastructure;

public class SqlContextParserTests
{
    [Theory]
    [InlineData("", 0, SqlCompletionContext.General)]
    [InlineData("SELECT ", 7, SqlCompletionContext.Columns)]
    [InlineData("FROM ", 5, SqlCompletionContext.Tables)]
    [InlineData("SELECT col1, col2 FROM ", 23, SqlCompletionContext.Tables)]
    [InlineData("SELECT * FROM table WHERE ", 26, SqlCompletionContext.Columns)]
    [InlineData("JOIN ", 5, SqlCompletionContext.Tables)]
    [InlineData("INNER JOIN ", 11, SqlCompletionContext.Tables)]
    [InlineData("LEFT JOIN table ON ", 19, SqlCompletionContext.Columns)]
    [InlineData("GROUP BY ", 9, SqlCompletionContext.Columns)]
    [InlineData("ORDER BY ", 9, SqlCompletionContext.Columns)]
    [InlineData("HAVING ", 7, SqlCompletionContext.Columns)]
    [InlineData("SELECT col FROM table WHERE col = 1 AND ", 41, SqlCompletionContext.Columns)]
    public void DetermineContext_ReturnsExpectedContext(string sqlText, int cursorPosition, SqlCompletionContext expected)
    {
        // Act
        var result = SqlContextParser.DetermineContext(sqlText, cursorPosition);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("SELECT col", 6, SqlCompletionContext.Columns)] // cursor after SELECT
    [InlineData("FROM tab", 5, SqlCompletionContext.Tables)] // cursor after FROM
    [InlineData("WHERE co", 6, SqlCompletionContext.Columns)] // cursor after WHERE
    public void DetermineContext_WithPartialTokens_ReturnsCorrectContext(string sqlText, int cursorPosition, SqlCompletionContext expected)
    {
        // Act
        var result = SqlContextParser.DetermineContext(sqlText, cursorPosition);
        
        // Assert
        Assert.Equal(expected, result);
    }
}