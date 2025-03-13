using LibraryAI.Core;
using LibraryAI.Tools;
using LibraryAI.Vector;

namespace LibraryAIDebugger.Demos;

public class ChunkDemo
{
    public static void Run()
    {
        var file = Environment.GetEnvironmentVariable("FILE");
        if (file == null)
        {
            Console.WriteLine("Please set the FILE environment variable.");
            return;
        }
        
        VectorDbContext.Init();
        var db = VectorDbContextFactory.Create("chunks.db");
        db.EnsureCreated();
        
        int counter = 0;
        void ChunkHandler(string chunk)
        {
            counter++;
            db.Vectors.Add(new VectorEntity
            {
                Id = counter,
                VectorId = $"SIMPLE-CHUNK:{counter}",
                Embedding = new float[] {},
                Text = chunk,
                Metadata = "[CHUNK]",
            });
            Console.WriteLine($"[^] Chunk Handler (In Chunk {counter}): {chunk.Length}");
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
        Console.WriteLine(Path.GetFullPath("chunks.db"));
        Console.WriteLine("[-] All Done.");
    }
}