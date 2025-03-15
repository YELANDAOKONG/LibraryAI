using System.Collections.Concurrent;
using ConsoleAI.Options;
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
        
        // 使用线程安全的字典存储处理结果
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
                // 设置最大并行度
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = options.MaxDegreeOfParallelism };
                
                Parallel.ForEach(pendingVectors, parallelOptions, vector =>
                {
                    var text = vector.Text;
                    var embedding = VectorTools.Generate(
                        options.OpenAiApiEndpoint,
                        options.ApiKey,
                        text,
                        options.EmbeddingModel,
                        null,
                        true,
                        options.Compatibility
                    );
                    
                    if (embedding.Item1 != null)
                    {
                        embeddingsDict.TryAdd(vector.Id, embedding.Item1);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red][[!]] Failed to process vector {vector.Id}![/]");
                    }
                    
                    mainTask.Increment(1); // 进度条线程安全
                });
                // 批量更新数据库
                foreach (var vector in pendingVectors)
                {
                    if (embeddingsDict.TryGetValue(vector.Id, out var embedding))
                    {
                        vector.Embedding = embedding;
                        vector.Status = (int)VectorStatus.Normal;
                    }
                }
                db.SaveChanges();
            });
    
        AnsiConsole.MarkupLine("\n[bold green][[%]] Vector processing completed![/]");
        return 0;
    }
}