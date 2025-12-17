namespace AtomUI.Controls;

public enum UploadFileStatus
{
    Error,
    Done,
    Uploading,
    Removed
}

public record UploadFileInfo
{
    public Guid Uid { get; set; }
    public int Size { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public UploadFileStatus Status { get; set; }
    public int LastModified { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public double Percent { get; set; }
}