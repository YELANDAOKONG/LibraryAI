using LibraryAI.Core;
using LibraryAI.Tools;
using LibraryAI.Vector;

namespace LibraryAIDebugger.Demos;

public class VecDbDemo
{
    public static void Run()
    {
        var key = Environment.GetEnvironmentVariable("KEY");
        var file = Environment.GetEnvironmentVariable("FILE");
        if (key == null)
        {
            Console.WriteLine("Please set the KEY environment variable.");
            return;
        }
        if (file == null)
        {
            Console.WriteLine("Please set the FILE environment variable.");
            return;
        }
        
        VectorDbContext.Init();
        
        var service = ClientBuilder.Build(
            "https://api.siliconflow.cn/v1/",
            key
        );

        
        var model = "Pro/BAAI/bge-m3";
        var embed = service.GetEmbeddingClient(model);
        var db = VectorDbContextFactory.Create("vector.db");
        db.EnsureCreated();
        
        int counter = 0;
        void ChunkHandler(string chunk)
        {
            counter++;
            var result = embed.GenerateEmbedding(chunk);
            if (result.Value == null)
            {
                Console.WriteLine($"[!] Chunk Handler Error (In Chunk {counter}): UNKNOWN ERROR");
                return;
            }
            
            db.Vectors.Add(new VectorEntity
            {
                Id = counter,
                VectorId = $"SIMPLE-CHUNK:{counter}",
                Embedding = result.Value.ToFloats().ToArray(),
                Text = chunk,
                Metadata = "",
            });
            Console.WriteLine($"[^] Chunk Handler (In Chunk {counter}): {chunk.Length} -> {result.Value.ToFloats().Length}");
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