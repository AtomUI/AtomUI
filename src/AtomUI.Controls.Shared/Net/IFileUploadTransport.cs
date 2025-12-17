namespace AtomUI.Controls;

public record FileUploadProgress
{
    public long BytesSent { get; set; }
    public long TotalBytes { get; set; }
    public double Percentage => TotalBytes > 0 ? (BytesSent * 100.0) / TotalBytes : 0;
}

public interface IFileUploadTransport
{
    Task<FileUploadResult> UploadAsync(
        UploadFileInfo fileInfo,
        object? context = null,
        IProgress<FileUploadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}