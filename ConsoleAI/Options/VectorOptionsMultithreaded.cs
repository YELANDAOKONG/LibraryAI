﻿using CommandLine;

namespace ConsoleAI.Options;

[Verb("vector-threads")]
public class VectorOptionsMultithreaded
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
    
    [Option('p', "max-degree-of-parallelism", Required = false, HelpText = "Max degree of parallelism. Default is 5.")]
    public int MaxDegreeOfParallelism { get; set; } = 2;
    
    [Option('b', "batch-size", Required = false, HelpText = "Batch size. Default is 50.")]
    public int BatchSize { get; set; } = 50;
}