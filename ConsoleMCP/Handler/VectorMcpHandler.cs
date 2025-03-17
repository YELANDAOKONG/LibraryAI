using System.Net;
using System.Text;
using System.Text.Json;
using ConsoleMCP.Options;
using ConsoleMCP.Services;
using LibraryAI.Tools;
using LibraryAI.Vector;
using McpDotNet;
using Microsoft.Extensions.Hosting;

namespace ConsoleMCP.Handler;

public class VectorMcpHandler
{
    public static int RunHandler(VectorMcpOptions options)
    {
        VectorDbContext.Init();
        var db = VectorDbContextFactory.Create(options.DatabaseFile);
        db.EnsureCreated();
        
        VectorSearchService.Database = db;
        VectorSearchService.CommandOptions = options;
        VectorSearchService.IsInitialized = true;
        
        var builder = Host.CreateEmptyApplicationBuilder(settings: null);
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithTools();
        builder.Build().Run();
        return 0;
    }
}