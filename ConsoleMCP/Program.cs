using CommandLine;
using ConsoleMCP.Handler;
using ConsoleMCP.Options;
using McpDotNet;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace ConsoleMCP;

class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var exitCode = Parser.Default.ParseArguments<
                    VectorMcpOptions
                >(args)
                .MapResult(
                    (VectorMcpOptions o) => VectorMcpHandler.RunHandler(o),
                    errors => HandleParseError(errors)
                );
            
            return exitCode;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return 1;
        }
    }

    private static int HandleParseError(IEnumerable<Error> errors)
    {
        AnsiConsole.MarkupLine("[bold red]Command line argument errors:[/]");
        foreach (var error in errors)
        {
            AnsiConsole.MarkupLine($"[red]  - {Markup.Escape(error.Tag.ToString())}[/]");
        }
        return 1;
    }

    private static void HandleException(Exception ex)
    {
        AnsiConsole.MarkupLine("[bold red][[!]] An unexpected error occurred:[/]");
        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShowLinks);
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold red][[#]] Full Stack Trace:[/]");
        
        AnsiConsole.Write(
            new Panel(Markup.Escape(ex.ToString()))
                .Expand()
                .BorderColor(Color.Red)
                .RoundedBorder()
        );
    }
}