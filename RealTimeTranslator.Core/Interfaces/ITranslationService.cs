using System.Threading.Tasks;

namespace RealTimeTranslator.Core.Interfaces
{
    public interface ITranslationService
    {
        Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage);
    }
} 