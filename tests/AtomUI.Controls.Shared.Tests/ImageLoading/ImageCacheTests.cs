using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class ImageCacheTests
{
    [Fact]
    public void Decoded_Cache_Eviction_Defers_Disposal_Until_Result_Lease_Is_Released()
    {
        using var cache = new ImageDecodedCache(maxBytes: 1_000, maxEntries: 1);
        var firstImage = new TestImage();
        var secondImage = new TestImage();
        var firstEntry = CreateEntry(firstImage, decodedBytes: 400);
        var secondEntry = CreateEntry(secondImage, decodedBytes: 400);
        var firstKey = CreateDecodeKey("first");
        var secondKey = CreateDecodeKey("second");

        cache.TryAdd(firstKey, firstEntry).ShouldBeTrue();
        using var firstResult = firstEntry.AcquireResult(
            ImageLoadOrigin.DecodedMemory,
            ImageSourceValidation.Current,
            firstKey.ContentId.Value);
        cache.TryAdd(secondKey, secondEntry).ShouldBeTrue();

        firstImage.DisposeCount.ShouldBe(0);
        firstResult.Dispose();
        firstImage.DisposeCount.ShouldBe(1);

        cache.Dispose();
        secondImage.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public void Decoded_Cache_Result_Acquisition_Holds_The_Image_Across_Clear()
    {
        using var cache = new ImageDecodedCache(maxBytes: 1_000, maxEntries: 1);
        var image = new TestImage();
        var entry = CreateEntry(image, decodedBytes: 400);
        var key = CreateDecodeKey("atomic-result");
        cache.TryAdd(key, entry).ShouldBeTrue();

        cache.TryAcquireResult(
                key,
                ImageLoadOrigin.DecodedMemory,
                ImageSourceValidation.Current,
                stageDurations: null,
                out var result)
            .ShouldBeTrue();
        cache.Clear(partitionHash: null);

        image.DisposeCount.ShouldBe(0);
        result.ShouldNotBeNull().Dispose();
        image.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public void Borrowed_Image_Is_Never_Disposed_By_Operation_Or_Result_Lease()
    {
        var image = new TestImage();
        var entry = new ImageDecodedCacheEntry(
            image,
            ownsImage: false,
            10,
            10,
            10,
            10,
            400,
            null,
            ImageLoadOrigin.Borrowed);
        entry.RetainOperation();

        using var result = entry.AcquireResult(
            ImageLoadOrigin.Borrowed,
            ImageSourceValidation.NotRequired,
            contentId: null);
        entry.ReleaseOperation();
        result.Dispose();

        image.DisposeCount.ShouldBe(0);
        result.Image.ShouldBeSameAs(image);
    }

    [Fact]
    public void Encoded_Cache_Uses_Content_Id_Lru_And_Partition_Clear()
    {
        using var cache = new ImageEncodedCache(maxBytes: 10, maxEntries: 2);
        var firstContent = CreateValidatedContent([1, 2]);
        var secondContent = CreateValidatedContent([3, 4]);
        var thirdContent = CreateValidatedContent([5, 6]);
        var first = new ImageEncodedContentKey("p1", firstContent.ContentId!.Value);
        var second = new ImageEncodedContentKey("p2", secondContent.ContentId!.Value);
        var third = new ImageEncodedContentKey("p1", thirdContent.ContentId!.Value);
        cache.Set(first, firstContent);
        cache.Set(second, secondContent);
        cache.TryGet(first, out _).ShouldBeTrue();

        cache.Set(third, thirdContent);

        cache.TryGet(second, out _).ShouldBeFalse();
        cache.TryGet(first, out _).ShouldBeTrue();
        cache.TryGet(third, out _).ShouldBeTrue();
        cache.Clear("p1");
        cache.Count.ShouldBe(0);
    }

    [Fact]
    public void Snapshot_Index_Rejects_An_Older_Completion()
    {
        var index = new ImageSourceSnapshotIndex();
        var sourceKey = new ImageSourceKey("source", "partition");
        var olderGeneration = index.BeginResolution(sourceKey);
        var newerGeneration = index.BeginResolution(sourceKey);
        var older = CreateSnapshot(sourceKey, "old", olderGeneration);
        var newer = CreateSnapshot(sourceKey, "new", newerGeneration);

        index.TryCommit(newer).ShouldBeTrue();
        index.TryCommit(older).ShouldBeFalse();
        index.TryGet(sourceKey, out var current).ShouldBeTrue();
        current.ShouldBe(newer);
    }

    [Fact]
    public async Task File_Cache_Uses_Stable_Content_And_Source_Layout()
    {
        var directory = CreateTempDirectory();
        try
        {
            using var cache = new ImageFileCache(directory, maxBytes: 1_000, maxEntries: 10);
            var content = CreateValidatedContent([1, 2, 3, 4]);
            var contentKey = new ImageEncodedContentKey("partition", content.ContentId!.Value);
            var sourceKey = new ImageSourceKey("source", "partition");
            var snapshot = new ImageSourceSnapshot(
                sourceKey,
                "revision",
                content.ContentId.Value,
                1,
                DateTimeOffset.UtcNow,
                SecurityPolicyVersion: ImageSecurityPolicy.Version);

            await cache.SetContentAsync(contentKey, content, CancellationToken.None);
            await cache.SetSourceSnapshotAsync(snapshot, CancellationToken.None);

            var loadedContent = await cache.TryGetContentAsync(contentKey, CancellationToken.None);
            var loadedSnapshot = await cache.TryGetSourceSnapshotAsync(sourceKey, CancellationToken.None);
            loadedContent.ShouldNotBeNull().Bytes.ShouldBe(content.Bytes);
            loadedContent.Origin.ShouldBe(ImageLoadOrigin.Persistent);
            loadedSnapshot.ShouldBe(snapshot);
            File.ReadAllText(Path.Combine(directory, "manifest")).ShouldContain("formatRevision=");
            Directory.Exists(Path.Combine(directory, "content", "partition")).ShouldBeTrue();
            Directory.Exists(Path.Combine(directory, "content-metadata", "partition")).ShouldBeTrue();
            Directory.Exists(Path.Combine(directory, "sources", "partition")).ShouldBeTrue();
            Directory.Exists(Path.Combine(directory, "locks")).ShouldBeTrue();
            Directory.Exists(Path.Combine(directory, "temp")).ShouldBeTrue();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task File_Cache_Treats_Corrupt_Content_As_A_Miss_And_Removes_It()
    {
        var directory = CreateTempDirectory();
        try
        {
            using var cache = new ImageFileCache(directory, maxBytes: 1_000, maxEntries: 10);
            var content = CreateValidatedContent([1, 2, 3, 4]);
            var key = new ImageEncodedContentKey("partition", content.ContentId!.Value);
            await cache.SetContentAsync(key, content, CancellationToken.None);
            var contentPath = Directory.EnumerateFiles(
                Path.Combine(directory, "content"), "*.bin", SearchOption.AllDirectories).Single();
            await File.WriteAllBytesAsync(contentPath, [9, 9, 9], TestContext.Current.CancellationToken);

            (await cache.TryGetContentAsync(key, CancellationToken.None)).ShouldBeNull();
            File.Exists(contentPath).ShouldBeFalse();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task File_Cache_Does_Not_Overwrite_A_Newer_Source_Snapshot()
    {
        var directory = CreateTempDirectory();
        try
        {
            using var cache = new ImageFileCache(directory, maxBytes: 1_000, maxEntries: 10);
            var olderContent = CreateValidatedContent([1, 2, 3, 4]);
            var newerContent = CreateValidatedContent([5, 6, 7, 8]);
            var sourceKey = new ImageSourceKey("source", "partition");
            var older = new ImageSourceSnapshot(
                sourceKey,
                "old",
                olderContent.ContentId!.Value,
                1,
                DateTimeOffset.UtcNow,
                SecurityPolicyVersion: ImageSecurityPolicy.Version);
            var newer = new ImageSourceSnapshot(
                sourceKey,
                "new",
                newerContent.ContentId!.Value,
                2,
                DateTimeOffset.UtcNow,
                SecurityPolicyVersion: ImageSecurityPolicy.Version);
            await cache.SetContentAsync(
                new ImageEncodedContentKey("partition", olderContent.ContentId.Value),
                olderContent,
                CancellationToken.None);
            await cache.SetContentAsync(
                new ImageEncodedContentKey("partition", newerContent.ContentId.Value),
                newerContent,
                CancellationToken.None);

            await cache.SetSourceSnapshotAsync(newer, CancellationToken.None);
            await cache.SetSourceSnapshotAsync(older, CancellationToken.None);

            (await cache.TryGetSourceSnapshotAsync(sourceKey, CancellationToken.None)).ShouldBe(newer);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task File_Cache_Rebuild_And_Clear_Only_Replace_Managed_Store_Content()
    {
        var directory = CreateTempDirectory();
        try
        {
            var unmanagedPath = Path.Combine(directory, "application-owned.txt");
            await File.WriteAllTextAsync(
                unmanagedPath,
                "keep",
                TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(
                Path.Combine(directory, "manifest"),
                "formatRevision=999\n",
                TestContext.Current.CancellationToken);
            var obsoleteContent = Path.Combine(directory, "content", "shared", "obsolete.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(obsoleteContent)!);
            await File.WriteAllBytesAsync(
                obsoleteContent,
                [1, 2, 3],
                TestContext.Current.CancellationToken);

            using var cache = new ImageFileCache(directory, maxBytes: 1_000, maxEntries: 10);

            File.Exists(obsoleteContent).ShouldBeFalse();
            File.ReadAllText(unmanagedPath).ShouldBe("keep");
            File.ReadAllText(Path.Combine(directory, "manifest")).ShouldBe("formatRevision=1\n");

            var content = CreateValidatedContent([4, 5, 6]);
            await cache.SetContentAsync(
                new ImageEncodedContentKey(string.Empty, content.ContentId!.Value),
                content,
                CancellationToken.None);
            await cache.ClearAsync(partitionHash: null, CancellationToken.None);

            File.ReadAllText(unmanagedPath).ShouldBe("keep");
            File.Exists(Path.Combine(directory, "manifest")).ShouldBeTrue();
            Directory.Exists(Path.Combine(directory, "locks")).ShouldBeTrue();
            Directory.EnumerateFiles(
                Path.Combine(directory, "content"),
                "*",
                SearchOption.AllDirectories).ShouldBeEmpty();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task File_Cache_Partition_Clear_Does_Not_Remove_Other_Partitions()
    {
        var directory = CreateTempDirectory();
        try
        {
            using var cache = new ImageFileCache(directory, maxBytes: 1_000, maxEntries: 10);
            var firstContent = CreateValidatedContent([1, 3, 5]);
            var secondContent = CreateValidatedContent([2, 4, 6]);
            var firstContentKey = new ImageEncodedContentKey("p1", firstContent.ContentId!.Value);
            var secondContentKey = new ImageEncodedContentKey("p2", secondContent.ContentId!.Value);
            var firstSourceKey = new ImageSourceKey("source-1", "p1");
            var secondSourceKey = new ImageSourceKey("source-2", "p2");
            await cache.SetContentAsync(firstContentKey, firstContent, CancellationToken.None);
            await cache.SetContentAsync(secondContentKey, secondContent, CancellationToken.None);
            await cache.SetSourceSnapshotAsync(
                new ImageSourceSnapshot(
                    firstSourceKey,
                    "v1",
                    firstContent.ContentId.Value,
                    1,
                    DateTimeOffset.UtcNow,
                    SecurityPolicyVersion: ImageSecurityPolicy.Version),
                CancellationToken.None);
            await cache.SetSourceSnapshotAsync(
                new ImageSourceSnapshot(
                    secondSourceKey,
                    "v2",
                    secondContent.ContentId.Value,
                    1,
                    DateTimeOffset.UtcNow,
                    SecurityPolicyVersion: ImageSecurityPolicy.Version),
                CancellationToken.None);

            await cache.ClearAsync("p1", CancellationToken.None);

            (await cache.TryGetContentAsync(firstContentKey, CancellationToken.None)).ShouldBeNull();
            (await cache.TryGetSourceSnapshotAsync(firstSourceKey, CancellationToken.None)).ShouldBeNull();
            (await cache.TryGetContentAsync(secondContentKey, CancellationToken.None)).ShouldNotBeNull();
            (await cache.TryGetSourceSnapshotAsync(secondSourceKey, CancellationToken.None)).ShouldNotBeNull();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static ImageDecodedCacheEntry CreateEntry(TestImage image, long decodedBytes) =>
        new(
            image,
            ownsImage: true,
            10,
            10,
            10,
            10,
            decodedBytes,
            "image/png");

    private static ImageDecodeKey CreateDecodeKey(string value) =>
        new(
            string.Empty,
            new ImageContentId(ImageCacheKey.Hash(value)),
            new ImageDecodeSpec(10, 10, "codec"));

    private static ImageEncodedContent CreateValidatedContent(byte[] bytes) =>
        ImageLoadingTestSupport.CreateContent(bytes).MarkValidated();

    private static ImageSourceSnapshot CreateSnapshot(
        ImageSourceKey key,
        string version,
        long generation) =>
        new(
            key,
            version,
            new ImageContentId(ImageCacheKey.Hash(version)),
            generation,
            DateTimeOffset.UtcNow,
            SecurityPolicyVersion: ImageSecurityPolicy.Version);

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"atomui-image-cache-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }
}
