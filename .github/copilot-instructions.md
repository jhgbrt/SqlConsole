# SqlConsole

SqlConsole is a .NET 8 console application that provides a generic SQL utility tool for running SQL queries, either directly from the command line or in an interactive console. It supports multiple database providers: SQL Server, SQLite, Oracle, DB2, MySQL, and PostgreSQL.

**Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Prerequisites and Setup
- Requires .NET 8.0 SDK or later
- No additional dependencies or SDKs required beyond .NET 8

### Build, Test, and Package Commands
Always run these commands from the repository root directory:

```bash
# Restore NuGet packages - takes ~30 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
dotnet restore

# Build the solution - takes ~20 seconds. NEVER CANCEL. Set timeout to 60+ seconds.
dotnet build --configuration Release --no-restore

# Run all unit tests - takes ~10 seconds. NEVER CANCEL. Set timeout to 60+ seconds.
dotnet test --configuration Release --no-restore --no-build --verbosity normal

# Package as .NET tool - takes ~15 seconds. NEVER CANCEL. Set timeout to 60+ seconds.
dotnet pack SqlConsole.Host/SqlConsole.Host.csproj --configuration Release --no-build --output ./packages
```

**CRITICAL TIMING EXPECTATIONS:**
- **NEVER CANCEL builds or tests** - All operations complete in under 1 minute
- Restore: ~21 seconds (set timeout to 120+ seconds)
- Build: ~13 seconds (set timeout to 60+ seconds) 
- Test: ~4 seconds with 123 tests (set timeout to 60+ seconds)
- Package: ~9 seconds (set timeout to 60+ seconds)

## Running the Application

### Development Mode (from source)
```bash
# Run with help
dotnet run --project SqlConsole.Host --configuration Release --no-build -- --help

# Run SQLite query
dotnet run --project SqlConsole.Host --configuration Release --no-build -- query sqlite --data-source ":memory:" "SELECT 1 as test"

# Run interactive SQLite console
dotnet run --project SqlConsole.Host --configuration Release --no-build -- console sqlite --data-source ":memory:"
```

### As Global Tool
```bash
# Install globally from local package
dotnet tool install --global --add-source ./packages Net.Code.SqlConsole --version 2.0.0-preview.1

# Use the tool
sqlc --help
sqlc query sqlite --data-source ":memory:" "SELECT 'Hello World' as message"
```

## Validation Scenarios

**ALWAYS run these validation scenarios after making changes:**

### 1. Basic Query Functionality Test
```bash
# Test in-memory SQLite query
sqlc query sqlite --data-source ":memory:" "SELECT 1 as id, 'test' as name"
# Expected: Table output showing id=1, name=test
```

### 2. File Input/Output Test
```bash
# Create test query file
echo "CREATE TABLE test (id INTEGER, name TEXT); INSERT INTO test VALUES (1, 'Alice'), (2, 'Bob'); SELECT * FROM test;" > /tmp/test.sql

# Test file input with empty query argument
sqlc query sqlite --data-source ":memory:" --input /tmp/test.sql ""

# Test CSV output
sqlc query sqlite --data-source ":memory:" --output /tmp/result.csv "SELECT 1 as id, 'test' as name"
cat /tmp/result.csv
# Expected: CSV format with headers and data
```

### 3. Help System Test
```bash
# Test main help
sqlc --help

# Test provider-specific help
sqlc query sqlite --help
sqlc console postgres --help
```

### 4. Build and Package Validation
```bash
# Full build pipeline
dotnet restore && dotnet build --configuration Release --no-restore && dotnet test --configuration Release --no-restore --no-build
# Expected: All commands succeed, 123 tests pass

# Package validation
dotnet pack SqlConsole.Host/SqlConsole.Host.csproj --configuration Release --no-build --output /tmp/packages
# Expected: Creates Net.Code.SqlConsole.2.0.0-preview.1.nupkg
```

## Key Projects and Structure

### SqlConsole.Host
- **Purpose**: Main console application project
- **Output**: `sqlc.dll` (packaged as global .NET tool)
- **Key Files**:
  - `Program.cs`: Entry point
  - `CommandFactory.cs`: CLI command setup using System.CommandLine
  - `Configuration/Provider.cs`: Database provider definitions
  - `Infrastructure/`: Core infrastructure (REPL, database connections)
  - `QueryHandler/`: SQL execution logic
  - `Visualizers/`: Output formatting (table, CSV)

### SqlConsole.UnitTests  
- **Purpose**: Unit and integration tests
- **Test Count**: 123 tests
- **Framework**: xUnit with NSubstitute for mocking
- **Key Test Areas**:
  - Infrastructure extensions and utilities
  - Query visualization and formatting
  - REPL functionality with SQLite integration
  - Script splitting and parsing

### Database Providers Supported
The application supports these database providers with full connection string options:
- **sqlserver**: SQL Server with comprehensive options (authentication, encryption, pooling)
- **sqlite**: SQLite with file/memory modes, encryption support
- **postgres**: PostgreSQL 
- **mysql**: MySQL
- **oracle**: Oracle Database
- **db2**: IBM DB2

## Common Issues and Troubleshooting

### Interactive Console Issues
- The interactive console mode uses a custom REPL implementation
- Terminal rendering may not work properly in non-interactive environments
- For testing, use the query mode instead of console mode

### Connection String Validation
- Each provider has specific connection string requirements
- Use provider-specific help commands to see all available options
- SQLite `:memory:` is the most reliable for testing without external dependencies

### Build Environment
- Requires .NET 8.0 SDK 
- All dependencies are managed via NuGet packages
- No native dependencies or external tools required
- Build artifacts go to `bin/` and `obj/` directories (excluded from git)

## CI/CD Pipeline
- **GitHub Actions**: `.github/workflows/build-test.yml` (PR builds) and `buildtestpublish.yml` (manual publish)
- **Build Process**: restore → build → test → (optional) publish to NuGet
- **NuGet Package**: `Net.Code.SqlConsole` with tool command `sqlc`

## Development Guidelines
- Always build in Release configuration for final validation
- Run the full test suite before committing changes  
- Use SQLite `:memory:` database for reliable testing
- Package and test as global tool to validate end-to-end functionality
- The application uses System.CommandLine for CLI parsing and follows .NET tool packaging conventions