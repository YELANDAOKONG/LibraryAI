using System.Net;
using System.Text;
using System.Text.Json;
using ConsoleAI.Options;
using LibraryAI.Tools;
using LibraryAI.Vector;
using McpDotNet;
using McpDotNet.Server;
using MCPSharp;
using MCPSharp.Model;
using MCPSharp.Model.Schemas;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace ConsoleAI.Handler.McpServices;

public class McpServiceHandler
{
    public static int RunHandler(McpServiceOptions options)
    {
        VectorDbContext.Init();
        var db = VectorDbContextFactory.Create(options.DatabaseFile);
        db.EnsureCreated();
        
        AnsiConsole.Write(new Rule("[yellow]Input Summary[/]").LeftJustified());
        AnsiConsole.MarkupLine($"[green]* API Endpoint : [white]{Markup.Escape(options.OpenAiApiEndpoint)}[/][/]");
        AnsiConsole.MarkupLine($"[blue]* Model : [white]{Markup.Escape(options.EmbeddingModel)}[/][/]");
        AnsiConsole.MarkupLine($"[#FFC0CB]* Compatibility : [white]{Markup.Escape(options.Compatibility.ToString())}[/][/]");
        AnsiConsole.MarkupLine($"[yellow]* Http Address : [white]{Markup.Escape(options.ListenAddress)}[/][/]");
        AnsiConsole.MarkupLine($"[yellow]* Http Port : [white]{Markup.Escape(options.ListenPort.ToString())}[/][/]");

        var pendingVectors = db.Vectors
            .Where(v => v.Status == (int)VectorStatus.Unprocessed)
            .ToList();
        
        MCPServer.AddToolHandler(new Tool() 
        {
            Name = "VectorSearch",
            Description = "Vector database search service.",
            InputSchema = new InputSchema {
                Type = "object",
                Required = ["input"],
                Properties = new Dictionary<string, ParameterSchema>{
                    {"input", new ParameterSchema{Type="string", Description="Data to search"}}
                }
            }
        }, (string input) =>
        {
            try
            {
                if (input.Length == 0)
                {
                    return "[TIPS] Input is empty! ";
                }
                if (!pendingVectors.Any())
                {
                    return "[TIPS] Vector database has no pending vectors to process! ";
                }
                var embedding = VectorTools.Generate(
                    options.OpenAiApiEndpoint,
                    options.ApiKey,
                    input,
                    options.EmbeddingModel,
                    null,
                    true,
                    options.Compatibility
                );
                if (embedding.Item1 == null)
                {
                    return "[ERROR] Failed to generate vectors! ";
                }
                
                var searchCounter = 0;
                var findCounter = 0;
                var startTime = DateTime.Now;
                var resultIds = new Dictionary<long, double>();
                var normalVectors = db.Vectors.Where(x => x.Status == (int)VectorStatus.Normal);
                var totalVectors = normalVectors.Count();
                
                foreach (var vector in normalVectors.AsEnumerable())
                {
                    searchCounter++;
                    var distance = VectorComparer.CosineSimilarity(embedding.Item1, vector.Embedding);
                    if (distance > options.MatchThreshold)
                    {
                        resultIds.Add(vector.Id, distance);
                        findCounter++;
                    }
                }
            
                if (resultIds.Count == 0)
                {
                    return "[TIPS] No results found! ";
                }
                var filterTime = DateTime.Now;
                var sortedDict = resultIds.OrderByDescending(x => x.Value)
                    .ToDictionary(x => x.Key, x => x.Value);
                if (sortedDict.Count > options.TopN)
                {
                    sortedDict = sortedDict.Take(options.TopN).ToDictionary(x => x.Key, x => x.Value);
                }

                StringBuilder resultBuilder = new StringBuilder();
                foreach (var id in sortedDict)
                {
                    var vector = db.Vectors.Find(id.Key);
                    if (vector == null)
                    {
                        continue;
                    }
                    resultBuilder.AppendLine("---------------------------");
                    resultBuilder.AppendLine($"@Title : {Markup.Escape(vector.Sources ?? "(Unknown)")}");
                    resultBuilder.AppendLine($"@Distance : {vector.Embedding.Length} / {id.Value.ToString("F")}");
                    resultBuilder.AppendLine($"@Text : \n{Markup.Escape(vector.Text)}");
                }

                return resultBuilder.ToString();
            }
            catch (Exception e)
            {
                return "[ERROR] Vector database unable to search! ";
            }
        });

        var mcpOptions = new McpServiceOptions()
        {
            ListenAddress = options.ListenAddress,
            ListenPort = options.ListenPort,
        };
        
        MCPServer.SetOutput(Console.Out);
        var task = MCPServer.StartAsync("VectorServer", "NULL");
        task.Wait();
        return 0;
    }
}