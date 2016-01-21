namespace SqlConsole.Host
{
    internal struct ParameterMapping
    {
        public readonly CommandLineParam CommandLineParam;
        public readonly ConnectionStringParam ConnectionStringParam;
        public readonly Provider Provider;

        public ParameterMapping(CommandLineParam commandLineParam, ConnectionStringParam connectionStringParam, Provider provider)
            : this()
        {
            CommandLineParam = commandLineParam;
            ConnectionStringParam = connectionStringParam;
            Provider = provider;
        }
    }
}