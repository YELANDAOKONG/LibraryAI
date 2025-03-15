using ConsoleAI.Options;
using LibraryAI.Tools;
using LibraryAI.Vector;
using Spectre.Console;

namespace ConsoleAI.Handler;

public class SearchHandler
{
    public static int RunHandler(SearchOptions options)
    {
        VectorDbContext.Init();
        var db = VectorDbContextFactory.Create(options.DatabaseFile);
        db.EnsureCreated();
        
        AnsiConsole.Write(new Rule("[yellow]Input Summary[/]").LeftJustified());
        AnsiConsole.MarkupLine($"[green]* API Endpoint : [white]{Markup.Escape(options.OpenAiApiEndpoint)}[/][/]");
        AnsiConsole.MarkupLine($"[blue]* Model : [white]{Markup.Escape(options.EmbeddingModel)}[/][/]");
        AnsiConsole.MarkupLine($"[#FFC0CB]* Compatibility : [white]{Markup.Escape(options.Compatibility.ToString())}[/][/]");

        string input = "";
        if (!options.MultilinesMode)
        {
            input = AnsiConsole.Ask<string>("[bold]=> Enter your query :[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold]=> Enter your query (enter \"{options.MultilinesEndString.EscapeMarkup()}\" to finish) :[/]");
            while (true)
            {
                var data = AnsiConsole.Ask<string>("");
                if (data.Trim().StartsWith(options.MultilinesEndString))
                {
                    break;
                }
                input += data + "\n";
                // AnsiConsole.MarkupLine($"Input {data.Length}");
            }
        }
        
        if (string.IsNullOrEmpty(input))
        {
            AnsiConsole.MarkupLine("\n[yellow][[!]] No input provided.[/]");
            return 0;
        }
        
        AnsiConsole.MarkupLine("\n[bold green][[%]] Generating vectors...[/]");
        var embedding = VectorTools.Generate(
            options.OpenAiApiEndpoint,
            options.ApiKey,
            input,
            options.EmbeddingModel,
            null,
            true,
            options.Compatibility
        );
        if (embedding.Item1 == null)
        {
            AnsiConsole.MarkupLine("[yellow][[!]] Failed to generate vectors.[/]");
            return 0;
        }
        AnsiConsole.MarkupLine($"[bold green][[*]] Vector processing completed: {embedding.Item1.Length.ToString()}[/]");
        AnsiConsole.MarkupLine("[bold green][[%]] Searching...[/]");

        var searchCounter = 0;
        var findCounter = 0;
        var startTime = DateTime.Now;
        var resultIds = new Dictionary<long, double>();
        var normalVectors = db.Vectors.Where(x => x.Status == (int)VectorStatus.Normal);
        // foreach (var vector in normalVectors.AsEnumerable())
        // {
        //     searchCounter++;
        //     var distance = VectorComparer.CosineSimilarity(embedding.Item1, vector.Embedding);
        //     if (distance > options.MatchThreshold)
        //     {
        //         resultIds.Add(vector.Id, distance);
        //         findCounter++;
        //     }
        //     // if (resultIds.Count >= options.TopN)
        //     // {
        //     //     break;
        //     // }
        // }
        var totalVectors = normalVectors.Count();
        var progress = AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false);
        progress.Start(ctx =>
        {
            var task = ctx.AddTask("[green]Processing vectors[/]", maxValue: totalVectors);
            foreach (var vector in normalVectors.AsEnumerable())
            {
                searchCounter++;
                var distance = VectorComparer.CosineSimilarity(embedding.Item1, vector.Embedding);
                if (distance > options.MatchThreshold)
                {
                    resultIds.Add(vector.Id, distance);
                    findCounter++;
                }
                task.Increment(1);
                // if (resultIds.Count >= options.TopN)
                // {
                //     break;
                // }
            }
        });
        
        if (resultIds.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow][[!]] No results found.[/]");
            return 0;
        }
        
        AnsiConsole.MarkupLine($"[bold green][[*]] Search completed: {resultIds.Count.ToString()}[/]");
        AnsiConsole.MarkupLine($"[bold green][[*]] Searched {searchCounter.ToString()} chunks in {(DateTime.Now - startTime).TotalSeconds.ToString("F")} seconds, find {findCounter.ToString()} results.[/]");
        
        
        var filterTime = DateTime.Now;
        var sortedDict = resultIds.OrderByDescending(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);
        if (sortedDict.Count > options.TopN)
        {
            sortedDict = sortedDict.Take(options.TopN).ToDictionary(x => x.Key, x => x.Value);
        }
        AnsiConsole.MarkupLine($"[bold blue][[*]] Filtered {sortedDict.Count.ToString()} chunks in {(DateTime.Now - filterTime).TotalSeconds.ToString("F")} seconds.[/]");
        foreach (var id in sortedDict)
        {
            var vector = db.Vectors.Find(id.Key);
            if (vector == null)
            {
                continue;
            }
            var distance = VectorComparer.CosineSimilarity(embedding.Item1, vector.Embedding);
            AnsiConsole.MarkupLine($"[bold yellow][[^]] Title : {Markup.Escape(vector.Sources ?? "(Unknown)")}[/]");
            AnsiConsole.MarkupLine($"[bold yellow][[^]] Distance : {vector.Embedding.Length} / {id.Value.ToString("F")}[/]");
            AnsiConsole.MarkupLine($"[bold yellow][[^]] Text : [/]");
            AnsiConsole.MarkupLine($"[white]{Markup.Escape(vector.Text)}[/]");
        }
        
        AnsiConsole.MarkupLine("\n[bold green][[%]] Search completed.[/]");
        return 0;
    }
}