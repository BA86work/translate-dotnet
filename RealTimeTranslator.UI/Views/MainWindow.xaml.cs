using RealTimeTranslator.UI.ViewModels;
using System.Windows;

namespace RealTimeTranslator.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 