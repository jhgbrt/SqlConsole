namespace SqlConsole.Host
{
    struct Value
    {
        private readonly string _value;

        private Value(string value)
            : this()
        {
            _value = value;
        }

        #region Equality
        public bool Equals(ConnectionStringParam other)
        {
            return string.Equals(_value, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ConnectionStringParam && Equals((ConnectionStringParam) obj);
        }

        public override int GetHashCode()
        {
            return (_value != null ? _value.GetHashCode() : 0);
        }

        public static bool operator ==(Value left, Value right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Value left, Value right)
        {
            return !(left == right);
        }
        #endregion

        public override string ToString()
        {
            return _value;
        }

        public static Value From(string value)
        {
            return new Value(value);
        }

        public string Get()
        {
            return _value;
        }
    }
}