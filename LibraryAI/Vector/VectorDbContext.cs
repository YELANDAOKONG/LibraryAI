// VectorDbContext.cs
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryAI.Vector;

public class VectorDbContext : DbContext
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public DbSet<VectorEntity> Vectors { get; set; }
    
    public VectorDbContext(DbContextOptions<VectorDbContext> options) : base(options) { }
    
    public VectorDbContext(string dbPath = ":memory:") : base(GetOptionsBuilder(dbPath).Options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VectorEntity>(ConfigureVectorEntity);
    }

    private static void ConfigureVectorEntity(EntityTypeBuilder<VectorEntity> builder)
    {
        builder.Property(e => e.Embedding)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<float[]>(v, JsonOptions) ?? Array.Empty<float>());
    }

    private static DbContextOptionsBuilder<VectorDbContext> GetOptionsBuilder(string dbPath)
    {
        var builder = new DbContextOptionsBuilder<VectorDbContext>();
        ApplySqliteConfiguration(builder, dbPath);
        return builder;
    }

    private static void ApplySqliteConfiguration(
        DbContextOptionsBuilder<VectorDbContext> builder, 
        string dbPath)
    {
        builder.UseSqlite($"Data Source={dbPath}", sqlOptions =>
        {
            sqlOptions.CommandTimeout(30);
        });
        
        #if RELEASE
        builder.EnableDetailedErrors(false);
        builder.EnableSensitiveDataLogging(false);
        #endif
    }

    public void EnsureCreated()
    {
        this.Database.EnsureCreated();
    }

    public static void Init()
    {
        SQLitePCL.Batteries_V2.Init();
    }
}