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
        private bool Equals(ConnectionStringParam other) => Equals(_value, other.Name);
        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (obj is ConnectionStringParam && Equals((ConnectionStringParam) obj));
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        public static bool operator ==(Value left, Value right) => left.Equals(right);
        public static bool operator !=(Value left, Value right) => !(left == right);
        #endregion

        public override string ToString() => _value;

        public static Value From(string value) => new Value(value);

        public string Get() => _value;
    }
}