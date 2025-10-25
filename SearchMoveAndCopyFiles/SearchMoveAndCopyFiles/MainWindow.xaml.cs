using FileFinderApp.ViewModels;
using System.Windows;


namespace FileFinderApp
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