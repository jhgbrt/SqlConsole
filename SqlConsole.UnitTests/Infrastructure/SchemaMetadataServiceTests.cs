using SqlConsole.Host;
using SqlConsole.Host.Infrastructure;
using Net.Code.ADONet;
using NSubstitute;
using Xunit;

namespace SqlConsole.UnitTests.Infrastructure;

public class SchemaMetadataServiceTests
{
    [Theory]
    [InlineData("sqlite")]
    [InlineData("sqlserver")]
    [InlineData("postgres")]
    [InlineData("mysql")]
    [InlineData("oracle")]
    [InlineData("db2")]
    public void Constructor_WithValidProvider_CreatesService(string providerName)
    {
        // Arrange
        var provider = Provider.All.First(p => p.Name == providerName);
        
        // Act
        var service = new SchemaMetadataService(provider);
        
        // Assert
        Assert.NotNull(service);
        Assert.False(service.IsLoaded);
        Assert.Empty(service.GetTableNames());
        Assert.Empty(service.GetAllColumnNames());
    }

    [Fact]
    public void GetColumnNames_WithUnknownTable_ReturnsEmpty()
    {
        // Arrange
        var provider = Provider.All.First(p => p.Name == "sqlite");
        var service = new SchemaMetadataService(provider);
        
        // Act
        var columns = service.GetColumnNames("NonExistentTable");
        
        // Assert
        Assert.Empty(columns);
    }

    [Fact]
    public void ClearCache_ResetsLoadedState()
    {
        // Arrange
        var provider = Provider.All.First(p => p.Name == "sqlite");
        var service = new SchemaMetadataService(provider);
        
        // Act
        service.ClearCache();
        
        // Assert
        Assert.False(service.IsLoaded);
        Assert.Empty(service.GetTableNames());
    }
}