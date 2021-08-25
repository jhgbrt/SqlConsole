using Microsoft.CodeAnalysis.CSharp;

using SqlConsole.Host.Infrastructure;

using System.Collections.Concurrent;
using System.CommandLine;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Xml.Linq;

namespace SqlConsole.Host;

using static SqlConsole.Host.Extension.State;
public static class Extension
{
    public static Action<string> Log = s => { };

    // argument list is used for type inference
    // ReSharper disable once UnusedParameter.Local
#pragma warning disable RCS1175 // Unused this parameter.
    static LambdaComparer<T> CompareBy<T>(this IEnumerable<T> list, Func<T?, int> f)
        => new LambdaComparer<T>(f);
#pragma warning restore RCS1175 // Unused this parameter.

    public static Dictionary<DataColumn, int> ColumnLengths(this DataTable dt, int totalWidth, int separatorSize)
    {
        var maxLengths = (
            from column in dt.Columns.OfType<DataColumn>()
            let rows = dt.Rows.OfType<DataRow>().Select(row => row[column].ToString()).ToList()
            let maxLength = new[] { column.ColumnName }.Concat(rows).Select(s => s.Length).DefaultIfEmpty().Max()
            select (column, maxLength)
            ).ToList();

        var comparer = maxLengths.CompareBy(x => -x.maxLength);

        maxLengths.Sort(comparer);

        while (maxLengths[0].maxLength >= 10 && maxLengths.Sum(x => x.maxLength) + (dt.Columns.Count - 1) * separatorSize > totalWidth - 1)
        {
            var (column, maxLength) = maxLengths[0];
            maxLengths[0] = (column, maxLength - 1);
            maxLengths.Sort(comparer);
        }

        return maxLengths.ToDictionary(x => x.column, x => x.maxLength);
    }

    public static string OfLength(this string s, int l)
    {
        s ??= string.Empty;
        var result = s.PadRight(l).Substring(0, l);
        if (result.Length < s.Length && result.Length > 3)
        {
            result = result[0..^3] + "...";
        }
        return result;
    }

    public static string SafeSubstring(this string s, int startIndex, int length)
    {
        startIndex = Math.Min(startIndex, s.Length);
        length = Math.Min(length, s.Length - startIndex);
        return s.Substring(startIndex, length);
    }

    public static IEnumerable<string> SplitOnGo(this string s)
        => new ScriptSplitter(s).Split();

    public static string ToCSharpLiteral<T>(this T s) => SymbolDisplay.FormatLiteral(s?.ToString() ?? string.Empty, false);
    public static string ToCSharpLiteral(this string s) => SymbolDisplay.FormatLiteral(s, false);
    public static string ToCSharpLiteral(this char c) => SymbolDisplay.FormatLiteral(c, false);

    internal enum State
    {
        UpperCase,
        LowerCase,
        Digit,
        WhiteSpace
    }

    struct Char
    {
        public Char(char c) { Value = c; }
        public char Value { get; init; }
        public bool IsLower => char.IsLower(Value);
        public bool IsUpper => char.IsUpper(Value);
        public bool IsDigit => char.IsDigit(Value);
        public bool IsLetter => char.IsLetter(Value);
        public bool IsLetterOrDigit => char.IsLetterOrDigit(Value);
        public bool IsWhiteSpace => char.IsWhiteSpace(Value);
        public bool IsUnderscore => Value == '_';
    }

    public static string ToHyphenedString(this string input)
    {

        var sb = new StringBuilder();

        var state = UpperCase;

        foreach (var c in input)
        {
            (state, sb) = (state, new Char(c)) switch
            {
                (UpperCase, { IsLower: true }) => (LowerCase, sb),
                (UpperCase, { IsDigit: true }) => (Digit, sb),
                (LowerCase, { IsUpper: true }) => (UpperCase, sb.Append('-')),
                (LowerCase, { IsDigit: true }) => (Digit, sb),
                (Digit, { IsLower: true }) => (LowerCase, sb),
                (Digit, { IsUpper: true }) => (UpperCase, sb.Append('-')),
                _ => (state, sb)
            };
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }

    private static StringBuilder ToLower(this StringBuilder sb)
    {
        for (int i = 0; i < sb.Length; i++)
        {
            if (char.IsUpper(sb[i])) sb[i] = char.ToLower(sb[i]);
        }
        return sb;
    }

    public static string ToSentence(this string input)
    {
        var sb = new StringBuilder();
        var buffer = new StringBuilder();
        var state = UpperCase;

        foreach (var c in input)
        {
            var inputstate = state;
            (state, sb, buffer) = (state, new Char(c)) switch
            {
                // although State is always UpperCase, the first char could be lowercase
                (UpperCase, _) when sb.Length == 0
                    => (state, sb.Append(char.ToUpper(c)), buffer),
                // when one character buffered and next char is lowercase, make the buffer lower case and transition to lowercase
                (UpperCase or WhiteSpace, { IsLower: true }) when buffer.Length == 1
                    => (LowerCase, sb.Append(buffer.ToLower()).Append(c), buffer.Clear()),
                // multiple uppercase characters should be preserved
                (UpperCase, { IsLower: true }) when buffer.Length > 1
                    => (LowerCase, sb.Append(buffer).Append(c), buffer.Clear()),
                // while buffering digits, when next char is uppercase: consider this next word 
                (Digit, { IsUpper: true })
                    => (UpperCase, sb.Append(buffer), buffer.Clear().Append(' ').Append(char.ToLower(c))),
                // while buffering uppercase, next char is digit: keep buffering
                (UpperCase, { IsDigit: true })
                    => (Digit, sb, buffer.Append(c)),
                // while in lowercase, if next char is underscore or whitespace: append a space and transition to whitespace
                (LowerCase, { IsUnderscore: true } or { IsWhiteSpace: true })
                    => (WhiteSpace, sb.Append(' '), buffer),
                // while in lowercase, if next char is a digit or uppercase: start a new word
                (LowerCase, { IsDigit: true } or { IsUpper: true })
                    => (WhiteSpace, sb.Append(' '), buffer.Append(c)),
                // while in whitespace, if next char is uppercase: start buffering
                (WhiteSpace, { IsUpper: true })
                    => (UpperCase, sb, buffer.Append(c)),
                // while in whitespace, if next char is digit: start buffering
                (WhiteSpace, { IsDigit: true })
                    => (Digit, sb, buffer.Append(c)),
                // buffering (uppercase or digit)
                (UpperCase or Digit, _)
                    => (state, sb, buffer.Append(c)),
                // adding characters in lowercase
                (LowerCase, _)
                    => (state, sb.Append(c), buffer),
                _ => (state, sb, buffer)
            };

            Log($"{inputstate} -> {c} -> {state}");
        }

        if (buffer.Length > 0)
            sb.Append(buffer);

        return sb.ToString();
    }

    public static T? GetAttribute<T>(this ICustomAttributeProvider prop) => prop.GetCustomAttributes(false).OfType<T>().FirstOrDefault();
    public static DbConnectionStringBuilder WithoutSensitiveInformation(this DbConnectionStringBuilder b)
    {
        foreach (var v in new[] { "password", "Password", "PWD", "Pwd", "pwd", "PASSWORD" })
        {
            if (b.ContainsKey(v)) b[v] = "******";
        }
        return b;
    }

    static readonly ConcurrentDictionary<Type, bool> IsSimpleTypeCache = new ConcurrentDictionary<Type, bool>();

    public static bool IsSimpleType(this Type type)
    {
        return IsSimpleTypeCache.GetOrAdd(type, t =>
            type.IsPrimitive ||
            type.IsEnum ||
            type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) ||
            type == typeof(Guid) ||
            IsNullableSimpleType(type));

        static bool IsNullableSimpleType(Type t)
        {
            var underlyingType = Nullable.GetUnderlyingType(t);
            return underlyingType != null && IsSimpleType(underlyingType);
        }
    }

    static bool IsNullableType(this Type t)
    {
        return Nullable.GetUnderlyingType(t) != null;
    }

    public static IEnumerable<Option> GetOptions(this Type type, bool nonNullableMeansRequired = false)
        => type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToOptions(nonNullableMeansRequired);
    public static IEnumerable<Option> ToOptions(this IEnumerable<PropertyInfo> properties, bool nonNullableMeansRequired = false)
    {
        try
        {
            return from property in properties
                   let description = property.GetAttribute<DescriptionAttribute>()?.Description
                                  ?? property.GetDescriptionFromDocumentation()
                                  ?? property.Name.ToSentence()
                   let required = property.GetAttribute<RequiredAttribute>() != null || (nonNullableMeansRequired && property.PropertyType.IsValueType && !property.PropertyType.IsNullableType())
                   select new Option($"--{property.Name.ToHyphenedString()}", description)
                   {
                       IsRequired = required,
                       Argument = new Argument { ArgumentType = property.PropertyType }
                   };
        }
        finally
        {
            XmlDocumentation.Clear();
        }
    }

    public static Command WithChildCommand(this Command parent, Command child)
    {
        parent.AddCommand(child);
        return parent;
    }
    public static Command WithChildCommands(this Command parent, params Command[] children)
    {
        foreach (var child in children) parent.AddCommand(child);
        return parent;
    }


    public static Command WithArgument(this Command command, Argument argument)
    {
        command.AddArgument(argument);
        return command;
    }
    public static Command WithOptions(this Command command, IEnumerable<Option> options)
    {
        foreach (var o in options) command.AddOption(o);
        return command;
    }

    public static string? GetDescriptionFromDocumentation(this PropertyInfo p)
    {
        var doc = p.GetDocumentation();
        if (doc != null)
        {
            var xdoc = XDocument.Parse("<root>" + doc + "</root>");
            var value = xdoc.Descendants("summary").First().Value.Trim();
            return Cleanup(value);
        }
        return null;
    }
    enum CleanupState
    {
        Whitespace,
        Nonwhitespace
    }
    static string Cleanup(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var sb = new StringBuilder();
        var state = CleanupState.Nonwhitespace;
        var start = value.StartsWith("Gets or sets ", StringComparison.OrdinalIgnoreCase) ? "Gets or sets ".Length : 0;
        for (int i = start; i < value.Length; i++)
        {
            var c = value[i];
            (state, sb) = (state, new Char(c)) switch
            {
                (CleanupState.Whitespace, { IsWhiteSpace: true }) => (state, sb),
                (CleanupState.Whitespace, { IsWhiteSpace: false }) => (CleanupState.Nonwhitespace, sb.Append(c)),
                (CleanupState.Nonwhitespace, { IsWhiteSpace: true }) => (CleanupState.Whitespace, sb.Append(' ')),
                (CleanupState.Nonwhitespace, { IsWhiteSpace: false }) => (state, sb.Append(c)),
                _ => throw new Exception("Unhandled switch case")
            };
        }
        sb[0] = char.ToUpper(sb[0]);
        return sb.ToString();
    }
}
