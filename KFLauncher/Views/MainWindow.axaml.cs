using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
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
            // the star and connect buttons live in rows of their own, double clicking one of those
            // is two clicks on that button, not a join
            if (e.Source is Visual source && source.FindAncestorOfType<Button>(true) is not null)
            {
                return;
            }

            if (this.DataContext is MainWindowViewModel model && sender is DataGrid { SelectedItem: ServerInfo server })
            {
                model.ConnectCommand.Execute(server);
            }
        }
    }
}
