﻿using ConsoleAI.Options;
using LibraryAI.Tools;
using LibraryAI.Vector;
using Spectre.Console;
using System.Linq;

namespace ConsoleAI.Handler;

public class ChunkHandler
{
    public static int RunHandler(ChunkOptions options)
    {
        VectorDbContext.Init();
        var db = VectorDbContextFactory.Create(options.DatabaseFile);
        db.EnsureCreated();
        
        var validFiles = options.InputFiles.Where(File.Exists).ToList();
        var invalidFiles = options.InputFiles.Except(validFiles).ToList();

        AnsiConsole.MarkupLine($"[bold]Input Summary:[/]");
        AnsiConsole.MarkupLine($"[green]* Valid files: {validFiles.Count}[/]");
        AnsiConsole.MarkupLine($"[red]* Invalid files: {invalidFiles.Count}[/]");

        if (invalidFiles.Any())
        {
            AnsiConsole.MarkupLine("\n[yellow]Invalid files:[/]");
            foreach (var file in invalidFiles)
            {
                AnsiConsole.MarkupLine($"[red]  - {Markup.Escape(file)}[/]");
            }
        }

        if (validFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No valid files to process. Exiting.[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"\n[bold]Starting to process {validFiles.Count} file(s)...[/]\n");

        long maxId = db.Vectors.Any() ? db.Vectors.Max(v => v.Id) : 0;

        AnsiConsole.Progress()
            .Start(ctx => 
            {
                var overallTask = ctx.AddTask("[green]Overall progress[/]", maxValue: validFiles.Count);

                foreach (var file in validFiles)
                {
                    var fileTask = ctx.AddTask($"[blue]{Path.GetFileName(file)}[/]", maxValue: 100);
                    
                    using var fs = File.OpenRead(file);
                    ChunkHandler handler = new ChunkHandler(db, file, fileTask, maxId + 1);
                    var chunker = new ChunkTools(
                        fs,
                        chunkSize: options.ChunkSize,
                        chunkOverlap: options.ChunkOverlap,
                        chunkCallback: chunk => handler.TextChunkHandler(chunk)
                    );
                    chunker.Chunk();
                    
                    maxId = handler.MaxId;
                    fileTask.Value = 100;
                    overallTask.Increment(1);
                }
            });
        
        AnsiConsole.MarkupLine("\n[bold green]Processing completed successfully![/]");
        return 0;
    }

    public long MaxId;
    public VectorDbContext DbContext;
    public string FilePath;
    private ProgressTask FileTask;
    private long TotalFileSize;
    
    public ChunkHandler(VectorDbContext dbContext, string filePath, ProgressTask fileTask, long startId)
    {
        DbContext = dbContext;
        FilePath = filePath;
        FileTask = fileTask;
        TotalFileSize = new FileInfo(filePath).Length;
        MaxId = startId - 1;
    }

    public void TextChunkHandler(string chunk)
    {
        MaxId++;
        DbContext.Vectors.Add(new VectorEntity
        {
            Id = MaxId,
            Status = 1,
            VectorId = $"CHUNK:{MaxId}",
            Embedding = [],
            Text = chunk,
            Sources = Path.GetFileName(FilePath),
            Metadata = null,
        });
        
        long processedSize = MaxId * chunk.Length;
        double progress = (double)processedSize / TotalFileSize * 100;
        FileTask.Value = Math.Min(progress, 99);
        
        DbContext.SaveChanges();
    }
}
