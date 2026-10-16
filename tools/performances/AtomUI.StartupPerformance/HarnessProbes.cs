using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace AtomUI.StartupPerformance;

/// <summary>首次 Render 后请求一个合成批次，批次渲染完成再等到 ApplicationIdle，然后退出进程。</summary>
internal sealed class RenderSentinel : Control
{
    private bool _reported;

    public RenderSentinel()
    {
        Width  = 8;
        Height = 8;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));
        if (_reported)
        {
            return;
        }
        _reported = true;
        StartupTimeline.Mark("Mark.FirstRender");
        Dispatcher.UIThread.Post(RequestRenderedBatch, DispatcherPriority.Send);
    }

    private void RequestRenderedBatch()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var visual   = topLevel is null ? null : ElementComposition.GetElementVisual(topLevel);
        if (visual is null)
        {
            Complete();
            return;
        }
        visual.Compositor.RequestCompositionBatchCommitAsync().Rendered.ContinueWith(
            static _ =>
            {
                StartupTimeline.Mark("Mark.FirstRenderedBatch");
                Dispatcher.UIThread.Post(Complete, DispatcherPriority.ApplicationIdle);
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private static void Complete()
    {
        StartupTimeline.Mark("Mark.DispatcherIdle");
        UiGapTracker.Stop();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}

/// <summary>
/// 用独立后台线程测量启动期间 UI 线程的最长不可响应间隔（真实调度延迟）。
/// 不用 DispatcherTimer：它由平台 runloop 服务，macOS 上主循环启动前几乎不触发
/// （实测 176–330 ms 窗口只 tick 1 次），测不出启动期的卡顿。
/// </summary>
internal static class UiGapTracker
{
    private static readonly object Gate = new();
    private static Dispatcher? _dispatcher;
    private static long _start;
    private static long _firstPump;
    private static double _maxGap;
    private static int _probeCount;
    private static volatile bool _running;
    private static Thread? _worker;

    internal static void Start()
    {
        if (_running)
        {
            return;
        }
        // Dispatcher.UIThread 首次访问必须在 UI 线程上完成：从后台线程访问会触发平台
        // dispatcher 初始化并以"不同线程拥有该对象"失败。这里先捕获实例再交给工作线程。
        _dispatcher = Dispatcher.UIThread;
        _running = true;
        _start = StartupTimeline.Now();
        _worker = new Thread(ProbeLoop, 64 * 1024)
        {
            IsBackground = true,
            Name = "AtomUI.StartupPerformance.UiGap"
        };
        _worker.Start();
    }

    private static void ProbeLoop()
    {
        while (_running)
        {
            var postedAt = StartupTimeline.Now();
            // 连续投递、不等回执：UI 线程被阻塞时多个探针会一起堆积，各自记录自己的等待时长，
            // 最大值即为该次阻塞的长度。若每个探针都等回执，长阻塞会把采样率拖到个位数。
            _dispatcher?.Post(() =>
            {
                var now = StartupTimeline.Now();
                lock (Gate)
                {
                    _probeCount++;
                    if (_firstPump == 0)
                    {
                        // 首个探针被服务：UI 线程此前根本没泵消息（主循环尚未开始），
                        // 这段等待属于"界面还没反应"，不算阻塞卡顿，单独记为 Mark.FirstUiPump。
                        _firstPump = now;
                    }
                    else if (postedAt >= _firstPump)
                    {
                        var gap = StartupTimeline.ToMilliseconds(now - postedAt);
                        if (gap > _maxGap)
                        {
                            _maxGap = gap;
                        }
                    }
                }
            }, DispatcherPriority.Send);
            Thread.Sleep(10);
        }
    }

    internal static void Stop()
    {
        if (!_running)
        {
            return;
        }
        _running = false;
        _worker?.Join(1000);
        _worker = null;
        lock (Gate)
        {
            if (_firstPump != 0)
            {
                StartupTimeline.Set("Mark.FirstUiPump", StartupTimeline.ToMilliseconds(_firstPump - _start));
            }
            StartupTimeline.Set("Mark.MaxUiGap", _maxGap);
            StartupTimeline.Set("Diag.UiGapProbes", _probeCount);
        }
        StartupTimeline.Set("Diag.UiGapWindowMs", StartupTimeline.ToMilliseconds(StartupTimeline.Now() - _start));
    }
}
