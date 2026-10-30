using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUIGallery.ShowCases.Upload;

public partial class UploadShowCase : GalleryReactiveUserControl<UploadViewModel>
{
    public const string LanguageId = nameof(UploadShowCase);

    private WindowMessageManager? _messageManager;

    public UploadShowCase()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (DataContext is UploadViewModel viewModel)
            {
                RefreshLocalizedFiles(viewModel);

                var languageManager = GalleryLocalization.GetLanguageManager();
                if (languageManager != null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshLocalizedFiles(viewModel);
                    languageManager.LanguageChanged += handler;
                    Disposable.Create(() => languageManager.LanguageChanged -= handler)
                              .DisposeWith(disposables);
                }

                Disposable.Create(() =>
                {
                    viewModel.DefaultFiles          = null;
                    viewModel.PicturesWallFiles     = null;
                    viewModel.PictureCircleFiles    = null;
                    viewModel.PictureListStyleFiles = null;
                    viewModel.ScrollableUploadFiles = null;
                }).DisposeWith(disposables);
            }
        });
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _messageManager?.Dispose();
        _messageManager = null;
    }

    private void RefreshLocalizedFiles(UploadViewModel viewModel)
    {
        InitDefaultFiles(viewModel);
        InitPictureWallFiles(viewModel);
        InitPictureCircleFiles(viewModel);
        InitPictureListStyleFiles(viewModel);
        InitScrollableFiles(viewModel);
    }

    private void InitDefaultFiles(UploadViewModel viewModel)
    {
        viewModel.DefaultFiles =
        [
            new UploadFileItem
            {
                Name        = "xxx.png",
                IsImageFile = true,
                Status      = FileUploadStatus.Uploading,
                Progress    = 33
            },
            new UploadFileItem
            {
                Name        = "yyy.png",
                IsImageFile = true,
                Status      = FileUploadStatus.Success,
                Progress    = 100
            },
            new UploadFileItem
            {
                Name         = "zzz.png",
                IsImageFile  = true,
                Status       = FileUploadStatus.Failed,
                ErrorMessage = UploadShowCaseLanguage.Get(
                    UploadShowCaseLangResourceKind.P2ErrorServer500,
                    "Server Error 500")
            },
        ];
    }

    private void InitPictureWallFiles(UploadViewModel uploadViewModel)
    {
        uploadViewModel.PicturesWallFiles =
        [
            CreatePictureFile(FileUploadStatus.Success),
            CreatePictureFile(FileUploadStatus.Success),
            CreatePictureFile(FileUploadStatus.Success),
            CreatePictureFile(FileUploadStatus.Success),
            new UploadFileItem
            {
                Name        = "image.png",
                IsImageFile = true,
                Status      = FileUploadStatus.Uploading,
                Progress    = 50
            },
            new UploadFileItem
            {
                Name         = "image.png",
                IsImageFile  = true,
                Status       = FileUploadStatus.Failed,
                ErrorMessage = UploadShowCaseLanguage.Get(
                    UploadShowCaseLangResourceKind.P2ErrorUpload,
                    "Upload error!")
            },
        ];
    }

    private void InitPictureCircleFiles(UploadViewModel uploadViewModel)
    {
        uploadViewModel.PictureCircleFiles =
        [
            CreatePictureFile(FileUploadStatus.Success),
            CreatePictureFile(FileUploadStatus.Success),
            CreatePictureFile(FileUploadStatus.Success)
        ];
    }

    private void InitPictureListStyleFiles(UploadViewModel uploadViewModel)
    {
        uploadViewModel.PictureListStyleFiles =
        [
            new UploadFileItem
            {
                Name        = "xxx.png",
                IsImageFile = true,
                Status      = FileUploadStatus.Uploading,
                Progress    = 33
            },
            CreatePictureFile(FileUploadStatus.Success, "yyy.png"),
            new UploadFileItem
            {
                Name         = "zzz.png",
                IsImageFile  = true,
                Status       = FileUploadStatus.Failed,
                ErrorMessage = UploadShowCaseLanguage.Get(
                    UploadShowCaseLangResourceKind.P2ErrorUpload,
                    "Upload error!")
            },
        ];
    }

    private void InitScrollableFiles(UploadViewModel uploadViewModel)
    {
        uploadViewModel.ScrollableUploadFiles =
        [
            CreateTextFile("design-spec.pdf", FileUploadStatus.Success),
            CreateTextFile("avatar.png", FileUploadStatus.Success),
            CreateTextFile("release-notes.md", FileUploadStatus.Uploading, 68),
            CreateTextFile("large-video.mov", FileUploadStatus.Failed, errorMessage: UploadShowCaseLanguage.Get(
                UploadShowCaseLangResourceKind.P2ErrorServer500,
                "Server Error 500")),
            CreateTextFile("contract.docx", FileUploadStatus.Success),
            CreateTextFile("screenshot.jpg", FileUploadStatus.Success),
            CreateTextFile("archive.zip", FileUploadStatus.Uploading, 41),
            CreateTextFile("data.csv", FileUploadStatus.Success)
        ];
    }

    private static UploadFileItem CreatePictureFile(FileUploadStatus status, string name = "image.png")
    {
        return new UploadFileItem
        {
            Name        = name,
            IsImageFile = true,
            Status      = status,
            Progress    = status == FileUploadStatus.Success ? 100 : 0,
            Path        = new Uri("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png")
        };
    }

    private static UploadFileItem CreateTextFile(
        string name,
        FileUploadStatus status,
        double progress = 100,
        string? errorMessage = null)
    {
        return new UploadFileItem
        {
            Name         = name,
            Status       = status,
            Progress     = progress,
            Size         = 1024 * 128,
            ErrorMessage = errorMessage
        };
    }

    private void HandleImageUploadAboutToScheduling(object? sender, UploadTaskAboutToSchedulingEventArgs e)
    {
        var fileInfo          = e.UploadFileInfo;
        var ext               = Path.GetExtension(fileInfo.Name);
        var isAllowedFileType = ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                ext.Equals(".png", StringComparison.OrdinalIgnoreCase);
        if (!isAllowedFileType)
        {
            e.Result       = UploadPredicateResult.CancelWithInTaskList;
            e.CancelReason = UploadShowCaseLanguage.Get(
                UploadShowCaseLangResourceKind.P2CancelJpgPngOnly,
                "You can only upload JPG/PNG file!");
            return;
        }

        var isLt2M = fileInfo.Size is null or < 2 * 1024 * 1024;
        if (!isLt2M)
        {
            e.Result       = UploadPredicateResult.CancelWithInTaskList;
            e.CancelReason = UploadShowCaseLanguage.Get(
                UploadShowCaseLangResourceKind.P2CancelImageSize,
                "Image must be smaller than 2MB!");
        }
    }

    private void HandlePngUploadAboutToScheduling(object? sender, UploadTaskAboutToSchedulingEventArgs e)
    {
        var fileInfo = e.UploadFileInfo;
        var ext      = Path.GetExtension(fileInfo.Name);
        if (!ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            e.Result       = UploadPredicateResult.Cancel;
            e.CancelReason = UploadShowCaseLanguage.Get(
                UploadShowCaseLangResourceKind.P2CancelPngOnly,
                "You can only upload PNG file!");
        }
    }

    private void HandleUploadFailed(object? sender, UploadTaskFailedEventArgs e)
    {
        var errorMsg = e.Result.UserFriendlyMessage;
        GetMessageManager()?.Show(new AtomUIMessage(
            type: MessageType.Error,
            content: $"{errorMsg}"
        ));
    }

    private void HandleUploadCompleted(object? sender, UploadTaskCompletedEventArgs e)
    {
        GetMessageManager()?.Show(new AtomUIMessage(
            type: MessageType.Success,
            content: UploadShowCaseLanguage.Format(
                UploadShowCaseLangResourceKind.P2UploadSuccessFormat,
                "{0} uploaded successfully!",
                e.UploadFileInfo.Name)
        ));
    }

    private WindowMessageManager? GetMessageManager()
    {
        if (_messageManager is not null)
        {
            return _messageManager;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return null;
        }

        _messageManager = new WindowMessageManager(topLevel)
        {
            MaxItems = 10
        };
        return _messageManager;
    }
}

internal static class UploadShowCaseLanguage
{
    public static string Get(UploadShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }

    public static string Format(UploadShowCaseLangResourceKind resourceKind, string fallback, params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }
}

public class UploadMockTransport : IFileUploadTransport
{
    public async Task<FileUploadResult> UploadAsync(
        UploadFileInfo fileInfo,
        object? context = null,
        IProgress<FileUploadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var totalBytes  = fileInfo.Size ?? 0;
        var bytesSent   = 0L;
        var elapsedTime = TimeSpan.Zero;
        try
        {
            var random        = Random.Shared.Next(1, 10);
            var isServerError = random % 3 == 0;
            var cycle         = 0;
            while (!cancellationToken.IsCancellationRequested && bytesSent < totalBytes)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                var delay = TimeSpan.FromMilliseconds(Random.Shared.Next(300, 1000));
                await Task.Delay(delay, cancellationToken);
                elapsedTime += delay;
                var minChunk = Math.Max(1, totalBytes / 20);
                var maxChunk = Math.Max(minChunk + 1, totalBytes / 10 + 1);
                bytesSent += Random.Shared.NextInt64(minChunk, maxChunk);
                bytesSent = Math.Min(bytesSent, totalBytes);
                var uploadProgress = new FileUploadProgress()
                {
                    TotalBytes = totalBytes,
                    BytesSent  = bytesSent,
                };
                progress?.Report(uploadProgress);
                if (isServerError && cycle > 2)
                {
                    return FileUploadResult.FailureResult(FileUploadErrorCode.ServerError, "Max number of elements reached for this resource!");
                }

                ++cycle;
            }

            return FileUploadResult.SuccessResult(
                fileInfo.Path ?? new Uri($"file:///{Uri.EscapeDataString(fileInfo.Name)}"),
                totalBytes,
                elapsedTime,
                "Success");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return FileUploadResult.FailureResult(FileUploadErrorCode.Unknown, "Upload failed: " + ex.Message);
        }
    }
}
