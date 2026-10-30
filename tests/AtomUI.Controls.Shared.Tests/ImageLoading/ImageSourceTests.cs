using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class ImageSourceTests
{
    [Fact]
    public void Parse_Returns_Normalized_Http_Source_And_Display_Name()
    {
        var source = ImageSource.Parse(
            "HTTPS://EXAMPLE.COM:443/assets/remote%20image.png?size=large#preview");

        var http = source.ShouldBeOfType<HttpImageSource>();
        http.DisplayName.ShouldBe("remote image.png");
        http.Uri.AbsoluteUri.ShouldBe("https://example.com/assets/remote%20image.png?size=large");
    }

    [Fact]
    public void Http_Display_Name_Uses_Meaningful_Segment_Before_Trailing_Dimensions()
    {
        ImageSource.Parse("https://picsum.photos/id/25/600/400")
            .DisplayName.ShouldBe("25");
    }

    [Fact]
    public void TryParse_Rejects_Unknown_And_Relative_Schemes()
    {
        ImageSource.TryParse("ftp://example.com/image.png", out var unknown).ShouldBeFalse();
        unknown.ShouldBeNull();
        ImageSource.TryParse("images/photo.png", out var relative).ShouldBeFalse();
        relative.ShouldBeNull();
    }

    [Theory]
    [InlineData("avares:///Assets/image.png")]
    [InlineData("avares://AtomUI.Tests/Assets/image.png?theme=dark")]
    [InlineData("avares://AtomUI.Tests/Assets/image.png#fragment")]
    public void Asset_Source_Requires_Assembly_And_Pure_Resource_Path(string value)
    {
        Should.Throw<ArgumentException>(() => new AssetImageSource(new Uri(value)));
    }

    [Fact]
    public void Bytes_Source_Defensively_Copies_And_Uses_Content_Identity()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var first = new BytesImageSource(bytes, "avatar");
        bytes[0] = 9;
        var second = new BytesImageSource(new byte[] { 1, 2, 3 }, "another name");

        first.Bytes.ToArray().ShouldBe(new byte[] { 1, 2, 3 });
        first.CacheIdentity.ShouldBe(second.CacheIdentity);
    }

    [Fact]
    public void Stream_Source_Exposes_Explicit_Identity_And_Revision_Contract()
    {
        static ValueTask<Stream> Open(CancellationToken _) =>
            ValueTask.FromResult<Stream>(new MemoryStream([1]));

        var source = new StreamImageSource(Open, "avatar", "revision-2", "avatar.png");

        source.Identity.ShouldBe("avatar");
        source.Revision.ShouldBe("revision-2");
        source.DisplayName.ShouldBe("avatar.png");
        source.CanPersistSourceSnapshot.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Stream_Source_Rejects_Empty_Identity_Or_Revision(string value)
    {
        static ValueTask<Stream> Open(CancellationToken _) =>
            ValueTask.FromResult<Stream>(new MemoryStream([1]));

        Should.Throw<ArgumentException>(() => new StreamImageSource(Open, value));
        Should.Throw<ArgumentException>(() => new StreamImageSource(Open, revision: value));
    }

    [Fact]
    public void Normalize_Disables_All_Sharing_And_Storage_For_Unpartitioned_Credentials()
    {
        var options = ImageLoadingTestSupport.CreateOptions();
        var request = new ImageLoadRequest(ImageSource.Parse("https://example.com/image.png"))
        {
            Options = new ImageRequestOptions
            {
                Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer secret" }
            }
        };

        var first = ImageCacheKey.Normalize(request, options, forceReload: false);
        var second = ImageCacheKey.Normalize(request, options, forceReload: false);

        first.CacheRead.ShouldBe(ImageCacheReadPolicy.ValidateSource);
        first.CacheStorage.ShouldBe(ImageCacheStoragePolicy.None);
        first.CanReadSharedCache.ShouldBeFalse();
        first.CanWriteMemory.ShouldBeFalse();
        first.CanPersist.ShouldBeFalse();
        first.CanShare.ShouldBeFalse();
        first.SourceOperationKey.ShareScope.ShouldNotBe(second.SourceOperationKey.ShareScope);
    }

    [Fact]
    public void Normalize_Allows_Partitioned_Credential_Request_To_Share_But_Not_Persist_By_Default()
    {
        var options = ImageLoadingTestSupport.CreateOptions();
        var normalized = ImageCacheKey.Normalize(
            new ImageLoadRequest(ImageSource.Parse("https://example.com/image.png"))
            {
                Options = new ImageRequestOptions
                {
                    CachePartition = "account:42",
                    Headers = new Dictionary<string, string> { ["Cookie"] = "session=secret" }
                }
            },
            options,
            forceReload: false);

        normalized.CacheRead.ShouldBe(ImageCacheReadPolicy.ValidateSource);
        normalized.CacheStorage.ShouldBe(ImageCacheStoragePolicy.MemoryAndDisk);
        normalized.CanShare.ShouldBeTrue();
        normalized.CanReadSharedCache.ShouldBeTrue();
        normalized.CanPersist.ShouldBeFalse();
        normalized.PartitionHash.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Normalize_Does_Not_Merge_Source_Operations_With_Different_Storage_Policies()
    {
        var options = ImageLoadingTestSupport.CreateOptions();
        var source = new StreamImageSource(
            _ => ValueTask.FromResult<Stream>(new MemoryStream([1])),
            identity: "storage-policy",
            revision: "v1");
        var noStore = ImageCacheKey.Normalize(
            new ImageLoadRequest(source)
            {
                Options = new ImageRequestOptions
                {
                    CacheStorage = ImageCacheStoragePolicy.None
                }
            },
            options,
            forceReload: false);
        var persistent = ImageCacheKey.Normalize(
            new ImageLoadRequest(source),
            options,
            forceReload: false);

        noStore.SourceKey.ShouldBe(persistent.SourceKey);
        noStore.SourceOperationKey.ShouldNotBe(persistent.SourceOperationKey);
        var decodeKey = new ImageDecodeKey(
            string.Empty,
            new ImageContentId(ImageCacheKey.Hash("content")),
            new ImageDecodeSpec(16, 16, "codec"));
        ImageCacheKey.CreateDecodedOperationKey(noStore, decodeKey)
            .ShouldNotBe(ImageCacheKey.CreateDecodedOperationKey(persistent, decodeKey));
    }

    [Theory]
    [InlineData("Host")]
    [InlineData("Content-Length")]
    [InlineData("Range")]
    public void Normalize_Rejects_Transport_Owned_Headers(string header)
    {
        var options = ImageLoadingTestSupport.CreateOptions();
        var request = new ImageLoadRequest(ImageSource.Parse("https://example.com/image.png"))
        {
            Options = new ImageRequestOptions
            {
                Headers = new Dictionary<string, string> { [header] = "value" }
            }
        };

        Should.Throw<ArgumentException>(() => ImageCacheKey.Normalize(request, options, false));
    }
}
