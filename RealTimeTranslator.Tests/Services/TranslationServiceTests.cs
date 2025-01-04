using Xunit;
using Moq;
using System.Threading.Tasks;
using RealTimeTranslator.Services.Interfaces;
using RealTimeTranslator.Services.Implementations;
using Microsoft.Extensions.Logging;

namespace RealTimeTranslator.Tests.Services
{
    public class TranslationServiceTests
    {
        private Mock<ITranslationService> _translationServiceMock;
        private Mock<ICommunityTranslationService> _communityServiceMock;
        private Mock<ITranslationCacheService> _cacheServiceMock;
        private ILogger<AzureTranslationService> _logger;

        public TranslationServiceTests()
        {
            _translationServiceMock = new Mock<ITranslationService>();
            _communityServiceMock = new Mock<ICommunityTranslationService>();
            _cacheServiceMock = new Mock<ITranslationCacheService>();
            _logger = new Mock<ILogger<AzureTranslationService>>().Object;
        }

        [Fact]
        public async Task TranslateText_WhenCached_ReturnsCachedTranslation()
        {
            // Arrange
            const string sourceText = "Hello";
            const string cachedTranslation = "こんにちは";
            
            _cacheServiceMock.Setup(x => x.GetCachedTranslationAsync(
                sourceText, "en", "ja")).ReturnsAsync(cachedTranslation);

            var service = new AzureTranslationService(
                "test-key",
                "test-region");

            // Act
            var result = await service.TranslateTextAsync(sourceText, "en", "ja");

            // Assert
            Assert.Equal(cachedTranslation, result);
            _translationServiceMock.Verify(
                x => x.TranslateTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), 
                Times.Never);
        }
    }
} 