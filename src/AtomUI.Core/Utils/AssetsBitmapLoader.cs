using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AtomUI.Utils;

public static class AssetsBitmapLoader
{
    public static Bitmap Load(string path)
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
            return new Bitmap(uri.LocalPath);
        }
        var assets = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();
        return new Bitmap(assets.Open(uri));
    }
}