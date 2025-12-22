namespace AtomUI.Controls;

public record UploadFileInfo
{
    public long Size { get; init; }
    public string Name { get; init; } = string.Empty;
    public Uri FilePath { get; init; }
    public DateTimeOffset? DateCreated { get; init; }
    public DateTimeOffset? DateModified { get; init; }

    public UploadFileInfo(string name, Uri filePath, long size, DateTimeOffset? dateCreated = null, DateTimeOffset? dateModified = null)
    {
        Name = name;
        FilePath = filePath;
        Size = size;
        DateCreated = dateCreated;
        DateModified = dateModified;
    }
}