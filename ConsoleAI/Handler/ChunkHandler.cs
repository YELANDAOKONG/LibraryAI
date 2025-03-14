using ConsoleAI.Options;
using LibraryAI.Tools;
using LibraryAI.Vector;
using Spectre.Console;

namespace ConsoleAI.Handler;

public class ChunkHandler
{
    public static int RunHandler(ChunkOptions options)
    {
        VectorDbContext.Init();
        var db = VectorDbContextFactory.Create(options.DatabaseFile);
        db.EnsureCreated();
        
        // Check if the file exists
        bool fileExists = true;
        foreach (var inputFile in options.InputFiles)
        {
            if (!File.Exists(inputFile))
            {
                fileExists = false;
                AnsiConsole.Write(new Markup($"[red][[!]] File not found: [/]"));
                AnsiConsole.Write(new Markup($"[yellow]{Markup.Escape(inputFile)}[/]"));
                AnsiConsole.WriteLine();
                continue;
            }
        }
        if (!fileExists) return 1;

        int counter = 0;
        foreach (var file in options.InputFiles)
        {
            using var fs = File.OpenRead(file);
            ChunkHandler handler = new ChunkHandler(db, file);
            var chunker = new ChunkTools(
                fs,
                chunkSize: options.ChunkSize,
                chunkOverlap: options.ChunkOverlap,
                chunkCallback: chunk => handler.TextChunkHandler(chunk)
            );
            chunker.Chunk();
            counter++;
        }
        
        
        return 0;
    }

    
    public int TextChunkCounter = 0;
    public VectorDbContext DbContext;
    public string FilePath;
    
    public ChunkHandler(VectorDbContext dbContext, string filePath)
    {
        this.DbContext = dbContext;
        this.FilePath = filePath;
    }

    public void TextChunkHandler(string chunk)
    {
        TextChunkCounter++;
        DbContext.Vectors.Add(new VectorEntity
        {
            Id = TextChunkCounter,
            Status = 1,
            VectorId = $"CHUNK:{TextChunkCounter}",
            Embedding = [],
            Text = chunk,
            Sources = Path.GetFileName(FilePath),
            Metadata = null,
        });
        AnsiConsole.MarkupLine($"[bold blue][[^]][/] Chunk Handler (In Chunk [green]{TextChunkCounter}[/]): [yellow]{chunk.Length}[/]");
        DbContext.SaveChanges();
    }
}