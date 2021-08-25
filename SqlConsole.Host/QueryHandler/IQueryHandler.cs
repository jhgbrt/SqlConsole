namespace SqlConsole.Host
{
    interface IQueryHandler : IDisposable
    {
        public string ConnectionStatus { get; }
        public void Disconnect();
        public void Connect();
        void Execute(string query);
    }
}