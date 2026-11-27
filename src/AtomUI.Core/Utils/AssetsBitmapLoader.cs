using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AtomUI.Utils;

/// <summary>
/// Opens assets and decodes images from a path that denotes either a local file or an Avalonia asset.
/// </summary>
public static class AssetsLoader
{
    /// <summary>
    /// Decodes the image at <paramref name="path" /> into a bitmap.
    /// </summary>
    /// <param name="path">
    /// Any value accepted by <see cref="OpenStream" />.
    /// </param>
    /// <returns>
    /// The decoded bitmap. The caller is responsible for disposing it.
    /// </returns>
    public static Bitmap LoadBitmap(string path)
    {
        return new Bitmap(OpenStream(path));
    }

    /// <summary>
    /// Opens a stream for reading on an asset: on the local file when the path resolves to one, and otherwise
    /// through the Avalonia <see cref="IAssetLoader" />.
    /// </summary>
    /// <param name="path">
    /// One of the following: a file URI, a rooted path, a path relative to the current working directory, or an
    /// Avalonia asset URI such as <c>avares://Assembly/Assets/logo.png</c>. A relative path names a file, never an
    /// Avalonia asset.
    /// </param>
    /// <returns>
    /// The open stream. The caller is responsible for disposing it.
    /// </returns>
    public static Stream OpenStream(string path)
    {
        if (TryResolveLocalFilePath(path, out var localFilePath))
        {
            return new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        var assets = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();
        return assets.Open(new Uri(path, UriKind.RelativeOrAbsolute));
    }

    internal static bool TryResolveLocalFilePath(string path, out string localFilePath)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            if (uri.IsFile)
            {
                localFilePath = uri.LocalPath;
                return true;
            }

            localFilePath = string.Empty;
            return false;
        }

        if (Path.IsPathFullyQualified(path) || Path.IsPathRooted(path))
        {
            localFilePath = Path.GetFullPath(path);
            return true;
        }

        if (!path.StartsWith("avares:", StringComparison.OrdinalIgnoreCase))
        {
            localFilePath = Path.GetFullPath(path);
            return true;
        }

        localFilePath = string.Empty;
        return false;
    }
}
