using System.ComponentModel;
using System.Text;
using ConsoleMCP.Options;
using LibraryAI.Tools;
using LibraryAI.Vector;
using McpDotNet.Server;

namespace ConsoleMCP.Services;

[McpToolType]
public static class VectorSearchService
{
    
    public static VectorDbContext? Database = null;
    public static VectorMcpOptions? CommandOptions = null;
    public static bool IsInitialized = false;
    
    [McpTool, Description("Search the vector database.")]
    public static string Search(string data)
    {
        VectorDbContext.Init();
        if (!IsInitialized || Database == null || CommandOptions == null)
        {
            return "[TIPS] Vector database is not initialized.";
        }
        Database.EnsureCreated();

        if (data.Length == 0)
        {
            return "[TIPS] Input is empty! ";
        }
        
        var pendingVectors = Database.Vectors.Where(v => v.Status == (int)VectorStatus.Normal).ToList();
        if (!pendingVectors.Any())
        {
            return "[TIPS] Vector database has no pending vectors to process! ";
        }

        try
        {
            var embedding = VectorTools.Generate(
                CommandOptions.OpenAiApiEndpoint,
                CommandOptions.ApiKey,
                data,
                CommandOptions.EmbeddingModel,
                null,
                false,
                CommandOptions.Compatibility
            );
            if (embedding.Item1 == null)
            {
                return "[ERROR] Failed to generate vectors! ";
            }

            var searchCounter = 0;
            var findCounter = 0;
            var startTime = DateTime.Now;
            var resultIds = new Dictionary<long, double>();
            var normalVectors = Database.Vectors.Where(x => x.Status == (int)VectorStatus.Normal);
            var totalVectors = normalVectors.Count();
            foreach (var vector in normalVectors.AsEnumerable())
            {
                searchCounter++;
                var distance = VectorComparer.CosineSimilarity(embedding.Item1, vector.Embedding);
                if (distance > CommandOptions.MatchThreshold)
                {
                    resultIds.Add(vector.Id, distance);
                    findCounter++;
                }
            }

            if (resultIds.Count == 0)
            {
                return "[TIPS] No results found! ";
            }

            var filterTime = DateTime.Now;
            var sortedDict = resultIds.OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);
            if (sortedDict.Count > CommandOptions.TopN)
            {
                sortedDict = sortedDict.Take(CommandOptions.TopN).ToDictionary(x => x.Key, x => x.Value);
            }

            StringBuilder resultBuilder = new StringBuilder();
            resultBuilder.AppendLine(
                $"[SYSTEM] Search completed! " +
                $"Found {findCounter} results in {searchCounter} vectors in {(filterTime - startTime).Seconds} seconds."
            );
            int textCounter = 0;
            foreach (var id in sortedDict)
            {
                textCounter++;
                var vector = Database.Vectors.Find(id.Key);
                if (vector == null)
                {
                    continue;
                }

                resultBuilder.AppendLine("---------------------------");
                resultBuilder.AppendLine($"@Id : {textCounter}");
                resultBuilder.AppendLine($"@Title : {vector.Sources ?? "(Unknown)"}");
                resultBuilder.AppendLine($"@Distance : {vector.Embedding.Length} / {id.Value.ToString("F")}");
                resultBuilder.AppendLine($"@Text : \n{vector.Text}");
            }

            return resultBuilder.ToString();
        }
        catch (Exception e)
        {
            return "[ERROR] Vector database unable to search! ";
        }
    }
}