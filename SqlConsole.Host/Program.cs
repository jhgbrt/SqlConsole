using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace SqlConsole.Host
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var command = CommandFactory.CreateCommand();
            await command.InvokeAsync(args);
        }
    }
}