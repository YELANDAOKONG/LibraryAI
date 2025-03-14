using CommandLine;

namespace ConsoleAI.Options;

[Verb("chunk")]
public class ChunkOptions
{
    [Value(0, HelpText = "Input Files.")]
    public IEnumerable<string> InputFiles { get; set; } = new List<string>();
    
    [Option('d', "database", Required = true, HelpText = "Database file name.")]
    public string DatabaseFile { get; set; } = "vector.db";

    [Option('s', "chunk-size", Required = false, HelpText = "Chunk size. Default 1024.")]
    public int ChunkSize { get; set; } = 1024;
    
    [Option('o', "chunk-overlap", Required = false, HelpText = "Chunk overlap. Default 512.")]
    public int ChunkOverlap { get; set; } = 256;
}