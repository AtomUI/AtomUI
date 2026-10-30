using Shouldly;
using Xunit;
using System.Reflection;
using System.Net;
using System.Net.Http.Headers;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class ImageLoaderPipelineTests
{
    [Fact]
    public async Task Construction_And_NonHttp_Load_Do_Not_Create_Http_Transport()
    {
        var creations = 0;
        var codec = new TestCodec();
        using var loader = new ImageLoader(
            ImageLoadingTestSupport.CreateOptions(),
            [codec],
            httpTransportFactory: () =>
            {
                creations++;
                return new HttpImageTransport(
                    ImageLoadingTestSupport.CreateOptions(),
                    new DelegateHttpMessageHandler(_ => throw new InvalidOperationException("HTTP must not be used.")));
            });

        creations.ShouldBe(0);
        using var result = await loader.LoadAsync(
            new ImageLoadRequest(new BytesImageSource(ImageLoadingTestSupport.CreatePngHeader(), "local")),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        creations.ShouldBe(0);
    }

    [Fact]
    public async Task First_Http_Load_Creates_One_Transport()
    {
        var creations = 0;
        var options = ImageLoadingTestSupport.CreateOptions();
        using var loader = new ImageLoader(
            options,
            [new TestCodec()],
            httpTransportFactory: () =>
            {
                creations++;
                return new HttpImageTransport(
                    options,
                    new DelegateHttpMessageHandler(_ =>
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new ByteArrayContent(ImageLoadingTestSupport.CreatePngHeader())
                        };
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                        return response;
                    }));
            });
        var request = new ImageLoadRequest(
            new HttpImageSource(new Uri("https://example.com/lazy-transport.png")));

        using var first = await loader.LoadAsync(request, TestContext.Current.CancellationToken);
        using var second = await loader.LoadAsync(request, TestContext.Current.CancellationToken);

        first.IsSuccess.ShouldBeTrue();
        second.IsSuccess.ShouldBeTrue();
        creations.ShouldBe(1);
    }

    [Fact]
    public async Task Canceled_Http_Read_Does_Not_Acquire_Transport()
    {
        var creations = 0;
        using var owner = new DeferredHttpImageTransport(() =>
        {
            creations++;
            return new HttpImageTransport(
                ImageLoadingTestSupport.CreateOptions(),
                new DelegateHttpMessageHandler(_ => throw new InvalidOperationException("HTTP must not start.")));
        });
        var reader = new HttpImageSourceReader(owner.Get);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var options = ImageLoadingTestSupport.CreateOptions();
        var request = ImageCacheKey.Normalize(
            new ImageLoadRequest(new HttpImageSource(new Uri("https://example.com/canceled.png"))),
            options,
            forceReload: false);

        await Should.ThrowAsync<OperationCanceledException>(() =>
            reader.ReadAsync(request, null, null, cancellation.Token));

        creations.ShouldBe(0);
        owner.IsValueCreated.ShouldBeFalse();
    }

    [Fact]
    public async Task Same_Decoded_Request_Shares_Read_And_Decode_And_Returns_Independent_Leases()
    {
        var streamStarted = NewSignal();
        var releaseStream = NewSignal();
        var streamCalls = 0;
        var codec = new TestCodec();
        var source = new StreamImageSource(
            async token =>
            {
                Interlocked.Increment(ref streamCalls);
                streamStarted.TrySetResult();
                await releaseStream.Task.WaitAsync(token);
                return new MemoryStream(ImageLoadingTestSupport.CreatePngHeader());
            },
            "shared",
            "v1",
            "shared.png");
        using var loader = CreateLoader(codec);
        var request = new ImageLoadRequest(source) { DecodePixelWidth = 32, DecodePixelHeight = 32 };
        var cancellationToken = TestContext.Current.CancellationToken;

        var firstTask = loader.LoadAsync(request, cancellationToken).AsTask();
        await streamStarted.Task;
        var secondTask = loader.LoadAsync(request, cancellationToken).AsTask();
        releaseStream.TrySetResult();
        using var first = await firstTask;
        using var second = await secondTask;

        streamCalls.ShouldBe(1);
        codec.DecodeCalls.ShouldBe(1);
        first.Image.ShouldBeSameAs(second.Image);
        first.IsSuccess.ShouldBeTrue();
        second.IsSuccess.ShouldBeTrue();
        first.Dispose();
        second.Dispose();
        codec.Images.Single().DisposeCount.ShouldBe(0);
        loader.Dispose();
        codec.Images.Single().DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Different_Decode_Sizes_Share_Encoded_Read_But_Not_Decode()
    {
        var streamStarted = NewSignal();
        var releaseStream = NewSignal();
        var streamCalls = 0;
        var codec = new TestCodec();
        var source = new StreamImageSource(
            async token =>
            {
                Interlocked.Increment(ref streamCalls);
                streamStarted.TrySetResult();
                await releaseStream.Task.WaitAsync(token);
                return new MemoryStream(ImageLoadingTestSupport.CreatePngHeader(64, 64));
            },
            "sized",
            "v1");
        using var loader = CreateLoader(codec);
        var cancellationToken = TestContext.Current.CancellationToken;

        var firstTask = loader.LoadAsync(new ImageLoadRequest(source)
        {
            DecodePixelWidth = 16,
            DecodePixelHeight = 16
        }, cancellationToken).AsTask();
        await streamStarted.Task;
        var secondTask = loader.LoadAsync(new ImageLoadRequest(source)
        {
            DecodePixelWidth = 32,
            DecodePixelHeight = 32
        }, cancellationToken).AsTask();
        releaseStream.TrySetResult();
        using var first = await firstTask;
        using var second = await secondTask;

        streamCalls.ShouldBe(1);
        codec.DecodeCalls.ShouldBe(2);
        first.Image.ShouldNotBeSameAs(second.Image);
    }

    [Fact]
    public async Task Sequential_Request_Hits_Decoded_Memory_Cache()
    {
        var codec = new TestCodec();
        var source = new BytesImageSource(
            ImageLoadingTestSupport.CreatePngHeader(),
            "cache");
        using var loader = CreateLoader(codec);
        var request = new ImageLoadRequest(source) { DecodePixelWidth = 16, DecodePixelHeight = 16 };
        var cancellationToken = TestContext.Current.CancellationToken;

        using var first = await loader.LoadAsync(request, cancellationToken);
        using var second = await loader.LoadAsync(request, cancellationToken);

        first.Origin.ShouldBe(ImageLoadOrigin.Local);
        second.Origin.ShouldBe(ImageLoadOrigin.DecodedMemory);
        codec.DecodeCalls.ShouldBe(1);
    }

    [Fact]
    public async Task New_Decode_Size_Reports_Encoded_Memory_When_The_Source_Content_Is_Reused()
    {
        var reads = 0;
        var source = new StreamImageSource(
            _ =>
            {
                reads++;
                return ValueTask.FromResult<Stream>(
                    new MemoryStream(ImageLoadingTestSupport.CreatePngHeader(64, 64)));
            },
            identity: "encoded-origin",
            revision: "v1");
        var codec = new TestCodec();
        using var loader = CreateLoader(codec);

        using var first = await loader.LoadAsync(
            new ImageLoadRequest(source) { DecodePixelWidth = 16, DecodePixelHeight = 16 },
            TestContext.Current.CancellationToken);
        using var second = await loader.LoadAsync(
            new ImageLoadRequest(source) { DecodePixelWidth = 32, DecodePixelHeight = 32 },
            TestContext.Current.CancellationToken);

        reads.ShouldBe(1);
        codec.DecodeCalls.ShouldBe(2);
        second.Origin.ShouldBe(ImageLoadOrigin.EncodedMemory);
        second.SourceValidation.ShouldBe(ImageSourceValidation.Current);
    }

    [Fact]
    public async Task Default_File_Request_Revalidates_A_Replaced_File_Before_Decoded_Cache_Lookup()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"atomui-image-revalidation-{Guid.NewGuid():N}");
        var filePath = Path.Combine(directory, "image.png");
        Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllBytesAsync(
                filePath,
                ImageLoadingTestSupport.CreatePngHeader(13, 17),
                TestContext.Current.CancellationToken);
            var codec = new TestCodec();
            using var loader = CreateLoader(codec);
            var request = new ImageLoadRequest(new FileImageSource(filePath));

            using var first = await loader.LoadAsync(request, TestContext.Current.CancellationToken);

            await File.WriteAllBytesAsync(
                filePath,
                ImageLoadingTestSupport.CreatePngHeader(29, 31),
                TestContext.Current.CancellationToken);
            File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow.AddSeconds(2));

            using var second = await loader.LoadAsync(request, TestContext.Current.CancellationToken);

            first.OriginalPixelWidth.ShouldBe(13);
            first.OriginalPixelHeight.ShouldBe(17);
            second.OriginalPixelWidth.ShouldBe(29);
            second.OriginalPixelHeight.ShouldBe(31);
            codec.DecodeCalls.ShouldBe(2);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Size_Independent_Codec_Reuses_One_Decode_Across_Display_Sizes()
    {
        var codec = new SizeIndependentTestCodec();
        using var loader = CreateLoader(codec);
        var source = new BytesImageSource(
            "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 64 64'/>"u8.ToArray(),
            "vector");

        using var small = await loader.LoadAsync(
            new ImageLoadRequest(source) { DecodePixelWidth = 32, DecodePixelHeight = 32 },
            TestContext.Current.CancellationToken);
        using var large = await loader.LoadAsync(
            new ImageLoadRequest(source) { DecodePixelWidth = 512, DecodePixelHeight = 512 },
            TestContext.Current.CancellationToken);

        codec.DecodeCalls.ShouldBe(1);
        large.Origin.ShouldBe(ImageLoadOrigin.DecodedMemory);
    }

    [Fact]
    public async Task Unversioned_Keyed_Bytes_Do_Not_Reuse_A_Previous_Content()
    {
        var codec = new TestCodec();
        using var loader = CreateLoader(codec);
        var cancellationToken = TestContext.Current.CancellationToken;

        using var first = await loader.LoadAsync(
            new ImageLoadRequest(new BytesImageSource(
                ImageLoadingTestSupport.CreatePngHeader(2, 3),
                "avatar")),
            cancellationToken);
        using var second = await loader.LoadAsync(
            new ImageLoadRequest(new BytesImageSource(
                ImageLoadingTestSupport.CreatePngHeader(8, 9),
                "avatar")),
            cancellationToken);

        first.OriginalPixelWidth.ShouldBe(2);
        first.OriginalPixelHeight.ShouldBe(3);
        second.OriginalPixelWidth.ShouldBe(8);
        second.OriginalPixelHeight.ShouldBe(9);
        codec.DecodeCalls.ShouldBe(2);
    }

    [Fact]
    public async Task Different_Sources_With_The_Same_Bytes_Share_One_Content_Decode()
    {
        var bytes = ImageLoadingTestSupport.CreatePngHeader(37, 41);
        var firstReads = 0;
        var secondReads = 0;
        var firstSource = new StreamImageSource(
            _ =>
            {
                firstReads++;
                return ValueTask.FromResult<Stream>(new MemoryStream(bytes));
            },
            "same-content-first");
        var secondSource = new StreamImageSource(
            _ =>
            {
                secondReads++;
                return ValueTask.FromResult<Stream>(new MemoryStream(bytes));
            },
            "same-content-second");
        var codec = new TestCodec();
        using var loader = CreateLoader(codec);

        using var first = await loader.LoadAsync(
            new ImageLoadRequest(firstSource), TestContext.Current.CancellationToken);
        using var second = await loader.LoadAsync(
            new ImageLoadRequest(secondSource), TestContext.Current.CancellationToken);

        firstReads.ShouldBe(1);
        secondReads.ShouldBe(1);
        codec.DecodeCalls.ShouldBe(1);
        first.ContentId.ShouldBe(second.ContentId);
        second.Origin.ShouldBe(ImageLoadOrigin.DecodedMemory);
    }

    [Fact]
    public async Task PreferCache_Reuses_The_Source_Mapping_Without_Reopening_An_Unversioned_Stream()
    {
        var reads = 0;
        var source = new StreamImageSource(
            _ =>
            {
                reads++;
                return ValueTask.FromResult<Stream>(
                    new MemoryStream(ImageLoadingTestSupport.CreatePngHeader(17, 19)));
            },
            "prefer-cache");
        using var loader = CreateLoader(new TestCodec());

        using var first = await loader.LoadAsync(
            new ImageLoadRequest(source), TestContext.Current.CancellationToken);
        using var second = await loader.LoadAsync(
            new ImageLoadRequest(source)
            {
                Options = new ImageRequestOptions
                {
                    CacheRead = ImageCacheReadPolicy.PreferCache
                }
            },
            TestContext.Current.CancellationToken);

        reads.ShouldBe(1);
        second.SourceValidation.ShouldBe(ImageSourceValidation.Unverified);
        second.Origin.ShouldBe(ImageLoadOrigin.DecodedMemory);
    }

    [Fact]
    public async Task ValidateSource_Reopens_An_Unversioned_Stream_But_Reuses_Identical_Decoded_Content()
    {
        var reads = 0;
        var source = new StreamImageSource(
            _ =>
            {
                reads++;
                return ValueTask.FromResult<Stream>(
                    new MemoryStream(ImageLoadingTestSupport.CreatePngHeader(23, 29)));
            },
            "validate-stream");
        var codec = new TestCodec();
        using var loader = CreateLoader(codec);
        var request = new ImageLoadRequest(source);

        using var first = await loader.LoadAsync(request, TestContext.Current.CancellationToken);
        using var second = await loader.LoadAsync(request, TestContext.Current.CancellationToken);

        reads.ShouldBe(2);
        codec.DecodeCalls.ShouldBe(1);
        second.SourceValidation.ShouldBe(ImageSourceValidation.Current);
        second.Origin.ShouldBe(ImageLoadOrigin.DecodedMemory);
    }

    [Fact]
    public async Task CacheStorage_None_Does_Not_Populate_Memory_Stores()
    {
        var codec = new TestCodec();
        using var loader = CreateLoader(codec);
        var request = new ImageLoadRequest(new BytesImageSource(
            ImageLoadingTestSupport.CreatePngHeader(43, 47)))
        {
            Options = new ImageRequestOptions
            {
                CacheStorage = ImageCacheStoragePolicy.None
            }
        };

        using var first = await loader.LoadAsync(request, TestContext.Current.CancellationToken);
        using var second = await loader.LoadAsync(request, TestContext.Current.CancellationToken);

        codec.DecodeCalls.ShouldBe(2);
        loader.Snapshot.EncodedCacheEntries.ShouldBe(0);
        loader.Snapshot.DecodedCacheEntries.ShouldBe(0);
    }

    [Fact]
    public async Task CacheStorage_None_Does_Not_Promote_A_Persistent_Hit_Into_Memory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"atomui-image-no-promotion-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var options = ImageLoadingTestSupport.CreateOptions(builder =>
            {
                builder.IsPersistentCacheEnabled = true;
                builder.PersistentCacheDirectory = directory;
            });
            var source = new StreamImageSource(
                _ => ValueTask.FromResult<Stream>(new MemoryStream(
                    ImageLoadingTestSupport.CreatePngHeader(43, 47))),
                identity: "persistent-no-promotion",
                revision: "v1");
            using (var writer = new ImageLoader(options, [new TestCodec()]))
            {
                using var populated = await writer.LoadAsync(
                    new ImageLoadRequest(source),
                    TestContext.Current.CancellationToken);
                populated.IsSuccess.ShouldBeTrue();
            }

            using var reader = new ImageLoader(options, [new TestCodec()]);
            using var cached = await reader.LoadAsync(
                new ImageLoadRequest(source)
                {
                    Options = new ImageRequestOptions
                    {
                        CacheRead = ImageCacheReadPolicy.CacheOnly,
                        CacheStorage = ImageCacheStoragePolicy.None
                    }
                },
                TestContext.Current.CancellationToken);

            cached.IsSuccess.ShouldBeTrue();
            cached.Origin.ShouldBe(ImageLoadOrigin.Persistent);
            reader.Snapshot.EncodedCacheEntries.ShouldBe(0);
            reader.Snapshot.DecodedCacheEntries.ShouldBe(0);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task NoStore_Response_Removes_The_Previous_Source_Memory_Entries()
    {
        var responseIndex = 0;
        using var handler = new DelegateHttpMessageHandler(_ =>
        {
            var current = Interlocked.Increment(ref responseIndex);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(ImageLoadingTestSupport.CreatePngHeader(
                    current == 1 ? 17 : 29,
                    current == 1 ? 19 : 31))
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            response.Headers.CacheControl = current == 1
                ? new CacheControlHeaderValue { MaxAge = TimeSpan.FromHours(1) }
                : new CacheControlHeaderValue { NoStore = true };
            return response;
        });
        var codec = new TestCodec();
        using var loader = new ImageLoader(
            ImageLoadingTestSupport.CreateOptions(),
            [codec],
            handler);
        var source = new HttpImageSource(new Uri("https://example.com/no-store.png"));

        using var first = await loader.LoadAsync(
            new ImageLoadRequest(source),
            TestContext.Current.CancellationToken);
        using var refreshed = await loader.LoadAsync(
            new ImageLoadRequest(source)
            {
                Options = new ImageRequestOptions
                {
                    CacheRead = ImageCacheReadPolicy.RefreshSource
                }
            },
            TestContext.Current.CancellationToken);

        first.IsSuccess.ShouldBeTrue();
        refreshed.IsSuccess.ShouldBeTrue();
        refreshed.OriginalPixelWidth.ShouldBe(29);
        loader.Snapshot.EncodedCacheEntries.ShouldBe(0);
        loader.Snapshot.DecodedCacheEntries.ShouldBe(0);

        using var cacheOnly = await loader.LoadAsync(
            new ImageLoadRequest(source)
            {
                Options = new ImageRequestOptions
                {
                    CacheRead = ImageCacheReadPolicy.CacheOnly
                }
            },
            TestContext.Current.CancellationToken);
        cacheOnly.IsSuccess.ShouldBeFalse();
        cacheOnly.Error.ShouldNotBeNull().Code.ShouldBe(ImageLoadErrorCode.CacheMiss);
    }

    [Fact]
    public async Task NoStore_Response_Removes_The_Previous_Persistent_Source_Variant()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"atomui-image-no-store-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var options = ImageLoadingTestSupport.CreateOptions(builder =>
            {
                builder.IsPersistentCacheEnabled = true;
                builder.PersistentCacheDirectory = directory;
            });
            var responseIndex = 0;
            using (var loader = new ImageLoader(
                       options,
                       [new TestCodec()],
                       new DelegateHttpMessageHandler(_ =>
                       {
                           var current = Interlocked.Increment(ref responseIndex);
                           var response = new HttpResponseMessage(HttpStatusCode.OK)
                           {
                               Content = new ByteArrayContent(ImageLoadingTestSupport.CreatePngHeader(
                                   current == 1 ? 37 : 41,
                                   current == 1 ? 43 : 47))
                           };
                           response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                           response.Headers.CacheControl = current == 1
                               ? new CacheControlHeaderValue { MaxAge = TimeSpan.FromHours(1) }
                               : new CacheControlHeaderValue { NoStore = true };
                           return response;
                       })))
            {
                var source = new HttpImageSource(new Uri("https://example.com/persistent-no-store.png"));
                using var first = await loader.LoadAsync(
                    new ImageLoadRequest(source),
                    TestContext.Current.CancellationToken);
                using var refreshed = await loader.LoadAsync(
                    new ImageLoadRequest(source)
                    {
                        Options = new ImageRequestOptions
                        {
                            CacheRead = ImageCacheReadPolicy.RefreshSource
                        }
                    },
                    TestContext.Current.CancellationToken);
                refreshed.IsSuccess.ShouldBeTrue();
            }

            using var cacheOnlyLoader = new ImageLoader(options, [new TestCodec()]);
            using var cacheOnly = await cacheOnlyLoader.LoadAsync(
                new ImageLoadRequest(new HttpImageSource(
                    new Uri("https://example.com/persistent-no-store.png")))
                {
                    Options = new ImageRequestOptions
                    {
                        CacheRead = ImageCacheReadPolicy.CacheOnly
                    }
                },
                TestContext.Current.CancellationToken);

            cacheOnly.IsSuccess.ShouldBeFalse();
            cacheOnly.Error.ShouldNotBeNull().Code.ShouldBe(ImageLoadErrorCode.CacheMiss);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task ContentHash_File_Validation_Detects_Replacement_With_Unchanged_Metadata()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"atomui-image-content-hash-{Guid.NewGuid():N}");
        var filePath = Path.Combine(directory, "image.png");
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllBytes(filePath, ImageLoadingTestSupport.CreatePngHeader(11, 13));
            var originalTimestamp = File.GetLastWriteTimeUtc(filePath);
            var codec = new TestCodec();
            using var loader = CreateLoader(codec);
            var request = new ImageLoadRequest(new FileImageSource(
                filePath,
                ImageFileValidationMode.ContentHash));

            using var first = await loader.LoadAsync(request, TestContext.Current.CancellationToken);
            File.WriteAllBytes(filePath, ImageLoadingTestSupport.CreatePngHeader(53, 59));
            File.SetLastWriteTimeUtc(filePath, originalTimestamp);
            using var second = await loader.LoadAsync(request, TestContext.Current.CancellationToken);

            second.OriginalPixelWidth.ShouldBe(53);
            second.OriginalPixelHeight.ShouldBe(59);
            second.ContentId.ShouldNotBe(first.ContentId);
            codec.DecodeCalls.ShouldBe(2);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task CacheOnly_File_Request_Uses_Persistent_Entry_Without_Probing_Source()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"atomui-image-cache-{Guid.NewGuid():N}");
        var filePath = Path.Combine(directory, "image.png");
        Directory.CreateDirectory(directory);
        await File.WriteAllBytesAsync(
            filePath,
            ImageLoadingTestSupport.CreatePngHeader(13, 17),
            TestContext.Current.CancellationToken);
        try
        {
            var options = ImageLoadingTestSupport.CreateOptions(builder =>
            {
                builder.IsPersistentCacheEnabled = true;
                builder.PersistentCacheDirectory = directory;
            });
            var firstCodec = new TestCodec();
            using (var firstLoader = new ImageLoader(options, [firstCodec]))
            {
                using var first = await firstLoader.LoadAsync(
                    new ImageLoadRequest(new FileImageSource(filePath)),
                    TestContext.Current.CancellationToken);
                first.IsSuccess.ShouldBeTrue();
            }

            File.Delete(filePath);
            var secondCodec = new TestCodec();
            using var secondLoader = new ImageLoader(options, [secondCodec]);
            using var second = await secondLoader.LoadAsync(
                new ImageLoadRequest(new FileImageSource(filePath))
                {
                    Options = new ImageRequestOptions { CacheRead = ImageCacheReadPolicy.CacheOnly }
                },
                TestContext.Current.CancellationToken);

            second.IsSuccess.ShouldBeTrue();
            second.OriginalPixelWidth.ShouldBe(13);
            second.OriginalPixelHeight.ShouldBe(17);
            second.Origin.ShouldBe(ImageLoadOrigin.Persistent);
            secondCodec.DecodeCalls.ShouldBe(1);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Cache_Clear_Epoch_Prevents_InFlight_Result_From_Repopulating_Decoded_Cache()
    {
        var decodeStarted = NewSignal();
        var releaseDecode = NewSignal();
        var codec = new TestCodec(decodeStarted, releaseDecode);
        var source = new BytesImageSource(
            ImageLoadingTestSupport.CreatePngHeader(),
            "epoch");
        using var loader = CreateLoader(codec);
        var request = new ImageLoadRequest(source) { DecodePixelWidth = 16, DecodePixelHeight = 16 };
        var cancellationToken = TestContext.Current.CancellationToken;

        var firstTask = loader.LoadAsync(request, cancellationToken).AsTask();
        await decodeStarted.Task;
        await loader.ClearCacheAsync(new ImageCacheClearRequest
        {
            CancelInFlight = false
        }, cancellationToken);
        releaseDecode.TrySetResult();
        using var first = await firstTask;
        using var second = await loader.LoadAsync(request, cancellationToken);

        codec.DecodeCalls.ShouldBe(2);
        second.Origin.ShouldNotBe(ImageLoadOrigin.DecodedMemory);
    }

    [Fact]
    public async Task Cache_Clear_Removes_Persistent_Source_Content_While_A_Decode_Is_In_Flight()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"atomui-image-clear-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var options = ImageLoadingTestSupport.CreateOptions(builder =>
            {
                builder.IsPersistentCacheEnabled = true;
                builder.PersistentCacheDirectory = directory;
            });
            var decodeStarted = NewSignal();
            var releaseDecode = NewSignal();
            var source = new StreamImageSource(
                _ => ValueTask.FromResult<Stream>(new MemoryStream(
                    ImageLoadingTestSupport.CreatePngHeader(31, 37))),
                identity: "persistent-clear",
                revision: "v1");
            using (var loader = new ImageLoader(options, [new TestCodec(decodeStarted, releaseDecode)]))
            {
                var load = loader.LoadAsync(
                    new ImageLoadRequest(source),
                    TestContext.Current.CancellationToken).AsTask();
                await decodeStarted.Task;
                await loader.ClearCacheAsync(
                    new ImageCacheClearRequest { CancelInFlight = false },
                    TestContext.Current.CancellationToken);
                releaseDecode.TrySetResult();
                using var result = await load;
                result.IsSuccess.ShouldBeTrue();
            }

            using var cacheOnlyLoader = new ImageLoader(options, [new TestCodec()]);
            using var cached = await cacheOnlyLoader.LoadAsync(
                new ImageLoadRequest(source)
                {
                    Options = new ImageRequestOptions
                    {
                        CacheRead = ImageCacheReadPolicy.CacheOnly
                    }
                },
                TestContext.Current.CancellationToken);

            cached.IsSuccess.ShouldBeFalse();
            cached.Error.ShouldNotBeNull().Code.ShouldBe(ImageLoadErrorCode.CacheMiss);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Post_Decode_Commit_Failure_Releases_The_Decoded_Image()
    {
        var decodeStarted = NewSignal();
        var releaseDecode = NewSignal();
        var codec = new TestCodec(decodeStarted, releaseDecode);
        using var loader = CreateLoader(codec);
        var load = loader.LoadAsync(
            new ImageLoadRequest(new BytesImageSource(
                ImageLoadingTestSupport.CreatePngHeader(),
                "post-decode-commit")),
            TestContext.Current.CancellationToken).AsTask();
        await decodeStarted.Task;

        var pipeline = typeof(ImageLoader)
            .GetField("_pipeline", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(loader)!;
        var decodedCache = (ImageDecodedCache)pipeline.GetType()
            .GetField("_decodedCache", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(pipeline)!;
        decodedCache.Dispose();
        releaseDecode.TrySetResult();

        using var result = await load;

        result.IsSuccess.ShouldBeFalse();
        codec.Images.Single().DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Borrowed_Image_Result_Is_Not_Disposed_By_Loader_Or_Lease()
    {
        var borrowed = new TestImage(20, 30);
        using var loader = CreateLoader(new TestCodec());

        using var result = await loader.LoadAsync(
            new ImageLoadRequest(new BorrowedImageSource(borrowed)),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Image.ShouldBeSameAs(borrowed);
        result.DecodedPixelWidth.ShouldBe(20);
        result.DecodedPixelHeight.ShouldBe(30);
        result.Dispose();
        loader.Dispose();
        borrowed.DisposeCount.ShouldBe(0);
    }

    private static ImageLoader CreateLoader(ImageCodec codec)
    {
        return new ImageLoader(ImageLoadingTestSupport.CreateOptions(), [codec]);
    }

    private static TaskCompletionSource NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private sealed class DelegateHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _send;

        internal DelegateHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> send)
        {
            _send = send;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_send(request));
        }
    }

    private sealed class TestCodec : ImageCodec
    {
        private readonly TaskCompletionSource? _decodeStarted;
        private readonly TaskCompletionSource? _releaseDecode;
        private int _decodeCalls;

        internal TestCodec(
            TaskCompletionSource? decodeStarted = null,
            TaskCompletionSource? releaseDecode = null)
        {
            _decodeStarted = decodeStarted;
            _releaseDecode = releaseDecode;
        }

        internal override string Id => "test.raster";

        internal override int Version => 1;

        internal int DecodeCalls => Volatile.Read(ref _decodeCalls);

        internal List<TestImage> Images { get; } = [];

        internal override bool CanDecode(ImageProbeResult probe, ImageSource source) => true;

        internal override async Task<ImageDecodedCacheEntry> DecodeAsync(
            ImageEncodedContent content,
            ImageProbeResult probe,
            NormalizedImageRequest request,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _decodeCalls);
            _decodeStarted?.TrySetResult();
            if (_releaseDecode is not null)
            {
                await _releaseDecode.Task.WaitAsync(cancellationToken);
            }
            var width = request.DecodePixelWidth > 0 ? request.DecodePixelWidth : probe.PixelWidth;
            var height = request.DecodePixelHeight > 0 ? request.DecodePixelHeight : probe.PixelHeight;
            var image = new TestImage(width, height);
            lock (Images)
            {
                Images.Add(image);
            }
            return new ImageDecodedCacheEntry(
                image,
                ownsImage: true,
                probe.PixelWidth,
                probe.PixelHeight,
                width,
                height,
                checked((long)width * height * 4),
                probe.MediaType,
                content.Origin);
        }
    }

    private sealed class SizeIndependentTestCodec : ImageCodec
    {
        private int _decodeCalls;

        internal override string Id => "test.vector";

        internal override int Version => 1;

        internal override bool IsDecodeSizeDependent => false;

        internal int DecodeCalls => Volatile.Read(ref _decodeCalls);

        internal override bool CanDecode(ImageProbeResult probe, ImageSource source) =>
            probe.Format == ImageContentFormat.Svg;

        internal override Task<ImageDecodedCacheEntry> DecodeAsync(
            ImageEncodedContent content,
            ImageProbeResult probe,
            NormalizedImageRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _decodeCalls);
            return Task.FromResult(new ImageDecodedCacheEntry(
                new TestImage(64, 64),
                ownsImage: true,
                64,
                64,
                64,
                64,
                probe.SvgMetadata!.EstimatedDecodedCost,
                probe.MediaType,
                content.Origin));
        }
    }
}
