using System.Reactive.Disposables;
using System.Reactive.Linq;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

internal sealed class WindowDialogPresenter : IDialogPresenter
{
    private readonly Dialog _dialog;
    private readonly TopLevel _owner;
    private readonly DialogSurface _surface;
    private readonly DialogResourceBridge _resourceBridge;
    private readonly CompositeDisposable _bindings = new();
    private readonly TaskCompletionSource _openedSource = new();
    private readonly TaskCompletionSource _closedSource =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private CancellationTokenSource? _openingCancellationSource;
    private Task? _modalTask;
    private Task? _showTask;
    private Task? _closeTask;
    private Task? _disposeTask;
    private MacOsDisabledOwnerInputActivationScope? _macOsDisabledOwnerInputActivationScope;
    private DialogSizeConstraints _normalSizeConstraints;
    private Size? _normalSurfaceSize;
    private Size _windowChromeSize;
    private bool _isApplyingNormalWindowSize;

    internal DialogWindow HostWindow { get; }

    public event EventHandler<DialogPresenterCloseRequestedEventArgs>? CloseRequested;

    public IInputElement FocusScope => _surface;

    internal WindowDialogPresenter(Dialog dialog, TopLevel owner)
    {
        _dialog = dialog;
        _owner = owner;
        _surface = new DialogSurface(dialog)
        {
            IsHeaderVisible = false,
            IsResizable = false,
            TitleIcon = null
        };
        _surface.Classes.Add("window-hosted");
        HostWindow = new DialogWindow
        {
            Content = _surface,
            WindowStartupLocation = WindowStartupLocation.Manual
        };
        _resourceBridge = new DialogResourceBridge(_dialog);
        HostWindow.Resources.MergedDictionaries.Add(_resourceBridge);

        ((ISetLogicalParent)HostWindow).SetParent(_dialog);
        BindDialogProperties();
        HostWindow.CloseRequested += HandleWindowCloseRequested;
        HostWindow.Opened += HandleWindowOpened;
        HostWindow.Closed += HandleWindowClosed;
        HostWindow.KeyDown += HandleWindowKeyDown;
        HostWindow.PositionChanged += HandleHostWindowPositionChanged;
        HostWindow.Resized += HandleHostWindowResized;
        HostWindow.ScalingChanged += HandleHostWindowScalingChanged;
        HostWindow.NativeUserResizeCompleted += HandleHostWindowNativeUserResizeCompleted;
        _surface.CloseRequested += HandleSurfaceCloseRequested;
        _surface.StructuralMinimumChanged += HandleStructuralMinimumChanged;
    }

    private void BindDialogProperties()
    {
        _bindings.Add(HostWindow.Bind(Window.TitleProperty, _dialog.GetObservable(Dialog.TitleProperty)));
        _bindings.Add(HostWindow.Bind(Window.LogoProperty, _dialog.GetObservable(Dialog.TitleIconProperty)));
        _bindings.Add(HostWindow.Bind(Window.CanResizeProperty, _dialog.GetObservable(Dialog.IsResizableProperty)));
        _bindings.Add(HostWindow.Bind(Window.CanMinimizeProperty,
            _dialog.GetObservable(Dialog.EffectiveMinimizableProperty)));
        _bindings.Add(Observable.CombineLatest(
                _dialog.GetObservable(Dialog.IsMaximizableProperty),
                _dialog.GetObservable(Dialog.HostMaxWidthProperty),
                _dialog.GetObservable(Dialog.HostMaxHeightProperty),
                static (isMaximizable, maxWidth, maxHeight) =>
                    isMaximizable &&
                    double.IsPositiveInfinity(maxWidth) &&
                    double.IsPositiveInfinity(maxHeight))
            .Subscribe(canMaximize => HostWindow.CanMaximize = canMaximize));
        _bindings.Add(HostWindow.Bind(Window.IsCloseCaptionButtonVisibleProperty,
            _dialog.GetObservable(Dialog.IsClosableProperty)));
        _bindings.Add(HostWindow.Bind(Window.IsMoveEnabledProperty,
            _dialog.GetObservable(Dialog.IsDragMovableProperty)));
        _bindings.Add(HostWindow.Bind(Window.TopmostProperty,
            _dialog.GetObservable(Dialog.IsTopmostProperty)));
        _bindings.Add(_dialog.GetObservable(Dialog.HostWidthProperty).Subscribe(HandleHostWidthChanged));
        _bindings.Add(_dialog.GetObservable(Dialog.HostHeightProperty).Subscribe(HandleHostHeightChanged));
        _bindings.Add(_dialog.GetObservable(Dialog.HostMinWidthProperty)
                             .Subscribe(_ => RefreshSizingPreservingActualSize()));
        _bindings.Add(_dialog.GetObservable(Dialog.HostMinHeightProperty)
                             .Subscribe(_ => RefreshSizingPreservingActualSize()));
        _bindings.Add(_dialog.GetObservable(Dialog.HostMaxWidthProperty)
                             .Subscribe(_ => RefreshSizingPreservingActualSize()));
        _bindings.Add(_dialog.GetObservable(Dialog.HostMaxHeightProperty)
                             .Subscribe(_ => RefreshSizingPreservingActualSize()));
        _bindings.Add(_dialog.GetObservable(Dialog.TitleProperty)
                             .Subscribe(_ => RefreshSizingPreservingActualSize()));
        _bindings.Add(_dialog.GetObservable(Dialog.TitleIconProperty)
                             .Subscribe(_ => RefreshSizingPreservingActualSize()));
        _bindings.Add(_dialog.GetObservable(Dialog.IsClosableProperty)
                             .Subscribe(_ => RefreshSizingPreservingActualSize()));
        _bindings.Add(_dialog.GetObservable(Dialog.EffectiveMinimizableProperty)
                             .Subscribe(_ => RefreshSizingPreservingActualSize()));
        _bindings.Add(_dialog.GetObservable(Dialog.IsMaximizableProperty)
                             .Subscribe(_ => RefreshSizingPreservingActualSize()));
        _bindings.Add(HostWindow.GetObservable(TemplatedControl.PaddingProperty)
                                .Subscribe(_ => HandleWindowChromeChanged()));
        _bindings.Add(HostWindow.GetObservable(Window.FrameShadowThicknessProperty)
                                .Subscribe(_ => HandleWindowChromeChanged()));
        _bindings.Add(HostWindow.GetObservable(Avalonia.Controls.Window.WindowDecorationMarginProperty)
                                .Subscribe(_ => HandleWindowChromeChanged()));
        _bindings.Add(HostWindow.GetObservable(Window.VisibleFrameBorderThicknessProperty)
                                .Subscribe(_ => HandleWindowChromeChanged()));
        _bindings.Add(HostWindow.GetObservable(Window.IsCsdEnabledProperty)
                                .Subscribe(_ => HandleWindowChromeChanged()));
        _bindings.Add(HostWindow.GetObservable(Window.TitleBarHeightProperty)
                                .Subscribe(_ => HandleWindowChromeChanged()));
        _bindings.Add(HostWindow.GetObservable(Window.IsTitleBarVisibleProperty)
                                .Subscribe(_ => HandleWindowChromeChanged()));
        _bindings.Add(HostWindow.GetObservable(Avalonia.Controls.Window.WindowStateProperty)
                                .Subscribe(HandleWindowStateChanged));
        _bindings.Add(HostWindow.GetObservable(TopLevel.ClientSizeProperty)
                                .Subscribe(HandleHostWindowClientSizeChanged));
    }

    public ValueTask ShowAsync(CancellationToken cancellationToken)
    {
        _showTask ??= ShowCoreAsync(cancellationToken);
        return new ValueTask(_showTask);
    }

    private async Task ShowCoreAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _openingCancellationSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var openingCancellationToken = _openingCancellationSource.Token;
        // Native Window content is not attached yet, so inherit Dialog resources for natural measurement.
        ((ISetInheritanceParent)_surface).SetParent(_dialog);
        var initialWindowSize = InitializeSizing();
        ApplyStartupPlacement(initialWindowSize);
        if (_dialog.IsModal &&
            _owner is Window ownerWindow &&
            RuntimePlatform.Features.SupportsWindowModalDialog)
        {
            _macOsDisabledOwnerInputActivationScope =
                CreateMacOsDisabledOwnerInputActivationScope(ownerWindow);
            try
            {
                _modalTask = HostWindow.ShowDialog(ownerWindow);
            }
            catch
            {
                ReleaseMacOsDisabledOwnerInputActivationScope();
                throw;
            }
        }
        else
        {
            HostWindow.Show();
        }

        // 语义部件契约：窗口已显示即成为跨根，通知语义预览重新解析（Overlay 宿主在挂层时同样上报）。
        _dialog.NotifyCrossRootsChanged();

        await _openedSource.Task.WaitAsync(openingCancellationToken);
        openingCancellationToken.ThrowIfCancellationRequested();
        // Opened 后模板已应用，宿主表面就绪，再通知一次覆盖“窗口先显示、模板后应用”的顺序。
        _dialog.NotifyCrossRootsChanged();
    }

    private MacOsDisabledOwnerInputActivationScope? CreateMacOsDisabledOwnerInputActivationScope(Window ownerWindow)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return null;
        }

        return MacOsDisabledOwnerInputActivationScope.TryAttach(ownerWindow.PlatformImpl, HostWindow);
    }

    private void ReleaseMacOsDisabledOwnerInputActivationScope()
    {
        _macOsDisabledOwnerInputActivationScope?.Dispose();
        _macOsDisabledOwnerInputActivationScope = null;
    }

    private void ApplyStartupPlacement(Size windowSize)
    {
        var ownerBounds = GetOwnerBounds(_owner);
        var offset = _dialog.CalculatePlacementOffset(windowSize, ownerBounds.Size);
        var point = ownerBounds.TopLeft + offset;
        var scaling = !HostWindow.IsVisible && _owner is Window ownerWindow
            ? ownerWindow.DesktopScaling
            : HostWindow.DesktopScaling;
        HostWindow.Position = new PixelPoint(
            (int)Math.Round(point.X * scaling),
            (int)Math.Round(point.Y * scaling));
    }

    private static Rect GetOwnerBounds(TopLevel owner)
    {
        if (owner is Avalonia.Controls.Window ownerWindow)
        {
            return new Rect(
                ownerWindow.Position.X / ownerWindow.DesktopScaling,
                ownerWindow.Position.Y / ownerWindow.DesktopScaling,
                ownerWindow.ClientSize.Width,
                ownerWindow.ClientSize.Height);
        }

        return new Rect(default, owner.ClientSize);
    }

    private Size InitializeSizing()
    {
        PrepareSizingVisuals();
        // Measuring the managed Window tree attaches its ContentPresenter before native Show,
        // so the Surface theme and natural size are resolved in the same geometry pass.
        HostWindow.Measure(Size.Infinity);
        var chrome = ResolveWindowChromeSize();
        var capacity = ResolveSurfaceCapacity(chrome);
        _normalSizeConstraints = ResolveNormalConstraints(capacity);

        var usesNaturalWidth = !double.IsFinite(_dialog.HostWidth);
        var usesNaturalHeight = !double.IsFinite(_dialog.HostHeight);
        var width = usesNaturalWidth
            ? Math.Clamp(
                ResolveNaturalSurfaceWidth(),
                _normalSizeConstraints.MinWidth,
                _normalSizeConstraints.MaxWidth)
            : Math.Clamp(
                ResolveInitialAxis(_dialog.HostWidth, _normalSizeConstraints.MinWidth),
                _normalSizeConstraints.MinWidth,
                _normalSizeConstraints.MaxWidth);
        var height = usesNaturalHeight
            ? Math.Clamp(
                ResolveNaturalSurfaceHeight(width),
                _normalSizeConstraints.MinHeight,
                _normalSizeConstraints.MaxHeight)
            : Math.Clamp(
                ResolveInitialAxis(_dialog.HostHeight, _normalSizeConstraints.MinHeight),
                _normalSizeConstraints.MinHeight,
                _normalSizeConstraints.MaxHeight);

        ApplyNormalConstraints(capacity, chrome);
        _windowChromeSize = chrome;
        _normalSurfaceSize = _normalSizeConstraints.Clamp(new Size(width, height));
        return ApplyNormalWindowSize(_normalSurfaceSize.Value);
    }

    private void RefreshSizingPreservingActualSize()
    {
        if (_normalSurfaceSize is not { } normalSurfaceSize)
        {
            return;
        }

        PrepareSizingVisuals();
        var chrome = ResolveWindowChromeSize();
        var capacity = ResolveSurfaceCapacity(chrome);
        _normalSizeConstraints = ResolveNormalConstraints(capacity);
        ApplyNormalConstraints(capacity, chrome);
        _normalSurfaceSize = _normalSizeConstraints.Clamp(normalSurfaceSize);
        _windowChromeSize = chrome;

        if (CanApplyNormalWindowSize)
        {
            ApplyNormalWindowSize(_normalSurfaceSize.Value);
        }
    }

    private DialogSizeConstraints ResolveNormalConstraints(Size capacity)
    {
        return DialogSizeConstraints.Resolve(
            _surface.MeasureStructuralMinimum(),
            new Size(_dialog.HostMinWidth, _dialog.HostMinHeight),
            new Size(_dialog.HostMaxWidth, _dialog.HostMaxHeight),
            capacity);
    }

    private void ApplyNormalConstraints(Size capacity, Size chrome)
    {
        _surface.MinWidth = _normalSizeConstraints.MinWidth;
        _surface.MinHeight = _normalSizeConstraints.MinHeight;
        _surface.MaxWidth = _normalSizeConstraints.MaxWidth;
        _surface.MaxHeight = _normalSizeConstraints.MaxHeight;

        var windowCapacity = capacity + chrome;
        var titleBarMinimumWidth = ResolveTitleBarMinimumWindowWidth(chrome);
        var windowMinWidth = Math.Min(
            Math.Max(_normalSizeConstraints.MinWidth + chrome.Width, titleBarMinimumWidth),
            windowCapacity.Width);
        var windowMinHeight = _normalSizeConstraints.MinHeight + chrome.Height;
        var windowMaxWidth = Math.Max(
            windowMinWidth,
            _normalSizeConstraints.MaxWidth + chrome.Width);
        var windowMaxHeight = Math.Max(
            windowMinHeight,
            _normalSizeConstraints.MaxHeight + chrome.Height);

        HostWindow.MinWidth = windowMinWidth;
        HostWindow.MinHeight = windowMinHeight;
        HostWindow.MaxWidth = windowMaxWidth;
        HostWindow.MaxHeight = windowMaxHeight;
    }

    private double ResolveNaturalSurfaceWidth()
    {
        _surface.Measure(Size.Infinity);
        return ResolveInitialAxis(
            _surface.DesiredSize.Width,
            _normalSizeConstraints.MinWidth);
    }

    private double ResolveNaturalSurfaceHeight(double width)
    {
        _surface.Measure(new Size(width, double.PositiveInfinity));
        return ResolveInitialAxis(
            _surface.DesiredSize.Height,
            _normalSizeConstraints.MinHeight);
    }

    private void PrepareSizingVisuals()
    {
        HostWindow.ApplyStyling();
        HostWindow.ApplyTemplate();
        _surface.ApplyStyling();
        _surface.ApplyTemplate();
    }

    private Size ResolveWindowChromeSize()
    {
        var padding = HostWindow.Padding;
        var frame = HostWindow.IsCsdEnabled
            ? HostWindow.WindowDecorationMargin
            : HostWindow.FrameShadowThickness;
        var titleBarHeight = 0d;
        if (!HostWindow.IsCsdEnabled && HostWindow.IsTitleBarVisible)
        {
            HostWindow.TitleBar?.Measure(Size.Infinity);
            titleBarHeight = Math.Max(
                HostWindow.TitleBarHeight,
                HostWindow.TitleBar?.DesiredSize.Height ?? 0);
        }

        return new Size(
            Math.Max(0, padding.Left + padding.Right + frame.Left + frame.Right),
            Math.Max(0, padding.Top + padding.Bottom + frame.Top + frame.Bottom + titleBarHeight));
    }

    private Size ResolveSurfaceCapacity(Size chrome)
    {
        var workingArea = ResolveLogicalWorkingAreaSize();
        return new Size(
            Math.Max(0, workingArea.Width - chrome.Width),
            Math.Max(0, workingArea.Height - chrome.Height));
    }

    private Size ResolveLogicalWorkingAreaSize()
    {
        if (!HostWindow.IsVisible && _owner is not Window)
        {
            var ownerSize = GetOwnerBounds(_owner).Size;
            return new Size(
                ownerSize.Width > 0 ? ownerSize.Width : double.PositiveInfinity,
                ownerSize.Height > 0 ? ownerSize.Height : double.PositiveInfinity);
        }

        var ownerWindow = !HostWindow.IsVisible ? _owner as Window : null;
        var screen = ownerWindow is not null
            ? ownerWindow.Screens.ScreenFromWindow(ownerWindow)
            : HostWindow.Screens.ScreenFromWindow(HostWindow);
        if (screen is null && _owner is WindowBase ownerWindowBase)
        {
            screen = ownerWindowBase.Screens.ScreenFromWindow(ownerWindowBase);
        }

        screen ??= HostWindow.Screens.Primary ?? _owner.Screens?.Primary;
        if (screen is null)
        {
            var ownerSize = GetOwnerBounds(_owner).Size;
            return new Size(
                ownerSize.Width > 0 ? ownerSize.Width : double.PositiveInfinity,
                ownerSize.Height > 0 ? ownerSize.Height : double.PositiveInfinity);
        }

        var scaling = ownerWindow?.DesktopScaling ?? HostWindow.DesktopScaling;
        if (scaling <= 0)
        {
            scaling = screen.Scaling > 0 ? screen.Scaling : 1;
        }
        return new Size(
            screen.WorkingArea.Width / scaling,
            screen.WorkingArea.Height / scaling);
    }

    private double ResolveTitleBarMinimumWindowWidth(Size chrome)
    {
        if (HostWindow.IsCsdEnabled ||
            !HostWindow.IsTitleBarVisible ||
            HostWindow.TitleBar is not { } titleBar)
        {
            return 0;
        }

        titleBar.Measure(Size.Infinity);
        return titleBar.DesiredSize.Width + chrome.Width;
    }

    private Size ApplyNormalWindowSize(Size surfaceSize)
    {
        if (ShouldDeferNormalWindowSizeApplication)
        {
            return HostWindow.ClientSize;
        }

        var requestedWindowSize = surfaceSize + _windowChromeSize;
        HostWindow.SizeToContent = SizeToContent.Manual;
        _isApplyingNormalWindowSize = true;
        try
        {
            var windowSize = HostWindow.ApplyRequestedSize(
                requestedWindowSize.Width,
                requestedWindowSize.Height);
            _normalSurfaceSize = ResolveSurfaceSize(windowSize, _windowChromeSize);
            return windowSize;
        }
        finally
        {
            _isApplyingNormalWindowSize = false;
        }
    }

    private static double ResolveInitialAxis(double requested, double natural)
    {
        return double.IsFinite(requested)
            ? Math.Max(0, requested)
            : double.IsFinite(natural)
                ? Math.Max(0, natural)
                : 0;
    }

    private static Size ResolveSurfaceSize(Size windowSize, Size chrome)
    {
        return new Size(
            Math.Max(0, windowSize.Width - chrome.Width),
            Math.Max(0, windowSize.Height - chrome.Height));
    }

    public ValueTask CloseAsync()
    {
        _closeTask ??= CloseCoreAsync();
        return new ValueTask(_closeTask);
    }

    private async Task CloseCoreAsync()
    {
        _openingCancellationSource?.Cancel();
        if (_showTask is not null)
        {
            try
            {
                await _showTask;
            }
            catch
            {
                // Show failures are reported by the session; close still owns complete teardown.
            }
        }

        Exception? firstException = null;
        await InvokeOnUIThreadAsync(() =>
        {
            try
            {
                _surface.DisconnectCompositionChildren();
            }
            catch (Exception ex)
            {
                firstException ??= ex;
            }

            try
            {
                _surface.Dispose();
            }
            catch (Exception ex)
            {
                firstException ??= ex;
            }
        });

        var wasVisible = false;
        var nativeCloseFailed = false;
        await InvokeOnUIThreadAsync(() =>
        {
            try
            {
                wasVisible = HostWindow.IsVisible;
                if (wasVisible)
                {
                    HostWindow.CloseFromPresenter();
                }
            }
            catch (Exception ex)
            {
                nativeCloseFailed = true;
                firstException ??= ex;
            }
        });
        if (wasVisible && !nativeCloseFailed)
        {
            await _closedSource.Task;
        }
        else if (!wasVisible)
        {
            _closedSource.TrySetResult();
        }

        if (_modalTask is not null)
        {
            try
            {
                await _modalTask;
            }
            catch (Exception ex)
            {
                firstException ??= ex;
            }
        }

        if (firstException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(firstException).Throw();
        }
    }

    public ValueTask DisposeAsync()
    {
        _disposeTask ??= DisposeCoreAsync();
        return new ValueTask(_disposeTask);
    }

    private async Task DisposeCoreAsync()
    {
        Exception? firstException = null;
        try
        {
            await CloseAsync();
        }
        catch (Exception ex)
        {
            firstException = ex;
        }

        await InvokeOnUIThreadAsync(() =>
        {
            HostWindow.CloseRequested -= HandleWindowCloseRequested;
            HostWindow.Opened -= HandleWindowOpened;
            HostWindow.Closed -= HandleWindowClosed;
            HostWindow.KeyDown -= HandleWindowKeyDown;
            HostWindow.PositionChanged -= HandleHostWindowPositionChanged;
            HostWindow.Resized -= HandleHostWindowResized;
            HostWindow.ScalingChanged -= HandleHostWindowScalingChanged;
            HostWindow.NativeUserResizeCompleted -= HandleHostWindowNativeUserResizeCompleted;
            _surface.CloseRequested -= HandleSurfaceCloseRequested;
            _surface.StructuralMinimumChanged -= HandleStructuralMinimumChanged;
            try
            {
                _bindings.Dispose();
            }
            catch (Exception ex)
            {
                firstException ??= ex;
            }

            try
            {
                _surface.DisconnectCompositionChildren();
            }
            catch (Exception ex)
            {
                firstException ??= ex;
            }

            try
            {
                _surface.Dispose();
            }
            catch (Exception ex)
            {
                firstException ??= ex;
            }

            _openingCancellationSource?.Dispose();
            _openingCancellationSource = null;
            HostWindow.Resources.MergedDictionaries.Remove(_resourceBridge);
            _resourceBridge.Dispose();
            HostWindow.ReleasePresenterHooks();
            HostWindow.Content = null;
            ((ISetInheritanceParent)_surface).SetParent(null);
            try
            {
                ((ISetLogicalParent)HostWindow).SetParent(null);
            }
            catch (Exception ex)
            {
                firstException ??= ex;
            }

            // 宿主窗口回收后跨根消失，通知语义预览清空该根下的高亮。
            _dialog.NotifyCrossRootsChanged();
        });

        if (firstException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(firstException).Throw();
        }
    }

    private void HandleWindowCloseRequested(object? sender, EventArgs e)
    {
        CloseRequested?.Invoke(this,
            new DialogPresenterCloseRequestedEventArgs(DialogCloseReason.HostCloseRequest));
    }

    private void HandleWindowOpened(object? sender, EventArgs e)
    {
        _openedSource.TrySetResult();
        var clientSizeBeforeRefresh = HostWindow.ClientSize;
        RefreshSizingPreservingActualSize();
        if (HostWindow.ClientSize != clientSizeBeforeRefresh)
        {
            ApplyStartupPlacement(HostWindow.ClientSize);
        }
    }

    private void HandleWindowClosed(object? sender, EventArgs e)
    {
        ReleaseMacOsDisabledOwnerInputActivationScope();
        _closedSource.TrySetResult();
    }

    private void HandleHostWidthChanged(double value)
    {
        if (_normalSurfaceSize is not { } normalSurfaceSize || !double.IsFinite(value))
        {
            return;
        }

        _normalSurfaceSize = new Size(
            Math.Clamp(value, _normalSizeConstraints.MinWidth, _normalSizeConstraints.MaxWidth),
            normalSurfaceSize.Height);
        if (CanApplyNormalWindowSize)
        {
            ApplyNormalWindowSize(_normalSurfaceSize.Value);
        }
    }

    private void HandleHostHeightChanged(double value)
    {
        if (_normalSurfaceSize is not { } normalSurfaceSize || !double.IsFinite(value))
        {
            return;
        }

        _normalSurfaceSize = new Size(
            normalSurfaceSize.Width,
            Math.Clamp(value, _normalSizeConstraints.MinHeight, _normalSizeConstraints.MaxHeight));
        if (CanApplyNormalWindowSize)
        {
            ApplyNormalWindowSize(_normalSurfaceSize.Value);
        }
    }

    private void HandleWindowChromeChanged()
    {
        if (!_openedSource.Task.IsCompleted)
        {
            return;
        }

        RefreshSizingPreservingActualSize();
    }

    private void HandleWindowStateChanged(WindowState state)
    {
        if (_openedSource.Task.IsCompleted &&
            state == WindowState.Normal)
        {
            RefreshSizingPreservingActualSize();
        }
    }

    private void HandleHostWindowResized(object? sender, WindowResizedEventArgs e)
    {
        if (_normalSurfaceSize is null ||
            !_openedSource.Task.IsCompleted ||
            !HostWindow.IsVisible ||
            HostWindow.WindowState != WindowState.Normal ||
            _isApplyingNormalWindowSize ||
            e.Reason is not (WindowResizeReason.User or WindowResizeReason.Unspecified))
        {
            return;
        }

        UpdateNormalSurfaceSizeFromClientSize(e.ClientSize);
    }

    private void HandleHostWindowClientSizeChanged(Size clientSize)
    {
        if (_normalSurfaceSize is null ||
            !_openedSource.Task.IsCompleted ||
            !HostWindow.IsVisible ||
            HostWindow.WindowState != WindowState.Normal ||
            _isApplyingNormalWindowSize ||
            !ShouldTrackPlatformClientSizeChanges)
        {
            return;
        }

        UpdateNormalSurfaceSizeFromClientSize(clientSize);
    }

    private void UpdateNormalSurfaceSizeFromClientSize(Size clientSize)
    {
        _normalSurfaceSize = _normalSizeConstraints.Clamp(
            ResolveSurfaceSize(clientSize, _windowChromeSize));
    }

    private void HandleHostWindowNativeUserResizeCompleted(object? sender, EventArgs e)
    {
        if (!_openedSource.Task.IsCompleted)
        {
            return;
        }

        RefreshSizingPreservingActualSize();
    }

    private void HandleHostWindowScalingChanged(object? sender, EventArgs e)
    {
        if (!_openedSource.Task.IsCompleted)
        {
            return;
        }

        RefreshSizingPreservingActualSize();
    }

    private void HandleHostWindowPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (!_openedSource.Task.IsCompleted)
        {
            return;
        }

        RefreshSizingPreservingActualSize();
    }

    private void HandleStructuralMinimumChanged(object? sender, EventArgs e)
    {
        if (!_openedSource.Task.IsCompleted)
        {
            return;
        }

        RefreshSizingPreservingActualSize();
    }

    private void HandleSurfaceCloseRequested(object? sender, DialogSurfaceCloseRequestedEventArgs e)
    {
        CloseRequested?.Invoke(this,
            new DialogPresenterCloseRequestedEventArgs(e.Reason, e.Result, e.SourceButton));
    }

    private void HandleWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (!e.Handled && _surface.TryInvokeStandardButton(e.Key))
        {
            e.Handled = true;
        }
    }

    private static async Task InvokeOnUIThreadAsync(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(action);
    }

    private bool CanApplyNormalWindowSize =>
        HostWindow.WindowState == WindowState.Normal &&
        !ShouldDeferNormalWindowSizeApplication;

    private bool ShouldDeferNormalWindowSizeApplication =>
        HostWindow.IsNativeUserResizeInProgress &&
        HostWindow.WindowState == WindowState.Normal;

    private bool ShouldTrackPlatformClientSizeChanges =>
        HostWindow.IsNativeUserResizeInProgress ||
        IsWaylandCustomResizerVisible;

    private bool IsWaylandCustomResizerVisible =>
        HostWindow.IsCustomResizerVisible &&
        OperatingSystem.IsLinux() &&
        AbstractLinuxWindowChromeManager.IsWayland(HostWindow);

    private sealed class MacOsDisabledOwnerInputActivationScope : IDisposable
    {
        private readonly Avalonia.Platform.IWindowImpl _ownerPlatformImpl;
        private readonly DialogWindow _hostWindow;
        private readonly Action? _originalCallback;
        private readonly Action _replacementCallback;
        private bool _isDisposed;

        private MacOsDisabledOwnerInputActivationScope(
            Avalonia.Platform.IWindowImpl ownerPlatformImpl,
            DialogWindow hostWindow)
        {
            _ownerPlatformImpl  = ownerPlatformImpl;
            _hostWindow         = hostWindow;
            _originalCallback   = ownerPlatformImpl.GotInputWhenDisabled;
            _replacementCallback = HandleGotInputWhenDisabled;
            _ownerPlatformImpl.GotInputWhenDisabled = _replacementCallback;
        }

        public static MacOsDisabledOwnerInputActivationScope? TryAttach(
            Avalonia.Platform.IWindowImpl? ownerPlatformImpl,
            DialogWindow hostWindow)
        {
            return ownerPlatformImpl is null
                ? null
                : new MacOsDisabledOwnerInputActivationScope(ownerPlatformImpl, hostWindow);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            if (_ownerPlatformImpl.GotInputWhenDisabled == _replacementCallback)
            {
                _ownerPlatformImpl.GotInputWhenDisabled =
                    ResolveRestoredCallback(_originalCallback);
            }
        }

        private void HandleGotInputWhenDisabled()
        {
            if (_hostWindow.IsVisible && _hostWindow.IsActive)
            {
                return;
            }

            var originalCallback = _originalCallback;
            originalCallback?.Invoke();
        }

        private static Action? ResolveRestoredCallback(Action? callback)
        {
            while (callback?.Target is MacOsDisabledOwnerInputActivationScope scope &&
                   callback == scope._replacementCallback &&
                   scope._isDisposed)
            {
                callback = scope._originalCallback;
            }

            return callback;
        }
    }
}
