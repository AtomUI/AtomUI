using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.Net;

public class UploadFileInfoTests
{
    [Fact]
    public void Constructor_Preserves_Stream_Source_And_Optional_Metadata()
    {
        var source = new TrackingUploadFileSource();
        var path = new Uri("content://documents/report.pdf");
        var created = new DateTimeOffset(2026, 8, 1, 10, 20, 30, TimeSpan.Zero);
        var modified = created.AddHours(2);

        var file = new UploadFileInfo(
            "report.pdf",
            source,
            path,
            42,
            "application/pdf",
            created,
            modified);

        file.Name.ShouldBe("report.pdf");
        file.Source.ShouldBeSameAs(source);
        file.Path.ShouldBe(path);
        file.Size.ShouldBe(42);
        file.ContentType.ShouldBe("application/pdf");
        file.DateCreated.ShouldBe(created);
        file.DateModified.ShouldBe(modified);
    }

    [Fact]
    public void Constructor_Allows_Path_And_Size_To_Be_Unknown()
    {
        var source = new TrackingUploadFileSource();

        var file = new UploadFileInfo("clipboard.bin", source);

        file.Path.ShouldBeNull();
        file.Size.ShouldBeNull();
        file.ContentType.ShouldBeNull();
        file.DateCreated.ShouldBeNull();
        file.DateModified.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_Rejects_Empty_File_Name(string name)
    {
        Should.Throw<ArgumentException>(() => new UploadFileInfo(name, new TrackingUploadFileSource()));
    }

    [Fact]
    public void Constructor_Rejects_Null_Source()
    {
        Should.Throw<ArgumentNullException>(() => new UploadFileInfo("file.bin", null!));
    }

    [Fact]
    public async Task Source_Receives_The_Caller_Cancellation_Token()
    {
        var source = new TrackingUploadFileSource();
        using var cancellationTokenSource = new CancellationTokenSource();

        await using var stream = await source.OpenReadAsync(cancellationTokenSource.Token);

        source.LastCancellationToken.ShouldBe(cancellationTokenSource.Token);
        stream.CanRead.ShouldBeTrue();
    }

    private sealed class TrackingUploadFileSource : IUploadFileSource
    {
        public CancellationToken LastCancellationToken { get; private set; }

        public ValueTask<Stream> OpenReadAsync(CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return ValueTask.FromResult<Stream>(new MemoryStream([1, 2, 3], writable: false));
        }
    }
}
