using System.Collections.Concurrent;
using ConsoleAI.Options;
using LibraryAI.Core;
using LibraryAI.Tools;
using LibraryAI.Vector;
using OpenAI.Embeddings;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleAI.Handler;

public class VectorHandlerMultithreaded
{
    private class ProcessedCountColumn : ProgressColumn
    {
        public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
        {
            return new Markup($"[silver]{task.Value}[/]/[dim]{task.MaxValue}[/]");
        }
    }
    
    public static int RunHandler(VectorOptionsMultithreaded options)
    {
        VectorDbContext.Init();
        var db = VectorDbContextFactory.Create(options.DatabaseFile);
        db.EnsureCreated();
        
        AnsiConsole.Write(new Rule("[yellow]Input Summary[/]").LeftJustified());
        AnsiConsole.MarkupLine($"[green]* API Endpoint : [white]{Markup.Escape(options.OpenAiApiEndpoint)}[/][/]");
        AnsiConsole.MarkupLine($"[blue]* Model : [white]{Markup.Escape(options.EmbeddingModel)}[/][/]");
        AnsiConsole.MarkupLine($"[#FFC0CB]* Compatibility : [white]{Markup.Escape(options.Compatibility.ToString())}[/][/]");
        var pendingVectors = db.Vectors
            .Where(v => v.Status == (int)VectorStatus.Unprocessed)
            .ToList();
        if (!pendingVectors.Any())
        {
            AnsiConsole.MarkupLine("\n[yellow][[!]] No pending vectors to process.[/]");
            return 0;
        }
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]=> Starting to process {pendingVectors.Count} vector(s)...[/]");
        
        var batches = pendingVectors.Chunk(options.BatchSize);
        var embeddingsDict = new ConcurrentDictionary<long, float[]>();
        
        AnsiConsole.Progress()
            .AutoClear(true)
            .HideCompleted(false)
            .AutoRefresh(true)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new ProcessedCountColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn(),
            })
            .Start(ctx =>
            {
                var mainTask = ctx.AddTask("[deepskyblue1]Processing vectors[/]",
                    new ProgressTaskSettings { MaxValue = pendingVectors.Count });
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = options.MaxDegreeOfParallelism
                };

                Parallel.ForEach(batches, parallelOptions, batch =>
                {
                    // 准备批量数据
                    var batchTexts = batch.Select(v => v.Text).ToList();

                    // 批量生成嵌入
                    var batchResult = VectorTools.Generate(
                        options.OpenAiApiEndpoint, 
                        options.ApiKey,
                        data: batchTexts,
                        model: options.EmbeddingModel,
                        throwExceptions: true,
                        compatibility: options.Compatibility
                    );
                    if (batchResult.Item1?.Count == batch.Length)
                    {
                        // 关联结果与原始数据
                        for (int i = 0; i < batch.Length; i++)
                        {
                            var vector = batch[i];
                            var embedding = batchResult.Item1[i];
                            embeddingsDict.TryAdd(vector.Id, embedding);
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red][[!]] Batch failed (Size: {batch.Length})[/]");
                    }

                    mainTask.Increment(batch.Length);
                });
                
                // 批量更新数据库
                var vectorsToUpdate = pendingVectors
                    .Where(v => embeddingsDict.ContainsKey(v.Id))
                    .ToList();
                // db.BulkUpdate(vectorsToUpdate);
                foreach (var chunk in vectorsToUpdate.Chunk(1000))
                {
                    db.UpdateRange(chunk);
                    db.SaveChanges();
                }
            });
    
        AnsiConsole.MarkupLine("\n[bold green][[%]] Vector processing completed![/]");
        return 0;
    }
}