using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace SqlConsole.Host
{
    static class Program
    {
        static async Task<int> Main(string[] args)
        {
            //while (true)
            //{
            //    var key = Console.ReadKey();
            //    Console.WriteLine($" - {key.Key} - {key.KeyChar}");
            //}
            var command = CommandFactory.CreateCommand();
            return await command.InvokeAsync(args);
        }
    }
}