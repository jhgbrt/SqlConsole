namespace SqlConsole.Host
{
    struct ProviderMapping
    {
        public ProviderMapping(string name, string provider)
        {
            Name = name;
            Provider = provider;
        }

        public readonly string Name;
        public readonly string Provider;
    }
}