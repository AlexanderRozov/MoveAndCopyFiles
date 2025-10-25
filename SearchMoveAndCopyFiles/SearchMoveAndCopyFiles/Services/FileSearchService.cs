using SearchMoveAndCopyFiles.Models;
using System.IO;



namespace SearchMoveAndCopyFiles.Services
{
    public class FileSearchService
    {
        public async Task<IEnumerable<FileItem>> SearchAsync(string root, string pattern, IProgress<int> progress, CancellationToken token)
        {
            var result = new List<FileItem>();
            
            if (!Directory.Exists(root)) return result;

            // Поддержка различных форматов паттернов
            var searchPatterns = ParseSearchPattern(pattern);
            
            var allFiles = new List<string>();
            
            // Если паттерн содержит маски файлов (например, *.txt, test*.*)
            if (searchPatterns.Any(p => p.Contains("*")))
            {
                foreach (var searchPattern in searchPatterns)
                {
                    var files = Directory.EnumerateFiles(root, searchPattern, SearchOption.AllDirectories);
                    allFiles.AddRange(files);
                }
                allFiles = allFiles.Distinct().ToList();
            }
            else
            {
                // Поиск по расширениям
                var extensions = searchPatterns.Select(p => p.Trim('*', '.')).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories);
                
                foreach (var file in files)
                {
                    token.ThrowIfCancellationRequested();
                    var ext = Path.GetExtension(file).TrimStart('.').ToLowerInvariant();
                    if (extensions.Length == 0 || extensions.Any(e => e.ToLowerInvariant() == ext))
                    {
                        allFiles.Add(file);
                    }
                }
            }

            int processed = 0;
            int total = allFiles.Count;

            foreach (var file in allFiles)
            {
                token.ThrowIfCancellationRequested();
                result.Add(FileItem.FromPath(file));
                processed++;
                progress?.Report((int)(processed * 100f / total));
                await Task.Yield();
            }

            return result;
        }

        private static List<string> ParseSearchPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return new List<string> { "*.*" };

            // Разделяем по различным разделителям
            var patterns = pattern.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => p.Trim())
                                 .Where(p => !string.IsNullOrWhiteSpace(p))
                                 .ToList();

            // Если паттерн не содержит точку, добавляем её как расширение
            for (int i = 0; i < patterns.Count; i++)
            {
                if (!patterns[i].Contains(".") && !patterns[i].Contains("*"))
                {
                    patterns[i] = "*." + patterns[i];
                }
            }

            return patterns;
        }
    }
}