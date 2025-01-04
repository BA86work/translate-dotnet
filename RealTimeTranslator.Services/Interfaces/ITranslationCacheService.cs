using System.Threading.Tasks;

namespace RealTimeTranslator.Services.Interfaces;

public interface ITranslationCacheService
{
    Task<string> GetCachedTranslationAsync(string text, string fromLanguage, string toLanguage);
    Task CacheTranslationAsync(string sourceText, string translatedText, string fromLanguage, string toLanguage);
    Task CleanupOldCacheEntriesAsync(int daysOld);
} 