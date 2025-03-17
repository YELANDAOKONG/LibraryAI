using CommandLine;

namespace ConsoleMCP.Options;

[Verb("vector")]
public class VectorMcpOptions
{
    [Option( 'd', "database", Required = true, HelpText = "Database file name.")] // -d
    public string DatabaseFile { get; set; } = "vector.db";
    
    [Option( 'e', "endpoint", Required = false, HelpText = "OpenAI API Endpoint. Default is \"http://127.0.0.1:1234/v1\".")]
    public string OpenAiApiEndpoint { get; set; } = "http://127.0.0.1:1234/v1";
    
    [Option( 'k', "key", Required = false, HelpText = "OpenAI API Key. Default is \"lm-studio\".")]
    public string ApiKey { get; set; } = "lm-studio";
    
    [Option('m', "embedding-model", Required = true, HelpText = "Embedding model." )]
    public string EmbeddingModel { get; set; } = "text-embedding-bge-m3@f16";
    
    [Option('c', "compatibility", Required = false, HelpText = "Compatibility mode. Default is false.")]
    public bool Compatibility { get; set; } = false;
    
    [Option('p', "match-threshold", Required = false, HelpText = "Search match threshold. Default is 0.55F.")]
    public float MatchThreshold { get; set; } = 0.55f;
    
    [Option('n', "top-n", Required = false, HelpText = "Top N results. Default is 10.")]
    public int TopN { get; set; } = 10;
    
    [Option('l', "listen-address", Required = false, HelpText = "Listen address. Default is \"127.0.0.1\"." )]
    public string ListenAddress { get; set; } = "127.0.0.1";
    
    [Option('p', "listen-port", Required = false, HelpText = "Listen port. Default is 1234." )]
    public int ListenPort { get; set; } = 1235;
}