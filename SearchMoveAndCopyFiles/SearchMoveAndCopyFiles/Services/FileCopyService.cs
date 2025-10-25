using SearchMoveAndCopyFiles.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace SearchMoveAndCopyFiles.Services
{
    public class FileCopyService
    {
        public async Task CopyOrMoveAsync(IEnumerable<FileItem> files, string destination, bool preserveStructure, bool move, IProgress<int> progress, CancellationToken token)
        {
            var fileList = files.ToList();
            var total = fileList.Count;
            int processed = 0;

            // Создаем папку назначения если она не существует
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            foreach (var file in fileList)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    var targetPath = GetTargetPath(file, destination, preserveStructure);
                    
                    // Создаем папку назначения если нужно
                    var targetDirectory = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    // Выполняем операцию
                    if (move)
                    {
                        if (File.Exists(targetPath))
                        {
                            File.Delete(targetPath);
                        }
                        File.Move(file.FullPath, targetPath);
                    }
                    else
                    {
                        File.Copy(file.FullPath, targetPath, true);
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но продолжаем обработку других файлов
                    System.Diagnostics.Debug.WriteLine($"Ошибка при обработке файла {file.FullPath}: {ex.Message}");
                    throw new InvalidOperationException($"Ошибка при обработке файла {file.Name}: {ex.Message}", ex);
                }

                processed++;
                progress?.Report((int)(processed * 100f / total));
                await Task.Yield();
            }
        }

        private static string GetTargetPath(FileItem file, string destination, bool preserveStructure)
        {
            if (!preserveStructure)
            {
                return Path.Combine(destination, file.Name);
            }

            // Сохраняем структуру папок
            var rootPath = Path.GetPathRoot(file.FullPath);
            var relativePath = Path.GetDirectoryName(file.FullPath)?.Replace(rootPath, "").TrimStart(Path.DirectorySeparatorChar);
            
            if (string.IsNullOrEmpty(relativePath))
            {
                return Path.Combine(destination, file.Name);
            }

            return Path.Combine(destination, relativePath, file.Name);
        }
    }
}