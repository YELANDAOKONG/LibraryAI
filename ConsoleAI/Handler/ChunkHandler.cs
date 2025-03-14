using ConsoleAI.Options;
using LibraryAI.Tools;
using LibraryAI.Vector;
using Spectre.Console;
using System.Linq;
using LibraryAI.Utils;
using Microsoft.EntityFrameworkCore;

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

        AnsiConsole.Write(new Rule("[yellow]Input Summary[/]").LeftJustified());
        AnsiConsole.MarkupLine($"[green]* Valid files  : {validFiles.Count}[/]");
        AnsiConsole.MarkupLine($"[red]* Invalid files: {invalidFiles.Count}[/]");

        if (invalidFiles.Any())
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]Invalid files:[/]");
            foreach (var file in invalidFiles)
            {
                AnsiConsole.MarkupLine($"[red]  - {Markup.Escape(file)}[/]");
            }
        }

        if (validFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("\n[red][[!]] No valid files to process. Exiting.[/]");
            return 1;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]=> Starting to process {validFiles.Count} file(s)...[/]");

        long maxId = db.Vectors.Any() ? db.Vectors.Max(v => v.Id) : 0;

        AnsiConsole.Progress()
            .AutoClear(true)
            .HideCompleted(false)
            .AutoRefresh(true)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn(),
            })
            .Start(ctx => 
            {
                var overallTask = ctx.AddTask("[green]Overall progress[/]", maxValue: validFiles.Count);

                foreach (var file in validFiles)
                {
                    var fileName = Path.GetFileName(file);
                    if (fileName.Length > 36) 
                        fileName = $"{fileName.Substring(0, 33)}...";

                    var fileTask = ctx.AddTask($"[blue]{Markup.Escape(fileName)}[/]", maxValue: 100);

                    var sourcesId = SearchMaxSourcesId(db);
                    var fileGuid = $"FILE:{sourcesId + 1}:" + GuidUtils.GetFormattedGuid();
                    if (options.IncludeFiles)
                    {
                        byte[] fileData = File.ReadAllBytes(file);
                        db.Sources.Add(new()
                        {
                            // Id = sourcesId + 1,
                            Status = (int)SourcesStatus.Normal,
                            Title = Path.GetFileName(file),
                            SourcesId = fileGuid,
                            Data = fileData,
                        });
                        db.SaveChanges();
                    }
                    
                    using var fs = File.OpenRead(file);
                    ChunkHandler handler = new ChunkHandler(options, db, file, fileTask, maxId + 1, fileGuid);
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
        
        AnsiConsole.MarkupLine("\n[bold green][[%]] Processing completed successfully![/]");
        return 0;
    }

    public static long SearchMaxId(VectorDbContext db)
    {
        return db.Vectors.Any() ? db.Vectors.Max(v => v.Id) : 0;
    }
    
    public static long SearchMaxSourcesId(VectorDbContext db)
    {
        return db.Sources.Any() ? db.Sources.Max(v => v.Id) : 0;
    }

    public ChunkOptions POptions;
    public long MaxId;
    public VectorDbContext DbContext;
    public string FilePath;
    public ProgressTask FileTask;
    public long TotalFileSize;
    public long ProcessedFileSize = 0;
    public string? SourcesId;
    public long ChunkCounter = 0;
    
    public ChunkHandler(ChunkOptions options, VectorDbContext dbContext, string filePath, ProgressTask fileTask, long startId, string sourcesId)
    {
        POptions = options;
        DbContext = dbContext;
        FilePath = filePath;
        FileTask = fileTask;
        TotalFileSize = new FileInfo(filePath).Length;
        MaxId = startId - 1;
        SourcesId = sourcesId;
    }

    public void TextChunkHandler(string chunk)
    {
        MaxId++;
        ChunkCounter++;
        ProcessedFileSize += chunk.Length;
        
        string? sourcesId = POptions.IncludeFiles ? SourcesId : null;
        var sourcesStatus = POptions.IncludeFiles ? (int)SourcesStatus.Normal : (int)SourcesStatus.Incomplete;
        long? sourcesIndex = POptions.IncludeFiles ? ChunkCounter : null;
        long? sourcesPosition = POptions.IncludeFiles ? ProcessedFileSize : null;
        
        
        DbContext.Vectors.Add(new VectorEntity
        {
            // Id = MaxId,
            Status = (int)VectorStatus.Unprocessed,
            // VectorId = $"CHUNK:{MaxId}",
            VectorId = $"CHUNK:{MaxId}:" + GuidUtils.GetFormattedGuid(),
            Embedding = [],
            Text = chunk,
            Sources = Path.GetFileName(FilePath),
            SourcesId = sourcesId,
            SourcesStatus = sourcesStatus,
            SourcesIndex = sourcesIndex,
            SourcesPosition = sourcesPosition,
            Metadata = null,
        });
        
        // long processedSize = MaxId * chunk.Length;
        // double progress = (double)processedSize / TotalFileSize * 100;
        // FileTask.Value = Math.Min(progress, 99);
        double progress = (double)ProcessedFileSize / TotalFileSize * 100;
        FileTask.Value = Math.Min(progress, 99);
        
        DbContext.SaveChanges();
    }
}
