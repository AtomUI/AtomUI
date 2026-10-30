using Avalonia.Platform.Storage;

namespace AtomUI.Desktop.Controls;

internal sealed class UploadInputCandidate
{
    internal IStorageFile StorageFile { get; }
    internal string Name { get; }
    internal Uri? Path { get; }

    internal UploadInputCandidate(IStorageFile storageFile)
    {
        ArgumentNullException.ThrowIfNull(storageFile);
        StorageFile = storageFile;
        Name         = storageFile.Name;
        Path         = storageFile.Path;
    }
}

internal sealed record UploadStorageEnumerationResult(
    IReadOnlyList<UploadInputCandidate> Candidates,
    IReadOnlyList<UploadRejectedItem> RejectedItems);
