namespace SqlConsole.Host.Infrastructure;

/// <summary>
/// Types of SQL contexts for completion
/// </summary>
public enum SqlCompletionContext
{
    /// <summary>
    /// General context - suggest keywords
    /// </summary>
    General,
    
    /// <summary>
    /// After FROM keyword - suggest table names
    /// </summary>
    Tables,
    
    /// <summary>
    /// After SELECT keyword or in column list - suggest column names
    /// </summary>
    Columns
}

/// <summary>
/// Parser to determine SQL context for appropriate completion suggestions
/// </summary>
public static class SqlContextParser
{
    private static readonly string[] TableContextKeywords = { "FROM", "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "CROSS" };
    private static readonly string[] ColumnContextKeywords = { "SELECT", "WHERE", "GROUP", "ORDER", "HAVING" };

    /// <summary>
    /// Determines the completion context based on the current SQL text and cursor position
    /// </summary>
    /// <param name="sqlText">The current SQL text</param>
    /// <param name="cursorPosition">Current cursor position</param>
    /// <returns>The appropriate completion context</returns>
    public static SqlCompletionContext DetermineContext(string sqlText, int cursorPosition)
    {
        if (string.IsNullOrWhiteSpace(sqlText))
            return SqlCompletionContext.General;

        // Get text up to cursor position
        var textUpToCursor = cursorPosition >= sqlText.Length 
            ? sqlText 
            : sqlText.Substring(0, cursorPosition);

        // Find the last significant keyword before cursor
        var lastKeyword = FindLastKeyword(textUpToCursor);
        
        return DetermineContextFromKeyword(lastKeyword, textUpToCursor);
    }

    private static string? FindLastKeyword(string text)
    {
        // Normalize whitespace and split into tokens
        var normalizedText = text.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
        var words = normalizedText.Split(new[] { ' ', '(', ')', ',', ';' }, 
            StringSplitOptions.RemoveEmptyEntries);
        
        // Look for compound keywords first (ORDER BY, GROUP BY)
        for (int i = words.Length - 2; i >= 0; i--)
        {
            var compound = $"{words[i]} {words[i + 1]}".ToUpperInvariant();
            if (compound == "ORDER BY" || compound == "GROUP BY")
            {
                return compound;
            }
        }
        
        // Look backwards for single SQL keywords
        for (int i = words.Length - 1; i >= 0; i--)
        {
            var word = words[i].ToUpperInvariant();
            if (IsKeyword(word))
            {
                return word;
            }
        }
        
        return null;
    }

    private static SqlCompletionContext DetermineContextFromKeyword(string? lastKeyword, string textUpToCursor)
    {
        if (lastKeyword == null)
            return SqlCompletionContext.General;

        // Check for table context keywords
        if (TableContextKeywords.Contains(lastKeyword, StringComparer.OrdinalIgnoreCase))
        {
            // Special case: "JOIN tablename ON" should suggest columns
            if (lastKeyword.Equals("JOIN", StringComparison.OrdinalIgnoreCase) && 
                textUpToCursor.Contains(" ON ", StringComparison.OrdinalIgnoreCase))
            {
                return SqlCompletionContext.Columns;
            }
            return SqlCompletionContext.Tables;
        }

        // Check for column context keywords (including compound keywords)
        if (ColumnContextKeywords.Contains(lastKeyword, StringComparer.OrdinalIgnoreCase) ||
            lastKeyword.Equals("ORDER BY", StringComparison.OrdinalIgnoreCase) ||
            lastKeyword.Equals("GROUP BY", StringComparison.OrdinalIgnoreCase))
        {
            return SqlCompletionContext.Columns;
        }

        // Special cases
        if (lastKeyword.Equals("ON", StringComparison.OrdinalIgnoreCase) ||
            lastKeyword.Equals("AND", StringComparison.OrdinalIgnoreCase) ||
            lastKeyword.Equals("OR", StringComparison.OrdinalIgnoreCase))
        {
            return SqlCompletionContext.Columns;
        }

        return SqlCompletionContext.General;
    }

    private static bool IsKeyword(string word)
    {
        // Check against common SQL keywords
        var commonKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT", "FROM", "WHERE", "INSERT", "UPDATE", "DELETE", "CREATE", "ALTER", "DROP",
            "TABLE", "INDEX", "VIEW", "DATABASE", "SCHEMA", "PROCEDURE", "FUNCTION", "TRIGGER",
            "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "OUTER", "CROSS", "UNION", "ALL", "DISTINCT",
            "GROUP", "BY", "ORDER", "HAVING", "LIMIT", "OFFSET", "TOP", "AS", "AND", "OR", "NOT",
            "ON", "IN", "LIKE", "BETWEEN", "EXISTS", "CASE", "WHEN", "THEN", "ELSE", "END"
        };
        
        return commonKeywords.Contains(word);
    }
}