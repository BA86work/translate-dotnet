using Microsoft.EntityFrameworkCore;
using RealTimeTranslator.Data.Entities;

namespace RealTimeTranslator.Data;

public class TranslatorDbContext : DbContext
{
    public DbSet<CommunityTranslation> CommunityTranslations { get; set; }
    public DbSet<TranslationCache> TranslationCaches { get; set; }

    public TranslatorDbContext(DbContextOptions<TranslatorDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CommunityTranslation>()
            .HasIndex(ct => new { ct.SourceText, ct.SourceLanguage, ct.TargetLanguage })
            .IsUnique();

        modelBuilder.Entity<TranslationCache>()
            .HasIndex(tc => new { tc.SourceText, tc.FromLanguage, tc.ToLanguage })
            .IsUnique();
    }
} 