using Net.Code.ADONet;
using SqlConsole.Host.Infrastructure;
using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlConsole.Host
{

    static class Program
    {
        static async Task Main(string[] args)
        {
#if DEBUG
            //GenerateSettings();
            //return;
#endif
            Action<Provider, DbConnectionStringBuilder, IConsole, InvocationContext> repl = DoRepl;
            Action<Provider, DbConnectionStringBuilder, IConsole, InvocationContext> doQuery = DoQuery;

            var console = (
                from dynamic p in Provider.All
                select CreateCommand(p, repl)
                ).OfType<Command>()
                .Aggregate(new Command("console") 
                {
                    Description = $"Run an interactive SQL console"
                }, (parent, command) => { parent.AddCommand(command); return parent; });

            var query = (
                from dynamic p in Provider.All
                select CreateCommand(p, doQuery)
                ).OfType<Command>()
                .Aggregate(CreateQueryCommand(), (parent, command) => { parent.AddCommand(command); return parent; });

            var root = new RootCommand
            {
                Description = $"A generic SQL utility tool for running SQL queries, either directly from the commandline or in an interactive console. " +
                    $"Supports the following providers: {string.Join(",", Provider.All.Select(p => p.Name))}. " +
                    $"Use the help function of each command for info on how to connect."
            };
            root.AddCommand(console);
            root.AddCommand(query);

            await root.InvokeAsync(args);
        }

        static readonly Option<bool> _scalar = new Option<bool>("--scalar");
        static readonly Option<bool> _nonquery = new Option<bool>("--nonquery");
        static readonly Option<string> _query= new Option<string>("--query");
        static readonly Option<FileInfo> _input = new Option<FileInfo>("--input");
        static readonly Option<FileInfo> _output = new Option<FileInfo>("--output");

        static Command CreateQueryCommand()
        {
            var command = new Command("query")
            {
                Description = $"Run a query."
            };

            command.AddGlobalOption(_scalar);
            command.AddGlobalOption(_nonquery);
            command.AddGlobalOption(_query);
            command.AddGlobalOption(_input);
            command.AddGlobalOption(_output);
            return command;
        }

        static void DoRepl(Provider provider, DbConnectionStringBuilder builder, IConsole console, InvocationContext context)
        {
            var connectionString = builder.ConnectionString;
            Console.WriteLine(builder.WithoutSensitiveInformation());

            using var db = new Db(connectionString, provider.DbConfig, provider.Factory);
            db.Connect();

            var queryHandler = new QueryHandler<DataTable>(db, Console.Out, cb => cb.AsDataTable(), new ConsoleTableFormatter(GetWindowWidth(), " | "));
            new Repl(queryHandler).Enter();
        }

        static void DoQuery(Provider provider, DbConnectionStringBuilder builder, IConsole console, InvocationContext context)
        {
            var connectionString = builder.ConnectionString;
            Console.WriteLine(builder.WithoutSensitiveInformation());

            using var db = new Db(connectionString, provider.DbConfig, provider.Factory);
            db.Connect();

            var scalar = context.ParseResult.ValueForOption(_scalar);
            var nonquery = context.ParseResult.ValueForOption(_nonquery);
            var input = context.ParseResult.ValueForOption(_input);
            var output = context.ParseResult.ValueForOption(_input);
            var query = context.ParseResult.ValueForOption(_query) ??
               (input is not null && input.Exists ? File.ReadAllText(input.FullName) : null);

            if (query is null)
                throw new ArgumentException("A query is required");

            IQueryHandler queryHandler;
            if (scalar)
            {
                queryHandler = new QueryHandler<object>(db, Console.Out, cb => cb.AsScalar(), new ScalarFormatter());
            }
            else if (nonquery)
            {
                queryHandler = new QueryHandler<int>(db, Console.Out, cb => cb.AsNonQuery(), new NonQueryFormatter());
            }
            else if (output != null)
            {
                using var writer = new StreamWriter(output.OpenWrite(), Encoding.UTF8);
                queryHandler = new QueryHandler<DataTable>(db, writer, cb => cb.AsDataTable(), new CsvFormatter());
            }
            else
            {
                queryHandler = new QueryHandler<DataTable>(db, Console.Out, cb => cb.AsDataTable(), new ConsoleTableFormatter(GetWindowWidth(), " | "));
            }

            queryHandler.Execute(query);
        }
        private static int GetWindowWidth()
        {
            try { return Console.WindowWidth; } catch { return 120; }
        }

        private static void GenerateSettings()
        {
            foreach (var typeargs in Provider.All.Select(p => p.GetType().GetGenericArguments()))
            {
                var type = typeargs[0];
                var typeName = type.Name.Replace("ConnectionStringBuilder", "Settings");
                Console.WriteLine($"class {typeName}");
                Console.WriteLine("{");

                var baseproperties = typeof(DbConnectionStringBuilder).GetProperties().Select(p => p.Name).ToHashSet();
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .Where(p => p.PropertyType.IsSimpleType() && p.GetSetMethod() is not null && !baseproperties.Contains(p.Name));

                foreach (var p in properties)
                {
                    Console.WriteLine($"   public {p.PropertyType.FullName}? {p.Name} {{ get; set; }}");
                }

                Console.WriteLine("}");
            }
        }

        public static Command CreateCommand<TConnectionStringBuilder, TSettings>(
            this Provider<TConnectionStringBuilder, TSettings> provider, 
            Action<Provider, DbConnectionStringBuilder, IConsole, InvocationContext> execute)
            where TConnectionStringBuilder : DbConnectionStringBuilder, new()
        {
            var command = (
                from item in Mapper.GetProperties<TSettings, TConnectionStringBuilder>()
                let property = item.targetproperty
                let required = property.GetAttribute<RequiredAttribute>() != null
                select new Option($"--{property.Name.ToHyphenedString()}", property.Name.ToSentence())
                {
                    IsRequired = required,
                    Argument = new Argument { ArgumentType = property.PropertyType }
                }).Aggregate(new Command(provider.Name), (command, option) => { command.AddOption(option); return command; });

            command.Handler = CommandHandler.Create((InvocationContext context) =>
            {
                var modelBinder = new ModelBinder<TSettings>();
                var settings = (TSettings)modelBinder.CreateInstance(context.BindingContext);
                var builder = settings!.Map<TSettings, TConnectionStringBuilder>();
                execute(provider, builder, context.Console, context);

                //if (!string.IsNullOrEmpty(config.Query))
                //{
                //    queryHandler.Execute(config.Query);
                //}
                //else
                //{
                //}

            });

            return command;
        }
    }
}