using System.Threading.Tasks;

namespace RealTimeTranslator.Services.Interfaces
{
    public interface ITranslationService
    {
        Task<string> TranslateTextAsync(string text, string fromLanguage, string toLanguage);
        Task<bool> ValidateApiKeyAsync();
    }
} 