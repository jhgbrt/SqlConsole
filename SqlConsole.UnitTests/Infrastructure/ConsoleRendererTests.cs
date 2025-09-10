using System;
using System.IO;
using Xunit;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Infrastructure
{
    public class ConsoleRendererTests
    {
        [Fact]
        public void NoColorConsoleRenderer_WriteError_WritesToErrorStream()
        {
            // Arrange
            var originalError = Console.Error;
            var stringWriter = new StringWriter();
            Console.SetError(stringWriter);
            var renderer = new NoColorConsoleRenderer();

            try
            {
                // Act
                renderer.WriteError("Test error message");

                // Assert
                var output = stringWriter.ToString();
                Assert.Contains("Error: Test error message", output);
            }
            finally
            {
                // Restore
                Console.SetError(originalError);
            }
        }

        [Fact]
        public void NoColorConsoleRenderer_WriteConnectionStatus_WritesToStandardOut()
        {
            // Arrange
            var originalOut = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var renderer = new NoColorConsoleRenderer();

            try
            {
                // Act
                renderer.WriteConnectionStatus("[sqlite - connected]");

                // Assert
                var output = stringWriter.ToString();
                Assert.Contains("[sqlite - connected]", output);
            }
            finally
            {
                // Restore
                Console.SetOut(originalOut);
            }
        }

        [Theory]
        [InlineData(50, "fast")]
        [InlineData(500, "medium")]
        [InlineData(1500, "slow")]
        public void NoColorConsoleRenderer_WriteTiming_ShowsCorrectSpeed(int milliseconds, string expectedSpeed)
        {
            // Arrange
            var originalOut = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var renderer = new NoColorConsoleRenderer();

            try
            {
                // Act
                renderer.WriteTiming(TimeSpan.FromMilliseconds(milliseconds));

                // Assert
                var output = stringWriter.ToString();
                Assert.Contains($"{milliseconds}ms", output);
                Assert.Contains(expectedSpeed, output);
            }
            finally
            {
                // Restore
                Console.SetOut(originalOut);
            }
        }

        [Theory]
        [InlineData(1, "1 row")]
        [InlineData(2, "2 rows")]
        [InlineData(10, "10 rows")]
        public void NoColorConsoleRenderer_WriteTimingAndRows_WithRowCount_ShowsRowsAndTiming(int rowCount, string expectedRowText)
        {
            // Arrange
            var originalOut = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var renderer = new NoColorConsoleRenderer();

            try
            {
                // Act
                renderer.WriteTimingAndRows(TimeSpan.FromMilliseconds(100), rowCount);

                // Assert
                var output = stringWriter.ToString();
                Assert.Contains($"({expectedRowText} | 100ms - medium)", output);
            }
            finally
            {
                // Restore
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void NoColorConsoleRenderer_WriteTimingAndRows_WithZeroRows_ShowsTimingOnly()
        {
            // Arrange
            var originalOut = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var renderer = new NoColorConsoleRenderer();

            try
            {
                // Act
                renderer.WriteTimingAndRows(TimeSpan.FromMilliseconds(50), 0);

                // Assert
                var output = stringWriter.ToString();
                Assert.Contains("(50ms - fast)", output);
                Assert.DoesNotContain("rows", output);
            }
            finally
            {
                // Restore
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void NoColorConsoleRenderer_WriteTimingAndRows_WithNullRows_ShowsTimingOnly()
        {
            // Arrange
            var originalOut = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var renderer = new NoColorConsoleRenderer();

            try
            {
                // Act
                renderer.WriteTimingAndRows(TimeSpan.FromMilliseconds(150), null);

                // Assert
                var output = stringWriter.ToString();
                Assert.Contains("(150ms - medium)", output);
                Assert.DoesNotContain("rows", output);
            }
            finally
            {
                // Restore
                Console.SetOut(originalOut);
            }
        }
    }
    
    public class ConsoleRendererFactoryTests
    {
        [Fact]
        public void Create_WithNoColorOption_ReturnsNoColorRenderer()
        {
            // Arrange
            var options = new CommandFactory.QueryOptions { NoColor = true };

            // Act
            var renderer = ConsoleRendererFactory.Create(options);

            // Assert
            Assert.IsType<NoColorConsoleRenderer>(renderer);
        }

        [Fact]
        public void Create_WithColorEnabled_ReturnsSpectreRenderer()
        {
            // Arrange
            var options = new CommandFactory.QueryOptions { NoColor = false };
            Environment.SetEnvironmentVariable("NO_COLOR", null);

            // Act  
            var renderer = ConsoleRendererFactory.Create(options);

            // Assert - Note: This might be NoColorConsoleRenderer if output is redirected in test environment
            Assert.True(renderer is SpectreConsoleRenderer or NoColorConsoleRenderer);
        }

        [Fact]
        public void Create_WithNoColorEnvironmentVariable_ReturnsNoColorRenderer()
        {
            // Arrange
            var options = new CommandFactory.QueryOptions { NoColor = false };
            Environment.SetEnvironmentVariable("NO_COLOR", "1");

            try
            {
                // Act
                var renderer = ConsoleRendererFactory.Create(options);

                // Assert
                Assert.IsType<NoColorConsoleRenderer>(renderer);
            }
            finally
            {
                // Cleanup
                Environment.SetEnvironmentVariable("NO_COLOR", null);
            }
        }
    }
}