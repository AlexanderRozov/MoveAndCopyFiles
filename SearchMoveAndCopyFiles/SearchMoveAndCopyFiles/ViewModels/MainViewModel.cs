using FileFinderApp.Helpers;
using FileFinderApp.Models;
using FileFinderApp.Services;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinForms = System.Windows.Forms;
using System.Windows.Input;

namespace FileFinderApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly FileSearchService _searchService = new();
        private readonly FileCopyService _copyService = new();
        private CancellationTokenSource _cts;

        public ObservableCollection<FileItem> FoundFiles { get; } = new();
        private string _rootDirectory;
        public string RootDirectory
        {
            get => _rootDirectory;
            set => SetProperty(ref _rootDirectory, value);
        }

        private string _searchPattern = "*.txt;*.png, *.pdf, *.doc, *.docx, *.dwg, *.jpg, *.jpeg";
        public string SearchPattern
        {
            get => _searchPattern;
            set => SetProperty(ref _searchPattern, value);
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private bool _preserveStructure = true;
        public bool PreserveStructure
        {
            get => _preserveStructure;
            set => SetProperty(ref _preserveStructure, value);
        }

        public ICommand BrowseCommand => new RelayCommand(_ => BrowseFolder());
        public ICommand SearchCommand => new RelayCommand(async _ => await SearchAsync().ConfigureAwait(false));
        public ICommand CancelCommand => new RelayCommand(_ => _cts?.Cancel());
        public ICommand CopyCommand => new RelayCommand(async _ => await CopyMoveAsync(false));
        public ICommand MoveCommand => new RelayCommand(async _ => await CopyMoveAsync(true));

        private void BrowseFolder()
        {
            var folderDialog = new OpenFolderDialog
            {
                // Set options here
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;
                // Do something with the result
            }
        }

        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(RootDirectory)) return;

            FoundFiles.Clear();
            _cts = new();
            var progress = new Progress<int>(v => Progress = v);

            try
            {
                var files = await _searchService.SearchAsync(RootDirectory, SearchPattern, progress, _cts.Token);
                foreach (var f in files)
                    FoundFiles.Add(f);
            }
            catch (TaskCanceledException)
            {
                // Поиск отменён пользователем
            }
        }

        private async Task CopyMoveAsync(bool move)
        {
            if (FoundFiles.Count == 0) return;

            var folderDialog = new OpenFolderDialog
            {
                // Set options here
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;
                // Do something with the result
            }

            var dest = folderDialog.DefaultDirectory;
            _cts = new();
            var progress = new Progress<int>(v => Progress = v);

            try
            {
                await _copyService.CopyOrMoveAsync(FoundFiles, dest, PreserveStructure, move, progress, _cts.Token);
            }
            catch (TaskCanceledException)
            {
                // Операция прервана пользователем
            }
        }
    }
}