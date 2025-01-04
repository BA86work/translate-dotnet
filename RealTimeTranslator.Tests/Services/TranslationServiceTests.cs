using Xunit;
using Moq;
using System.Threading.Tasks;
using RealTimeTranslator.Services.Interfaces;
using RealTimeTranslator.Services.Implementations;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RealTimeTranslator.Data;
using Microsoft.Extensions.Options;
using RealTimeTranslator.Core.Configuration;

namespace RealTimeTranslator.Tests.Services
{
    public class TranslationServiceTests : IDisposable
    {
        private Mock<ITranslationService> _translationServiceMock;
        private Mock<ICommunityTranslationService> _communityServiceMock;
        private Mock<ITranslationCacheService> _cacheServiceMock;
        private TranslatorDbContext _dbContext;

        public TranslationServiceTests()
        {
            _translationServiceMock = new Mock<ITranslationService>();
            _communityServiceMock = new Mock<ICommunityTranslationService>();
            _cacheServiceMock = new Mock<ITranslationCacheService>();

            var options = new DbContextOptionsBuilder<TranslatorDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _dbContext = new TranslatorDbContext(options);
        }

        [Fact]
        public async Task TranslateText_WhenCached_ReturnsCachedTranslation()
        {
            // Arrange
            const string sourceText = "Hello";
            const string cachedTranslation = "こんにちは";
            
            var service = new TranslationCacheService(_dbContext);
            await service.CacheTranslationAsync(sourceText, cachedTranslation, "en", "ja");

            // Act
            var result = await service.GetCachedTranslationAsync(sourceText, "en", "ja");

            // Assert
            Assert.Equal(cachedTranslation, result);
        }

        [Fact(Skip = "Requires Azure Translator API Key")]
        public async Task AzureTranslator_TranslatesTextCorrectly()
        {
            // Arrange
            var apiKey = Environment.GetEnvironmentVariable("AZURE_TRANSLATOR_KEY");
            var region = Environment.GetEnvironmentVariable("AZURE_TRANSLATOR_REGION");
            
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(region))
            {
                throw new Exception("Azure Translator API Key and Region are required for this test");
            }

            var config = Options.Create(new AzureTranslatorConfig 
            { 
                Key = apiKey,
                Region = region
            });
            var service = new AzureTranslationService(config);
            const string sourceText = "Hello";

            // Act
            var result = await service.TranslateTextAsync(sourceText, "en", "ja");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.NotEqual(sourceText, result);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
} 