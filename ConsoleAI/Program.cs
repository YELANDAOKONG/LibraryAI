using CommandLine;
using ConsoleAI.Handler;
using ConsoleAI.Options;
using Microsoft.Extensions.Options;

namespace ConsoleAI;

class Program
{
    public static int Main(string[] args)
    {
        var exitCode = Parser.Default.ParseArguments<
                ChunkOptions
            >(args)
            .MapResult(
                (ChunkOptions o) => ChunkHandler.RunHandler(o),
                error => 1
            );
        
        return exitCode;
    }
}