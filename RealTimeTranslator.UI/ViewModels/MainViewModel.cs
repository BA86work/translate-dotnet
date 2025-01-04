using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace RealTimeTranslator.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private bool _isTranslating;
        public bool IsTranslating
        {
            get => _isTranslating;
            set => SetProperty(ref _isTranslating, value);
        }

        public ICommand StartTranslationCommand { get; }
        public ICommand StopTranslationCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public MainViewModel()
        {
            StartTranslationCommand = new RelayCommand(StartTranslation, () => !IsTranslating);
            StopTranslationCommand = new RelayCommand(StopTranslation, () => IsTranslating);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
        }

        private void StartTranslation()
        {
            IsTranslating = true;
            // Implementation will be added
        }

        private void StopTranslation()
        {
            IsTranslating = false;
            // Implementation will be added
        }

        private void OpenSettings()
        {
            // Implementation will be added
        }
    }
} 