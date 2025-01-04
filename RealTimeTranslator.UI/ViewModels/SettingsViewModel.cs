using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RealTimeTranslator.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RealTimeTranslator.UI.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ITranslationService _translationService;
        private readonly IOcrService _ocrService;

        private string _apiKey = string.Empty;
        public string ApiKey
        {
            get => _apiKey;
            set => SetProperty(ref _apiKey, value);
        }

        private string _region = string.Empty;
        public string Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
        }

        private string _selectedSourceLanguage = string.Empty;
        public string SelectedSourceLanguage
        {
            get => _selectedSourceLanguage;
            set => SetProperty(ref _selectedSourceLanguage, value);
        }

        private string _selectedTargetLanguage = string.Empty;
        public string SelectedTargetLanguage
        {
            get => _selectedTargetLanguage;
            set => SetProperty(ref _selectedTargetLanguage, value);
        }

        public ObservableCollection<string> AvailableLanguages { get; } = new();

        private ICommand? _saveCommand;
        public ICommand SaveCommand => _saveCommand ??= new RelayCommand(Save);

        private void Save()
        {
            // TODO: Save settings
        }

        public SettingsViewModel(ITranslationService translationService, IOcrService ocrService)
        {
            _translationService = translationService;
            _ocrService = ocrService;

            // Add common languages
            AvailableLanguages.Add("en");
            AvailableLanguages.Add("ja");
            AvailableLanguages.Add("ko");
            AvailableLanguages.Add("zh-Hans");
            AvailableLanguages.Add("th");
        }
    }
} 