using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using System.Linq;
using KFLauncher.Models;
using KFLauncher.ViewModels;

namespace KFLauncher.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // the data context is handed to us after the constructor, so the saved window goes on
            // when it opens, and comes back off on the way out
            this.Opened += (_, _) => this.Remembered(restore: true);
            this.Closing += (_, _) => this.Remembered(restore: false);
        }

        private void Remembered(bool restore)
        {
            if (this.DataContext is not MainWindowViewModel model)
            {
                return;
            }

            if (restore)
            {
                this.Width = model.Config.WindowWidth;
                this.Height = model.Config.WindowHeight;
                this.RestoreColumns(model.Config.ServerColumns);

                return;
            }

            model.Config.WindowWidth = this.Bounds.Width;
            model.Config.WindowHeight = this.Bounds.Height;
            model.Config.ServerColumns = string.Join(",", this.ServerGrid.Columns.Select(
                column => column.Width.IsStar ? "*" : ((int)column.ActualWidth).ToString()));
        }

        private void RestoreColumns(string saved)
        {
            string[] widths = saved.Split(',');
            if (widths.Length != this.ServerGrid.Columns.Count)
            {
                return;
            }

            for (int i = 0; i < widths.Length; i++)
            {
                if (widths[i] == "*")
                {
                    this.ServerGrid.Columns[i].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                }
                else if (double.TryParse(widths[i], out double width) && width >= 16)
                {
                    this.ServerGrid.Columns[i].Width = new DataGridLength(width);
                }
            }
        }

        /// <summary>The grid does the sorting, we only write down which way it was left.</summary>
        private void OnServerSorting(object? sender, DataGridColumnEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel model && e.Column.SortMemberPath is { Length: > 0 } path)
            {
                model.RememberSort(path);
            }
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
