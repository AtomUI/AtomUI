using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

public class ImagePreviewerImageLoadingTests
{
    public ImagePreviewerImageLoadingTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Clearing_And_Readding_The_Same_File_Path_Loads_Replaced_Content()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                $"atomui-previewer-file-revalidation-{Guid.NewGuid():N}");
            var path = Path.Combine(directory, "image.png");
            Directory.CreateDirectory(directory);
            try
            {
                File.WriteAllBytes(path, CreatePng(13, 17));
                var items = new ObservableCollection<ImagePreviewItem>
                {
                    new(new FileImageSource(path))
                };
                var previewer = CreatePreviewer(items);
                using var host = new PreviewerHost(previewer);
                var firstEntry = previewer.EffectiveItems.ShouldNotBeNull().Single();
                WaitUntil(() => firstEntry.ThumbnailState == ImageLoadState.Loaded, "initial file cover");
                GetThumbnailOriginalSize(firstEntry).ShouldBe(new PixelSize(13, 17));

                File.WriteAllBytes(path, CreatePng(29, 31));
                File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddSeconds(2));
                items.Clear();
                items.Add(new ImagePreviewItem(new FileImageSource(path)));

                var secondEntry = previewer.EffectiveItems.ShouldNotBeNull().Single();
                WaitUntil(() => secondEntry.ThumbnailState == ImageLoadState.Loaded, "replaced file cover");
                GetThumbnailOriginalSize(secondEntry).ShouldBe(new PixelSize(29, 31));
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        });
    }

    [Fact]
    public void Open_Previewer_Loads_Replacement_Enumerable_Current_Item()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = new TestImage();
            var second = new TestImage();
            var previewer = CreatePreviewer(new ImagePreviewItem(new BorrowedImageSource(first)));
            using var host = new PreviewerHost(previewer);
            previewer.OpenDialog();
            WaitUntil(() => previewer.IsCurrentLoaded, "initial current item");

            previewer.ItemsSource =
            [
                new ImagePreviewItem(new BorrowedImageSource(second))
            ];

            WaitUntil(() => previewer.IsCurrentLoaded, "replacement current item");
            previewer.CurrentItem.ShouldNotBeNull().Source.ShouldBeSameAs(
                previewer.ItemsSource.ShouldNotBeNull().Single().Source);
            previewer.EffectiveItems.ShouldNotBeNull().Single().FullImage.ShouldBeSameAs(second);
        });
    }

    [Fact]
    public void Current_Index_Switch_Does_Not_Commit_The_Previous_Full_Request()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var firstStarted = NewSignal();
            var releaseFirst = NewSignal();
            var firstSource = new StreamImageSource(
                async token =>
                {
                    firstStarted.TrySetResult();
                    await releaseFirst.Task.WaitAsync(token);
                    return new MemoryStream(CreatePng(32, 32));
                },
                $"preview-switch-first-{Guid.NewGuid():N}",
                "v1");
            var secondImage = new TestImage();
            var previewer = CreatePreviewer(
                new ImagePreviewItem(firstSource),
                new ImagePreviewItem(new BorrowedImageSource(secondImage)));
            using var host = new PreviewerHost(previewer);

            previewer.OpenDialog();
            WaitUntil(() => firstStarted.Task.IsCompleted, "first full request start");

            previewer.CurrentIndex = 1;
            WaitUntil(
                () => previewer.IsCurrentLoaded &&
                      ReferenceEquals(previewer.EffectiveItems![1].FullImage, secondImage),
                "second current full request");

            releaseFirst.TrySetResult();
            WaitUntil(() => previewer.EffectiveItems![0].FullState != ImageLoadState.Loading,
                "cancelled first full request cleanup");

            var entries = previewer.EffectiveItems.ShouldNotBeNull();
            previewer.CurrentIndex.ShouldBe(1);
            previewer.CurrentItem.ShouldBeSameAs(previewer.ItemsSource!.ElementAt(1));
            entries[1].FullImage.ShouldBeSameAs(secondImage);
        });
    }

    [Fact]
    public void Open_Previewer_Loads_Current_Item_After_Observable_Reset()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = new TestImage();
            var second = new TestImage();
            var items = new ResettableCollection<ImagePreviewItem>(
            [
                new ImagePreviewItem(new BorrowedImageSource(first))
            ]);
            var previewer = CreatePreviewer(items);
            using var host = new PreviewerHost(previewer);
            previewer.OpenDialog();
            WaitUntil(() => previewer.IsCurrentLoaded, "initial current item");

            items.ResetWith(
            [
                new ImagePreviewItem(new BorrowedImageSource(second))
            ]);

            WaitUntil(() => previewer.IsCurrentLoaded, "reset current item");
            previewer.EffectiveItems.ShouldNotBeNull().Single().FullImage.ShouldBeSameAs(second);
        });
    }

    [Fact]
    public void Observable_Collection_Operations_Preserve_Or_Dispose_The_Correct_Entries()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = CreateItem("first");
            var second = CreateItem("second");
            var third = CreateItem("third");
            var items = new ObservableCollection<ImagePreviewItem>([first, second, third]);
            var previewer = CreatePreviewer(items);
            using var host = new PreviewerHost(previewer);
            var initial = previewer.EffectiveItems.ShouldNotBeNull().ToArray();

            var added = CreateItem("added");
            items.Insert(1, added);

            previewer.EffectiveItems.ShouldNotBeNull()[0].ShouldBeSameAs(initial[0]);
            previewer.EffectiveItems[1].Item.ShouldBeSameAs(added);
            previewer.EffectiveItems[2].ShouldBeSameAs(initial[1]);
            previewer.EffectiveItems[3].ShouldBeSameAs(initial[2]);

            items.Move(2, 0);

            previewer.EffectiveItems[0].ShouldBeSameAs(initial[1]);
            var replacedEntry = previewer.EffectiveItems[1];
            var replacement = CreateItem("replacement");
            items[1] = replacement;

            previewer.EffectiveItems[1].Item.ShouldBeSameAs(replacement);
            Should.Throw<ObjectDisposedException>(() =>
                replacedEntry.LoadFull(16, 16, ImageRequestPriority.Critical));

            var removedEntry = previewer.EffectiveItems[2];
            items.RemoveAt(2);

            Should.Throw<ObjectDisposedException>(() =>
                removedEntry.LoadThumbnail(16, 16, ImageRequestPriority.High));
            previewer.EffectiveItems.Count.ShouldBe(3);
        });
    }

    [Fact]
    public void Detach_Releases_Entries_Unsubscribes_Collection_And_Reattach_Reconciles_Changes()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var full = new TestImage();
            var thumbnail = new TestImage();
            var first = new ImagePreviewItem(new BorrowedImageSource(full))
            {
                ThumbnailSource = new BorrowedImageSource(thumbnail)
            };
            var items = new ObservableCollection<ImagePreviewItem>([first]);
            var previewer = CreatePreviewer(items);
            using var host = new PreviewerHost(previewer);
            previewer.RequestPreviewLoads();
            var entry = previewer.EffectiveItems.ShouldNotBeNull().Single();
            WaitUntil(
                () => entry.FullState == ImageLoadState.Loaded &&
                      entry.ThumbnailState == ImageLoadState.Loaded,
                "full and thumbnail loads");

            host.Detach();

            entry.FullImage.ShouldBeNull();
            entry.ThumbnailImage.ShouldBeNull();
            entry.FullState.ShouldBe(ImageLoadState.Idle);
            entry.ThumbnailState.ShouldBe(ImageLoadState.Idle);
            items.Add(CreateItem("added-while-detached"));
            previewer.EffectiveItems.Count.ShouldBe(1);

            host.Attach(previewer);

            WaitUntil(() => previewer.EffectiveItems?.Count == 2, "reattach collection reconciliation");
        });
    }

    [Fact]
    public void Entries_Hold_Loading_State_Independently_From_The_Immutable_Item()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var image = new TestImage();
            var item = new ImagePreviewItem(new BorrowedImageSource(image));
            using var first = new ImagePreviewEntry(item);
            using var second = new ImagePreviewEntry(item);

            first.LoadFull(16, 16, ImageRequestPriority.Critical);
            WaitUntil(() => first.FullState == ImageLoadState.Loaded, "first entry load");

            first.FullImage.ShouldBeSameAs(image);
            second.FullImage.ShouldBeNull();
            second.FullState.ShouldBe(ImageLoadState.Idle);
            item.GetType().GetProperties().Select(property => property.Name)
                .ShouldNotContain(nameof(ImagePreviewEntry.FullState));
        });
    }

    [Fact]
    public void Dispose_Notifies_Subscribers_To_Drop_Image_References()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var image = new TestImage();
            var item = new ImagePreviewItem(new BorrowedImageSource(image))
            {
                ThumbnailSource = new BorrowedImageSource(new TestImage())
            };
            var entry = new ImagePreviewEntry(item);
            var fullImageReset = false;
            var fullStateReset = false;
            var thumbnailImageReset = false;
            entry.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(ImagePreviewEntry.FullImage))
                {
                    fullImageReset = true;
                }
                if (args.PropertyName == nameof(ImagePreviewEntry.FullState))
                {
                    fullStateReset = true;
                }
                if (args.PropertyName == nameof(ImagePreviewEntry.ThumbnailImage))
                {
                    thumbnailImageReset = true;
                }
            };

            entry.LoadFull(16, 16, ImageRequestPriority.Critical);
            entry.LoadThumbnail(16, 16, ImageRequestPriority.High);
            WaitUntil(() => entry.FullState == ImageLoadState.Loaded &&
                             entry.ThumbnailState == ImageLoadState.Loaded, "entry loads");

            // 排除加载阶段 CommitFull/CommitThumbnail 的常规通知，只观察 Dispose 的重置通知
            fullImageReset      = false;
            fullStateReset      = false;
            thumbnailImageReset = false;

            entry.Dispose();

            entry.FullImage.ShouldBeNull();
            entry.ThumbnailImage.ShouldBeNull();
            fullImageReset.ShouldBeTrue();
            fullStateReset.ShouldBeTrue();
            thumbnailImageReset.ShouldBeTrue();
        });
    }

    [Fact]
    public void Cover_Size_Change_Upgrades_The_Thumbnail_Decode_Bucket()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var source = new BytesImageSource(
                CreatePng(128, 128),
                $"cover-size-{Guid.NewGuid():N}");
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer
            {
                CoverWidth = 16,
                CoverHeight = 16,
                ItemsSource = [new ImagePreviewItem(source)]
            };
            using var host = new PreviewerHost(previewer);
            var entry = previewer.EffectiveItems.ShouldNotBeNull().Single();
            WaitUntil(
                () => entry.ThumbnailState == ImageLoadState.Loaded,
                "initial thumbnail load");
            GetThumbnailPixelSize(entry).ShouldBe(new PixelSize(16, 16));

            previewer.CoverWidth = 96;
            previewer.CoverHeight = 96;

            WaitUntil(
                () => entry.ThumbnailState == ImageLoadState.Loaded &&
                      GetThumbnailPixelSize(entry) == new PixelSize(96, 96),
                "upgraded thumbnail load");
        });
    }

    [Fact]
    public void Auto_Sized_Cover_Upgrades_From_The_Placeholder_To_Final_Bounds()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var source = new BytesImageSource(
                CreatePng(512, 512),
                $"auto-sized-cover-{Guid.NewGuid():N}");
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer
            {
                Width = 200,
                ItemsSource = [new ImagePreviewItem(source)]
            };
            using var host = new PreviewerHost(previewer);
            var entry = previewer.EffectiveItems.ShouldNotBeNull().Single();

            WaitUntil(
                () => entry.ThumbnailState == ImageLoadState.Loaded &&
                      GetThumbnailPixelSize(entry).Width >= 200 &&
                      GetThumbnailPixelSize(entry).Height >= 200,
                "auto-sized cover final decode bucket");

            GetThumbnailPixelSize(entry).ShouldBe(new PixelSize(208, 208));
        });
    }

    [Fact]
    public void Repeating_The_Same_Thumbnail_Request_Is_Idempotent()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var reads = 0;
            var bytes = CreatePng(128, 128);
            var source = new StreamImageSource(
                _ =>
                {
                    reads++;
                    return ValueTask.FromResult<Stream>(new MemoryStream(bytes));
                },
                $"thumbnail-idempotency-{Guid.NewGuid():N}",
                "v1");
            var item = new ImagePreviewItem(source)
            {
                RequestOptions = new ImageRequestOptions { CacheStorage = ImageCacheStoragePolicy.None }
            };
            using var entry = new ImagePreviewEntry(item);

            entry.LoadThumbnail(32, 32, ImageRequestPriority.High);
            WaitUntil(() => entry.ThumbnailState == ImageLoadState.Loaded, "initial thumbnail request");
            reads.ShouldBe(1);

            entry.LoadThumbnail(32, 32, ImageRequestPriority.High);
            WaitUntil(() => entry.ThumbnailState != ImageLoadState.Loading, "repeated thumbnail request");

            reads.ShouldBe(1);
            GetThumbnailPixelSize(entry).ShouldBe(new PixelSize(32, 32));
        });
    }

    [Fact]
    public void Smaller_Thumbnail_Request_Reuses_The_Existing_Decode()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var reads = 0;
            var bytes = CreatePng(512, 256);
            var source = new StreamImageSource(
                _ =>
                {
                    reads++;
                    return ValueTask.FromResult<Stream>(new MemoryStream(bytes));
                },
                $"thumbnail-downsize-{Guid.NewGuid():N}",
                "v1");
            var item = new ImagePreviewItem(source)
            {
                RequestOptions = new ImageRequestOptions { CacheStorage = ImageCacheStoragePolicy.None }
            };
            using var entry = new ImagePreviewEntry(item);

            entry.LoadThumbnail(208, 208, ImageRequestPriority.High);
            WaitUntil(() => entry.ThumbnailState == ImageLoadState.Loaded, "initial thumbnail request");

            entry.LoadThumbnail(208, 112, ImageRequestPriority.High);
            Dispatcher.UIThread.RunJobs();

            reads.ShouldBe(1);
            entry.ThumbnailState.ShouldBe(ImageLoadState.Loaded);
        });
    }

    [Fact]
    public void Svg_Cover_And_Full_Request_Reuse_The_Same_Vector_Decode()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var reads = 0;
            var bytes = Encoding.UTF8.GetBytes(
                "<svg xmlns='http://www.w3.org/2000/svg' width='128' height='96' " +
                "viewBox='0 0 128 96'><rect width='128' height='96' fill='#1677ff'/></svg>");
            var source = new StreamImageSource(
                _ =>
                {
                    reads++;
                    return ValueTask.FromResult<Stream>(new MemoryStream(bytes));
                },
                $"preview-vector-{Guid.NewGuid():N}",
                "v1");
            var previewer = CreatePreviewer(new ImagePreviewItem(source));
            using var host = new PreviewerHost(previewer);
            var entry = previewer.EffectiveItems.ShouldNotBeNull().Single();
            WaitUntil(() => previewer.IsCoverLoaded, "SVG cover load");

            previewer.OpenDialog();
            WaitUntil(() => previewer.IsCurrentLoaded, "SVG full load");

            reads.ShouldBe(1);
            entry.FullImage.ShouldBeSameAs(entry.ThumbnailImage);
            entry.FullOrigin.ShouldBe(ImageLoadOrigin.DecodedMemory);
            entry.FullImage.ShouldNotBeNull().Size.ShouldBe(new Size(128, 96));
        });
    }

    [Fact]
    public void Auto_Sized_Rectangular_Cover_Settles_After_The_Initial_Load()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var reads = 0;
            var bytes = CreatePng(512, 256);
            var source = new StreamImageSource(
                _ =>
                {
                    reads++;
                    return ValueTask.FromResult<Stream>(new MemoryStream(bytes));
                },
                $"auto-cover-settle-{Guid.NewGuid():N}",
                "v1");
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer
            {
                Width = 200,
                ItemsSource =
                [
                    new ImagePreviewItem(source)
                    {
                        RequestOptions = new ImageRequestOptions { CacheStorage = ImageCacheStoragePolicy.None }
                    }
                ]
            };
            using var host = new PreviewerHost(previewer);

            WaitUntil(() => previewer.IsCoverLoaded, "auto-sized rectangular cover");
            for (var index = 0; index < 10; index++)
            {
                Dispatcher.UIThread.RunJobs();
            }

            reads.ShouldBeLessThanOrEqualTo(2);
            previewer.Bounds.Width.ShouldBe(200);
            previewer.Bounds.Height.ShouldBeGreaterThan(0);
        });
    }

    [Fact]
    public void Width_Only_Cover_Decode_Leaves_The_Auto_Height_Unbounded()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var source = new BytesImageSource(
                CreatePng(1004, 986),
                $"width-only-cover-{Guid.NewGuid():N}");
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer
            {
                Width = 200,
                ItemsSource = [new ImagePreviewItem(source)]
            };
            using var host = new PreviewerHost(previewer, renderScaling: 2);
            var entry = previewer.EffectiveItems.ShouldNotBeNull().Single();

            WaitUntil(() => entry.ThumbnailState == ImageLoadState.Loaded, "width-only cover decode");

            GetThumbnailResultRequestSize(entry).ShouldBe(new PixelSize(400, 0));
            GetThumbnailPixelSize(entry).Width.ShouldBe(400);
        });
    }

    [Fact]
    public void Thumbnail_Upgrade_Keeps_The_Previous_Image_Until_Replacement_Succeeds()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var reads = 0;
            var upgradeStarted = NewSignal();
            var releaseUpgrade = NewSignal();
            var bytes = CreatePng(128, 128);
            var source = new StreamImageSource(
                async token =>
                {
                    if (Interlocked.Increment(ref reads) == 2)
                    {
                        upgradeStarted.TrySetResult();
                        await releaseUpgrade.Task.WaitAsync(token);
                    }
                    return new MemoryStream(bytes);
                },
                $"thumbnail-upgrade-{Guid.NewGuid():N}",
                "v1");
            var item = new ImagePreviewItem(source)
            {
                RequestOptions = new ImageRequestOptions { CacheStorage = ImageCacheStoragePolicy.None }
            };
            using var entry = new ImagePreviewEntry(item);

            entry.LoadThumbnail(16, 16, ImageRequestPriority.High);
            WaitUntil(() => entry.ThumbnailState == ImageLoadState.Loaded, "initial thumbnail");
            var previous = entry.ThumbnailImage.ShouldNotBeNull();
            GetThumbnailPixelSize(entry).ShouldBe(new PixelSize(16, 16));

            entry.LoadThumbnail(96, 96, ImageRequestPriority.High);
            WaitUntil(() => upgradeStarted.Task.IsCompleted, "thumbnail upgrade start");

            entry.ThumbnailState.ShouldBe(ImageLoadState.Loading);
            entry.ThumbnailImage.ShouldBeSameAs(previous);
            GetThumbnailPixelSize(entry).ShouldBe(new PixelSize(16, 16));

            releaseUpgrade.TrySetResult();
            WaitUntil(
                () => entry.ThumbnailState == ImageLoadState.Loaded &&
                      GetThumbnailPixelSize(entry) == new PixelSize(96, 96),
                "thumbnail upgrade completion");
            entry.ThumbnailImage.ShouldNotBeSameAs(previous);
        });
    }

    [Fact]
    public void Current_Item_Fallback_Does_Not_Change_Other_Item_Failure_State()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var fallback = new TestImage();
            var current = new ImagePreviewItem(
                new BytesImageSource(new byte[] { 1 }, "current-primary"))
            {
                FallbackSource = new BorrowedImageSource(fallback)
            };
            var neighbor = new ImagePreviewItem(
                new BytesImageSource(new byte[] { 2 }, "neighbor-primary"));
            var previewer = new TestPreviewer
            {
                Width = 96,
                Height = 96,
                PreloadCount = 1,
                ItemsSource = new[] { current, neighbor }
            };
            ImagePreviewOpenedEventArgs? opened = null;
            ImagePreviewFailedEventArgs? failed = null;
            previewer.ImageOpened += (_, args) => opened = args;
            previewer.ImageFailed += (_, args) => failed = args;
            using var host = new PreviewerHost(previewer);

            previewer.RequestPreviewLoads();
            var entries = previewer.EffectiveItems.ShouldNotBeNull();
            WaitUntil(
                () => entries[0].FullState == ImageLoadState.Loaded &&
                      entries[1].FullState == ImageLoadState.Failed,
                "fallback and neighbor terminal states");

            entries[0].FullImage.ShouldBeSameAs(fallback);
            entries[1].FullImage.ShouldBeNull();
            opened.ShouldNotBeNull().Item.ShouldBeSameAs(current);
            failed.ShouldBeNull();
        });
    }

    [Fact]
    public void Reload_Methods_Only_Restart_The_Target_Entry()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var firstReads = 0;
            var secondReads = 0;
            var first = new ImagePreviewItem(CreateFailingStreamSource("first", () => firstReads++));
            var second = new ImagePreviewItem(CreateFailingStreamSource("second", () => secondReads++));
            var previewer = new TestPreviewer
            {
                Width = 96,
                Height = 96,
                PreloadCount = 1,
                ItemsSource = new[] { first, second }
            };
            using var host = new PreviewerHost(previewer);

            previewer.RequestPreviewLoads();
            var entries = previewer.EffectiveItems.ShouldNotBeNull();
            WaitUntil(
                () => entries.All(entry => entry.FullState == ImageLoadState.Failed),
                "initial failed reads");
            firstReads.ShouldBe(1);
            secondReads.ShouldBe(1);

            previewer.ReloadItem(1);
            WaitUntil(() => secondReads == 2 && entries[1].FullState == ImageLoadState.Failed, "item reload");
            firstReads.ShouldBe(1);

            previewer.ReloadCurrent();
            WaitUntil(() => firstReads == 2 && entries[0].FullState == ImageLoadState.Failed, "current reload");
            secondReads.ShouldBe(2);
        });
    }

    [Fact]
    public void ReloadCover_Only_Restarts_The_Cover_Thumbnail_Channel()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var thumbnailReads = 0;
            var full = new TestImage();
            var previewer = CreatePreviewer(new ImagePreviewItem(new BorrowedImageSource(full))
            {
                ThumbnailSource = CreateFailingStreamSource("cover-thumbnail", () => thumbnailReads++)
            });
            using var host = new PreviewerHost(previewer);
            var entry = previewer.EffectiveItems.ShouldNotBeNull().Single();
            WaitUntil(() => previewer.IsCoverLoaded, "initial cover fallback");
            thumbnailReads.ShouldBe(1);
            entry.FullState.ShouldBe(ImageLoadState.Idle);

            previewer.ReloadCover();

            WaitUntil(
                () => thumbnailReads == 2 && previewer.CoverLoadState == ImageLoadState.Loaded,
                "cover reload");
            entry.FullState.ShouldBe(ImageLoadState.Idle);
            entry.FullImage.ShouldBeNull();
        });
    }

    [Fact]
    public void Dialog_Close_Releases_Full_Leases_But_Keeps_Cover_Lease()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var full = new TestImage();
            var thumbnail = new TestImage();
            var previewer = new ClosableImagePreviewer
            {
                Width = 96,
                Height = 96,
                ItemsSource =
                [
                    new ImagePreviewItem(new BorrowedImageSource(full))
                    {
                        ThumbnailSource = new BorrowedImageSource(thumbnail)
                    }
                ]
            };
            using var host = new PreviewerHost(previewer);
            WaitUntil(() => previewer.IsCoverLoaded, "cover load");
            previewer.OpenDialog();
            WaitUntil(() => previewer.IsCurrentLoaded, "full preview load");
            var entry = previewer.EffectiveItems.ShouldNotBeNull().Single();

            previewer.CloseForTest();

            entry.FullImage.ShouldBeNull();
            entry.FullState.ShouldBe(ImageLoadState.Idle);
            entry.ThumbnailImage.ShouldBeSameAs(thumbnail);
            entry.ThumbnailState.ShouldBe(ImageLoadState.Loaded);
        });
    }

    [Fact]
    public void Scheduler_Starts_Current_Then_Cover_Then_Neighbor_When_The_Read_Slot_Opens()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var blockerStarted = NewSignal();
            var releaseBlocker = NewSignal();
            var criticalStarted = NewSignal();
            var releaseCritical = NewSignal();
            var highStarted = NewSignal();
            var releaseHigh = NewSignal();
            var preloadStarted = NewSignal();
            var releasePreload = NewSignal();
            var loader = Application.Current.ShouldNotBeNull().GetImageLoader();
            var blockerTask = loader.LoadAsync(new ImageLoadRequest(CreateGatedSource(
                "blocker",
                blockerStarted,
                releaseBlocker))).AsTask();
            WaitUntil(() => blockerStarted.Task.IsCompleted, "scheduler blocker");

            var current = new ImagePreviewItem(CreateGatedSource(
                "critical",
                criticalStarted,
                releaseCritical))
            {
                ThumbnailSource = CreateGatedSource("high", highStarted, releaseHigh)
            };
            var neighbor = new ImagePreviewItem(CreateGatedSource(
                "preload",
                preloadStarted,
                releasePreload));
            var previewer = CreatePreviewer(current, neighbor);
            previewer.PreloadCount = 1;
            using var host = new PreviewerHost(previewer);
            WaitUntil(() => previewer.CoverLoadState == ImageLoadState.Loading, "queued cover load");
            previewer.RequestPreviewLoads();

            try
            {
                releaseBlocker.TrySetResult();
                WaitUntil(() => criticalStarted.Task.IsCompleted, "critical request start");
                highStarted.Task.IsCompleted.ShouldBeFalse();
                preloadStarted.Task.IsCompleted.ShouldBeFalse();

                releaseCritical.TrySetResult();
                WaitUntil(() => highStarted.Task.IsCompleted, "high request start");
                preloadStarted.Task.IsCompleted.ShouldBeFalse();

                releaseHigh.TrySetResult();
                WaitUntil(() => preloadStarted.Task.IsCompleted, "preload request start");
            }
            finally
            {
                releaseBlocker.TrySetResult();
                releaseCritical.TrySetResult();
                releaseHigh.TrySetResult();
                releasePreload.TrySetResult();
            }

            WaitUntil(() => blockerTask.IsCompleted, "blocker completion");
            blockerTask.GetAwaiter().GetResult().Dispose();
        });
    }

    [Fact]
    public void ImageSwitchMode_Defaults_To_Immediate()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var previewer = CreatePreviewer();
            previewer.ImageSwitchMode.ShouldBe(ImageSwitchMode.Immediate);
            previewer.ImageSwitchMode = ImageSwitchMode.WaitForLoaded;
            previewer.ImageSwitchMode.ShouldBe(ImageSwitchMode.WaitForLoaded);
        });
    }

    [Fact]
    public void Dialog_WaitForLoaded_Holds_The_Previous_Image_Until_The_Next_Load_Completes()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = new TestImage();
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var secondSource = new StreamImageSource(
                async token =>
                {
                    secondStarted.TrySetResult();
                    await releaseSecond.Task.WaitAsync(token);
                    return new MemoryStream(CreatePng(32, 32));
                },
                $"dialog-hold-{Guid.NewGuid():N}", "v1");
            var previewer = new TestPreviewer
            {
                Width = 96,
                Height = 96,
                PreloadCount = 0,
                ImageSwitchMode = ImageSwitchMode.WaitForLoaded,
                ItemsSource = new[]
                {
                    new ImagePreviewItem(new BorrowedImageSource(first)),
                    new ImagePreviewItem(secondSource)
                }
            };
            using var host = new PreviewerHost(previewer);
            var entries = previewer.EffectiveItems.ShouldNotBeNull();
            previewer.RequestPreviewLoads();
            WaitUntil(() => entries[0].FullState == ImageLoadState.Loaded, "first full load");

            var dialog = new ImagePreviewerDialog(new global::Avalonia.Controls.Window(), previewer);
            dialog.ItemsSource = entries;
            Dispatcher.UIThread.RunJobs();
            dialog.CurrentImage.ShouldBeSameAs(first);

            entries[1].LoadFull(16, 16, ImageRequestPriority.Critical);
            dialog.CurrentIndex = 1;
            WaitUntil(() => secondStarted.Task.IsCompleted, "second load start");
            Dispatcher.UIThread.RunJobs();
            dialog.CurrentImage.ShouldBeSameAs(first); // 保留上一张
            dialog.IsCurrentImageLoading.ShouldBeTrue();

            releaseSecond.TrySetResult();
            WaitUntil(() => entries[1].FullState == ImageLoadState.Loaded, "second full load");
            WaitUntil(() => ReferenceEquals(dialog.CurrentImage, entries[1].FullImage), "display swap");
            dialog.IsCurrentImageLoading.ShouldBeFalse();

            dialog.Close();
            dialog.CurrentImage.ShouldBeNull();
        });
    }

    [Fact]
    public void Dialog_Immediate_Clears_The_Previous_Image_And_Activates_Loading_Before_The_Request_Starts()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = new TestImage();
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var secondSource = new StreamImageSource(
                async token =>
                {
                    secondStarted.TrySetResult();
                    await releaseSecond.Task.WaitAsync(token);
                    return new MemoryStream(CreatePng(32, 32));
                },
                $"dialog-immediate-{Guid.NewGuid():N}", "v1");
            var previewer = new TestPreviewer
            {
                Width = 96,
                Height = 96,
                PreloadCount = 0,
                ImageSwitchMode = ImageSwitchMode.Immediate,
                ItemsSource = new[]
                {
                    new ImagePreviewItem(new BorrowedImageSource(first)),
                    new ImagePreviewItem(secondSource)
                }
            };
            using var host = new PreviewerHost(previewer);
            var entries = previewer.EffectiveItems.ShouldNotBeNull();
            previewer.RequestPreviewLoads();
            WaitUntil(() => entries[0].FullState == ImageLoadState.Loaded, "first full load");

            var dialog = new ImagePreviewerDialog(new global::Avalonia.Controls.Window(), previewer)
            {
                ItemsSource = entries
            };
            Dispatcher.UIThread.RunJobs();
            dialog.CurrentImage.ShouldBeSameAs(first);

            dialog.CurrentIndex = 1;
            Dispatcher.UIThread.RunJobs();

            dialog.CurrentImage.ShouldBeNull();
            dialog.IsCurrentImageLoading.ShouldBeTrue();
            var viewer = dialog.Content.ShouldBeOfType<Panel>()
                                  .Children.OfType<ImageViewer>().Single();
            viewer.Classes.Contains(":has-image").ShouldBeFalse();
            viewer.Classes.Contains(":loading").ShouldBeTrue();

            entries[1].LoadFull(16, 16, ImageRequestPriority.Critical);
            WaitUntil(() => secondStarted.Task.IsCompleted, "second load start");
            dialog.CurrentImage.ShouldBeNull();
            dialog.IsCurrentImageLoading.ShouldBeTrue();

            releaseSecond.TrySetResult();
            WaitUntil(() => entries[1].FullImage is not null &&
                            ReferenceEquals(dialog.CurrentImage, entries[1].FullImage),
                "second display");
            dialog.IsCurrentImageLoading.ShouldBeFalse();

            dialog.Close();
        });
    }

    [Fact]
    public void Dialog_Changing_To_Immediate_Recomputes_The_Pending_Target_Immediately()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = new TestImage();
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var secondSource = CreateGatedSource(
                $"dialog-mode-change-{Guid.NewGuid():N}",
                secondStarted,
                releaseSecond);
            var previewer = new TestPreviewer
            {
                Width = 96,
                Height = 96,
                PreloadCount = 0,
                ImageSwitchMode = ImageSwitchMode.WaitForLoaded,
                ItemsSource =
                [
                    new ImagePreviewItem(new BorrowedImageSource(first)),
                    new ImagePreviewItem(secondSource)
                ]
            };
            using var host = new PreviewerHost(previewer);
            var entries = previewer.EffectiveItems.ShouldNotBeNull();
            previewer.OpenDialog();
            WaitUntil(() => entries[0].FullState == ImageLoadState.Loaded, "first full load");
            var dialog = FindOpenHost(previewer).ShouldBeOfType<ImagePreviewerDialog>();

            previewer.CurrentIndex = 1;
            WaitUntil(() => secondStarted.Task.IsCompleted, "second load start");
            dialog.CurrentImage.ShouldBeSameAs(first);

            previewer.ImageSwitchMode = ImageSwitchMode.Immediate;
            Dispatcher.UIThread.RunJobs();

            dialog.CurrentImage.ShouldBeNull();
            dialog.IsCurrentImageLoading.ShouldBeTrue();

            releaseSecond.TrySetResult();
            previewer.IsOpen = false;
        });
    }

    [Fact]
    public void Dialog_WaitForLoaded_Shows_Error_When_The_Target_Fails()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = new TestImage();
            var previewer = new TestPreviewer
            {
                Width = 96,
                Height = 96,
                PreloadCount = 0,
                ImageSwitchMode = ImageSwitchMode.WaitForLoaded,
                ItemsSource = new[]
                {
                    new ImagePreviewItem(new BorrowedImageSource(first)),
                    new ImagePreviewItem(new BytesImageSource(new byte[] { 1 }, "dialog-fail"))
                }
            };
            using var host = new PreviewerHost(previewer);
            var entries = previewer.EffectiveItems.ShouldNotBeNull();
            previewer.RequestPreviewLoads();
            WaitUntil(() => entries[0].FullState == ImageLoadState.Loaded, "first full load");

            var dialog = new ImagePreviewerDialog(new global::Avalonia.Controls.Window(), previewer);
            dialog.ItemsSource = entries;
            Dispatcher.UIThread.RunJobs();

            entries[1].LoadFull(16, 16, ImageRequestPriority.Critical);
            dialog.CurrentIndex = 1;
            WaitUntil(() => dialog.IsCurrentImageFailed && dialog.CurrentImage is null, "failed display");
        });
    }

    [Fact]
    public void Dialog_WaitForLoaded_Clears_The_Held_Image_When_Its_Source_Item_Is_Removed()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = new TestImage();
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var secondSource = new StreamImageSource(
                async token =>
                {
                    secondStarted.TrySetResult();
                    await releaseSecond.Task.WaitAsync(token);
                    return new MemoryStream(CreatePng(32, 32));
                },
                $"dialog-remove-{Guid.NewGuid():N}", "v1");
            var items = new ObservableCollection<ImagePreviewItem>(
            [
                new ImagePreviewItem(new BorrowedImageSource(first)),
                new ImagePreviewItem(secondSource)
            ]);
            var previewer = new TestPreviewer
            {
                Width = 96,
                Height = 96,
                PreloadCount = 0,
                ImageSwitchMode = ImageSwitchMode.WaitForLoaded,
                ItemsSource = items
            };
            using var host = new PreviewerHost(previewer);
            var entries = previewer.EffectiveItems.ShouldNotBeNull();
            previewer.RequestPreviewLoads();
            WaitUntil(() => entries[0].FullState == ImageLoadState.Loaded, "first full load");

            var dialog = new ImagePreviewerDialog(new global::Avalonia.Controls.Window(), previewer);
            dialog.ItemsSource = entries;
            Dispatcher.UIThread.RunJobs();
            entries[1].LoadFull(16, 16, ImageRequestPriority.Critical);
            dialog.CurrentIndex = 1;
            WaitUntil(() => secondStarted.Task.IsCompleted, "second load start");
            Dispatcher.UIThread.RunJobs();
            dialog.CurrentImage.ShouldNotBeNull();
            dialog.CurrentImage.ShouldBeSameAs(first);

            items.RemoveAt(0); // 移除保留帧源

            WaitUntil(() => dialog.CurrentImage is null, "held image cleared after item removal");
            releaseSecond.TrySetResult();
        });
    }

    [Fact]
    public void Rapid_Shared_Source_Switching_Never_Reports_Cancellation_As_Failure()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            // 与 issue #450 demo 完全同构：两个 previewer 共享 item，每 tick 从磁盘复制出新文件、
            // 追加、裁剪并切换到最新；PreloadCount=1 令邻项在 Preload→Critical 升级时取消旧请求。
            // 同一图片源的请求在管线内合并为共享操作，竞争退出/拆除产生的内部取消
            // 绝不能被 ImageLoader 误判为 InvalidSource 失败（"canceled its own load operation"）。
            var root = Path.Combine(Path.GetTempPath(), $"issue450-regress-{Guid.NewGuid():N}");
            var templateDir = Path.Combine(root, "tpl");
            var feedDir = Path.Combine(root, "feed");
            Directory.CreateDirectory(templateDir);
            Directory.CreateDirectory(feedDir);
            try
            {
                var png = CreatePng(800, 520);
                var templates = new string[60];
                for (var t = 0; t < templates.Length; t++)
                {
                    templates[t] = Path.Combine(templateDir, $"tpl-{t:D4}.png");
                    File.WriteAllBytes(templates[t], png);
                }

                var failures = new List<string>();
                for (var round = 0; round < 2; round++)
                {
                    var items = new ObservableCollection<ImagePreviewItem>();
                    var holdPreviewer = new TestPreviewer
                    {
                        Width = 96,
                        Height = 96,
                        PreloadCount = 1,
                        ImageSwitchMode = ImageSwitchMode.WaitForLoaded,
                        ItemsSource = items
                    };
                    var immediatePreviewer = new TestPreviewer
                    {
                        Width = 96,
                        Height = 96,
                        PreloadCount = 1,
                        ImageSwitchMode = ImageSwitchMode.Immediate,
                        ItemsSource = items
                    };
                    holdPreviewer.ImageFailed += (_, args) =>
                        failures.Add($"hold:{args.Error.Code}:{args.Error.Message}");
                    immediatePreviewer.ImageFailed += (_, args) =>
                        failures.Add($"immediate:{args.Error.Code}:{args.Error.Message}");
                    using var holdHost = new PreviewerHost(holdPreviewer);
                    using var immediateHost = new PreviewerHost(immediatePreviewer);
                    holdPreviewer.OpenDialog();
                    immediatePreviewer.OpenDialog();

                    var counter = 0;
                    for (var tick = 0; tick < 150; tick++)
                    {
                        var feedFile = Path.Combine(feedDir, $"frame-{round}-{counter:D6}.png");
                        File.Copy(templates[counter % templates.Length], feedFile, true);
                        counter++;
                        items.Add(new ImagePreviewItem(ImageSource.Parse(new Uri(feedFile).AbsoluteUri)));
                        if (items.Count > 60)
                        {
                            items.RemoveAt(0);
                        }
                        holdPreviewer.CurrentIndex      = items.Count - 1;
                        immediatePreviewer.CurrentIndex = items.Count - 1;
                        Dispatcher.UIThread.RunJobs();
                        Thread.Sleep(2);
                    }
                    for (var spin = 0; spin < 40; spin++)
                    {
                        Dispatcher.UIThread.RunJobs();
                        Thread.Sleep(10);
                    }
                }

                failures.ShouldBeEmpty();
            }
            finally
            {
                Directory.Delete(root, true);
            }
        });
    }

    [Fact]
    public void Internal_Shared_Cancel_Does_Not_Fail_The_Preview_Entry()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var started = NewSignal();
            var release = NewSignal();
            var source = new StreamImageSource(
                async token =>
                {
                    started.TrySetResult();
                    await release.Task.WaitAsync(token);
                    return new MemoryStream(CreatePng(16, 16));
                },
                $"entry-foreign-cancel-{Guid.NewGuid():N}",
                "v1");
            var entry = new ImagePreviewEntry(new ImagePreviewItem(source));
            var failedRaised = false;
            entry.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(ImagePreviewEntry.IsFullFailed) && entry.IsFullFailed)
                {
                    failedRaised = true;
                }
            };

            entry.LoadFull(16, 16, ImageRequestPriority.Critical);
            WaitUntil(() => started.Task.IsCompleted, "full load start");

            // ClearCacheAsync(CancelInFlight) 以内部取消拆除在途共享操作，entry 自身 token 未取消；
            // 该取消必须按取消处理：不产生 Failed 提交与 FullError。
            Application.Current.ShouldNotBeNull()
                .GetImageLoader()
                .ClearCacheAsync(new ImageCacheClearRequest())
                .AsTask()
                .Wait(TimeSpan.FromSeconds(5));

            WaitUntil(() => entry.FullState != ImageLoadState.Loading, "foreign cancel resolves");
            entry.FullState.ShouldBe(ImageLoadState.Idle);
            entry.FullError.ShouldBeNull();
            failedRaised.ShouldBeFalse();

            release.TrySetResult();
            entry.Dispose();
        });
    }

    [Fact]
    public void Cover_WaitForLoaded_Keeps_The_Previous_Image_While_The_New_Cover_Loads()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = new TestImage();
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var secondSource = new StreamImageSource(
                async token =>
                {
                    secondStarted.TrySetResult();
                    await releaseSecond.Task.WaitAsync(token);
                    return new MemoryStream(CreatePng(32, 32));
                },
                $"cover-hold-{Guid.NewGuid():N}", "v1");
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer
            {
                Width = 96,
                Height = 96,
                ImageSwitchMode = ImageSwitchMode.WaitForLoaded,
                ItemsSource = new[]
                {
                    new ImagePreviewItem(new BorrowedImageSource(first)),
                    new ImagePreviewItem(secondSource)
                }
            };
            using var host = new PreviewerHost(previewer);
            WaitUntil(() => previewer.IsCoverLoaded, "first cover load");
            previewer.EffectiveCoverImage.ShouldBeSameAs(first);

            previewer.CoverIndex = 1;
            WaitUntil(() => secondStarted.Task.IsCompleted, "second cover load start");
            Dispatcher.UIThread.RunJobs();
            previewer.EffectiveCoverImage.ShouldBeSameAs(first); // 封面保持上一张
            previewer.IsCoverLoading.ShouldBeTrue();

            releaseSecond.TrySetResult();
            var entries = previewer.EffectiveItems.ShouldNotBeNull();
            WaitUntil(() => previewer.CoverLoadState == ImageLoadState.Loaded &&
                             ReferenceEquals(previewer.EffectiveCoverImage, entries[1].ThumbnailImage),
                "cover swap after load");
        });
    }

    [Fact]
    public void Cover_Immediate_Clears_The_Previous_Image_And_Shows_Loading_While_The_New_Cover_Loads()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = new TestImage();
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var secondSource = new StreamImageSource(
                async token =>
                {
                    secondStarted.TrySetResult();
                    await releaseSecond.Task.WaitAsync(token);
                    return new MemoryStream(CreatePng(32, 32));
                },
                $"cover-immediate-{Guid.NewGuid():N}", "v1");
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer
            {
                Width = 96,
                Height = 96,
                ImageSwitchMode = ImageSwitchMode.Immediate,
                ItemsSource = new[]
                {
                    new ImagePreviewItem(new BorrowedImageSource(first)),
                    new ImagePreviewItem(secondSource)
                }
            };
            using var host = new PreviewerHost(previewer);
            WaitUntil(() => previewer.IsCoverLoaded, "first cover load");

            previewer.CoverIndex = 1;
            WaitUntil(() => secondStarted.Task.IsCompleted, "second cover load start");
            Dispatcher.UIThread.RunJobs();
            previewer.EffectiveCoverImage.ShouldBeNull();
            previewer.IsCoverLoading.ShouldBeTrue();

            releaseSecond.TrySetResult();
        });
    }

    [Fact]
    public void Cover_Changing_To_Immediate_Recomputes_The_Pending_Target_Immediately()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = new TestImage();
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer
            {
                Width = 96,
                Height = 96,
                ImageSwitchMode = ImageSwitchMode.WaitForLoaded,
                ItemsSource =
                [
                    new ImagePreviewItem(new BorrowedImageSource(first)),
                    new ImagePreviewItem(CreateGatedSource(
                        $"cover-mode-change-{Guid.NewGuid():N}",
                        secondStarted,
                        releaseSecond))
                ]
            };
            using var host = new PreviewerHost(previewer);
            WaitUntil(() => previewer.IsCoverLoaded, "first cover load");

            previewer.CoverIndex = 1;
            WaitUntil(() => secondStarted.Task.IsCompleted, "second cover load start");
            previewer.EffectiveCoverImage.ShouldBeSameAs(first);

            previewer.ImageSwitchMode = ImageSwitchMode.Immediate;
            Dispatcher.UIThread.RunJobs();

            previewer.EffectiveCoverImage.ShouldBeNull();
            previewer.IsCoverLoading.ShouldBeTrue();
            releaseSecond.TrySetResult();
        });
    }

    [Fact]
    public void Cover_WaitForLoaded_Never_Flashes_A_Null_Image_During_Target_Switches()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = new TestImage();
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var secondSource = new StreamImageSource(
                async token =>
                {
                    secondStarted.TrySetResult();
                    await releaseSecond.Task.WaitAsync(token);
                    return new MemoryStream(CreatePng(32, 32));
                },
                $"cover-no-flash-{Guid.NewGuid():N}", "v1");
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer
            {
                Width = 96,
                Height = 96,
                ImageSwitchMode = ImageSwitchMode.WaitForLoaded,
                ItemsSource = new[]
                {
                    new ImagePreviewItem(new BorrowedImageSource(first)),
                    new ImagePreviewItem(secondSource)
                }
            };
            using var host = new PreviewerHost(previewer);
            WaitUntil(() => previewer.IsCoverLoaded, "first cover load");

            // 记录切换全程的 EffectiveCoverImage 通知值：不允许出现 null
            // （Idle 瞬态的 null 会经 mask 透明度过渡放大为可见闪烁）
            var observed = new List<IImage?>();
            previewer.GetPropertyChangedObservable(global::AtomUI.Desktop.Controls.ImagePreviewer.EffectiveCoverImageProperty)
                .Subscribe(_ => observed.Add(previewer.EffectiveCoverImage));
            previewer.CoverIndex = 1;
            WaitUntil(() => secondStarted.Task.IsCompleted, "second cover load start");
            releaseSecond.TrySetResult();
            WaitUntil(() => previewer.CoverLoadState == ImageLoadState.Loaded, "second cover load");

            observed.ShouldNotBeEmpty();
            observed.Count(v => v is null).ShouldBe(0);
        });
    }

    [Fact]
    public void Same_Bucket_Priority_Upgrade_Promotes_The_Queued_Request_Without_Restarting_It()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var blockerStarted = NewSignal();
            var releaseBlocker = NewSignal();
            var promotedStarted = NewSignal();
            var releasePromoted = NewSignal();
            var competingStarted = NewSignal();
            var releaseCompeting = NewSignal();
            var loader = Application.Current.ShouldNotBeNull().GetImageLoader();
            var blockerTask = loader.LoadAsync(new ImageLoadRequest(CreateGatedSource(
                $"priority-blocker-{Guid.NewGuid():N}",
                blockerStarted,
                releaseBlocker))).AsTask();
            WaitUntil(() => blockerStarted.Task.IsCompleted, "priority blocker");

            var promoted = new ImagePreviewEntry(new ImagePreviewItem(CreateGatedSource(
                $"priority-promoted-{Guid.NewGuid():N}",
                promotedStarted,
                releasePromoted)));
            var competing = new ImagePreviewEntry(new ImagePreviewItem(CreateGatedSource(
                $"priority-competing-{Guid.NewGuid():N}",
                competingStarted,
                releaseCompeting)));

            try
            {
                promoted.LoadFull(16, 16, ImageRequestPriority.Preload);
                competing.LoadFull(16, 16, ImageRequestPriority.High);

                // 同尺寸桶下 Preload→Critical 必须提升原在途 waiter，不能取消并重启源。
                promoted.LoadFull(16, 16, ImageRequestPriority.Critical);
                releaseBlocker.TrySetResult();

                WaitUntil(
                    () => promotedStarted.Task.IsCompleted || competingStarted.Task.IsCompleted,
                    "first queued request");
                promotedStarted.Task.IsCompleted.ShouldBeTrue();
                competingStarted.Task.IsCompleted.ShouldBeFalse();
            }
            finally
            {
                releaseBlocker.TrySetResult();
                releasePromoted.TrySetResult();
                releaseCompeting.TrySetResult();
                promoted.Dispose();
                competing.Dispose();
            }

            WaitUntil(() => blockerTask.IsCompleted, "priority blocker completion");
            blockerTask.GetAwaiter().GetResult().Dispose();
        });
    }

    private static global::AtomUI.Desktop.Controls.ImagePreviewer CreatePreviewer(
        params ImagePreviewItem[] items)
    {
        return CreatePreviewer((IEnumerable<ImagePreviewItem>)items);
    }

    private static global::AtomUI.Desktop.Controls.ImagePreviewer CreatePreviewer(
        IEnumerable<ImagePreviewItem> items)
    {
        return new global::AtomUI.Desktop.Controls.ImagePreviewer
        {
            Width = 96,
            Height = 96,
            PreloadCount = 0,
            ItemsSource = items
        };
    }

    private static ImagePreviewItem CreateItem(string key)
    {
        return new ImagePreviewItem(new BorrowedImageSource(new TestImage(), key));
    }

    private static ImageSource CreateFailingStreamSource(string key, Action onOpen)
    {
        return new StreamImageSource(
            _ =>
            {
                onOpen();
                return ValueTask.FromResult<Stream>(new MemoryStream(new byte[] { 1 }));
            },
            key,
            "v1");
    }

    private static ImageSource CreateGatedSource(
        string key,
        TaskCompletionSource started,
        TaskCompletionSource release)
    {
        return new StreamImageSource(
            async token =>
            {
                started.TrySetResult();
                await release.Task.WaitAsync(token);
                return new MemoryStream(new byte[] { 1 });
            },
            key,
            "v1");
    }

    private static TaskCompletionSource NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static PixelSize GetThumbnailPixelSize(ImagePreviewEntry entry)
    {
        return entry.ThumbnailImage.ShouldBeOfType<Bitmap>().PixelSize;
    }

    private static PixelSize? GetThumbnailResultRequestSize(ImagePreviewEntry entry)
    {
        return (PixelSize?)typeof(ImagePreviewEntry)
            .GetField(
                "_thumbnailResultRequestSize",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(entry);
    }

    private static PixelSize GetThumbnailOriginalSize(ImagePreviewEntry entry)
    {
        var result = (ImageLoadResult?)typeof(ImagePreviewEntry)
            .GetField(
                "_thumbnailResult",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(entry);
        result.ShouldNotBeNull();
        return new PixelSize(result.OriginalPixelWidth, result.OriginalPixelHeight);
    }

    private static Control? FindOpenHost(AbstractImagePreviewer previewer)
    {
        var state = typeof(AbstractImagePreviewer)
            .GetField("_openState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(previewer);
        if (state is null)
        {
            return null;
        }
        return state.GetType().GetProperty("DialogHost")?.GetValue(state) as Control
               ?? state.GetType().GetProperty("PreviewHost")?.GetValue(state) as Control;
    }

    private static byte[] CreatePng(int width, int height)
    {
        using var output = new MemoryStream();
        output.Write([0x89, (byte)'P', (byte)'N', (byte)'G', 0x0d, 0x0a, 0x1a, 0x0a]);

        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header[..4], checked((uint)width));
        BinaryPrimitives.WriteUInt32BigEndian(header[4..8], checked((uint)height));
        header[8] = 8;
        header[9] = 6;
        WriteChunk(output, "IHDR", header);

        using var raw = new MemoryStream();
        var row = new byte[checked(width * 4 + 1)];
        for (var y = 0; y < height; y++)
        {
            raw.Write(row);
        }
        raw.Position = 0;
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            raw.CopyTo(zlib);
        }
        WriteChunk(output, "IDAT", compressed.ToArray());
        WriteChunk(output, "IEND", []);
        return output.ToArray();
    }

    private static void WriteChunk(Stream output, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, checked((uint)data.Length));
        output.Write(length);
        var typeBytes = Encoding.ASCII.GetBytes(type);
        output.Write(typeBytes);
        output.Write(data);

        var crcBytes = new byte[typeBytes.Length + data.Length];
        typeBytes.CopyTo(crcBytes, 0);
        data.CopyTo(crcBytes.AsSpan(typeBytes.Length));
        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, CalculateCrc32(crcBytes));
        output.Write(crc);
    }

    private static uint CalculateCrc32(ReadOnlySpan<byte> bytes)
    {
        var crc = uint.MaxValue;
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) != 0 ? 0xedb88320u ^ (crc >> 1) : crc >> 1;
            }
        }
        return ~crc;
    }

    private static void WaitUntil(Func<bool> predicate, string description)
    {
        var timeout = System.Diagnostics.Stopwatch.StartNew();
        while (!predicate())
        {
            Dispatcher.UIThread.RunJobs();
            if (timeout.Elapsed >= TimeSpan.FromSeconds(5))
            {
                throw new TimeoutException($"Timed out waiting for {description}.");
            }
            Thread.Yield();
        }
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class PreviewerHost : IDisposable
    {
        private readonly Avalonia.Controls.Window _window;

        internal PreviewerHost(Control previewer, double? renderScaling = null)
        {
            _window = new Avalonia.Controls.Window
            {
                Width = 320,
                Height = 240,
                Content = previewer
            };
            _window.Show();
            if (renderScaling is not null)
            {
                _window.SetRenderScaling(renderScaling.Value);
                _window.UpdateLayout();
            }
            Dispatcher.UIThread.RunJobs();
        }

        internal void Detach()
        {
            _window.Content = null;
            Dispatcher.UIThread.RunJobs();
        }

        internal void Attach(Control previewer)
        {
            _window.Content = previewer;
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class TestPreviewer : AbstractImagePreviewer
    {
    }

    private sealed class ClosableImagePreviewer : global::AtomUI.Desktop.Controls.ImagePreviewer
    {
        internal void CloseForTest()
        {
            CloseDialog();
        }
    }

    private sealed class ResettableCollection<T> : ObservableCollection<T>
    {
        internal ResettableCollection(IEnumerable<T> items)
            : base(items)
        {
        }

        internal void ResetWith(IEnumerable<T> items)
        {
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    private sealed class TestImage : IImage
    {
        public Size Size => new(24, 24);

        public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
        {
        }
    }
}
