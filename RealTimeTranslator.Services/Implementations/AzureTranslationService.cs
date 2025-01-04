using Microsoft.Extensions.Options;
using RealTimeTranslator.Core.Configuration;
using RealTimeTranslator.Services.Interfaces;
using Azure.AI.Translation.Text;
using Azure;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace RealTimeTranslator.Services.Implementations
{
    public class AzureTranslationService : ITranslationService
    {
        private readonly TextTranslationClient _client;

        public AzureTranslationService(IOptions<AzureTranslatorConfig> config)
        {
            var credentials = new AzureKeyCredential(config.Value.Key);
            _client = new TextTranslationClient(credentials, config.Value.Region);
        }

        public async Task<string> TranslateTextAsync(string text, string fromLanguage, string toLanguage)
        {
            try 
            {
                var response = await _client.TranslateAsync(toLanguage, text, fromLanguage);
                var translation = response.Value.FirstOrDefault();
                return translation?.Translations.FirstOrDefault()?.Text ?? string.Empty;
            }
            catch (Exception ex)
            {
                // Log the error or handle it appropriately
                return $"Translation Error: {ex.Message}";
            }
        }

        public async Task<bool> ValidateApiKeyAsync()
        {
            try
            {
                var response = await _client.TranslateAsync("ja", "test");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 