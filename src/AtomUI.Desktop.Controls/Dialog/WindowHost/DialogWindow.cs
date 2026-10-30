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

    // TopLevel 默认把样式宿主父级固定为 Application（IStyleHost.StylingParent => _globalStyles），
    // owner 实例级 Semantic Style 因此永远进不了独立窗口宿主；窗口模板应用时 ContentPresenter
    // 还会改写 surface 的继承父，模板之后才挂载的内容/按钮同样走不到 owner 样式链。
    // 按 Avalonia PopupRoot 的既有范式（PopupRoot: IStyleHost.StylingParent => Parent）把样式
    // 宿主链交还给逻辑父（owner Dialog），Dialog.Styles 里的 owner 作用域样式与一级嵌套样式
    // 才能级联到窗口宿主子树。判断的是父级是否生根（窗口 Show 后自身即 ILogicalRoot，
    // 自身的附加状态不可作依据）：owner 未生根（脱离页面树直接构造 presenter 的场景）时
    // 退回 Application，保证 ControlTheme 仍可达；已挂载时全局样式经 owner 所在主窗口的链继续可达。
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
