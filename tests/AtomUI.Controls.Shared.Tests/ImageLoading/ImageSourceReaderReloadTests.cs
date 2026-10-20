using Avalonia;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using System.Reflection;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class ImageSourceReaderReloadTests
{
    [Fact]
    public async Task File_Reload_Reads_The_Source_Instead_Of_Reusing_Stale_Content()
    {
        var path = Path.Combine(Path.GetTempPath(), $"atomui-image-reload-{Guid.NewGuid():N}.bin");
        var fresh = new byte[] { 4, 5, 6 };
        await File.WriteAllBytesAsync(path, fresh, TestContext.Current.CancellationToken);
        try
        {
            var options = ImageLoadingTestSupport.CreateOptions();
            var request = NormalizeReload(new FileImageSource(path), options);
            var stale = ImageLoadingTestSupport.CreateContent([1, 2, 3]) with
            {
                SourceVersion = $"{fresh.Length}:{File.GetLastWriteTimeUtc(path).Ticks}"
            };

            var result = await new FileImageSourceReader(options).ReadAsync(
                request,
                stale,
                null,
                TestContext.Current.CancellationToken);

            result.EncodedContent.ShouldNotBeNull().Bytes.ShouldBe(fresh);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Asset_Reload_Reads_The_Resource_Instead_Of_Reusing_Stale_Content()
    {
        var fresh = "fresh-asset"u8.ToArray();
        AvaloniaLocator.CurrentMutable.Bind<IAssetLoader>().ToConstant(new TestAssetLoader(fresh));
        var options = ImageLoadingTestSupport.CreateOptions();
        var request = NormalizeReload(
            new AssetImageSource(new Uri(
                "avares://AtomUI.Controls.Shared.Tests/Resources/reload-source.bin")),
            options);
        var stale = ImageLoadingTestSupport.CreateContent([1, 2, 3]) with
        {
            SourceVersion = "application-resource-v1"
        };

        var result = await new AssetImageSourceReader(options).ReadAsync(
            request,
            stale,
            null,
            TestContext.Current.CancellationToken);

        result.EncodedContent.ShouldNotBeNull().Bytes.ShouldNotBe(stale.Bytes);
        result.EncodedContent.Bytes.ShouldBe(fresh);
    }

    [Fact]
    public async Task StorageFile_Reload_Opens_The_File_Even_When_The_Version_Is_Unchanged()
    {
        var options = ImageLoadingTestSupport.CreateOptions();
        var file = new TestStorageFile([4, 5, 6]);
        var request = NormalizeReload(
            new StorageFileImageSource(file, "v1"),
            options);
        var stale = ImageLoadingTestSupport.CreateContent([1, 2, 3]) with { SourceVersion = "v1" };

        var result = await new StorageFileImageSourceReader(options).ReadAsync(
            request,
            stale,
            null,
            TestContext.Current.CancellationToken);

        file.OpenReadCount.ShouldBe(1);
        result.EncodedContent.ShouldNotBeNull().Bytes.ShouldBe(new byte[] { 4, 5, 6 });
    }

    [Fact]
    public async Task Stream_Reload_Invokes_The_Factory_Even_When_The_Version_Is_Unchanged()
    {
        var options = ImageLoadingTestSupport.CreateOptions();
        var openCount = 0;
        var source = new StreamImageSource(
            _ =>
            {
                Interlocked.Increment(ref openCount);
                return ValueTask.FromResult<Stream>(new MemoryStream([4, 5, 6]));
            },
            "stream",
            "v1");
        var request = NormalizeReload(source, options);
        var stale = ImageLoadingTestSupport.CreateContent([1, 2, 3]) with { SourceVersion = "v1" };

        var result = await new StreamImageSourceReader(options).ReadAsync(
            request,
            stale,
            null,
            TestContext.Current.CancellationToken);

        openCount.ShouldBe(1);
        result.EncodedContent.ShouldNotBeNull().Bytes.ShouldBe(new byte[] { 4, 5, 6 });
    }

    private static NormalizedImageRequest NormalizeReload(
        ImageSource source,
        ImageLoadingOptions options)
    {
        return ImageCacheKey.Normalize(
            new ImageLoadRequest(source)
            {
                Options = new ImageRequestOptions { CacheRead = ImageCacheReadPolicy.RefreshSource }
            },
            options,
            forceReload: false);
    }

    private sealed class TestStorageFile : IStorageFile
    {
        private readonly byte[] _bytes;

        internal TestStorageFile(byte[] bytes)
        {
            _bytes = bytes;
        }

        public string Name => "reload.bin";

        public Uri Path => new("file:///reload.bin");

        public bool CanBookmark => false;

        internal int OpenReadCount { get; private set; }

        public Task<StorageItemProperties> GetBasicPropertiesAsync() =>
            Task.FromResult(new StorageItemProperties((ulong)_bytes.Length));

        public Task<Stream> OpenReadAsync()
        {
            OpenReadCount++;
            return Task.FromResult<Stream>(new MemoryStream(_bytes, writable: false));
        }

        public Task<Stream> OpenWriteAsync() =>
            Task.FromResult<Stream>(new MemoryStream());

        public Task<string?> SaveBookmarkAsync() => Task.FromResult<string?>(null);

        public Task<IStorageFolder?> GetParentAsync() => Task.FromResult<IStorageFolder?>(null);

        public Task DeleteAsync() => Task.CompletedTask;

        public Task<IStorageItem?> MoveAsync(IStorageFolder destination) =>
            Task.FromResult<IStorageItem?>(null);

        public void Dispose()
        {
        }
    }

    private sealed class TestAssetLoader : IAssetLoader
    {
        private readonly byte[] _bytes;

        internal TestAssetLoader(byte[] bytes)
        {
            _bytes = bytes;
        }

        public void SetDefaultAssembly(Assembly assembly)
        {
        }

        public bool Exists(Uri uri, Uri? baseUri = null) => true;

        public Stream Open(Uri uri, Uri? baseUri = null) =>
            new MemoryStream(_bytes, writable: false);

        public (Stream stream, Assembly assembly) OpenAndGetAssembly(Uri uri, Uri? baseUri = null) =>
            (Open(uri, baseUri), typeof(TestAssetLoader).Assembly);

        public Assembly? GetAssembly(Uri uri, Uri? baseUri = null) =>
            typeof(TestAssetLoader).Assembly;

        public IEnumerable<Uri> GetAssets(Uri uri, Uri? baseUri) => [uri];

        public void InvalidateAssemblyCache(string name)
        {
        }

        public void InvalidateAssemblyCache()
        {
        }
    }
}
