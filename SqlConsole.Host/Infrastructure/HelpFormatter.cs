using System.CommandLine;

namespace SqlConsole.Host.Infrastructure;

/// <summary>
/// Provides enhanced help formatting with TTY detection and provider examples.
/// </summary>
public static class HelpFormatter
{
    /// <summary>
    /// Checks if the current console supports ANSI color codes.
    /// </summary>
    public static bool SupportsAnsiColors
    {
        get
        {
            try
            {
                // Check if we're running in a TTY
                return !Console.IsOutputRedirected && 
                       !Console.IsErrorRedirected && 
                       Environment.GetEnvironmentVariable("NO_COLOR") == null &&
                       Environment.GetEnvironmentVariable("TERM") != "dumb";
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Formats text with ANSI color codes if supported.
    /// </summary>
    public static string Colorize(string text, ConsoleColor color)
    {
        if (!SupportsAnsiColors)
            return text;

        var ansiCode = color switch
        {
            ConsoleColor.Green => "\x1b[32m",
            ConsoleColor.Yellow => "\x1b[33m",
            ConsoleColor.Blue => "\x1b[34m",
            ConsoleColor.Magenta => "\x1b[35m",
            ConsoleColor.Cyan => "\x1b[36m",
            ConsoleColor.White => "\x1b[37m",
            ConsoleColor.Gray => "\x1b[90m",
            _ => ""
        };

        return $"{ansiCode}{text}\x1b[0m";
    }

    /// <summary>
    /// Formats the provider list for the root command description.
    /// </summary>
    public static string FormatProviderList()
    {
        var providers = Provider.All.Select(p => Colorize(p.Name, ConsoleColor.Cyan));
        return $"Supported providers: {string.Join(", ", providers)}";
    }

    /// <summary>
    /// Gets provider-specific connection examples.
    /// </summary>
    public static string GetProviderExamples(string providerName)
    {
        var examples = providerName.ToLowerInvariant() switch
        {
            "sqlserver" => new[]
            {
                "# Connect to local SQL Server with Windows Authentication:",
                "sqlc query sqlserver --server localhost --database MyDB \"SELECT 1\"",
                "",
                "# Connect with SQL Server Authentication:",
                "sqlc query sqlserver --server myserver.com --database MyDB --user myuser --password mypass \"SELECT COUNT(*) FROM Users\""
            },
            "sqlite" => new[]
            {
                "# Query in-memory SQLite database:",
                "sqlc query sqlite --data-source \":memory:\" \"SELECT 'Hello World' AS message\"",
                "",
                "# Query SQLite database file:",
                "sqlc query sqlite --data-source ./mydata.db \"SELECT * FROM customers LIMIT 10\""
            },
            "postgres" => new[]
            {
                "# Connect to PostgreSQL with basic authentication:",
                "sqlc query postgres --host localhost --database mydb --username myuser --password mypass \"SELECT version()\"",
                "",
                "# Connect to PostgreSQL with SSL:",
                "sqlc query postgres --host myserver.com --port 5432 --database mydb --username myuser --ssl-mode Require \"SELECT current_database()\""
            },
            "mysql" => new[]
            {
                "# Connect to MySQL server:",
                "sqlc query mysql --server localhost --database mydb --uid myuser --password mypass \"SELECT VERSION()\"",
                "",
                "# Connect to MySQL with specific port:",
                "sqlc query mysql --server myserver.com --port 3306 --database mydb --uid myuser \"SHOW TABLES\""
            },
            "oracle" => new[]
            {
                "# Connect to Oracle database:",
                "sqlc query oracle --data-source \"myserver:1521/XE\" --user-id myuser --password mypass \"SELECT SYSDATE FROM DUAL\"",
                "",
                "# Connect with TNS name:",
                "sqlc query oracle --data-source \"MyTNSName\" --user-id myuser --password mypass \"SELECT USER FROM DUAL\""
            },
            "db2" => new[]
            {
                "# Connect to DB2 database:",
                "sqlc query db2 --database mydb --uid myuser --pwd mypass \"SELECT CURRENT TIMESTAMP FROM SYSIBM.SYSDUMMY1\"",
                "",
                "# Connect to remote DB2 server:",
                "sqlc query db2 --server myserver --database mydb --uid myuser --pwd mypass \"SELECT * FROM SYSCAT.TABLES FETCH FIRST 5 ROWS ONLY\""
            },
            _ => new[] { "# No specific examples available for this provider." }
        };

        var formattedExamples = examples.Select(line => 
        {
            if (line.StartsWith("#"))
                return Colorize(line, ConsoleColor.Green);
            else if (line.StartsWith("sqlc"))
                return line.Replace("sqlc", Colorize("sqlc", ConsoleColor.Yellow));
            else
                return line;
        });

        return string.Join(Environment.NewLine, formattedExamples);
    }

    /// <summary>
    /// Creates an enhanced description for provider commands.
    /// </summary>
    public static string CreateProviderDescription(string providerName, string baseDescription)
    {
        var examples = GetProviderExamples(providerName);
        
        return $@"{baseDescription}

{Colorize("Examples:", ConsoleColor.Magenta)}
{examples}

{Colorize("Installation of shell completion:", ConsoleColor.Magenta)}
# Bash
sqlc completion --shell bash >> ~/.bashrc

# Zsh
sqlc completion --shell zsh >> ~/.zshrc

# PowerShell
sqlc completion --shell powershell >> $PROFILE

# Fish
sqlc completion --shell fish > ~/.config/fish/completions/sqlc.fish";
    }
}