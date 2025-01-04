using System.Threading.Tasks;

namespace RealTimeTranslator.Core.Interfaces
{
    public interface ITranslationCacheService
    {
        Task<string> GetCachedTranslationAsync(string sourceText, string sourceLanguage, string targetLanguage);
        Task CacheTranslationAsync(string sourceText, string translatedText, string sourceLanguage, string targetLanguage);
    }
} 