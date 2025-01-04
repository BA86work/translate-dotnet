using System.Collections.Generic;
using System.Threading.Tasks;
using RealTimeTranslator.Data.Entities;

namespace RealTimeTranslator.Services.Interfaces;

public interface ICommunityTranslationService
{
    Task<CommunityTranslation> GetCommunityTranslationAsync(string text, string fromLanguage, string toLanguage);
    Task<IEnumerable<CommunityTranslation>> GetTopTranslationsAsync(string text, string fromLanguage, string toLanguage);
    Task SubmitTranslationAsync(string sourceText, string translatedText, string fromLanguage, string toLanguage, string userId);
    Task VoteTranslationAsync(int translationId, string userId, bool isUpvote);
} 