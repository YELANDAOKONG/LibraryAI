using ConsoleAI.Options;
using LibraryAI.Tools;
using LibraryAI.Vector;
using OpenAI.Embeddings;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleAI.Handler;

public class VectorHandler
{
    private class ProcessedCountColumn : ProgressColumn
    {
        public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
        {
            return new Markup($"[silver]{task.Value}[/]/[dim]{task.MaxValue}[/]");
        }
    }
    
    public static int RunHandler(VectorOptions options)
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
                foreach (var vector in pendingVectors)
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
                        vector.Embedding = embedding.Item1;
                        vector.Status = (int)VectorStatus.Normal;
                        db.SaveChanges();
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red][[!]] Failed to process vector {Markup.Escape(vector.Id.ToString())}![/]");
                        // AnsiConsole.MarkupLine($"[red][[!]] Error : {embedding.Item2?.Error?.Message}[/]");
                    }
                    mainTask.Increment(1);
                }
            });
    
        AnsiConsole.MarkupLine("\n[bold green][[%]] Vector processing completed![/]");
        return 0;
    }
    
    
}