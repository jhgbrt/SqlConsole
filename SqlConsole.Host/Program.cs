using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlConsole.Host
{
    static class Program
    {
        static async Task<int> Main(string[] args)
        {
            var command = CommandFactory.CreateCommand();
            return await command.InvokeAsync(args);
        }
    }
}