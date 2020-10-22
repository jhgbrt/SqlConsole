using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SqlConsole.Host.Infrastructure;

namespace SqlConsole.Host
{
    public static class Extension
    {
        // argument list is used for type inference
        // ReSharper disable once UnusedParameter.Local
        static LambdaComparer<T> CompareBy<T>(this IEnumerable<T> list, Func<T, int> f) 
            => new LambdaComparer<T>(f);

        private static string RegexReplace(this string input, string pattern, string replace) 
            => Regex.Replace(input, pattern, replace);
        private static string RegexReplace(this string input, string pattern, MatchEvaluator evaluator) 
            => Regex.Replace(input, pattern, evaluator);
        
        public static string BookTitleToSentence(this string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            
            var result = s
                .RegexReplace(@"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2")               // ABc => A Bc, 1Bc => 1 Bc
                .RegexReplace(@"(\p{Ll})(\P{Ll})", "$1 $2")                     // aB  => a B , a1  => a 1
                .RegexReplace(@"( )(\p{Lu})(\p{Ll})", m => m.Value.ToLower());  // ' Aa' => ' aa'

            return result;
        }

        public static string FirstWord(this string s) 
            => new string(s.TakeWhile(char.IsLetterOrDigit).ToArray());

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


        enum State
        {
            UpperCase,
            LowerCase
        }

        public static string ToHyphenedString(this string input)
        {

            var sb = new StringBuilder();

            var state = State.UpperCase;

            foreach (var c in input)
            {
                (state, sb) = state switch
                {
                    State.UpperCase when char.IsLower(c) => (State.LowerCase, sb),
                    State.LowerCase when char.IsUpper(c) => (State.UpperCase, sb.Append('-')),
                    _ => (state, sb)
                };
                if (!char.IsWhiteSpace(c))
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
            var state = State.UpperCase;

            foreach (var c in input)
            {
                switch (state)
                {
                    case State.UpperCase when sb.Length == 0:
                        sb.Append(char.ToUpper(c));
                        break;
                    case State.UpperCase when char.IsLower(c):
                        if (buffer.Length == 1)
                            sb.Append(buffer.ToString().ToLower());
                        else
                            sb.Append(buffer);
                        buffer.Clear();
                        sb.Append(c);
                        state = State.LowerCase;
                        break;
                    case State.UpperCase:
                        buffer.Append(c);
                        break;
                    case State.LowerCase when char.IsUpper(c):
                        sb.Append(' ');
                        buffer.Append(c);
                        state = State.UpperCase;
                        break;
                    case State.LowerCase:
                        sb.Append(c);
                        break;
                }
            }

            if (buffer.Length > 0)
                sb.Append(buffer);

            return sb.ToString();
        }

        public static T? GetAttribute<T>(this ICustomAttributeProvider prop) => prop.GetCustomAttributes(false).OfType<T>().FirstOrDefault();
        public static DbConnectionStringBuilder WithoutSensitiveInformation(this DbConnectionStringBuilder b)
        {
            foreach (var v in new[] { "password", "Password", "PWD", "Pwd", "pwd" })
            {
                if (b.ContainsKey(v)) b[v] = "******";
            }
            return b;
        }
    }
}