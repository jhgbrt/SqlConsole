﻿using System.ComponentModel;

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

        public string GetQuery() => (Query != null && File.Exists(Query) ? File.ReadAllText(Query) : Query) ?? string.Empty;
    }
}
