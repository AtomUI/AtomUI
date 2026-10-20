using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

public class ImagePreviewDisplayTrackerTests
{
    public ImagePreviewDisplayTrackerTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void WaitForLoaded_Keeps_The_Previous_Image_While_The_Target_Is_Loading()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = NewLoadedEntry("first");
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var second = NewGatedEntry("second", secondStarted, releaseSecond);
            var mode = ImageSwitchMode.WaitForLoaded;
            using var tracker = NewTracker(() => mode);

            tracker.SetCurrentItem(first);
            WaitUntil(() => ReferenceEquals(tracker.EffectiveImage, first.FullImage), "first display");

            tracker.SetCurrentItem(second);
            StartGatedLoad(second);
            WaitUntil(() => secondStarted.Task.IsCompleted, "second load start");
            Dispatcher.UIThread.RunJobs();
            tracker.EffectiveImage.ShouldBeSameAs(first.FullImage);
            tracker.IsCurrentLoading.ShouldBeTrue();
            tracker.IsCurrentFailed.ShouldBeFalse();

            releaseSecond.TrySetResult();
            WaitUntil(() => ReferenceEquals(tracker.EffectiveImage, second.FullImage), "display swap after load");
            tracker.IsCurrentLoading.ShouldBeFalse();

            second.Dispose();
            first.Dispose();
        });
    }

    [Fact]
    public void Immediate_Clears_The_Previous_Image_And_Reports_Loading_While_The_Target_Is_Pending()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = NewLoadedEntry("first");
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var second = NewGatedEntry("second", secondStarted, releaseSecond);
            var mode = ImageSwitchMode.Immediate;
            using var tracker = NewTracker(() => mode);

            tracker.SetCurrentItem(first);
            WaitUntil(() => ReferenceEquals(tracker.EffectiveImage, first.FullImage), "first display");

            tracker.SetCurrentItem(second);
            tracker.EffectiveImage.ShouldBeNull();
            tracker.IsCurrentLoading.ShouldBeTrue(); // Idle 瞬态也必须立即驱动 loading presenter

            StartGatedLoad(second);
            WaitUntil(() => secondStarted.Task.IsCompleted, "second load start");
            Dispatcher.UIThread.RunJobs();
            tracker.EffectiveImage.ShouldBeNull();
            tracker.IsCurrentLoading.ShouldBeTrue();

            releaseSecond.TrySetResult();
            WaitUntil(() => ReferenceEquals(tracker.EffectiveImage, second.FullImage), "display after load");

            second.Dispose();
            first.Dispose();
        });
    }

    [Fact]
    public void WaitForLoaded_Drops_The_Held_Image_When_The_Target_Fails()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = NewLoadedEntry("first");
            var failing = new ImagePreviewEntry(new ImagePreviewItem(new BytesImageSource(
                new byte[] { 1 }, $"tracker-fail-{Guid.NewGuid():N}")));
            var mode = ImageSwitchMode.WaitForLoaded;
            using var tracker = NewTracker(() => mode);

            tracker.SetCurrentItem(first);
            WaitUntil(() => ReferenceEquals(tracker.EffectiveImage, first.FullImage), "first display");

            tracker.SetCurrentItem(failing);
            failing.LoadFull(16, 16, ImageRequestPriority.Critical);
            WaitUntil(() => tracker.IsCurrentFailed && tracker.EffectiveImage is null, "failed display");

            failing.Dispose();
            first.Dispose();
        });
    }

    [Fact]
    public void Disposing_The_Retained_Source_Clears_The_Held_Image()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = NewLoadedEntry("first");
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var second = NewGatedEntry("second", secondStarted, releaseSecond);
            var mode = ImageSwitchMode.WaitForLoaded;
            using var tracker = NewTracker(() => mode);

            tracker.SetCurrentItem(first);
            WaitUntil(() => ReferenceEquals(tracker.EffectiveImage, first.FullImage), "first display");
            tracker.SetCurrentItem(second);
            StartGatedLoad(second);
            WaitUntil(() => secondStarted.Task.IsCompleted && tracker.EffectiveImage is not null,
                "held display");
            tracker.EffectiveImage.ShouldBeSameAs(first.FullImage);

            first.Dispose(); // 模拟集合移除保留帧源

            WaitUntil(() => tracker.EffectiveImage is null, "held image cleared after dispose");
            tracker.IsCurrentLoading.ShouldBeTrue();

            releaseSecond.TrySetResult();
            second.Dispose();
        });
    }

    [Fact]
    public void Disposing_An_Entry_Clears_Display_Holders_Before_Releasing_The_Image_Lease()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var image = new TestImage();
            ImagePreviewDisplayTracker? tracker = null;
            IImage? displayAtLeaseRelease = image;
            var lease = new CallbackDisposable(() => displayAtLeaseRelease = tracker?.EffectiveImage);
            var result = new ImageLoadResult(
                image,
                lease,
                24,
                24,
                24,
                24,
                "image/test",
                ImageLoadOrigin.Local,
                ImageSourceValidation.Current,
                "test-content");
            var entry = new ImagePreviewEntry(
                new ImagePreviewItem(new BorrowedImageSource(image, "dispose-order")));
            typeof(ImagePreviewEntry)
                .GetField("_fullResult", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(entry, result);
            var mode = ImageSwitchMode.WaitForLoaded;
            tracker = NewTracker(() => mode);
            tracker.SetCurrentItem(entry);
            tracker.EffectiveImage.ShouldBeSameAs(image);

            entry.Dispose();

            displayAtLeaseRelease.ShouldBeNull();
            tracker.EffectiveImage.ShouldBeNull();
            tracker.Dispose();
        });
    }

    [Fact]
    public void Unloading_An_Entry_Clears_Display_Holders_Before_Releasing_The_Image_Lease()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var image = new TestImage();
            ImagePreviewDisplayTracker? tracker = null;
            IImage? displayAtLeaseRelease = image;
            var entry = NewEntryWithInjectedFullResult(
                image,
                new CallbackDisposable(() => displayAtLeaseRelease = tracker?.EffectiveImage),
                "unload-order");
            var mode = ImageSwitchMode.WaitForLoaded;
            tracker = NewTracker(() => mode);
            tracker.SetCurrentItem(entry);
            tracker.EffectiveImage.ShouldBeSameAs(image);

            entry.UnloadFull();

            displayAtLeaseRelease.ShouldBeNull();
            tracker.EffectiveImage.ShouldBeNull();
            tracker.Dispose();
            entry.Dispose();
        });
    }

    [Fact]
    public void Failed_Commit_Never_Publishes_A_Failed_State_With_The_Previous_Image()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var image = new TestImage();
            var entry = NewEntryWithInjectedFullResult(
                image,
                new CallbackDisposable(() => { }),
                "failed-commit",
                new BytesImageSource(new byte[] { 1 }, $"failed-commit-{Guid.NewGuid():N}"));
            var mode = ImageSwitchMode.WaitForLoaded;
            ImagePreviewDisplayTracker? tracker = null;
            var observedInvalidState = false;
            tracker = new ImagePreviewDisplayTracker(
                () => mode,
                () => observedInvalidState |= tracker!.IsCurrentFailed && tracker.EffectiveImage is not null);
            tracker.SetCurrentItem(entry);

            entry.LoadFull(24, 24, ImageRequestPriority.Critical, reload: true);
            WaitUntil(() => tracker.IsCurrentFailed, "failed replacement commit");

            observedInvalidState.ShouldBeFalse();
            tracker.EffectiveImage.ShouldBeNull();
            tracker.Dispose();
            entry.Dispose();
        });
    }

    [Fact]
    public void Clear_Drops_Everything_And_Unsubscribes_All_Entries()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var notified = false;
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var first = NewLoadedEntry("first");
            var second = NewGatedEntry("second", secondStarted, releaseSecond);
            var mode = ImageSwitchMode.WaitForLoaded;
            using var tracker = new ImagePreviewDisplayTracker(() => mode, () => notified = true);

            tracker.SetCurrentItem(first);
            WaitUntil(() => tracker.EffectiveImage is not null, "first display");
            tracker.SetCurrentItem(second);
            StartGatedLoad(second);
            WaitUntil(() => secondStarted.Task.IsCompleted, "second load start");

            tracker.Clear();

            tracker.EffectiveImage.ShouldBeNull();
            tracker.IsCurrentLoading.ShouldBeFalse();
            tracker.IsCurrentFailed.ShouldBeFalse();
            notified.ShouldBeTrue(); // Clear 归零显示，回调按设计触发一次
            notified = false;

            releaseSecond.TrySetResult();
            WaitUntil(() => second.FullState == ImageLoadState.Loaded, "second load completes unobserved");
            Dispatcher.UIThread.RunJobs();
            notified.ShouldBeFalse(); // 退订生效：后续事件不再触达 tracker
            tracker.EffectiveImage.ShouldBeNull();

            second.Dispose();
            first.Dispose();
        });
    }

    [Fact]
    public void Switching_Current_Unsubscribes_The_Previous_Current_Entry()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var notified = false;
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var first = NewLoadedEntry("first");
            var second = NewGatedEntry("second", secondStarted, releaseSecond);
            var third = NewLoadedEntry("third");
            var mode = ImageSwitchMode.Immediate;
            using var tracker = new ImagePreviewDisplayTracker(() => mode, () => notified = true);

            tracker.SetCurrentItem(second);
            StartGatedLoad(second);
            WaitUntil(() => secondStarted.Task.IsCompleted, "second load start");
            tracker.SetCurrentItem(third);
            WaitUntil(() => tracker.EffectiveImage is not null, "third display");
            notified = false;

            releaseSecond.TrySetResult(); // 已被切走的 second 完成，不应触达 tracker
            WaitUntil(() => second.FullState == ImageLoadState.Loaded, "second load completes unobserved");
            Dispatcher.UIThread.RunJobs();
            notified.ShouldBeFalse();
            tracker.EffectiveImage.ShouldBeSameAs(third.FullImage);

            second.Dispose();
            third.Dispose();
            first.Dispose();
        });
    }

    [Fact]
    public void Immediate_Mode_Does_Not_Retain_The_Previous_Entry()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = NewLoadedEntry("first");
            var pending = new ImagePreviewEntry(
                new ImagePreviewItem(new BytesImageSource(
                    new byte[] { 1 }, $"tracker-immediate-pending-{Guid.NewGuid():N}")));
            var mode = ImageSwitchMode.Immediate;
            using var tracker = NewTracker(() => mode);

            tracker.SetCurrentItem(first);
            WaitUntil(() => tracker.EffectiveImage is not null, "first display");
            tracker.SetCurrentItem(pending);

            tracker.EffectiveImage.ShouldBeNull();
            tracker.IsCurrentLoading.ShouldBeTrue();
            GetRetainedItem(tracker).ShouldBeNull();

            first.Dispose();
            pending.Dispose();
        });
    }

    [Fact]
    public void Retained_Item_Invariant_Holds_Across_Switches()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = NewLoadedEntry("first");
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var second = NewGatedEntry("second", secondStarted, releaseSecond);
            var mode = ImageSwitchMode.WaitForLoaded;
            using var tracker = NewTracker(() => mode);

            tracker.SetCurrentItem(first);
            WaitUntil(() => tracker.EffectiveImage is not null, "first display");
            tracker.SetCurrentItem(second);
            StartGatedLoad(second);
            WaitUntil(() => secondStarted.Task.IsCompleted, "second load start");

            var retained = GetRetainedItem(tracker).ShouldNotBeNull();
            retained.ShouldBeSameAs(first);
            retained.FullImage.ShouldNotBeNull(); // 不变量：retained 恒持有可用图

            releaseSecond.TrySetResult();
            WaitUntil(() => ReferenceEquals(tracker.EffectiveImage, second.FullImage), "display swap");
            GetRetainedItem(tracker).ShouldBeSameAs(second);

            second.Dispose();
            first.Dispose();
        });
    }

    [Fact]
    public void WaitForLoaded_Holds_The_Retained_Image_During_The_Idle_Transient()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = NewLoadedEntry("first");
            // 目标项尚处 Idle（LoadFull 未调用）：切换瞬间的重算不得产生显示空洞
            var pending = new ImagePreviewEntry(
                new ImagePreviewItem(new BytesImageSource(
                    new byte[] { 1 }, $"tracker-idle-{Guid.NewGuid():N}")));
            var mode = ImageSwitchMode.WaitForLoaded;
            using var tracker = NewTracker(() => mode);

            tracker.SetCurrentItem(first);
            WaitUntil(() => ReferenceEquals(tracker.EffectiveImage, first.FullImage), "first display");

            tracker.SetCurrentItem(pending); // Idle：无图、未加载、未失败

            tracker.EffectiveImage.ShouldBeSameAs(first.FullImage);

            pending.Dispose();
            first.Dispose();
        });
    }

    [Fact]
    public void WaitForLoaded_Pulls_A_Seed_Targeted_By_The_Current_Tracker()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var seedStarted = NewSignal();
            var releaseSeed = NewSignal();
            var seed = NewGatedEntry("seed", seedStarted, releaseSeed);
            var pending = new ImagePreviewEntry(
                new ImagePreviewItem(new BytesImageSource(
                    new byte[] { 1 }, $"tracker-seed-{Guid.NewGuid():N}")));
            ImagePreviewEntry? latestLoaded = null;
            var mode = ImageSwitchMode.WaitForLoaded;
            using var tracker = new ImagePreviewDisplayTracker(
                () => mode,
                () => { },
                () => latestLoaded);

            tracker.SetCurrentItem(seed);
            StartGatedLoad(seed);
            WaitUntil(() => seedStarted.Task.IsCompleted, "seed load start");
            tracker.SetCurrentItem(pending);

            releaseSeed.TrySetResult();
            WaitUntil(() => seed.FullImage is not null, "superseded seed completion");
            latestLoaded = seed;
            tracker.Refresh();

            tracker.EffectiveImage.ShouldBeSameAs(seed.FullImage);

            seed.Dispose();
            WaitUntil(() => tracker.EffectiveImage is null, "seed invalidated clears display");

            pending.Dispose();
        });
    }

    [Fact]
    public void WaitForLoaded_Does_Not_Reuse_A_Target_Marker_From_Another_Tracker_Session()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var seed = NewLoadedEntry("other-session-seed");
            var mode = ImageSwitchMode.WaitForLoaded;
            using (var otherTracker = NewTracker(() => mode))
            {
                otherTracker.SetCurrentItem(seed);
            }
            var pending = new ImagePreviewEntry(
                new ImagePreviewItem(new BytesImageSource(
                    new byte[] { 1 }, $"tracker-new-session-{Guid.NewGuid():N}")));
            using var tracker = new ImagePreviewDisplayTracker(
                () => mode,
                () => { },
                () => seed);

            tracker.SetCurrentItem(pending);

            tracker.EffectiveImage.ShouldBeNull();
            tracker.IsCurrentLoading.ShouldBeTrue();

            pending.Dispose();
            seed.Dispose();
        });
    }

    [Fact]
    public void WaitForLoaded_Does_Not_Display_A_Preloaded_Entry_That_Was_Never_A_Target()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var preload = NewLoadedEntry("preload");
            var pending = new ImagePreviewEntry(
                new ImagePreviewItem(new BytesImageSource(
                    new byte[] { 1 }, $"tracker-current-{Guid.NewGuid():N}")));
            var mode = ImageSwitchMode.WaitForLoaded;
            using var tracker = new ImagePreviewDisplayTracker(
                () => mode,
                () => { },
                () => preload);

            tracker.SetCurrentItem(pending);

            tracker.EffectiveImage.ShouldBeNull();
            tracker.IsCurrentLoading.ShouldBeTrue();

            pending.Dispose();
            preload.Dispose();
        });
    }

    [Fact]
    public void WaitForLoaded_Advances_To_A_Newer_Completed_Previous_Target()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = NewLoadedEntry("first");
            var secondStarted = NewSignal();
            var releaseSecond = NewSignal();
            var second = NewGatedEntry("second", secondStarted, releaseSecond);
            var thirdStarted = NewSignal();
            var releaseThird = NewSignal();
            var third = NewGatedEntry("third", thirdStarted, releaseThird);
            ImagePreviewEntry latestLoaded = first;
            var mode = ImageSwitchMode.WaitForLoaded;
            using var tracker = new ImagePreviewDisplayTracker(
                () => mode,
                () => { },
                () => latestLoaded);

            tracker.SetCurrentItem(first);
            WaitUntil(() => ReferenceEquals(tracker.EffectiveImage, first.FullImage), "first display");

            tracker.SetCurrentItem(second);
            StartGatedLoad(second);
            WaitUntil(() => secondStarted.Task.IsCompleted, "second load start");
            tracker.SetCurrentItem(third);

            releaseSecond.TrySetResult();
            WaitUntil(() => second.FullImage is not null, "second load completion");
            latestLoaded = second;
            tracker.SetCurrentItem(third); // 当前目标未变时也允许重新计算最新完成候选

            tracker.EffectiveImage.ShouldBeSameAs(second.FullImage);
            tracker.IsCurrentLoading.ShouldBeTrue();

            StartGatedLoad(third);
            WaitUntil(() => thirdStarted.Task.IsCompleted, "third load start");
            releaseThird.TrySetResult();
            WaitUntil(() => third.FullImage is not null &&
                            ReferenceEquals(tracker.EffectiveImage, third.FullImage),
                "third display");

            third.Dispose();
            second.Dispose();
            first.Dispose();
        });
    }

    [Fact]
    public void WaitForLoaded_Does_Not_Regress_When_An_Older_Target_Completes_Late()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var first = NewLoadedEntry("first");
            var olderStarted = NewSignal();
            var releaseOlder = NewSignal();
            var older = NewGatedEntry("older", olderStarted, releaseOlder);
            var newer = NewLoadedEntry("newer");
            var pending = new ImagePreviewEntry(
                new ImagePreviewItem(new BytesImageSource(
                    new byte[] { 1 }, $"tracker-pending-{Guid.NewGuid():N}")));
            ImagePreviewEntry latestLoaded = first;
            var mode = ImageSwitchMode.WaitForLoaded;
            using var tracker = new ImagePreviewDisplayTracker(
                () => mode,
                () => { },
                () => latestLoaded);

            tracker.SetCurrentItem(first);
            tracker.SetCurrentItem(older);
            StartGatedLoad(older);
            WaitUntil(() => olderStarted.Task.IsCompleted, "older load start");
            tracker.SetCurrentItem(newer);
            WaitUntil(() => ReferenceEquals(tracker.EffectiveImage, newer.FullImage), "newer display");
            tracker.SetCurrentItem(pending);

            releaseOlder.TrySetResult();
            WaitUntil(() => older.FullImage is not null, "older late completion");
            latestLoaded = older;
            tracker.SetCurrentItem(pending);

            tracker.EffectiveImage.ShouldBeSameAs(newer.FullImage);

            pending.Dispose();
            newer.Dispose();
            older.Dispose();
            first.Dispose();
        });
    }

    private static ImagePreviewDisplayTracker NewTracker(Func<ImageSwitchMode> mode)
    {
        return new ImagePreviewDisplayTracker(mode, () => { });
    }

    private static ImagePreviewEntry NewLoadedEntry(string key)
    {
        var entry = new ImagePreviewEntry(
            new ImagePreviewItem(new BorrowedImageSource(new TestImage(), key)));
        // tracker 不发起加载，测试显式驱动（与既有 entry 级测试一致）
        entry.LoadFull(16, 16, ImageRequestPriority.Critical);
        WaitUntil(() => entry.FullState == ImageLoadState.Loaded, $"{key} load");
        return entry;
    }

    private static ImagePreviewEntry NewGatedEntry(
        string key,
        TaskCompletionSource started,
        TaskCompletionSource release)
    {
        return new ImagePreviewEntry(new ImagePreviewItem(new StreamImageSource(
            async token =>
            {
                started.TrySetResult();
                await release.Task.WaitAsync(token);
                return new MemoryStream(CreatePng(16, 16));
            },
            $"tracker-{key}-{Guid.NewGuid():N}",
            "v1")));
    }

    private static void StartGatedLoad(ImagePreviewEntry entry)
    {
        entry.LoadFull(16, 16, ImageRequestPriority.Critical);
    }

    private static ImagePreviewEntry? GetRetainedItem(ImagePreviewDisplayTracker tracker)
    {
        return (ImagePreviewEntry?)typeof(ImagePreviewDisplayTracker)
            .GetField(
                "_retainedItem",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(tracker);
    }

    private static ImagePreviewEntry NewEntryWithInjectedFullResult(
        IImage image,
        IDisposable lease,
        string key,
        ImageSource? source = null)
    {
        var result = new ImageLoadResult(
            image,
            lease,
            24,
            24,
            24,
            24,
            "image/test",
            ImageLoadOrigin.Local,
            ImageSourceValidation.Current,
            "test-content");
        var entry = new ImagePreviewEntry(
            new ImagePreviewItem(source ?? new BorrowedImageSource(image, key)));
        typeof(ImagePreviewEntry)
            .GetField("_fullResult", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(entry, result);
        return entry;
    }

    private static TaskCompletionSource NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

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

    private static uint CalculateCrc32(byte[] bytes)
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

    private sealed class TestImage : IImage
    {
        public Size Size => new(24, 24);

        public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
        {
        }
    }

    private sealed class CallbackDisposable(Action callback) : IDisposable
    {
        private Action? _callback = callback;

        public void Dispose()
        {
            Interlocked.Exchange(ref _callback, null)?.Invoke();
        }
    }
}
