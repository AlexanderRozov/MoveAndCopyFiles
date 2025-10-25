using FileFinderApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace FileFinderApp.Services
{
public class FileCopyService
{
public async Task CopyOrMoveAsync(IEnumerable<FileItem> files, string destination, bool preserveStructure, bool move, IProgress<int> progress, CancellationToken token)
{
var total = 0;
foreach (var f in files) total++;
int processed = 0;


foreach (var file in files)
{
token.ThrowIfCancellationRequested();
var dest = preserveStructure ? Path.Combine(destination, Path.GetDirectoryName(file.FullPath).Replace(Path.GetPathRoot(file.FullPath), "")) : destination;
Directory.CreateDirectory(dest);
var target = Path.Combine(dest, file.Name);


if (move) File.Move(file.FullPath, target, true);
else File.Copy(file.FullPath, target, true);


processed++;
progress?.Report((int)(processed * 100f / total));
await Task.Yield();
}
}
}
}