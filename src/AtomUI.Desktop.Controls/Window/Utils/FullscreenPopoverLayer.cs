using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
internal class FullscreenPopoverLayer : TemplatedControl
{
    private const double PopoverTriggerZoneHeight = 1;

    private Window? _hostWindow;
    private Border? _popoverBorder;
    private CompositeDisposable? _disposables;
    private bool _popoverEnabled;

    public FullscreenPopoverLayer()
    {
        IsVisible = false;
    }

    public void Attach(Window hostWindow)
    {
        if (_disposables != null)
        {
            return;
        }

        _hostWindow = hostWindow;
        _disposables = new CompositeDisposable(2);

        _disposables.Add(hostWindow.GetObservable(Window.WindowStateProperty)
            .Subscribe(OnWindowStateChanged));

        hostWindow.PointerMoved += OnHostWindowPointerMoved;
        _disposables.Add(Disposable.Create(() =>
            hostWindow.PointerMoved -= OnHostWindowPointerMoved));
    }

    public void Detach()
    {
        if (_disposables == null)
        {
            return;
        }

        _disposables.Dispose();
        _disposables = null;
        _popoverEnabled = false;
        _hostWindow = null;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _popoverBorder = e.NameScope.Find<Border>("PART_PopoverBorder");
    }

    private void OnWindowStateChanged(WindowState state)
    {
        if (state == WindowState.FullScreen)
        {
            IsVisible = true;
            SetPopoverVisible(false);
            _popoverEnabled = true;
        }
        else
        {
            IsVisible = false;
            SetPopoverVisible(false);
            _popoverEnabled = false;
        }
    }

    private void OnHostWindowPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_popoverEnabled || _hostWindow == null || _popoverBorder == null)
        {
            return;
        }

        var pos = e.GetPosition(_hostWindow);
        var titleBarHeight = _hostWindow.TitleBarHeight;

        if (!_popoverBorder.IsVisible && pos.Y <= PopoverTriggerZoneHeight)
        {
            SetPopoverVisible(true);
        }
        else if (_popoverBorder.IsVisible && pos.Y > titleBarHeight)
        {
            SetPopoverVisible(false);
        }
    }

    private void SetPopoverVisible(bool visible)
    {
        if (_popoverBorder != null)
        {
            _popoverBorder.IsVisible = visible;
        }
    }

}
