using SqlConsole.Host;
using System.CommandLine;
using System.CommandLine.Parsing;

var command = CommandFactory.CreateCommand();
return await command.InvokeAsync(args);
