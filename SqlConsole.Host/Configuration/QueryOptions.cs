using System.ComponentModel;

namespace SqlConsole.Host;

static partial class CommandFactory
{
    public class QueryOptions
    {
        [Description("Interpret the query result as a scalar value")]
        public bool AsScalar { get; set; }
        [Description("The query is not to be considered as an actual query (typically, an INSERT, UPDATE or DELETE statement). Only the number of affecterd records will be reported.")]
        public bool AsNonquery { get; set; }
        [Description("Inline SQL query to be executed")]
        public string? Query { get; set; }
        [Description("A file that contains the SQL query to be executed.")]
        public FileInfo? Input { get; set; }
        [Description("A file to which the results should be written. Standard query results are written in CSV format.")]
        public FileInfo? Output { get; set; }
        [Description("Disable colored output")]
        public bool NoColor { get; set; }
        [Description("Output results in CSV format")]
        public bool Csv { get; set; }

        public string GetQuery() => (Query != null && File.Exists(Query) ? File.ReadAllText(Query) : Query) ?? string.Empty;
        
        /// <summary>
        /// Determine the output mode based on the options
        /// </summary>
        public Rendering.OutputMode GetOutputMode()
        {
            // CSV mode if explicitly requested or output file is specified
            if (Csv || Output != null)
            {
                return Rendering.OutputMode.Csv;
            }
            
            return Rendering.OutputMode.Interactive;
        }
    }
}
