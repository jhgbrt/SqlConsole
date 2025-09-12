using Net.Code.ADONet;

namespace SqlConsole.Host.Infrastructure;

/// <summary>
/// Schema information for a database table
/// </summary>
public record TableInfo(string Name, IReadOnlyList<string> Columns);

/// <summary>
/// Service for loading and caching database schema metadata
/// </summary>
public interface ISchemaMetadataService
{
    /// <summary>
    /// Gets all table names. May return empty list if not yet loaded.
    /// </summary>
    IReadOnlyList<string> GetTableNames();
    
    /// <summary>
    /// Gets column names for a specific table. May return empty list if not yet loaded.
    /// </summary>
    IReadOnlyList<string> GetColumnNames(string tableName);
    
    /// <summary>
    /// Gets all column names from all tables. May return empty list if not yet loaded.
    /// </summary>
    IReadOnlyList<string> GetAllColumnNames();
    
    /// <summary>
    /// Returns true if schema metadata has been loaded
    /// </summary>
    bool IsLoaded { get; }
    
    /// <summary>
    /// Starts asynchronous loading of schema metadata
    /// </summary>
    Task LoadSchemaAsync(IDb db);
    
    /// <summary>
    /// Refreshes the schema metadata
    /// </summary>
    Task RefreshSchemaAsync(IDb db);
    
    /// <summary>
    /// Clears cached schema metadata
    /// </summary>
    void ClearCache();
}