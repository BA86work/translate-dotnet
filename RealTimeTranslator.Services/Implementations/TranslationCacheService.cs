using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealTimeTranslator.Services.Interfaces;
using RealTimeTranslator.Data.Entities;
using RealTimeTranslator.Data;

namespace RealTimeTranslator.Services.Implementations;

public class TranslationCacheService : ITranslationCacheService
{
    private readonly TranslatorDbContext _dbContext;

    public TranslationCacheService(TranslatorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GetCachedTranslationAsync(string text, string fromLanguage, string toLanguage)
    {
        var cached = await _dbContext.TranslationCaches
            .FirstOrDefaultAsync(t => t.SourceText == text 
                && t.FromLanguage == fromLanguage 
                && t.ToLanguage == toLanguage
                && (!t.ExpiresAt.HasValue || t.ExpiresAt > DateTime.UtcNow));

        if (cached != null)
        {
            return cached.TranslatedText;
        }

        return null;
    }

    public async Task CacheTranslationAsync(string sourceText, string translatedText, string fromLanguage, string toLanguage)
    {
        var cache = new TranslationCache
        {
            SourceText = sourceText,
            TranslatedText = translatedText,
            FromLanguage = fromLanguage,
            ToLanguage = toLanguage,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30) // Cache entries expire after 30 days
        };

        _dbContext.TranslationCaches.Add(cache);
        await _dbContext.SaveChangesAsync();
    }

    public async Task CleanupOldCacheEntriesAsync(int daysOld)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldEntries = await _dbContext.TranslationCaches
            .Where(t => t.CreatedAt < cutoffDate || (t.ExpiresAt.HasValue && t.ExpiresAt < DateTime.UtcNow))
            .ToListAsync();

        _dbContext.TranslationCaches.RemoveRange(oldEntries);
        await _dbContext.SaveChangesAsync();
    }
} 