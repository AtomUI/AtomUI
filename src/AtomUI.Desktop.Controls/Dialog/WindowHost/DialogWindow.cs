using AtomUI.Native;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

internal sealed class DialogWindow : Window, IStyleHost
{
    private readonly INativeWindowSizingHook? _nativeSizingHook;
    private DialogWindowCloseState _closeState;
    private bool _isNativeUserResizeInProgress;

    internal event EventHandler? CloseRequested;
    internal event EventHandler? NativeUserResizeCompleted;

    internal bool IsNativeUserResizeInProgress =>
        _nativeSizingHook?.IsUserResizeInProgress ?? _isNativeUserResizeInProgress;

    protected override Type StyleKeyOverride { get; } = typeof(Window);

    // TopLevel fixes the style host parent to Application (IStyleHost.StylingParent => _globalStyles) by default,
    // so the instance level Semantic Styles of the owner never reach the standalone window host. When the window
    // template is applied, the ContentPresenter also rewrites the inherited parent of the surface, so content and
    // buttons attached only after the template cannot reach the owner style chain either.
    // Following the existing Avalonia PopupRoot pattern (PopupRoot: IStyleHost.StylingParent => Parent), the style
    // host chain is handed back to the logical parent (the owner Dialog). Only then can the owner scoped styles in
    // Dialog.Styles and the styles nested one level deep cascade into the window host subtree.
    // The condition tests whether the parent is rooted. After Show the window is itself an ILogicalRoot, so its own
    // attachment state cannot be the criterion. When the owner is not rooted (a presenter constructed directly,
    // detached from the page tree), this falls back to Application so that the ControlTheme stays reachable; when
    // it is attached, the global styles stay reachable through the chain of the main window that holds the owner.
    IStyleHost? IStyleHost.StylingParent
        => Parent is { } parent && ((ILogical)parent).IsAttachedToLogicalTree
            ? parent
            : Application.Current;

    internal DialogWindow()
    {
        _nativeSizingHook = NativeWindowSizing.TryAttachCsdSizingHook(this, () => IsCsdEnabled);
        if (_nativeSizingHook is not null)
        {
            _nativeSizingHook.UserResizeCompleted += HandleNativeUserResizeCompleted;
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (_closeState == DialogWindowCloseState.Open &&
            e.CloseReason != WindowCloseReason.OwnerWindowClosing)
        {
            e.Cancel = true;
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        ReleaseNativeSizingHook();
        CancelNativeUserResize();
        base.OnClosed(e);
    }

    internal void CloseFromPresenter()
    {
        _closeState = DialogWindowCloseState.Closing;
        Close();
    }

    internal void ReleasePresenterHooks()
    {
        ReleaseNativeSizingHook();
        CancelNativeUserResize();
    }

    internal Size ApplyRequestedSize(double width, double height)
    {
        var requestedSize = new Size(
            double.IsNaN(width)
                ? Math.Clamp(ClientSize.Width, MinWidth, MaxWidth)
                : Math.Clamp(width, MinWidth, MaxWidth),
            double.IsNaN(height)
                ? Math.Clamp(ClientSize.Height, MinHeight, MaxHeight)
                : Math.Clamp(height, MinHeight, MaxHeight));
        if (IsVisible && IsNativeUserResizeInProgress)
        {
            return ClientSize;
        }

        Width  = requestedSize.Width;
        Height = requestedSize.Height;
        if (!IsVisible)
        {
            if (ClientSize != requestedSize)
            {
                ClientSize = requestedSize;
            }

            return requestedSize;
        }

        // Width/Height request a platform resize through layout. Once shown, ClientSize
        // belongs to the platform callback: native constraints may reject or adjust the
        // request, leaving an unpainted strip if the requested size is published as actual.
        return requestedSize;
    }

    internal void BeginNativeUserResize()
    {
        if (_nativeSizingHook is not null)
        {
            _nativeSizingHook.BeginUserResize();
            return;
        }

        _isNativeUserResizeInProgress = true;
    }

    internal void CompleteNativeUserResize()
    {
        if (_nativeSizingHook is not null)
        {
            _nativeSizingHook.CompleteUserResize();
            return;
        }

        if (!_isNativeUserResizeInProgress)
        {
            return;
        }

        _isNativeUserResizeInProgress = false;
        NativeUserResizeCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void CancelNativeUserResize()
    {
        _nativeSizingHook?.CancelUserResize();
        _isNativeUserResizeInProgress = false;
    }

    private void ReleaseNativeSizingHook()
    {
        if (_nativeSizingHook is null)
        {
            return;
        }

        _nativeSizingHook.UserResizeCompleted -= HandleNativeUserResizeCompleted;
        _nativeSizingHook.Dispose();
    }

    private void HandleNativeUserResizeCompleted(object? sender, EventArgs e)
    {
        NativeUserResizeCompleted?.Invoke(this, EventArgs.Empty);
    }

    private enum DialogWindowCloseState
    {
        Open,
        Closing
    }
}
