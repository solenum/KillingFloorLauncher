using Avalonia.Controls;
using Avalonia.Input;
using KFLauncher.Models;
using KFLauncher.ViewModels;

namespace KFLauncher.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>Double clicking a row joins it, the way every other server browser behaves.</summary>
        private void OnServerDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel model && sender is DataGrid { SelectedItem: ServerInfo server })
            {
                model.ConnectCommand.Execute(server);
            }
        }
    }
}
