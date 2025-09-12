using Net.Code.ADONet;
using System.Collections.Concurrent;

namespace SqlConsole.Host.Infrastructure;

/// <summary>
/// Implementation of schema metadata service that loads and caches database schema information
/// </summary>
internal class SchemaMetadataService : ISchemaMetadataService
{
    private readonly ConcurrentDictionary<string, TableInfo> _tables = new();
    private readonly object _loadLock = new();
    private volatile bool _isLoaded = false;
    private readonly Provider _provider;

    public SchemaMetadataService(Provider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public bool IsLoaded => _isLoaded;

    public IReadOnlyList<string> GetTableNames()
    {
        return _tables.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public IReadOnlyList<string> GetColumnNames(string tableName)
    {
        if (_tables.TryGetValue(tableName, out var tableInfo))
        {
            return tableInfo.Columns;
        }
        return Array.Empty<string>();
    }

    public IReadOnlyList<string> GetAllColumnNames()
    {
        return _tables.Values
            .SelectMany(t => t.Columns)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task LoadSchemaAsync(IDb db)
    {
        if (_isLoaded) return;

        lock (_loadLock)
        {
            if (_isLoaded) return;
        }

        try
        {
            await DoLoadSchemaAsync(db);
            _isLoaded = true;
        }
        catch
        {
            // Silently fail - schema loading is optional for completion
            // Users can still use keyword completion
        }
    }

    public async Task RefreshSchemaAsync(IDb db)
    {
        ClearCache();
        await LoadSchemaAsync(db);
    }

    public void ClearCache()
    {
        _tables.Clear();
        _isLoaded = false;
    }

    private async Task DoLoadSchemaAsync(IDb db)
    {
        var tables = await GetTablesAsync(db);
        
        foreach (var tableName in tables)
        {
            try
            {
                var columns = await GetColumnsAsync(db, tableName);
                _tables[tableName] = new TableInfo(tableName, columns.ToList());
            }
            catch
            {
                // Skip individual table if column loading fails
                continue;
            }
        }
    }

    private async Task<IReadOnlyList<string>> GetTablesAsync(IDb db)
    {
        var query = GetTablesQuery();
        if (string.IsNullOrEmpty(query)) return Array.Empty<string>();

        try
        {
            var tables = new List<string>();
            await foreach (var row in db.Sql(query).AsEnumerableAsync())
            {
                // Get first column value
                var rowDict = (IDictionary<string, object?>)row;
                var tableName = rowDict.Values.FirstOrDefault()?.ToString();
                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    tables.Add(tableName);
                }
            }
            return tables;
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private async Task<IReadOnlyList<string>> GetColumnsAsync(IDb db, string tableName)
    {
        var query = GetColumnsQuery(tableName);
        if (string.IsNullOrEmpty(query)) return Array.Empty<string>();

        try
        {
            var columns = new List<string>();
            await foreach (var row in db.Sql(query).AsEnumerableAsync())
            {
                // For SQLite PRAGMA table_info, column name is in the 'name' field (index 1)
                // For INFORMATION_SCHEMA queries, it's the first/only column
                var rowDict = (IDictionary<string, object?>)row;
                string? columnName = null;
                
                if (_provider.Name.ToLowerInvariant() == "sqlite")
                {
                    // PRAGMA table_info returns: cid, name, type, notnull, dflt_value, pk
                    columnName = rowDict.Values.Skip(1).FirstOrDefault()?.ToString();
                }
                else
                {
                    // INFORMATION_SCHEMA queries return column name as first column
                    columnName = rowDict.Values.FirstOrDefault()?.ToString();
                }
                
                if (!string.IsNullOrWhiteSpace(columnName))
                {
                    columns.Add(columnName);
                }
            }
            return columns;
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private string GetTablesQuery()
    {
        return _provider.Name.ToLowerInvariant() switch
        {
            "sqlite" => "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name",
            "sqlserver" => @"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
                           WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = 'dbo' 
                           ORDER BY TABLE_NAME",
            "postgres" => @"SELECT tablename FROM pg_tables 
                          WHERE schemaname = 'public' 
                          ORDER BY tablename",
            "mysql" => @"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
                       WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = DATABASE() 
                       ORDER BY TABLE_NAME",
            "oracle" => @"SELECT TABLE_NAME FROM USER_TABLES ORDER BY TABLE_NAME",
            "db2" => @"SELECT TABNAME FROM SYSCAT.TABLES 
                     WHERE TABSCHEMA = USER AND TYPE = 'T' 
                     ORDER BY TABNAME",
            _ => string.Empty
        };
    }

    private string GetColumnsQuery(string tableName)
    {
        return _provider.Name.ToLowerInvariant() switch
        {
            "sqlite" => $"PRAGMA table_info({EscapeIdentifier(tableName)})",
            "sqlserver" => $@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = '{EscapeLiteral(tableName)}' AND TABLE_SCHEMA = 'dbo' 
                            ORDER BY ORDINAL_POSITION",
            "postgres" => $@"SELECT column_name FROM information_schema.columns 
                           WHERE table_name = '{EscapeLiteral(tableName.ToLowerInvariant())}' AND table_schema = 'public' 
                           ORDER BY ordinal_position",
            "mysql" => $@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = '{EscapeLiteral(tableName)}' AND TABLE_SCHEMA = DATABASE() 
                        ORDER BY ORDINAL_POSITION",
            "oracle" => $@"SELECT COLUMN_NAME FROM USER_TAB_COLUMNS 
                         WHERE TABLE_NAME = '{EscapeLiteral(tableName.ToUpperInvariant())}' 
                         ORDER BY COLUMN_ID",
            "db2" => $@"SELECT COLNAME FROM SYSCAT.COLUMNS 
                      WHERE TABNAME = '{EscapeLiteral(tableName.ToUpperInvariant())}' AND TABSCHEMA = USER 
                      ORDER BY COLNO",
            _ => string.Empty
        };
    }

    private string EscapeIdentifier(string identifier)
    {
        // Basic identifier escaping - could be enhanced per provider
        return identifier.Replace("'", "''");
    }

    private string EscapeLiteral(string literal)
    {
        // Basic SQL string literal escaping
        return literal.Replace("'", "''");
    }
}