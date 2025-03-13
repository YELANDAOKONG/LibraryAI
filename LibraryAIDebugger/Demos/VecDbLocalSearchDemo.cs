using System.ClientModel;
using System.Text.Json;
using LibraryAI.Core;
using LibraryAI.Vector;
using OpenAI.Embeddings;

namespace LibraryAIDebugger.Demos;

public class VecDbLocalSearchDemo
{
    public static void Run()
    {
        var service = ClientBuilder.Build(
            "http://127.0.0.1:1234/v1",
            "lm-studio"
        );
        var models = service.GetOpenAIModelClient().GetModels();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"Got Models: {models.Value.Count}");
        Console.ForegroundColor = ConsoleColor.Gray;
        foreach (var theModel in models.Value)
        {
            Console.WriteLine($"{theModel.Id}, {theModel.CreatedAt}, {theModel.OwnedBy}");
        }
        
        var model = "text-embedding-bge-m3@f16";
        // var model = "text-embedding-bge-m3@q4_k_s";
        var embed = service.GetEmbeddingClient(model);
        var db = VectorDbContextFactory.Create("vector.db");
        db.EnsureCreated();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("[?] Input Search: (@EOF)");
        Console.ResetColor();
        // var inputData = Console.ReadLine();
        var inputData = "";
        while (true)
        {
            var line = Console.ReadLine();
            if (line != null && line.Equals("@EOF"))
            {
                break;
            }
            else
            {
                inputData += line + "\n";
            }
        }
        
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("[%] Start Chunking...");
        float[] embedData = RunRequest(embed, inputData!);
        Console.WriteLine($"[/] Embedding: {embedData.Length}");
        
        var similarityResults = new List<(long Id, float Score)>();
        foreach (var entity in db.Vectors)
        {
            try 
            {
                float similarity = VectorComparer.CosineSimilarity(
                    embedData, 
                    entity.Embedding ?? Array.Empty<float>()
                );
                
                similarityResults.Add((entity.Id, similarity));
                
            }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"️[!] Error processing vector {entity.Id}: {ex.Message}");
                Console.ResetColor();
            }
        }

        var orderedResults = similarityResults
            .OrderByDescending(x => x.Score)
            .Take(10) // Top 10
            .ToList();


        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n[*] Top Matches:");
        Console.ResetColor();
        foreach (var result in orderedResults)
        {
            // Console.WriteLine($"-> ID: {result.Id.ToString().Substring(0,8)}... | Score: {result.Score:F4}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"-> ID: {result.Id.ToString()}, Score: {result.Score}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(db.Vectors.First(x => x.Id == result.Id).Text);
            Console.ResetColor();
        }




    }
    
    public static float[] RunRequest(EmbeddingClient client, string data)
    {
        BinaryData input = BinaryData.FromObjectAsJson(new
        {
            model = "text-embedding-bge-m3@f16",
            input = data,
            encoding_format = "float"
        });
        using BinaryContent content = BinaryContent.Create(input);
        var result = client.GenerateEmbeddings(content);
        // var result = embed.GenerateEmbedding(chunk);
        BinaryData output = result.GetRawResponse().Content;
        using JsonDocument outputAsJson = JsonDocument.Parse(output.ToString());
        JsonElement vector = outputAsJson.RootElement
            .GetProperty("data"u8)[0]
            .GetProperty("embedding"u8);
            
        var embedData = new float[vector.GetArrayLength()];
        int embedCounter = 0;
        foreach (JsonElement element in vector.EnumerateArray())
        {
            embedData[embedCounter] = (float)element.GetDouble();
            embedCounter++;
        }

        return embedData;
    }
}