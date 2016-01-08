using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SqlConsole.Host.Infrastructure;

namespace SqlConsole.Host
{
    static class Extension
    {
        static LambdaComparer<T> CompareBy<T>(this IEnumerable<T> list, Func<T, int> f)
        {
            return new LambdaComparer<T>(f);
        }

        public static IEnumerable<string> Words(this string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (!char.IsWhiteSpace(c))
                    sb.Append(c);
                else
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
            }
            if (sb.Length > 0)
                yield return sb.ToString();
            sb.Clear();
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

            while (maxLengths.Sum(x => x.MaxLength) + (dt.Columns.Count - 1) * separatorSize > totalWidth - 1)
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
    }
}