using Azure;
using Azure.AI.Translation.Text;
using RealTimeTranslator.Services.Interfaces;
using System.Threading.Tasks;

namespace RealTimeTranslator.Services.Implementations
{
    public class AzureTranslationService : ITranslationService
    {
        private readonly TextTranslationClient _client;

        public AzureTranslationService(string key, string region)
        {
            var credential = new AzureKeyCredential(key);
            _client = new TextTranslationClient(credential, region);
        }

        public async Task<string> TranslateTextAsync(string text, string fromLanguage, string toLanguage)
        {
            var response = await _client.TranslateAsync(toLanguage, text, fromLanguage);
            return response.Value[0].Translations[0].Text;
        }

        public async Task<bool> ValidateApiKeyAsync()
        {
            try
            {
                await _client.GetLanguagesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 