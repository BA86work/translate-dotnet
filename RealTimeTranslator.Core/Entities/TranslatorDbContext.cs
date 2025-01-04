using Microsoft.EntityFrameworkCore;

namespace RealTimeTranslator.Core.Entities
{
    public class TranslatorDbContext : DbContext
    {
        public TranslatorDbContext(DbContextOptions<TranslatorDbContext> options)
            : base(options)
        {
        }

        public DbSet<CommunityTranslation> CommunityTranslations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CommunityTranslation>()
                .HasIndex(t => new { t.SourceText, t.SourceLanguage, t.TargetLanguage });
        }
    }
} 