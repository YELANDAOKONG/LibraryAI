using CommandLine;
using ConsoleAI.Handler;
using ConsoleAI.Options;
using Spectre.Console;
using System;

namespace ConsoleAI;

class Program
{
    public static int Main(string[] args)
    {
        try
        {
            AnsiConsole.Write(
                new FigletText("Console AI")
                    .LeftJustified()
                    .Color(Color.Cyan1));
            
            var exitCode = Parser.Default.ParseArguments<ChunkOptions>(args)
                .MapResult(
                    (ChunkOptions o) => ChunkHandler.RunHandler(o),
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
            AnsiConsole.MarkupLine($"[red]  • {error.Tag}[/]");
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
            new Panel(ex.ToString())
                .Expand()
                .BorderColor(Color.Red)
                .RoundedBorder()
        );
    }
}