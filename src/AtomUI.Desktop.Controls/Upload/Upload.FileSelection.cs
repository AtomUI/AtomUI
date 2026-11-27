using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AtomUI.Desktop.Controls;

public partial class Upload
{
    internal IUploadStorageProviderAdapter? StorageProviderAdapter { get; set; }

    /// <summary>
    /// Opens the system file picker and feeds the picked files into the upload. The picker uses
    /// <see cref="AllowedFileTypes" /> as its filter and offers several files only when
    /// <see cref="IsMultipleEnabled" /> is <see langword="true" />. Nothing happens when the platform storage
    /// provider is missing or cannot open files.
    /// </summary>
    /// <param name="cancellationToken">
    /// Passed to the picker and to the processing of the picked files.
    /// </param>
    /// <remarks>
    /// The task completes when the input pipeline has processed the picked files; it does not wait for the
    /// uploads to finish.
    /// </remarks>
    public async Task SelectFilesAsync(CancellationToken cancellationToken = default)
    {
        var storageProvider = ResolveStorageProviderAdapter();
        if (storageProvider is null || !storageProvider.CanOpenFiles)
        {
            return;
        }

        var files = await storageProvider.OpenFilesAsync(new FilePickerOpenOptions
        {
            AllowMultiple = IsMultipleEnabled,
            FileTypeFilter = AllowedFileTypes
        }, cancellationToken).ConfigureAwait(false);

        await ProcessStorageItemsAsync(
            UploadInputSource.FilePicker,
            files.Cast<IStorageItem>().ToArray(),
            UploadDirectoryDropMode.Reject,
            0,
            10_000,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens the system folder picker and feeds the files found directly inside the picked folders into the
    /// upload. The picker offers several folders only when <see cref="IsMultipleEnabled" /> is
    /// <see langword="true" />. Nested folders are not read, and at most 10,000 items are enumerated. Nothing
    /// happens when the platform storage provider is missing or cannot open folders.
    /// </summary>
    /// <param name="cancellationToken">
    /// Passed to the picker and to the processing of the picked folders.
    /// </param>
    /// <remarks>
    /// The task completes when the input pipeline has processed the picked folders; it does not wait for the
    /// uploads to finish.
    /// </remarks>
    public async Task SelectDirectoriesAsync(CancellationToken cancellationToken = default)
    {
        var storageProvider = ResolveStorageProviderAdapter();
        if (storageProvider is null || !storageProvider.CanOpenFolders)
        {
            return;
        }

        var folders = await storageProvider.OpenFoldersAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = IsMultipleEnabled
        }, cancellationToken).ConfigureAwait(false);

        await ProcessStorageItemsAsync(
            UploadInputSource.DirectoryPicker,
            folders.Cast<IStorageItem>().ToArray(),
            UploadDirectoryDropMode.TopLevelFiles,
            0,
            10_000,
            cancellationToken).ConfigureAwait(false);
    }

    private IUploadStorageProviderAdapter? ResolveStorageProviderAdapter()
    {
        if (StorageProviderAdapter is not null)
        {
            return StorageProviderAdapter;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        return topLevel is null
            ? null
            : new AvaloniaUploadStorageProviderAdapter(topLevel.StorageProvider);
    }
}
