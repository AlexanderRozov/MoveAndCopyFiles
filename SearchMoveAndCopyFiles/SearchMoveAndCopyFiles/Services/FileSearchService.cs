using FileFinderApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace FileFinderApp.Services
{
public class FileSearchService
{
public async Task<IEnumerable<FileItem>> SearchAsync(string root, string pattern, IProgress<int> progress, CancellationToken token)
{
var result = new List<FileItem>();
var extensions = pattern.Split(';').Select(p => p.Trim('*', '.')).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();


if (!Directory.Exists(root)) return result;


var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories);
var count = files.Count();
int processed = 0;


foreach (var f in files)
{
token.ThrowIfCancellationRequested();
var ext = Path.GetExtension(f).TrimStart('.');
if (extensions.Length == 0 || extensions.Contains(ext))
result.Add(FileItem.FromPath(f));
processed++;
progress?.Report((int)(processed * 100f / count));
await Task.Yield();
}
return result;
}
}
}