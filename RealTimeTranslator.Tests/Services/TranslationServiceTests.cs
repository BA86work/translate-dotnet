using Xunit;
using Moq;
using System.Threading.Tasks;
using RealTimeTranslator.Services.Interfaces;
using RealTimeTranslator.Services.Implementations;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RealTimeTranslator.Data;

namespace RealTimeTranslator.Tests.Services
{
    public class TranslationServiceTests : IDisposable
    {
        private Mock<ITranslationService> _translationServiceMock;
        private Mock<ICommunityTranslationService> _communityServiceMock;
        private Mock<ITranslationCacheService> _cacheServiceMock;
        private ILogger<AzureTranslationService> _logger;
        private TranslatorDbContext _dbContext;

        public TranslationServiceTests()
        {
            _translationServiceMock = new Mock<ITranslationService>();
            _communityServiceMock = new Mock<ICommunityTranslationService>();
            _cacheServiceMock = new Mock<ITranslationCacheService>();
            _logger = new Mock<ILogger<AzureTranslationService>>().Object;

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

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
} 