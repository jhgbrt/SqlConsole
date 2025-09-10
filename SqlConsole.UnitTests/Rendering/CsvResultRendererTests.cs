using System.CommandLine.IO;
using System.Text;
using SqlConsole.Host.Rendering;
using SqlConsole.Host.ResultModel;
using Xunit;
using Xunit.Abstractions;

namespace SqlConsole.UnitTests.Rendering;

public class CsvResultRendererTests
{
    private readonly ITestOutputHelper _output;

    public CsvResultRendererTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void RenderTable_ProducesCorrectCsv()
    {
        // Arrange
        var writer = new TestWriter();
        var renderer = new CsvResultRenderer(writer);
        var tableView = new TableView(
            new[] { "Id", "Name" },
            new[] 
            {
                new[] { "1", "Test" },
                new[] { "2", "With \"quotes\"" }
            });

        // Act
        _output.WriteLine($"Headers count: {tableView.Headers.Length}");
        _output.WriteLine($"Rows count: {tableView.Rows.Count()}");
        renderer.Render(tableView);

        // Assert
        var lines = writer.GetLines();
        _output.WriteLine($"Lines captured: {lines.Length}");
        foreach (var line in lines)
        {
            _output.WriteLine($"Line: '{line}'");
        }
        
        Assert.Equal(3, lines.Length);
        Assert.Equal("\"Id\";\"Name\"", lines[0]);
        Assert.Equal("\"1\";\"Test\"", lines[1]);
        Assert.Equal("\"2\";\"With \"\"quotes\"\"\"", lines[2]);
    }

    [Fact]
    public void RenderScalar_ProducesValue()
    {
        // Arrange
        var writer = new TestWriter();
        var renderer = new CsvResultRenderer(writer);
        var scalarView = new ScalarView("Hello World");

        // Act
        renderer.Render(scalarView);

        // Assert
        var lines = writer.GetLines();
        Assert.Single(lines);
        Assert.Equal("Hello World", lines[0]);
    }

    [Fact]
    public void RenderNonQuery_ProducesMessage()
    {
        // Arrange
        var writer = new TestWriter();
        var renderer = new CsvResultRenderer(writer);
        var nonQueryView = new NonQueryView(5);

        // Act
        renderer.Render(nonQueryView);

        // Assert
        var lines = writer.GetLines();
        Assert.Single(lines);
        Assert.Equal("5 rows affected", lines[0]);
    }

    [Fact]
    public void RenderNonQuery_NegativeRows_ProducesNothing()
    {
        // Arrange
        var writer = new TestWriter();
        var renderer = new CsvResultRenderer(writer);
        var nonQueryView = new NonQueryView(-1);

        // Act
        renderer.Render(nonQueryView);

        // Assert
        var lines = writer.GetLines();
        Assert.Empty(lines);
    }

    private class TestWriter : IStandardStreamWriter
    {
        private readonly List<string> _lines = new();
        private readonly StringBuilder _currentLine = new();

        public void Write(string value)
        {
            _currentLine.Append(value);
            
            // If the value ends with a newline, treat it as a complete line
            if (value.EndsWith('\n'))
            {
                var line = _currentLine.ToString().TrimEnd('\r', '\n');
                _lines.Add(line);
                _currentLine.Clear();
            }
        }

        public void WriteLine(string value)
        {
            _currentLine.Append(value);
            var line = _currentLine.ToString();
            _lines.Add(line);
            _currentLine.Clear();
        }

        public string[] GetLines() 
        {
            // If there's remaining content, add it as a line
            if (_currentLine.Length > 0)
            {
                _lines.Add(_currentLine.ToString());
                _currentLine.Clear();
            }
            return _lines.ToArray();
        }
    }
}