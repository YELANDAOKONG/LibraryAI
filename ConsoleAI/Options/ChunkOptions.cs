using CommandLine;

namespace ConsoleAI.Options;

[Verb("chunk")]
public class ChunkOptions
{
    [Value(0, HelpText = "Input Files.")]
    public IEnumerable<string> InputFiles { get; set; } = new List<string>();
    
    [Option( 'd', "database", Required = true, HelpText = "Database file name.")] // -d
    public string DatabaseFile { get; set; } = "vector.db";

    [Option( "chunk-size", Required = false, HelpText = "Chunk size. Default 1024.")] // -s
    public int ChunkSize { get; set; } = 1024;
    
    [Option( "chunk-overlap", Required = false, HelpText = "Chunk overlap. Default 256.")] // -o
    public int ChunkOverlap { get; set; } = 256;
    
    [Option("include-files", Required = false, HelpText = "Include files to database.")] // -f
    public bool IncludeFiles { get; set; } = false;
}