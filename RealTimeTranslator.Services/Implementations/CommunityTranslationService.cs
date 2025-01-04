using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealTimeTranslator.Services.Interfaces;
using RealTimeTranslator.Data.Entities;
using RealTimeTranslator.Data;

namespace RealTimeTranslator.Services.Implementations;

public class CommunityTranslationService : ICommunityTranslationService
{
    private readonly TranslatorDbContext _dbContext;

    public CommunityTranslationService(TranslatorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommunityTranslation> GetCommunityTranslationAsync(string text, string fromLanguage, string toLanguage)
    {
        return await _dbContext.CommunityTranslations
            .FirstOrDefaultAsync(t => t.SourceText == text 
                && t.SourceLanguage == fromLanguage 
                && t.TargetLanguage == toLanguage);
    }

    public async Task<IEnumerable<CommunityTranslation>> GetTopTranslationsAsync(string text, string fromLanguage, string toLanguage)
    {
        return await _dbContext.CommunityTranslations
            .Where(t => t.SourceText == text 
                && t.SourceLanguage == fromLanguage 
                && t.TargetLanguage == toLanguage)
            .OrderByDescending(t => t.Votes)
            .Take(5)
            .ToListAsync();
    }

    public async Task SubmitTranslationAsync(string sourceText, string translatedText, string fromLanguage, string toLanguage, string userId)
    {
        var translation = new CommunityTranslation
        {
            SourceText = sourceText,
            TranslatedText = translatedText,
            SourceLanguage = fromLanguage,
            TargetLanguage = toLanguage,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Votes = 0
        };

        _dbContext.CommunityTranslations.Add(translation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task VoteTranslationAsync(int translationId, string userId, bool isUpvote)
    {
        var translation = await _dbContext.CommunityTranslations.FindAsync(translationId);
        if (translation == null)
            throw new ArgumentException("Translation not found", nameof(translationId));

        translation.Votes += isUpvote ? 1 : -1;
        translation.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
} 