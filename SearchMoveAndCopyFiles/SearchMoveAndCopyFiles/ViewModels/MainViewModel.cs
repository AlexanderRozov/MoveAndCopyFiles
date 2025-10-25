using SearchMoveAndCopyFiles.Helpers;
using SearchMoveAndCopyFiles.Models;
using SearchMoveAndCopyFiles.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SearchMoveAndCopyFiles.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly FileSearchService _searchService = new();
        private readonly FileCopyService _copyService = new();
        private CancellationTokenSource? _cts;

        public MainViewModel()
        {
            FoundFiles.CollectionChanged += (s, e) => RaiseCommandsCanExecuteChanged();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = SettingsManager.Settings;
            RootDirectory = settings.LastSearchDirectory;
            SearchPattern = settings.SearchPattern;
            PreserveStructure = settings.PreserveStructure;
        }

        private void SaveSettings()
        {
            var settings = SettingsManager.Settings;
            settings.LastSearchDirectory = RootDirectory;
            settings.SearchPattern = SearchPattern;
            settings.PreserveStructure = PreserveStructure;
            SettingsManager.SaveSettings();
        }

        private ObservableCollection<FileItem> _foundFiles = new();
        public ObservableCollection<FileItem> FoundFiles 
        { 
            get => _foundFiles;
            private set 
            { 
                SetProperty(ref _foundFiles, value);
                RaiseCommandsCanExecuteChanged();
            }
        }
        private string? _rootDirectory;
        public string? RootDirectory
        {
            get => _rootDirectory;
            set 
            { 
                SetProperty(ref _rootDirectory, value);
                RaiseCommandsCanExecuteChanged();
                SaveSettings();
            }
        }

        private string _searchPattern = "*.txt;*.png, *.pdf, *.doc, *.docx, *.dwg, *.jpg, *.jpeg";
        public string SearchPattern
        {
            get => _searchPattern;
            set 
            { 
                SetProperty(ref _searchPattern, value);
                SaveSettings();
            }
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private void RaiseCommandsCanExecuteChanged()
        {
            _searchCommand?.RaiseCanExecuteChanged();
            _cancelCommand?.RaiseCanExecuteChanged();
            _copyCommand?.RaiseCanExecuteChanged();
            _moveCommand?.RaiseCanExecuteChanged();
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set 
            { 
                SetProperty(ref _isSearching, value);
                RaiseCommandsCanExecuteChanged();
            }
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set 
            { 
                SetProperty(ref _isProcessing, value);
                RaiseCommandsCanExecuteChanged();
            }
        }

        private bool _preserveStructure = true;
        public bool PreserveStructure
        {
            get => _preserveStructure;
            set 
            { 
                SetProperty(ref _preserveStructure, value);
                SaveSettings();
            }
        }

        private string _statusMessage = "Готов к работе";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private RelayCommand? _searchCommand;
        private RelayCommand? _cancelCommand;
        private RelayCommand? _copyCommand;
        private RelayCommand? _moveCommand;

        public ICommand BrowseCommand => new RelayCommand(_ => BrowseFolder());
        
        public ICommand SearchCommand => _searchCommand ??= new RelayCommand(
            _ => _ = SearchAsync(), 
            _ => !IsSearching && !string.IsNullOrWhiteSpace(RootDirectory));
            
        public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(
            _ => _cts?.Cancel(), 
            _ => IsSearching || IsProcessing);
            
        public ICommand CopyCommand => _copyCommand ??= new RelayCommand(
            _ => _ = CopyMoveAsync(false), 
            _ => !IsProcessing && FoundFiles.Count > 0);
            
        public ICommand MoveCommand => _moveCommand ??= new RelayCommand(
            _ => _ = CopyMoveAsync(true), 
            _ => !IsProcessing && FoundFiles.Count > 0);

        private void BrowseFolder()
        {
            var folderDialog = new OpenFolderDialog
            {
                Title = "Выберите папку для поиска файлов"
            };

            if (folderDialog.ShowDialog() == true)
            {
                RootDirectory = folderDialog.FolderName;
            }
        }

        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(RootDirectory))
            {
                StatusMessage = "Выберите папку для поиска";
                return;
            }

            if (!Directory.Exists(RootDirectory))
            {
                StatusMessage = "Выбранная папка не существует";
                return;
            }

            FoundFiles.Clear();
            IsSearching = true;
            StatusMessage = "Поиск файлов...";
            Progress = 0;
            _cts = new();
            var progress = new Progress<int>(v => Progress = v);

            try
            {
                var files = await _searchService.SearchAsync(RootDirectory, SearchPattern, progress, _cts.Token);
                foreach (var f in files)
                    FoundFiles.Add(f);
                
                StatusMessage = $"Всего найдено файлов: {FoundFiles.Count}";
                
                // Сохраняем настройки после успешного поиска
                SaveSettings();
            }
            catch (TaskCanceledException)
            {
                StatusMessage = "Поиск отменён пользователем";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при поиске: {ex.Message}";
                System.Windows.MessageBox.Show($"Ошибка при поиске файлов: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsSearching = false;
            }
        }

        private async Task CopyMoveAsync(bool move)
        {
            if (FoundFiles.Count == 0) return;

            var folderDialog = new OpenFolderDialog
            {
                Title = move ? "Выберите папку для перемещения файлов" : "Выберите папку для копирования файлов"
            };

            if (folderDialog.ShowDialog() != true) return;

            var destination = folderDialog.FolderName;
            IsProcessing = true;
            StatusMessage = move ? "Перемещение файлов..." : "Копирование файлов...";
            Progress = 0;
            _cts = new();
            var progress = new Progress<int>(v => Progress = v);

            try
            {
                await _copyService.CopyOrMoveAsync(FoundFiles, destination, PreserveStructure, move, progress, _cts.Token);
                StatusMessage = move ? $"Перемещено файлов: {FoundFiles.Count}" : $"Скопировано файлов: {FoundFiles.Count}";
                
                if (move)
                {
                    FoundFiles.Clear();
                }
            }
            catch (TaskCanceledException)
            {
                StatusMessage = "Операция прервана пользователем";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при выполнении операции: {ex.Message}";
                System.Windows.MessageBox.Show($"Ошибка при выполнении операции: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}