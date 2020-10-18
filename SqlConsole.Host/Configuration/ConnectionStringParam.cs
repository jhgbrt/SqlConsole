namespace SqlConsole.Host
{
    record ConnectionStringParam(string Name)
    {
        public override string ToString() => Name;
        public static readonly ConnectionStringParam DataSource = new ConnectionStringParam("Data Source");
        public static readonly ConnectionStringParam Server = new ConnectionStringParam("Server");
        public static readonly ConnectionStringParam Port = new ConnectionStringParam("Port");
        public static readonly ConnectionStringParam InitialCatalog = new ConnectionStringParam("Initial Catalog");
        public static readonly ConnectionStringParam Database = new ConnectionStringParam("Database");
        public static readonly ConnectionStringParam UserId = new ConnectionStringParam("User Id");
        public static readonly ConnectionStringParam Uid = new ConnectionStringParam("UID");
        public static readonly ConnectionStringParam Password = new ConnectionStringParam("Password");
        public static readonly ConnectionStringParam Pwd = new ConnectionStringParam("PWD");
        public static readonly ConnectionStringParam IntegratedSecurity = new ConnectionStringParam("Integrated Security");
        public static readonly ConnectionStringParam Attachdbfilename = new ConnectionStringParam("AttachDbFileName");
    }
}