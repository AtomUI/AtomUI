using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace AtomUI.Desktop.Controls;

public class UploadFilesDroppedEventArgs : RoutedEventArgs
{
    public IReadOnlyList<IStorageFile> Files { get; }

    public UploadFilesDroppedEventArgs(IReadOnlyList<IStorageFile> files)
    {
        Files = files;
    }
}