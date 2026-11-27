using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// Subscribes a handler to the deactivation of a window on the platforms that report window deactivation; the
/// color picker in AtomUI.Desktop.Controls.ColorPicker uses it.
/// </summary>
/// <remarks>
/// <c>Subscribe</c> returns <see langword="null" /> and attaches nothing when the runtime platform does not support
/// window deactivation or the top level is not a <c>WindowBase</c>. Otherwise the returned subscription detaches
/// the handler when it is disposed.
/// </remarks>
internal static class TopLevelDeactivation
{
    internal static IDisposable? Subscribe(TopLevel? topLevel, EventHandler handler)
    {
        if (!RuntimePlatform.Features.SupportsWindowDeactivation ||
            topLevel is not WindowBase window)
        {
            return null;
        }

        window.Deactivated += handler;
        return new WindowDeactivationSubscription(window, handler);
    }

    private sealed class WindowDeactivationSubscription : IDisposable
    {
        private WindowBase? _window;
        private EventHandler? _handler;

        public WindowDeactivationSubscription(WindowBase window, EventHandler handler)
        {
            _window  = window;
            _handler = handler;
        }

        public void Dispose()
        {
            if (_window is not null && _handler is not null)
            {
                _window.Deactivated -= _handler;
                _window              =  null;
                _handler             =  null;
            }
        }
    }
}
