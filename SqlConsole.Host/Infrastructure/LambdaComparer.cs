using System;
using System.Collections.Generic;

namespace SqlConsole.Host.Infrastructure
{
    class LambdaComparer<T> : IComparer<T>
    {
        private Func<T, int> _f;

        public LambdaComparer(Func<T, int> f)
        {
            _f = f;
        }

        public int Compare(T x, T y)
        {
            return _f(x).CompareTo(_f(y));
        }
    }
}