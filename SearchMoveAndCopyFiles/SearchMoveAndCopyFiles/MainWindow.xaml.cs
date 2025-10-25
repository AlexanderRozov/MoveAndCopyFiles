using SearchMoveAndCopyFiles.ViewModels;
using SearchMoveAndCopyFiles.Helpers;
using System.Windows;


namespace SearchMoveAndCopyFiles
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            LoadWindowSettings();
        }

        private void LoadWindowSettings()
        {
            var settings = SettingsManager.Settings;
            Width = settings.WindowWidth;
            Height = settings.WindowHeight;
            Left = settings.WindowLeft;
            Top = settings.WindowTop;
            WindowState = settings.WindowState;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SettingsManager.UpdateWindowSettings(this);
            base.OnClosing(e);
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                var dataGrid = sender as System.Windows.Controls.DataGrid;
                if (dataGrid != null)
                {
                    viewModel.SelectedFiles = dataGrid.SelectedItems;
                }
            }
        }
    }
}