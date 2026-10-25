using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace AtomUI.Utils;

internal static class PointerOverRefresh
{
    private static readonly ConditionalWeakTable<TopLevel, PendingRefresh> s_pending = new();

    internal static void Request(Visual owner)
    {
        var topLevel = TopLevel.GetTopLevel(owner);
        if (topLevel?.PlatformImpl is null || !topLevel.IsVisible)
        {
            return;
        }

        s_pending.GetValue(topLevel, static root => new PendingRefresh(root)).Request();
    }

    private sealed class PendingRefresh
    {
        private readonly WeakReference<TopLevel> _owner;
        private readonly Dispatcher _dispatcher;
        private bool _pending;

        internal PendingRefresh(TopLevel owner)
        {
            _owner = new WeakReference<TopLevel>(owner);
            _dispatcher = owner.Dispatcher;
        }

        internal void Request()
        {
            _dispatcher.VerifyAccess();
            if (_pending || !_owner.TryGetTarget(out var owner))
            {
                return;
            }

            var visual = ElementComposition.GetElementVisual(owner);
            if (visual is null)
            {
                return;
            }

            // A scene notification already in flight can observe an intermediate
            // layout. Follow the committed frame with one redraw per TopLevel;
            // the renderer then refreshes pointer-over against the final scene.
            var rendered = visual.Compositor.RequestCompositionBatchCommitAsync().Rendered;
            _pending = true;
            rendered.ContinueWith(static (task, state) => ((PendingRefresh)state!).Queue(task),
                this, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private void Queue(Task rendered)
        {
            if (rendered.IsFaulted)
            {
                _ = rendered.Exception;
            }
            try
            {
                _dispatcher.Post(() => Complete(rendered), DispatcherPriority.Background);
            }
            catch (InvalidOperationException)
            {
                // The original dispatcher can reject work during shutdown. This
                // state owns only a weak window reference and cannot retain it.
            }
        }

        private void Complete(Task rendered)
        {
            _pending = false;
            if (rendered.IsCompletedSuccessfully && _owner.TryGetTarget(out var owner) &&
                owner.IsVisible && owner.PlatformImpl is not null)
            {
                owner.InvalidateVisual();
            }
        }
    }
}
