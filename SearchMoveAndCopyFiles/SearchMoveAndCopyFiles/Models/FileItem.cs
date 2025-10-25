using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;


namespace SearchMoveAndCopyFiles.Models
{
public class FileItem
{
public string Name { get; set; }
public string FullPath { get; set; }
public string Extension { get; set; }
public BitmapSource Icon { get; set; }


public static FileItem FromPath(string path)
{
return new FileItem
{
Name = Path.GetFileName(path),
FullPath = path,
Extension = Path.GetExtension(path),
Icon = System.Drawing.Icon.ExtractAssociatedIcon(path)?.ToImageSource()
};
}
}


public static class IconHelper
{
public static BitmapSource ToImageSource(this System.Drawing.Icon icon)
{
if (icon == null) return null;
using var bmp = icon.ToBitmap();
var hBitmap = bmp.GetHbitmap();
return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
}
}
}