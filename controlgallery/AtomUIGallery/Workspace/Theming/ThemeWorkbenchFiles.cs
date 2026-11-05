using System.Text;
using Avalonia.Platform.Storage;

namespace AtomUIGallery.Workspace.Theming;

internal static class ThemeWorkbenchFiles
{
    private const int MaxInputBytes = 1024 * 1024;
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

    internal static async Task<string> ReadAsync(IStorageFile file, CancellationToken cancellationToken)
    {
        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream, StrictUtf8, true);
        var buffer = new char[4096];
        var text = new StringBuilder();
        while (await reader.ReadAsync(buffer.AsMemory(), cancellationToken) is var count && count != 0)
        {
            text.Append(buffer, 0, count);
            if (text.Length > MaxInputBytes)
            {
                throw new InvalidDataException("The recipe exceeds the one MiB input limit.");
            }
        }
        var result = text.ToString();
        if (StrictUtf8.GetByteCount(result) > MaxInputBytes)
        {
            throw new InvalidDataException("The recipe exceeds the one MiB UTF-8 input limit.");
        }
        return result;
    }

    internal static async Task WriteAsync(IStorageFile file, string json, CancellationToken cancellationToken)
    {
        var bytes = StrictUtf8.GetBytes(json);
        await using var stream = await file.OpenWriteAsync();
        if (stream.CanSeek)
        {
            stream.SetLength(0);
        }
        await stream.WriteAsync(bytes, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }
}
