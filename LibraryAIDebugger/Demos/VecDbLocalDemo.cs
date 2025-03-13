using System.ClientModel;
using System.Text;
using System.Text.Json;
using LibraryAI.Core;
using LibraryAI.Tools;
using LibraryAI.Vector;
using OpenAI;
using OpenAI.Embeddings;

namespace LibraryAIDebugger.Demos;

public class VecDbLocalDemo
{
    public static void Run()
    {
        // var options = new OpenAIClientOptions();
        // options.Endpoint = new Uri("http://127.0.0.1:1234/v1");
        // EmbeddingClient client = new EmbeddingClient(
        //     "text-embedding-bge-m3@f16",
        //     new ApiKeyCredential("lm-studio"),
        //     options
        // );
        //
        // BinaryData input = BinaryData.FromObjectAsJson(new
        // {
        //     model = "text-embedding-bge-m3@f16",
        //     input = "Hello world",
        //     encoding_format = "float"
        // });
        // using BinaryContent content = BinaryContent.Create(input);
        // Console.WriteLine(client.GenerateEmbeddings(content));
        // return;
        
        var file = Environment.GetEnvironmentVariable("FILE");
        if (file == null)
        {
            Console.WriteLine("Please set the FILE environment variable.");
            return;
        }
        
        VectorDbContext.Init();
        
        var service = ClientBuilder.Build(
            "http://127.0.0.1:1234/v1",
            "lm-studio"
        );
        var models = service.GetOpenAIModelClient().GetModels();
        Console.WriteLine($"Got Models: {models.Value.Count}");
        foreach (var theModel in models.Value)
        {
            Console.WriteLine($"{theModel.Id}, {theModel.CreatedAt}, {theModel.OwnedBy}");
        }
        
        var model = "text-embedding-bge-m3@f16";
        // var model = "text-embedding-bge-m3@q4_k_s";
        var embed = service.GetEmbeddingClient(model);
        var db = VectorDbContextFactory.Create("vector.db");
        db.EnsureCreated();
        
        int counter = 0;
        void ChunkHandler(string chunk)
        {
            counter++;
            
            BinaryData input = BinaryData.FromObjectAsJson(new
            {
                model = "text-embedding-bge-m3@f16",
                input = chunk,
                encoding_format = "float"
            });
            using BinaryContent content = BinaryContent.Create(input);
            var result = embed.GenerateEmbeddings(content);
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
            
            db.Vectors.Add(new VectorEntity
            {
                Id = counter,
                VectorId = $"SIMPLE-CHUNK:{counter}",
                Embedding = embedData,
                Text = chunk,
                Metadata = "",
            });
            Console.WriteLine($"[^] Chunk Handler (In Chunk {counter}): {chunk.Length} -> {embedData.Length}");
            db.SaveChanges();
        }
        
        Console.WriteLine("[+] Start Chunking...");
        using var fs = File.OpenRead(file);
        var chunker = new ChunkTools(
            fs,
            chunkSize: 1024,
            chunkOverlap: 256,
            chunkCallback: chunk => ChunkHandler(chunk)
        );
        
        chunker.Chunk();
        
        Console.WriteLine(Path.GetFullPath(file));
        Console.WriteLine(Path.GetFullPath("vector.db"));
        Console.WriteLine("[-] All Done.");
    }
}