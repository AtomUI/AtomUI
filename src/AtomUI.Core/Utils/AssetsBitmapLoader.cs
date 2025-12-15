using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AtomUI.Utils;

public static class AssetsLoader
{
    public static Bitmap LoadBitmap(string path)
    {
        return new Bitmap(OpenStream(path));
    }

    public static Stream OpenStream(string path)
    {
        Uri? uri = null;
        if (path.StartsWith("avares:"))
        {
            uri = new Uri(path);
        }
        else
        {
            uri = path.StartsWith("/")
                ? new Uri(path, UriKind.Relative)
                : new Uri(path, UriKind.RelativeOrAbsolute);
        }
        if (uri.IsAbsoluteUri && uri.IsFile)
        {
            return new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read);
        }
        var assets = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();
        return assets.Open(uri);
    }
}