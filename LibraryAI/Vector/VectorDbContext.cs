// VectorDbContext.cs
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace LibraryAI.Vector;

public class VectorDbContext : DbContext
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DbSet<VectorEntity> Vectors { get; set; }
    
    public VectorDbContext() : this(":memory:") { }
    
    public VectorDbContext(string dbPath)
    {
        DbPath = dbPath;
    }

    public VectorDbContext(DbContextOptions<VectorDbContext> options) : base(options) { }


    public string DbPath { get; } = ":memory:";

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VectorEntity>()
            .Property(e => e.Embedding)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<float[]>(v, JsonOptions) ?? Array.Empty<float>()
            );
    }
}