using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.ImageLoading;

public class AsyncImageLazyTests
{
    static AsyncImageLazyTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class CountingSource
    {
        private int _requests;

        // When a gate is supplied the stream stays pending until the gate completes.
        internal CountingSource(string identity, Task? gate = null)
        {
            Source = new StreamImageSource(
                async _ =>
                {
                    Interlocked.Increment(ref _requests);
                    if (gate is not null)
                    {
                        await gate;
                    }
                    return new MemoryStream(ImageControlTestHost.CreatePng(32, 32));
                },
                identity,
                "v1");
        }

        internal ImageSource Source { get; }

        internal int Requests => Volatile.Read(ref _requests);
    }

    // A 200px viewport with a 1000px spacer above the image, so the image starts below the fold.
    private static (ScrollContentPresenter Scroller, AsyncImage Image) CreateScrollHost(CountingSource source, bool isLazy)
    {
        var image = new AsyncImage
        {
            Width = 32,
            Height = 32,
            IsLazy = isLazy,
            Source = source.Source
        };
        var panel = new StackPanel
        {
            Children =
            {
                new Border { Height = 1000 },
                image,
                new Border { Height = 1000 }
            }
        };
        var scroller = new ScrollContentPresenter
        {
            Width = 200,
            Height = 200,
            CanVerticallyScroll = true,
            Content = panel
        };
        return (scroller, image);
    }

    private static void ScrollTo(ScrollContentPresenter scroller, double y)
    {
        scroller.Offset = new Vector(0, y);
        Dispatcher.UIThread.RunJobs();
        scroller.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public void IsLazy_defaults_to_false()
    {
        new AsyncImage().IsLazy.ShouldBeFalse();
    }

    [Fact]
    public void Lazy_image_outside_viewport_does_not_request()
    {
        var source = new CountingSource("lazy-outside");
        var (scroller, image) = CreateScrollHost(source, isLazy: true);
        using var host = new ImageControlTestHost(scroller, 200, 200);
        for (var i = 0; i < 3; i++)
        {
            scroller.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        source.Requests.ShouldBe(0);
        image.LoadState.ShouldBe(ImageLoadState.Idle);
    }

    [Fact]
    public void Lazy_image_requests_once_after_entering_viewport()
    {
        var source = new CountingSource("lazy-enter");
        var (scroller, image) = CreateScrollHost(source, isLazy: true);
        using var host = new ImageControlTestHost(scroller, 200, 200);
        source.Requests.ShouldBe(0);

        ScrollTo(scroller, 900);
        ImageControlTestHost.WaitUntil(() => image.IsLoaded, "lazy image load after entering viewport");
        source.Requests.ShouldBe(1);

        // Moving around inside the viewport must not issue further requests.
        ScrollTo(scroller, 950);
        ScrollTo(scroller, 920);
        source.Requests.ShouldBe(1);
    }

    [Fact]
    public void Lazy_image_scrolled_out_keeps_result()
    {
        var source = new CountingSource("lazy-keep");
        var (scroller, image) = CreateScrollHost(source, isLazy: true);
        using var host = new ImageControlTestHost(scroller, 200, 200);
        source.Requests.ShouldBe(0);
        ScrollTo(scroller, 900);
        ImageControlTestHost.WaitUntil(() => image.IsLoaded, "lazy image load");

        ScrollTo(scroller, 0);
        image.IsLoaded.ShouldBeTrue();
        image.LoadedImage.ShouldNotBeNull();

        ScrollTo(scroller, 900);
        image.IsLoaded.ShouldBeTrue();
        source.Requests.ShouldBe(1);
    }

    [Fact]
    public void Zero_size_does_not_count_as_visible()
    {
        var source = new CountingSource("lazy-zero");
        var image = new AsyncImage
        {
            Width = 0,
            Height = 0,
            IsLazy = true,
            Source = source.Source
        };
        using var host = new ImageControlTestHost(image);
        for (var i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }

        source.Requests.ShouldBe(0);
        image.LoadState.ShouldBe(ImageLoadState.Idle);

        image.Width = 32;
        image.Height = 32;
        ImageControlTestHost.WaitUntil(() => image.IsLoaded, "lazy image load after gaining size");
        source.Requests.ShouldBe(1);
    }

    [Fact]
    public void Lazy_image_under_collapsed_parent_never_loads()
    {
        var source = new CountingSource("lazy-collapsed");
        var image = new AsyncImage
        {
            Width = 32,
            Height = 32,
            IsLazy = true,
            Source = source.Source
        };
        var parent = new Border { IsVisible = false, Child = image };
        var root = new StackPanel { Children = { parent } };
        using var host = new ImageControlTestHost(root);

        // Layout jitter around a collapsed subtree must not trigger or re-register anything.
        for (var i = 0; i < 5; i++)
        {
            root.Width = 100 + i;
            root.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        source.Requests.ShouldBe(0);
        image.LoadState.ShouldBe(ImageLoadState.Idle);

        // Once the parent is expanded the image becomes visible and loads exactly once.
        parent.IsVisible = true;
        ImageControlTestHost.WaitUntil(() => image.IsLoaded, "lazy image load after expand");
        source.Requests.ShouldBe(1);
    }

    [Fact]
    public void Release_load_cancels_and_reloads_on_demand()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var source = new CountingSource("release-cancel", gate.Task);
        var (scroller, image) = CreateScrollHost(source, isLazy: false);
        var opened = 0;
        image.ImageOpened += (_, _) => opened++;
        using var host = new ImageControlTestHost(scroller, 200, 200);
        ImageControlTestHost.WaitUntil(() => source.Requests == 1, "initial request");
        image.IsLoading.ShouldBeTrue();

        // The image sits below the fold, so after the release it waits for the viewport.
        image.ReleaseLoad();
        image.LoadState.ShouldBe(ImageLoadState.Idle);
        image.LoadedImage.ShouldBeNull();

        // The abandoned request completing late must not resurrect the released image.
        gate.SetResult();
        for (var i = 0; i < 10; i++)
        {
            Thread.Sleep(20);
            Dispatcher.UIThread.RunJobs();
        }
        image.LoadState.ShouldBe(ImageLoadState.Idle);
        image.LoadedImage.ShouldBeNull();
        opened.ShouldBe(0);

        ScrollTo(scroller, 900);
        ImageControlTestHost.WaitUntil(() => image.IsLoaded, "reload after returning to the viewport");
        opened.ShouldBe(1);
    }

    [Fact]
    public void Release_load_in_viewport_reloads_on_next_layout()
    {
        var source = new CountingSource("release-visible");
        var image = new AsyncImage
        {
            Width = 32,
            Height = 32,
            Source = source.Source
        };
        using var host = new ImageControlTestHost(image);
        ImageControlTestHost.WaitUntil(() => image.IsLoaded, "initial load");

        image.ReleaseLoad();
        image.LoadState.ShouldBe(ImageLoadState.Idle);
        image.LoadedImage.ShouldBeNull();

        ImageControlTestHost.WaitUntil(() => image.IsLoaded, "reload while still visible");
        image.LoadedImage.ShouldNotBeNull();
    }

    [Fact]
    public void Release_load_outside_viewport_waits_and_source_change_reloads()
    {
        var first = new CountingSource("release-first");
        var second = new CountingSource("release-second");
        var (scroller, image) = CreateScrollHost(first, isLazy: false);
        using var host = new ImageControlTestHost(scroller, 200, 200);
        ImageControlTestHost.WaitUntil(() => image.IsLoaded, "initial load");

        image.ReleaseLoad();
        image.LoadedImage.ShouldBeNull();
        image.LoadState.ShouldBe(ImageLoadState.Idle);
        for (var i = 0; i < 3; i++)
        {
            scroller.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }
        first.Requests.ShouldBe(1);
        image.LoadState.ShouldBe(ImageLoadState.Idle);

        image.Source = second.Source;
        ImageControlTestHost.WaitUntil(() => image.IsLoaded, "load after source change");
        second.Requests.ShouldBe(1);
    }

    [Fact]
    public void Non_lazy_behavior_unchanged()
    {
        var source = new CountingSource("non-lazy");
        var (scroller, image) = CreateScrollHost(source, isLazy: false);
        using var host = new ImageControlTestHost(scroller, 200, 200);

        // Without IsLazy the image loads on attach even though it sits below the fold.
        ImageControlTestHost.WaitUntil(() => image.IsLoaded, "non-lazy image load");
        source.Requests.ShouldBe(1);
    }
}
