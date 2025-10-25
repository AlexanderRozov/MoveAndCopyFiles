using SearchMoveAndCopyFiles.ViewModels;
using System.Windows;


namespace SearchMoveAndCopyFiles
{
public partial class MainWindow : Window
{
public MainWindow()
{
InitializeComponent();
DataContext = new MainViewModel();
}
}
}