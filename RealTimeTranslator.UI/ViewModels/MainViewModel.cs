using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RealTimeTranslator.Services.Interfaces;

namespace RealTimeTranslator.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITranslationService _translationService;
        private readonly IScreenCaptureService _screenCaptureService;
        private string _sourceText = string.Empty;
        private string _translatedText = string.Empty;
        private string _selectedSourceLanguage = "en";
        private string _selectedTargetLanguage = "th";
        private string _statusMessage = "Ready";

        public MainViewModel(
            ITranslationService translationService,
            IScreenCaptureService screenCaptureService)
        {
            _translationService = translationService;
            _screenCaptureService = screenCaptureService;

            // Initialize commands
            SwapLanguagesCommand = new RelayCommand(SwapLanguages);
            CaptureScreenCommand = new RelayCommand(CaptureScreen);

            // Initialize language lists
            SourceLanguages = new ObservableCollection<string> { "en", "th", "ja", "ko", "zh" };
            TargetLanguages = new ObservableCollection<string> { "th", "en", "ja", "ko", "zh" };
        }

        public ObservableCollection<string> SourceLanguages { get; }
        public ObservableCollection<string> TargetLanguages { get; }

        public string SourceText
        {
            get => _sourceText;
            set
            {
                if (_sourceText != value)
                {
                    _sourceText = value;
                    OnPropertyChanged();
                    TranslateText();
                }
            }
        }

        public string TranslatedText
        {
            get => _translatedText;
            private set
            {
                if (_translatedText != value)
                {
                    _translatedText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedSourceLanguage
        {
            get => _selectedSourceLanguage;
            set
            {
                if (_selectedSourceLanguage != value)
                {
                    _selectedSourceLanguage = value;
                    OnPropertyChanged();
                    TranslateText();
                }
            }
        }

        public string SelectedTargetLanguage
        {
            get => _selectedTargetLanguage;
            set
            {
                if (_selectedTargetLanguage != value)
                {
                    _selectedTargetLanguage = value;
                    OnPropertyChanged();
                    TranslateText();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SwapLanguagesCommand { get; }
        public ICommand CaptureScreenCommand { get; }

        private async void TranslateText()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SourceText))
                {
                    TranslatedText = string.Empty;
                    return;
                }

                StatusMessage = "Translating...";
                TranslatedText = await _translationService.TranslateTextAsync(
                    SourceText,
                    SelectedSourceLanguage,
                    SelectedTargetLanguage);
                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Translation error: {ex.Message}";
            }
        }

        private void SwapLanguages()
        {
            var temp = SelectedSourceLanguage;
            SelectedSourceLanguage = SelectedTargetLanguage;
            SelectedTargetLanguage = temp;
        }

        private async void CaptureScreen()
        {
            try
            {
                StatusMessage = "Capturing screen...";
                var text = await _screenCaptureService.CaptureAndRecognizeTextAsync();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    SourceText = text;
                }
                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Screen capture error: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();
    }
} 