using System.CommandLine;
using System.CommandLine.Invocation;
using SqlConsole.Host.Infrastructure;

namespace SqlConsole.Host;

/// <summary>
/// Provides shell completion scripts for various shells.
/// </summary>
public static class CompletionCommand
{
    public static Command Create()
    {
        var shellOption = new Option<string>(
            "--shell",
            "The shell to generate completion script for")
        {
            IsRequired = true
        };
        shellOption.FromAmong("bash", "zsh", "powershell", "fish");

        var completionCommand = new Command("completion", 
            "Generate shell completion scripts for sqlc.\n\n" +
            HelpFormatter.Colorize("Usage examples:", ConsoleColor.Magenta) + "\n" +
            "# Install bash completion\n" +
            "sqlc completion --shell bash >> ~/.bashrc\n" +
            "source ~/.bashrc\n\n" +
            "# Install zsh completion\n" +
            "sqlc completion --shell zsh >> ~/.zshrc\n" +
            "source ~/.zshrc\n\n" +
            "# Install PowerShell completion\n" +
            "sqlc completion --shell powershell >> $PROFILE\n\n" +
            "# Install fish completion\n" +
            "sqlc completion --shell fish > ~/.config/fish/completions/sqlc.fish")
        {
            Handler = CommandHandler.Create<string, IConsole>(GenerateCompletion)
        };
        completionCommand.AddOption(shellOption);

        return completionCommand;
    }

    private static void GenerateCompletion(string shell, IConsole console)
    {
        // Don't create the root command recursively, just generate completion directly
        var completionScript = GenerateCompletionScript(shell);
        console.Out.Write(completionScript);
    }

    private static string GenerateCompletionScript(string shell)
    {
        return shell.ToLowerInvariant() switch
        {
            "bash" => GenerateBashCompletion(),
            "zsh" => GenerateZshCompletion(),
            "powershell" => GeneratePowerShellCompletion(),
            "fish" => GenerateFishCompletion(),
            _ => throw new ArgumentException($"Unsupported shell: {shell}")
        };
    }

    private static string GenerateBashCompletion()
    {
        var commandName = "sqlc";
        var providers = Provider.All.Select(p => p.Name).ToArray();

        return $@"# Bash completion for {commandName}
_sqlc_completion() {{
    local cur prev words cword
    _init_completion || return

    case ${{prev}} in
        sqlc)
            COMPREPLY=($(compgen -W ""console query completion --help --version"" -- ""${{cur}}""))
            return 0
            ;;
        console|query)
            COMPREPLY=($(compgen -W ""{string.Join(" ", providers)} --help"" -- ""${{cur}}""))
            return 0
            ;;
        completion)
            COMPREPLY=($(compgen -W ""--shell bash zsh powershell fish --help"" -- ""${{cur}}""))
            return 0
            ;;
        --shell)
            COMPREPLY=($(compgen -W ""bash zsh powershell fish"" -- ""${{cur}}""))
            return 0
            ;;
    esac

    # Handle provider-specific completions
    case ${{words[1]}} in
        console|query)
            if [[ ${{#words[@]}} -eq 3 ]]; then
                COMPREPLY=($(compgen -W ""{string.Join(" ", providers)}"" -- ""${{cur}}""))
            fi
            ;;
    esac
}}

complete -F _sqlc_completion sqlc
";
    }

    private static string GenerateZshCompletion()
    {
        var providers = Provider.All.Select(p => p.Name).ToArray();

        return $@"#compdef sqlc

_sqlc() {{
    local context state line
    typeset -A opt_args

    _arguments -C \
        '1: :->command' \
        '*: :->args' && return 0

    case $state in
        command)
            _alternative \
                'commands:commands:(console query completion)' \
                'options:options:(--help --version)'
            ;;
        args)
            case ${{words[2]}} in
                console|query)
                    _alternative \
                        'providers:providers:({string.Join(" ", providers)})' \
                        'options:options:(--help)'
                    ;;
                completion)
                    _arguments \
                        '--shell[Shell type]:(bash zsh powershell fish)' \
                        '--help[Show help]'
                    ;;
            esac
            ;;
    esac
}}

_sqlc ""$@""
";
    }

    private static string GeneratePowerShellCompletion()
    {
        var providers = Provider.All.Select(p => p.Name).ToArray();

        return $@"# PowerShell completion for sqlc
Register-ArgumentCompleter -Native -CommandName sqlc -ScriptBlock {{
    param($commandName, $wordToComplete, $cursorPosition)
    
    $words = $wordToComplete.Split(' ', [StringSplitOptions]::RemoveEmptyEntries)
    
    switch ($words.Count) {{
        1 {{
            'console', 'query', 'completion', '--help', '--version' | Where-Object {{ $_ -like ""$wordToComplete*"" }}
        }}
        2 {{
            switch ($words[0]) {{
                'console' {{
                    {string.Join(", ", providers.Select(p => $"'{p}'"))} | Where-Object {{ $_ -like ""$wordToComplete*"" }}
                }}
                'query' {{
                    {string.Join(", ", providers.Select(p => $"'{p}'"))} | Where-Object {{ $_ -like ""$wordToComplete*"" }}
                }}
                'completion' {{
                    '--shell', '--help' | Where-Object {{ $_ -like ""$wordToComplete*"" }}
                }}
            }}
        }}
        3 {{
            if ($words[1] -eq '--shell') {{
                'bash', 'zsh', 'powershell', 'fish' | Where-Object {{ $_ -like ""$wordToComplete*"" }}
            }}
        }}
    }}
}}
";
    }

    private static string GenerateFishCompletion()
    {
        var providers = Provider.All.Select(p => p.Name).ToArray();

        return $@"# Fish completion for sqlc
complete -c sqlc -f

# Main commands
complete -c sqlc -n '__fish_use_subcommand' -a 'console' -d 'Run an interactive SQL console'
complete -c sqlc -n '__fish_use_subcommand' -a 'query' -d 'Run a SQL query inline or from a file'
complete -c sqlc -n '__fish_use_subcommand' -a 'completion' -d 'Generate shell completion scripts'
complete -c sqlc -n '__fish_use_subcommand' -l help -d 'Show help and usage information'
complete -c sqlc -n '__fish_use_subcommand' -l version -d 'Show version information'

# Console subcommand providers
{string.Join("\n", providers.Select(p => $"complete -c sqlc -n '__fish_seen_subcommand_from console' -a '{p}' -d 'Use {p} provider'"))}

# Query subcommand providers  
{string.Join("\n", providers.Select(p => $"complete -c sqlc -n '__fish_seen_subcommand_from query' -a '{p}' -d 'Use {p} provider'"))}

# Completion subcommand options
complete -c sqlc -n '__fish_seen_subcommand_from completion' -l shell -d 'Shell type' -a 'bash zsh powershell fish'
complete -c sqlc -n '__fish_seen_subcommand_from completion' -l help -d 'Show help'
";
    }

    private static string[] GetSubcommands(Command command)
    {
        // For System.CommandLine 2.0 beta, we'll hardcode the known subcommands
        return new[] { "console", "query", "completion" };
    }
}