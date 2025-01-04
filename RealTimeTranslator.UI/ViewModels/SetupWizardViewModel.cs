using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RealTimeTranslator.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RealTimeTranslator.UI.ViewModels;

public class SetupWizardViewModel : ViewModelBase
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

    private ICommand? _validateApiKeyCommand;
    public ICommand ValidateApiKeyCommand => _validateApiKeyCommand ??= new RelayCommand(async () => await ValidateApiKey());

    private ICommand? _completeSetupCommand;
    public ICommand CompleteSetupCommand => _completeSetupCommand ??= new RelayCommand(async () => await CompleteSetup());

    private async Task ValidateApiKey()
    {
        IsBusy = true;
        StatusMessage = "Validating API key...";

        try
        {
            var isValid = await _translationService.ValidateApiKeyAsync();
            if (isValid)
            {
                StatusMessage = "API key is valid!";
            }
            else
            {
                StatusMessage = "Invalid API key";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CompleteSetup()
    {
        // TODO: Save settings and initialize services
        await Task.CompletedTask;
    }

    public SetupWizardViewModel(ITranslationService translationService, IOcrService ocrService)
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