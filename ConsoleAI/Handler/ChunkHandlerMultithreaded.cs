using System.Collections.Concurrent;
using ConsoleAI.Options;
using LibraryAI.Tools;
using LibraryAI.Utils;
using LibraryAI.Vector;
using Spectre.Console;

namespace ConsoleAI.Handler;

public class ChunkHandlerMultithreaded
{
    private static readonly object DbLock = new();
    
    public static int RunHandler(ChunkOptionsMultithreaded options)
    {
        VectorDbContext.Init();
        using var mainDb = VectorDbContextFactory.Create(options.DatabaseFile);
        mainDb.EnsureCreated();
        
        throw new NotImplementedException();
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
}