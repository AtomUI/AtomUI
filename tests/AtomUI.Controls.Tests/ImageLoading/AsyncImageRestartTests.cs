using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.ImageLoading;

// A Source assigned before the first layout pass starts a request with a provisional decode size; the arrange pass
// then restarts it with the real size while the first source read is still pending. The restart must not inherit
// the cancellation of the request it replaced.
public class AsyncImageRestartTests
{
    private const int Iterations = 200;

    static AsyncImageRestartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class GatedSource
    {
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _requests;

        // The stream stays pending until Complete(), so every restart happens while the first read is in flight.
        internal GatedSource(string name)
        {
            var bytes = ImageControlTestHost.CreatePng(8, 8);
            Source = new StreamImageSource(
                async _ =>
                {
                    Interlocked.Increment(ref _requests);
                    await _gate.Task;
                    return new MemoryStream(bytes);
                },
                $"restart-{name}-{Guid.NewGuid():N}",
                "v1");
        }

        internal ImageSource Source { get; }

        internal int Requests => Volatile.Read(ref _requests);

        internal void Complete() => _gate.TrySetResult();
    }

    public enum SourceTiming
    {
        BeforeWindowShow,
        BeforeAttach,
        AfterAttachBeforeLayout
    }

    [Theory]
    [InlineData(SourceTiming.BeforeWindowShow)]
    [InlineData(SourceTiming.BeforeAttach)]
    [InlineData(SourceTiming.AfterAttachBeforeLayout)]
    public void Source_Set_Before_First_Layout_Loads_After_Size_Restart(SourceTiming timing)
    {
        var failures = new List<string>();
        var restarts = 0;
        for (var iteration = 0; iteration < Iterations; iteration++)
        {
            var source = new GatedSource($"{timing}-{iteration}");
            // Only the height is fixed, so the pre-layout decode size (0 x 32) differs from the arranged one.
            var image = new AsyncImage { Height = 32 };
            var panel = new Panel();
            var window = new Window { Width = 160, Height = 120, Content = panel };
            var starts = CountLoaderEvents(ImageLoaderEventKind.Started, out var unsubscribe);
            if (timing == SourceTiming.BeforeWindowShow)
            {
                image.Source = source.Source;
                panel.Children.Add(image);
                window.Show();
            }
            else
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                if (timing == SourceTiming.BeforeAttach)
                {
                    image.Source = source.Source;
                    panel.Children.Add(image);
                }
                else
                {
                    panel.Children.Add(image);
                    image.Source = source.Source;
                }
            }
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            try
            {
                ImageControlTestHost.WaitUntil(() => source.Requests > 0, "source read start");
                source.Complete();
                ImageControlTestHost.WaitUntil(
                    () => image.LoadState is ImageLoadState.Loaded or ImageLoadState.Failed,
                    "terminal load state");
                if (starts() > 1)
                {
                    restarts++;
                }
                if (image.LoadState != ImageLoadState.Loaded)
                {
                    failures.Add($"#{iteration}: {image.LoadState} {image.LoadError?.Message} " +
                                 $"{image.LoadError?.Exception?.GetType().Name}");
                }
            }
            finally
            {
                unsubscribe();
                window.Close();
                Dispatcher.UIThread.RunJobs();
            }
        }
        // Every iteration must actually exercise the size restart, otherwise the test proves nothing.
        restarts.ShouldBe(Iterations);
        failures.ShouldBeEmpty($"{failures.Count}/{Iterations} iterations did not load");
    }

    [Fact]
    public void Reassigned_Source_Does_Not_Join_A_Read_Its_Previous_Request_Abandoned()
    {
        var source = new GatedSource("reassigned");
        var image = new AsyncImage { Width = 32, Height = 32, Source = source.Source };
        var cancellations = CountLoaderEvents(ImageLoaderEventKind.Canceled, out var unsubscribe);
        using var host = new ImageControlTestHost(image);
        try
        {
            ImageControlTestHost.WaitUntil(() => source.Requests == 1, "first source read");

            // The only waiter leaves and the shared read is told to cancel; it may not have finished yet.
            image.Source = null;
            ImageControlTestHost.WaitUntil(() => cancellations() == 1, "first request cancellation");

            image.Source = source.Source;
            source.Complete();
            ImageControlTestHost.WaitUntil(
                () => image.LoadState is ImageLoadState.Loaded or ImageLoadState.Failed,
                "terminal load state");

            image.LoadError.ShouldBeNull();
            image.LoadState.ShouldBe(ImageLoadState.Loaded);
            source.Requests.ShouldBe(2);
        }
        finally
        {
            unsubscribe();
        }
    }

    // Counts loader events of one kind raised from now on; completion events arrive from pool threads.
    private static Func<int> CountLoaderEvents(ImageLoaderEventKind kind, out Action unsubscribe)
    {
        var loader = (ImageLoader)Avalonia.Application.Current!.GetImageLoader();
        var count = 0;
        EventHandler<ImageLoaderEventArgs> handler = (_, args) =>
        {
            if (args.Kind == kind)
            {
                Interlocked.Increment(ref count);
            }
        };
        loader.LoadEvent += handler;
        unsubscribe = () => loader.LoadEvent -= handler;
        return () => Volatile.Read(ref count);
    }
}
