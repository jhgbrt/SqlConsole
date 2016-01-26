using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Subtext.Scripting;

namespace SqlConsole.Host
{
    public static class Extension
    {
        // argument list is used for type inference
        // ReSharper disable once UnusedParameter.Local
        static LambdaComparer<T> CompareBy<T>(this IEnumerable<T> list, Func<T, int> f)
        {
            return new LambdaComparer<T>(f);
        }

        private static string RegexReplace(this string input, string pattern, string replace)
        {
            return Regex.Replace(input, pattern, replace);
        }
        private static string RegexReplace(this string input, string pattern, MatchEvaluator evaluator)
        {
            return Regex.Replace(input, pattern, evaluator);
        }
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
        {
            return new string(s.TakeWhile(char.IsLetterOrDigit).ToArray());
        } 

        // needed instead of anonymous types b/c MaxLength field needs to be mutable
        class Item
        {
            public DataColumn Column;
            public int MaxLength;
        }
        public static Dictionary<DataColumn, int> ColumnLengths(this DataTable dt, int totalWidth, int separatorSize)
        {
            var maxLengths = (
                from col in dt.Columns.OfType<DataColumn>()
                let rows = dt.Rows.OfType<DataRow>().Select(row => row[col].ToString()).ToList()
                let maxLength = new[] { col.ColumnName }.Concat(rows).Select(s => s.Length).DefaultIfEmpty().Max()
                select new Item { Column = col, MaxLength = maxLength }
                ).ToList();

            var comparer = maxLengths.CompareBy(x => -x.MaxLength);

            maxLengths.Sort(comparer);

            while (maxLengths[0].MaxLength >= 10 && maxLengths.Sum(x => x.MaxLength) + (dt.Columns.Count - 1) * separatorSize > totalWidth - 1)
            {
                maxLengths[0].MaxLength--;
                maxLengths.Sort(comparer);
            }

            return maxLengths.ToDictionary(x => x.Column, x => x.MaxLength);
        }

        public static string OfLength(this string s, int l)
        {
            s = s ?? string.Empty;
            var result = s.PadRight(l).Substring(0, l);
            if (result.Length < s.Length && result.Length > 3)
            {
                result = result.Substring(0, result.Length - 3) + "...";
            }
            return result;
        }

        public static string SafeSubstring(this string s, int startIndex, int length)
        {
            return s.Substring(startIndex, Math.Min(length, s.Length));
        }

        public static IEnumerable<string> SplitOnGo(this string s)
        {
            var splitter = new ScriptSplitter(s);
            return splitter;
        } 
    }
}