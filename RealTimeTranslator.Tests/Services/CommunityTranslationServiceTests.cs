using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RealTimeTranslator.Core.Entities;
using RealTimeTranslator.Services.Interfaces;
using RealTimeTranslator.Services.Implementations;
using RealTimeTranslator.Data;

namespace RealTimeTranslator.Tests.Services
{
    public class CommunityTranslationServiceTests
    {
        private RealTimeTranslator.Data.TranslatorDbContext _dbContext;
        private ICommunityTranslationService _service;

        public CommunityTranslationServiceTests()
        {
            var options = new DbContextOptionsBuilder<RealTimeTranslator.Data.TranslatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new RealTimeTranslator.Data.TranslatorDbContext(options);
            _service = new CommunityTranslationService(_dbContext);
        }

        [Fact]
        public async Task SubmitTranslation_SavesTranslationToDatabase()
        {
            // Arrange
            const string sourceText = "Hello";
            const string translatedText = "こんにちは";
            const string userId = "user1";

            // Act
            await _service.SubmitTranslationAsync(sourceText, translatedText, "en", "ja", userId);

            // Assert
            var savedTranslation = await _dbContext.CommunityTranslations
                .FirstOrDefaultAsync(t => t.SourceText == sourceText);

            Assert.NotNull(savedTranslation);
            Assert.Equal(translatedText, savedTranslation.TranslatedText);
            Assert.Equal(userId, savedTranslation.CreatedBy);
        }
    }
} 