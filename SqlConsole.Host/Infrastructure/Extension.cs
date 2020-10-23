using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using SqlConsole.Host.Infrastructure;

namespace SqlConsole.Host
{
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
            => s.Substring(startIndex, Math.Min(length, s.Length));

        public static IEnumerable<string> SplitOnGo(this string s) 
            => new ScriptSplitter(s).Split();


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
                    (Digit, {IsLower: true }) => (LowerCase, sb),
                    (Digit, {IsUpper: true }) => (UpperCase, sb.Append('-')),
                    _ => (state, sb)
                };
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToLowerInvariant(c));
                }
            }

            return sb.ToString();
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
                    (UpperCase or WhiteSpace, { IsLower: true })
                        => (LowerCase, sb.Append(buffer.Length == 1 ? buffer.ToString().ToLower() : buffer.ToString()).Append(c), buffer.Clear()),
                    (Digit, { IsUpper: true })
                        => (UpperCase, sb.Append(buffer), buffer.Clear().Append(' ').Append(char.ToLower(c))),
                    (UpperCase, { IsDigit: true })
                        => (Digit, sb, buffer.Append(c)),
                    (LowerCase, { IsUnderscore: true } or { IsWhiteSpace: true })
                        => (WhiteSpace, sb.Append(' '), buffer),
                    (LowerCase, { IsDigit: true } or { IsUpper: true})
                        => (WhiteSpace, sb.Append(' '), buffer.Append(c)),
                    (WhiteSpace, { IsUpper: true })
                        => (UpperCase, sb, buffer.Append(c)),
                    (WhiteSpace, { IsDigit: true })
                        => (Digit, sb, buffer.Append(c)),
                    (UpperCase or Digit, _)
                        => (state, sb, buffer.Append(c)),
                    (LowerCase, _)
                        => (state, sb.Append(c), buffer),
                    _ => (state,sb,buffer)
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
    }
}