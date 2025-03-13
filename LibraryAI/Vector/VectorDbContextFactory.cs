using Microsoft.EntityFrameworkCore;

namespace LibraryAI.Vector;

public class VectorDbContextFactory
{
    public static VectorDbContext Create(string dbPath)
    {
        var options = new DbContextOptionsBuilder<VectorDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        return new VectorDbContext(options);
    }
    
    public static void Init()
    {
        VectorDbContext.Init();
    }

    public static void EnsureCreated(VectorDbContext dbContext)
    {
        dbContext.EnsureCreated();
    }
}