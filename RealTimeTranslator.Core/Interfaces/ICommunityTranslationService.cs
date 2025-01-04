using System.Threading.Tasks;

namespace RealTimeTranslator.Core.Interfaces
{
    public interface ICommunityTranslationService
    {
        Task SubmitTranslationAsync(string sourceText, string translatedText, string sourceLanguage, string targetLanguage, string userId);
        Task<string> GetCommunityTranslationAsync(string sourceText, string sourceLanguage, string targetLanguage);
    }
} 