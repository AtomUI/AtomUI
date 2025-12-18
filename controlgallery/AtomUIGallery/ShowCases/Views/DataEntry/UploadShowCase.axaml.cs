using AtomUI.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class UploadShowCase : ReactiveUserControl<UploadViewModel>
{
    public UploadShowCase()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
        BasicUpload.UploadTransport = new UploadMockTransport();
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
        var totalBytes = fileInfo.Size;
        var bytesSent  = 0ul;
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
                bytesSent += (ulong)(totalBytes *
                                     ((double)Random.Shared.NextInt64((long)totalBytes / 20, (long)totalBytes / 10) /
                                      totalBytes));
                bytesSent = Math.Min(bytesSent, totalBytes);
                var uploadProgress = new FileUploadProgress()
                {
                    TotalBytes = fileInfo.Size,
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
                fileInfo.FilePath,
                fileInfo.Size,
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