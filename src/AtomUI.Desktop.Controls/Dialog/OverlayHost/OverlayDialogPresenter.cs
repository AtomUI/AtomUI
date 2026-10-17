using System.Reactive.Disposables;
using System.Reactive.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.MotionScene;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal sealed class OverlayDialogPresenter : ContentControl,
                                               IDialogPresenter
{
    #region 公共属性定义

    public IInputElement FocusScope => _surface;

    #endregion

    #region 公共事件定义

    public event EventHandler<DialogPresenterCloseRequestedEventArgs>? CloseRequested;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsModalProperty =
        Dialog.IsModalProperty.AddOwner<OverlayDialogPresenter>();

    internal static readonly StyledProperty<bool> IsMaskClosableProperty =
        Dialog.IsMaskClosableProperty.AddOwner<OverlayDialogPresenter>();

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        Dialog.IsMotionEnabledProperty.AddOwner<OverlayDialogPresenter>();

    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        AvaloniaProperty.Register<OverlayDialogPresenter, TimeSpan>(nameof(MotionDuration));

    internal bool IsModal
    {
        get => GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
    }

    internal bool IsMaskClosable
    {
        get => GetValue(IsMaskClosableProperty);
        set => SetValue(IsMaskClosableProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    #endregion

    #region 内部协作 API

    internal DialogSurface Surface => _surface;

    internal void UpdateLayerBounds(Size layerSize)
    {
        if (layerSize.Width <= 0 || layerSize.Height <= 0)
        {
            return;
        }

        var visibleFrameBounds = ResolveOwnerBounds(layerSize);
        ApplyMaskBounds(layerSize);
        if (_surface.IsDialogMaximized)
        {
            ApplyMaximizedBounds(ResolveDialogBodyOwnerBounds(visibleFrameBounds));
            return;
        }

        var ownerBounds = ResolveDialogBodyOwnerBounds(visibleFrameBounds);
        ApplyNormalSurfaceSizeConstraints(ownerBounds);
        UpdateSurfacePlacement(ownerBounds);
    }

    internal bool TryInvokeStandardButton(Key key)
    {
        return _surface.TryInvokeStandardButton(key);
    }

    #endregion

    private static readonly TimeSpan CompositionMotionCompletionSlack = TimeSpan.FromMilliseconds(32);

    private readonly Dialog _dialog;
    private readonly Control _placementTarget;
    private readonly DialogSurface _surface;
    private readonly MatrixTransform _surfacePositionTransform = new();
    private readonly CompositeDisposable _bindings = new();
    private DialogOverlayLayer? _dialogLayer;
    private Window? _ownerWindow;
    private IDisposable? _drawnChromeOverlaySuppression;
    private bool _ownerIsWayland;
    private MotionActor? _maskMotionActor;
    private MotionActor? _surfaceMotionActor;
    private OverlayDialogMask? _dialogMask;
    private CancellationTokenSource? _openingMotionCancellationSource;
    private Task? _showTask;
    private Task? _closeTask;
    private Task? _disposeTask;
    private Point? _dragPointerOffset;
    private Point _surfacePosition;
    private DialogSizeConstraints _normalSizeConstraints;
    private Rect? _resizeOriginBounds;
    // Normal Width/Height own each axis' sizing intent: NaN stays natural until explicitly sized.
    // Maximization temporarily replaces those inputs, so retain them rather than actual bounds.
    private Size? _restoreSize;
    private bool _isInitialSizeResolved;

    internal OverlayDialogPresenter(Dialog dialog, Control placementTarget)
    {
        _dialog = dialog;
        _placementTarget = placementTarget;
        _surface = new DialogSurface(dialog)
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            RenderTransform = _surfacePositionTransform
        };
        _surface.Classes.Add("overlay-hosted");
        Content = _surface;

        _bindings.Add(Bind(IsModalProperty, dialog.GetObservable(Dialog.IsModalProperty)));
        _bindings.Add(this.GetObservable(IsModalProperty)
            .Subscribe(_ => UpdateDrawnChromeOverlaySuppression()));
        _bindings.Add(Bind(IsMaskClosableProperty, dialog.GetObservable(Dialog.IsMaskClosableProperty)));
        _bindings.Add(Bind(IsMotionEnabledProperty, dialog.GetObservable(Dialog.IsMotionEnabledProperty)));
        _bindings.Add(dialog.GetObservable(Dialog.HostWidthProperty).Skip(1).Subscribe(HandleHostWidthChanged));
        _bindings.Add(dialog.GetObservable(Dialog.HostHeightProperty).Skip(1).Subscribe(HandleHostHeightChanged));
        _bindings.Add(dialog.GetObservable(Dialog.HostMinWidthProperty).Subscribe(_ => UpdateCurrentLayerBounds()));
        _bindings.Add(dialog.GetObservable(Dialog.HostMinHeightProperty).Subscribe(_ => UpdateCurrentLayerBounds()));
        _bindings.Add(dialog.GetObservable(Dialog.HostMaxWidthProperty).Subscribe(_ => UpdateCurrentLayerBounds()));
        _bindings.Add(dialog.GetObservable(Dialog.HostMaxHeightProperty).Subscribe(_ => UpdateCurrentLayerBounds()));
        _surface.CloseRequested += HandleSurfaceCloseRequested;
        _surface.HostCloseRequested += HandleHostCloseRequested;
        _surface.MaximizeRequested += HandleMaximizeRequested;
        _surface.RestoreRequested += HandleRestoreRequested;
        _surface.ResizeStarted += HandleResizeStarted;
        _surface.ResizeRequested += HandleResizeRequested;
        _surface.ResizeCompleted += HandleResizeCompleted;
        _surface.StructuralMinimumChanged += HandleStructuralMinimumChanged;
        _surface.HeaderPointerPressed += HandleHeaderPointerPressed;
        _surface.HeaderPointerMoved += HandleHeaderPointerMoved;
        _surface.HeaderPointerReleased += HandleHeaderPointerReleased;
        _surface.SizeChanged += HandleSurfaceSizeChanged;
        AddHandler(PointerPressedEvent, HandlePresenterPointerPressed, RoutingStrategies.Tunnel, true);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ReleaseDialogMask();
        _maskMotionActor = e.NameScope.Find<MotionActor>("PART_MaskMotionActor");
        _surfaceMotionActor = e.NameScope.Find<MotionActor>("PART_SurfaceMotionActor");
        _dialogMask = e.NameScope.Find<OverlayDialogMask>("PART_DialogMask");
        if (_dialogMask is not null)
        {
            _dialogMask.PointerPressed += HandleMaskPointerPressed;
        }
    }

    public ValueTask ShowAsync(CancellationToken cancellationToken)
    {
        _showTask ??= ShowCoreAsync(cancellationToken);
        return new ValueTask(_showTask);
    }

    private async Task ShowCoreAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_dialogLayer is not null)
        {
            return;
        }

        ((ISetInheritanceParent)this).SetParent(
            ((ILogical)_dialog).IsAttachedToLogicalTree ? _dialog : _placementTarget);
        _dialogLayer = DialogOverlayLayer.GetOrCreate(_placementTarget, _dialog.OverlayScope);
        // 显式 OverlayScope 时宿主没有 Window frame 契约：不解析 owning Window，因此不参与 frame 内缩、
        // 也不申请 drawn chrome 抑制。未显式指定时保持既有行为（owner window 仍按 placement target 的 TopLevel 解析，
        // 与宿主 layer 的解析结果无关）。
        _ownerWindow = _dialog.OverlayScope is null
            ? TopLevel.GetTopLevel(_placementTarget) as Window
            : null;
        _ownerIsWayland = OperatingSystem.IsLinux() &&
                          _ownerWindow is { } ownerWindow &&
                          AbstractLinuxWindowChromeManager.IsWayland(ownerWindow);
        UpdateDrawnChromeOverlaySuppression();
        // 语义部件契约：owner 已附加到逻辑树时，把 presenter 逻辑挂到 Dialog 下，owner 作用域生成的 Semantic
        // Style 才能沿逻辑祖先链命中 presenter 子树；必须在 layer.Children.Add 之前设置（Panel 不会覆盖已显式
        // 设置的逻辑父，先例：DrawerContainer.AttachToLayer）。owner 未附加时保持 layer 所有权：此时 owner
        // 作用域样式本就无法激活，也避免改变 presenter 的 Parent 语义。该条件与上面的 inheritance parent 一致。
        if (((ILogical)_dialog).IsAttachedToLogicalTree)
        {
            ((ISetLogicalParent)this).SetParent(_dialog);
        }

        _dialogLayer.Add(this);
        _dialog.NotifyCrossRootsChanged();
        AttachOwnerGeometryBindings();
        UpdateLayerBounds(_dialogLayer.AvailableSize);
        ApplyTemplate();
        _surface.ApplyTemplate();
        UpdateLayerBounds(_dialogLayer.AvailableSize);
        if (_surfaceMotionActor is not null)
        {
            _surfaceMotionActor.Opacity = IsMotionEnabled ? 0 : 1;
        }

        await Dispatcher.UIThread.InvokeAsync(
            () => UpdateLayerBounds(_dialogLayer?.AvailableSize ?? default),
            DispatcherPriority.Loaded);
        _isInitialSizeResolved = true;

        if (IsMotionEnabled && _surfaceMotionActor is not null)
        {
            _openingMotionCancellationSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var motionToken = _openingMotionCancellationSource.Token;
            if (!await TryRunCompositorOpeningMotionAsync(motionToken))
            {
                await CreateSurfaceMotion(isOpening: true)
                    .RunAsync(_surfaceMotionActor, cancellationToken: motionToken);
            }
        }
    }

    public ValueTask CloseAsync()
    {
        _closeTask ??= CloseCoreAsync();
        return new ValueTask(_closeTask);
    }

    private async Task CloseCoreAsync()
    {
        _openingMotionCancellationSource?.Cancel();
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

        if (IsMotionEnabled && _surfaceMotionActor is not null && _dialogLayer is not null)
        {
            if (!await TryRunCompositorClosingMotionAsync())
            {
                var motionTasks = new List<Task>
                {
                    CreateSurfaceMotion(isOpening: false).RunAsync(_surfaceMotionActor)
                };
                if (_surface.SurfaceContentLayer is { } surfaceContentLayer)
                {
                    motionTasks.Add(RunSurfaceContentCloseMotionAsync(surfaceContentLayer));
                }

                await Task.WhenAll(motionTasks);
            }
        }

        _surface.DisconnectCompositionChildren();
        _surface.Dispose();
        RemoveFromDialogLayer();
    }

    private async Task RunSurfaceContentCloseMotionAsync(Control surfaceContentLayer)
    {
        var animation = new Animation
        {
            Duration = MotionDuration,
            Easing = new LinearEasing(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters = { new Setter(Visual.OpacityProperty, 1d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(Visual.OpacityProperty, 0d) }
                }
            }
        };

        try
        {
            await animation.RunAsync(surfaceContentLayer);
        }
        finally
        {
            // The surface is torn down immediately after all close motions;
            // restore the live tree for re-use if teardown is interrupted.
            surfaceContentLayer.Opacity = 1;
        }
    }

    private async Task<bool> TryRunCompositorOpeningMotionAsync(CancellationToken cancellationToken)
    {
        var motion = CreateSurfaceMotion(isOpening: true);
        var surfaceVisual = ElementComposition.GetElementVisual(_surfaceMotionActor!);
        if (surfaceVisual is null)
        {
            return false;
        }

        var isZoom = motion is DialogZoomInMotion;
        try
        {
            // A newly attached CompositionVisual must reach the server before its first animation batch.
            await surfaceVisual.Compositor.RequestCommitAsync().WaitAsync(cancellationToken);
            if (isZoom)
            {
                var openingStart = ZoomBigMotionDefinition.OpeningStart;
                var visible = ZoomBigMotionDefinition.Visible;
                var origin = ResolveMotionOrigin().Point;
                surfaceVisual.CenterPoint = new Vector3D(origin.X, origin.Y, 0);
                StartScaleAnimation(
                    surfaceVisual,
                    openingStart.Scale,
                    visible.Scale,
                    motion.Easing,
                    motion.Duration);
            }

            StartOpacityAnimation(
                surfaceVisual,
                isZoom ? ZoomBigMotionDefinition.OpeningStart.Opacity : 0.0,
                ZoomBigMotionDefinition.Visible.Opacity,
                motion.Easing,
                motion.Duration);
            await WaitForCompositionMotionAsync(
                surfaceVisual.Compositor,
                motion.Duration + CompositionMotionCompletionSlack,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            CompleteCompositorOpeningMotion(surfaceVisual, isZoom);
            throw;
        }

        CompleteCompositorOpeningMotion(surfaceVisual, isZoom);
        return true;
    }

    private async Task<bool> TryRunCompositorClosingMotionAsync()
    {
        var motion = CreateSurfaceMotion(isOpening: false);
        var surfaceVisual = ElementComposition.GetElementVisual(_surfaceMotionActor!);
        if (surfaceVisual is null)
        {
            return false;
        }

        CompositionVisual? contentVisual = null;
        if (_surface.SurfaceContentLayer is { } surfaceContentLayer)
        {
            contentVisual = ElementComposition.GetElementVisual(surfaceContentLayer);
            if (contentVisual is null)
            {
                return false;
            }
        }

        var isZoom = motion is DialogZoomOutMotion;
        if (isZoom)
        {
            var visible = ZoomBigMotionDefinition.Visible;
            var closingEnd = ZoomBigMotionDefinition.ClosingEnd;
            var origin = ResolveMotionOrigin().Point;
            surfaceVisual.CenterPoint = new Vector3D(origin.X, origin.Y, 0);
            StartScaleAnimation(
                surfaceVisual,
                visible.Scale,
                closingEnd.Scale,
                motion.Easing,
                motion.Duration);
        }

        var endOpacity = isZoom ? ZoomBigMotionDefinition.ClosingEnd.Opacity : 0.0;
        StartOpacityAnimation(
            surfaceVisual,
            ZoomBigMotionDefinition.Visible.Opacity,
            endOpacity,
            motion.Easing,
            motion.Duration);
        if (contentVisual is not null)
        {
            StartOpacityAnimation(contentVisual, 1.0, 0.0, new LinearEasing(), MotionDuration);
        }

        await WaitForCompositionMotionAsync(
            surfaceVisual.Compositor,
            motion.Duration + CompositionMotionCompletionSlack,
            CancellationToken.None);
        CompleteCompositorClosingMotion(
            surfaceVisual,
            contentVisual,
            isZoom,
            endOpacity);
        return true;
    }

    private void CompleteCompositorOpeningMotion(CompositionVisual surfaceVisual,
                                                 bool includesScale)
    {
        var visible = ZoomBigMotionDefinition.Visible;
        surfaceVisual.StopAnimation("Opacity");
        surfaceVisual.Opacity = (float)visible.Opacity;
        if (includesScale)
        {
            surfaceVisual.StopAnimation("Scale");
            surfaceVisual.Scale       = new Vector3D(visible.Scale, visible.Scale, 1);
            surfaceVisual.CenterPoint = default;
        }

        _surfaceMotionActor!.Opacity = visible.Opacity;
    }

    private static void CompleteCompositorClosingMotion(CompositionVisual surfaceVisual,
                                                        CompositionVisual? contentVisual,
                                                        bool includesScale,
                                                        double endOpacity)
    {
        surfaceVisual.StopAnimation("Opacity");
        surfaceVisual.Opacity = (float)endOpacity;
        if (includesScale)
        {
            var closingEnd = ZoomBigMotionDefinition.ClosingEnd;
            surfaceVisual.StopAnimation("Scale");
            surfaceVisual.Scale = new Vector3D(closingEnd.Scale, closingEnd.Scale, 1);
        }

        contentVisual?.StopAnimation("Opacity");
        if (contentVisual is not null)
        {
            contentVisual.Opacity = 1;
        }
    }

    private static void StartOpacityAnimation(CompositionVisual visual,
                                              double from,
                                              double to,
                                              Easing easing,
                                              TimeSpan duration)
    {
        var animation = visual.Compositor.CreateScalarKeyFrameAnimation();
        animation.Duration = duration;
        animation.InsertKeyFrame(0, (float)from, easing);
        animation.InsertKeyFrame(1, (float)to, easing);
        visual.StartAnimation("Opacity", animation);
    }

    private static void StartScaleAnimation(CompositionVisual visual,
                                            double from,
                                            double to,
                                            Easing easing,
                                            TimeSpan duration)
    {
        var animation = visual.Compositor.CreateVector3DKeyFrameAnimation();
        animation.Duration = duration;
        animation.InsertKeyFrame(0, new Vector3D(from, from, 1), easing);
        animation.InsertKeyFrame(1, new Vector3D(to, to, 1), easing);
        // StartAnimation does not update the client base value. Keep it distinct from the
        // endpoint so completing or cancelling the motion serializes the final scale.
        visual.Scale = new Vector3D(from, from, 1);
        visual.StartAnimation("Scale", animation);
    }

    private static async Task WaitForCompositionMotionAsync(Compositor compositor,
                                                            TimeSpan duration,
                                                            CancellationToken cancellationToken)
    {
        // Submission may wait behind layout/render work. Count the motion from the batch
        // reaching the render thread, otherwise a delayed batch can be stopped mid-motion.
        await compositor.RequestCommitAsync().WaitAsync(cancellationToken);
        var completionSource = new TaskCompletionSource();
        using var timer = DispatcherTimer.RunOnce(
            completionSource.SetResult,
            duration,
            DispatcherPriority.Normal);
        await completionSource.Task.WaitAsync(cancellationToken);
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

        _surface.CloseRequested -= HandleSurfaceCloseRequested;
        _surface.HostCloseRequested -= HandleHostCloseRequested;
        _surface.MaximizeRequested -= HandleMaximizeRequested;
        _surface.RestoreRequested -= HandleRestoreRequested;
        _surface.ResizeStarted -= HandleResizeStarted;
        _surface.ResizeRequested -= HandleResizeRequested;
        _surface.ResizeCompleted -= HandleResizeCompleted;
        _surface.StructuralMinimumChanged -= HandleStructuralMinimumChanged;
        _surface.HeaderPointerPressed -= HandleHeaderPointerPressed;
        _surface.HeaderPointerMoved -= HandleHeaderPointerMoved;
        _surface.HeaderPointerReleased -= HandleHeaderPointerReleased;
        _surface.SizeChanged -= HandleSurfaceSizeChanged;
        RemoveHandler(PointerPressedEvent, HandlePresenterPointerPressed);
        try
        {
            _bindings.Dispose();
        }
        catch (Exception ex)
        {
            firstException ??= ex;
        }

        _openingMotionCancellationSource?.Dispose();
        _openingMotionCancellationSource = null;
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

        try
        {
            RemoveFromDialogLayer();
        }
        catch (Exception ex)
        {
            firstException ??= ex;
        }

        ReleaseDialogMask();
        ReleaseDrawnChromeOverlaySuppression();
        Content = null;
        _ownerWindow = null;
        _ownerIsWayland = false;
        _maskMotionActor = null;
        _surfaceMotionActor = null;
        try
        {
            ((ISetInheritanceParent)this).SetParent(null);
        }
        catch (Exception ex)
        {
            firstException ??= ex;
        }

        if (firstException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(firstException).Throw();
        }
    }

    private void RemoveFromDialogLayer()
    {
        ReleaseDrawnChromeOverlaySuppression();
        if (_dialogLayer is not { } dialogLayer)
        {
            return;
        }

        dialogLayer.Remove(this);
        _dialogLayer = null;
        // 只清除本 presenter 自己建立的 owner 逻辑父，避免误清 layer 建立的所有权。
        if (ReferenceEquals(((ILogical)this).LogicalParent, _dialog))
        {
            ((ISetLogicalParent)this).SetParent(null);
        }

        _dialog.NotifyCrossRootsChanged();
    }

    private void UpdateDrawnChromeOverlaySuppression()
    {
        if (_dialogLayer is not null && IsModal && _ownerWindow is { } window)
        {
            _drawnChromeOverlaySuppression ??= window.SuppressDrawnChromeOverlay();
            return;
        }

        ReleaseDrawnChromeOverlaySuppression();
    }

    private void ReleaseDrawnChromeOverlaySuppression()
    {
        _drawnChromeOverlaySuppression?.Dispose();
        _drawnChromeOverlaySuppression = null;
    }

    private void ApplyNormalSurfaceSizeConstraints(Rect ownerBounds)
    {
        ResolveNormalSurfaceSizeConstraints(ownerBounds);
        if (!_isInitialSizeResolved)
        {
            _surface.Width = ClampExplicitSize(
                _dialog.HostWidth,
                _normalSizeConstraints.MinWidth,
                _normalSizeConstraints.MaxWidth);
            _surface.Height = ClampExplicitSize(
                _dialog.HostHeight,
                _normalSizeConstraints.MinHeight,
                _normalSizeConstraints.MaxHeight);
            return;
        }

        ClampActualSurfaceGeometry(ownerBounds);
    }

    private void ResolveNormalSurfaceSizeConstraints(Rect ownerBounds)
    {
        _normalSizeConstraints = DialogSizeConstraints.Resolve(
            _surface.MeasureStructuralMinimum(),
            new Size(_dialog.HostMinWidth, _dialog.HostMinHeight),
            new Size(_dialog.HostMaxWidth, _dialog.HostMaxHeight),
            ownerBounds.Size);

        _surface.MinWidth = _normalSizeConstraints.MinWidth;
        _surface.MinHeight = _normalSizeConstraints.MinHeight;
        _surface.MaxWidth = _normalSizeConstraints.MaxWidth;
        _surface.MaxHeight = _normalSizeConstraints.MaxHeight;
    }

    private void ClampActualSurfaceGeometry(Rect ownerBounds)
    {
        var actualSize = ResolveActualSurfaceSize();
        if (actualSize.Width <= 0 || actualSize.Height <= 0)
        {
            ClampPendingExplicitSize();
            return;
        }

        var clampedSize = _normalSizeConstraints.Clamp(actualSize);
        // Owner-driven bounds correction is transient; OffsetX/Y retain the requested placement.
        var clampedPosition = ConstrainAndRoundSurfacePosition(ownerBounds, clampedSize, _surfacePosition);
        if (clampedSize == actualSize)
        {
            SetSurfacePosition(clampedPosition);
            return;
        }

        if (double.IsFinite(_surface.Width) && !MathUtils.AreClose(clampedSize.Width, actualSize.Width))
        {
            _surface.Width = clampedSize.Width;
        }

        if (double.IsFinite(_surface.Height) && !MathUtils.AreClose(clampedSize.Height, actualSize.Height))
        {
            _surface.Height = clampedSize.Height;
        }

        SetSurfacePosition(clampedPosition);
    }

    private void ClampPendingExplicitSize()
    {
        if (double.IsFinite(_surface.Width))
        {
            _surface.Width = Math.Clamp(
                _surface.Width,
                _normalSizeConstraints.MinWidth,
                _normalSizeConstraints.MaxWidth);
        }

        if (double.IsFinite(_surface.Height))
        {
            _surface.Height = Math.Clamp(
                _surface.Height,
                _normalSizeConstraints.MinHeight,
                _normalSizeConstraints.MaxHeight);
        }
    }

    private void ApplyMaximizedBounds(Rect ownerBounds)
    {
        _surface.MinWidth = 0;
        _surface.MinHeight = 0;
        _surface.MaxWidth = double.PositiveInfinity;
        _surface.MaxHeight = double.PositiveInfinity;
        _surface.Width = ownerBounds.Width;
        _surface.Height = ownerBounds.Height;
        SetSurfacePosition(ownerBounds.Position);
        _surface.CornerRadius = default;
    }

    private void UpdateCurrentLayerBounds()
    {
        var layerSize = _dialogLayer?.AvailableSize ?? Bounds.Size;
        UpdateLayerBounds(layerSize);
    }

    private void AttachOwnerGeometryBindings()
    {
        if (_ownerWindow is not { } window)
        {
            return;
        }

        _bindings.Add(window.GetObservable(Window.FrameShadowThicknessProperty)
                            .CombineLatest(
                                window.GetObservable(Window.VisibleFrameBorderThicknessProperty),
                                static (frameShadowThickness, visibleFrameBorderThickness) =>
                                    (frameShadowThickness, visibleFrameBorderThickness))
                            .DistinctUntilChanged()
                            .Subscribe(_ => UpdateCurrentLayerBounds()));
    }

    private void UpdateSurfacePlacement(Rect ownerBounds)
    {
        if (_surface.IsDialogMaximized)
        {
            return;
        }

        _surface.Measure(ownerBounds.Size);
        var surfaceSize = _surface.DesiredSize;
        var offset = _dialog.CalculatePlacementOffset(surfaceSize, ownerBounds.Size);
        var position = ConstrainAndRoundSurfacePosition(
            ownerBounds,
            surfaceSize,
            new Point(ownerBounds.X + offset.X, ownerBounds.Y + offset.Y));
        SetSurfacePosition(position);
    }

    private Rect ResolveOwnerBounds(Size layerSize)
    {
        return _ownerWindow is { } window
            ? WindowVisualLayerClip.CalculateClipBounds(layerSize, window.FrameShadowThickness)
            : new Rect(default, layerSize);
    }

    private Rect ResolveDialogBodyOwnerBounds(Rect visibleFrameBounds)
    {
        return _ownerWindow is { } window
            ? visibleFrameBounds.Deflate(window.VisibleFrameBorderThickness)
            : visibleFrameBounds;
    }

    private void ApplyMaskBounds(Size layerSize)
    {
        if (_maskMotionActor is null)
        {
            return;
        }

        var maskSize = _ownerIsWayland
            ? WindowVisualLayerClip.CalculateWaylandMaskSize(
                layerSize,
                TopLevel.GetTopLevel(this)?.RenderScaling ?? 1)
            : layerSize;

        _maskMotionActor.HorizontalAlignment = HorizontalAlignment.Left;
        _maskMotionActor.VerticalAlignment = VerticalAlignment.Top;
        _maskMotionActor.Width = maskSize.Width;
        _maskMotionActor.Height = maskSize.Height;
        _maskMotionActor.Margin = default;
    }

    private static Point ConstrainSurfacePosition(Rect ownerBounds, Size surfaceSize, Point position)
    {
        return new Point(
            Math.Clamp(
                position.X,
                ownerBounds.X,
                Math.Max(ownerBounds.X, ownerBounds.Right - surfaceSize.Width)),
            Math.Clamp(
                position.Y,
                ownerBounds.Y,
                Math.Max(ownerBounds.Y, ownerBounds.Bottom - surfaceSize.Height)));
    }

    private Point ConstrainAndRoundSurfacePosition(Rect ownerBounds, Size surfaceSize, Point position)
    {
        var constrained = ConstrainSurfacePosition(ownerBounds, surfaceSize, position);
        if (!_surface.UseLayoutRounding)
        {
            return constrained;
        }

        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var rounded = LayoutHelper.RoundLayoutPoint(constrained, scaling);
        return ConstrainSurfacePosition(ownerBounds, surfaceSize, rounded);
    }

    private void SetSurfacePosition(Point position)
    {
        if (_surfacePosition == position)
        {
            return;
        }

        _surfacePosition = position;
        _surfacePositionTransform.Matrix = Matrix.CreateTranslation(position.X, position.Y);
    }

    private Size ResolveActualSurfaceSize()
    {
        var boundsSize = _surface.Bounds.Size;
        var width = double.IsFinite(_surface.Width)
            ? _surface.Width
            : boundsSize.Width > 0
                ? boundsSize.Width
                : _surface.DesiredSize.Width;
        var height = double.IsFinite(_surface.Height)
            ? _surface.Height
            : boundsSize.Height > 0
                ? boundsSize.Height
                : _surface.DesiredSize.Height;
        return new Size(width, height);
    }

    private Rect ResolveActualSurfaceBounds()
    {
        return new Rect(_surfacePosition, ResolveActualSurfaceSize());
    }

    private void ApplyNormalSurfacePlacement(Rect ownerBounds, Size size, Point position)
    {
        SynchronizeDialogOffsets(ownerBounds, size, position);
        SetSurfacePosition(position);
    }

    private void SynchronizeDialogOffsets(Rect ownerBounds, Size surfaceSize, Point position)
    {
        var currentPlacement = _dialog.CalculatePlacementOffset(surfaceSize, ownerBounds.Size);
        var baseOffset = new Point(
            currentPlacement.X - _dialog.OffsetX,
            currentPlacement.Y - _dialog.OffsetY);
        _dialog.SetCurrentValue(
            Dialog.OffsetXProperty,
            position.X - ownerBounds.X - baseOffset.X);
        _dialog.SetCurrentValue(
            Dialog.OffsetYProperty,
            position.Y - ownerBounds.Y - baseOffset.Y);
    }

    private void HandleHostWidthChanged(double value)
    {
        if (!double.IsFinite(value))
        {
            return;
        }

        if (_surface.IsDialogMaximized && _restoreSize is { } restoreSize)
        {
            _restoreSize = new Size(Math.Max(0, value), restoreSize.Height);
            return;
        }

        ApplyRequestedSize(new Size(value, _surface.Height));
    }

    private void HandleHostHeightChanged(double value)
    {
        if (!double.IsFinite(value))
        {
            return;
        }

        if (_surface.IsDialogMaximized && _restoreSize is { } restoreSize)
        {
            _restoreSize = new Size(restoreSize.Width, Math.Max(0, value));
            return;
        }

        ApplyRequestedSize(new Size(_surface.Width, value));
    }

    private void ApplyRequestedSize(Size requestedSize)
    {
        if (_dialogLayer is null || !_isInitialSizeResolved)
        {
            return;
        }

        var ownerBounds = ResolveCurrentDialogBodyOwnerBounds();
        ResolveNormalSurfaceSizeConstraints(ownerBounds);
        _surface.Width = ClampExplicitSize(
            requestedSize.Width, _normalSizeConstraints.MinWidth, _normalSizeConstraints.MaxWidth);
        _surface.Height = ClampExplicitSize(
            requestedSize.Height, _normalSizeConstraints.MinHeight, _normalSizeConstraints.MaxHeight);
        // An explicit request may reflow the other natural axis before placement is resolved.
        _surface.Measure(ownerBounds.Size);
        var surfaceSize = _surface.DesiredSize;
        var position = ConstrainAndRoundSurfacePosition(ownerBounds, surfaceSize, _surfacePosition);
        ApplyNormalSurfacePlacement(ownerBounds, surfaceSize, position);
    }

    private Rect ResolveCurrentDialogBodyOwnerBounds()
    {
        var visibleFrameBounds = ResolveOwnerBounds(_dialogLayer?.AvailableSize ?? Bounds.Size);
        return ResolveDialogBodyOwnerBounds(visibleFrameBounds);
    }

    private void HandleStructuralMinimumChanged(object? sender, EventArgs e)
    {
        if (_dialogLayer is null || _surface.IsDialogMaximized)
        {
            return;
        }

        var ownerBounds = ResolveCurrentDialogBodyOwnerBounds();
        if (ownerBounds.Width <= 0 || ownerBounds.Height <= 0)
        {
            return;
        }

        ApplyNormalSurfaceSizeConstraints(ownerBounds);
    }

    private static double ClampExplicitSize(double value, double min, double max)
    {
        return double.IsFinite(value) ? Math.Clamp(value, min, max) : double.NaN;
    }

    private AbstractMotion CreateSurfaceMotion(bool isOpening)
    {
        if (!_dialog.UsesPlacementTargetAsMotionAnchor)
        {
            return isOpening
                ? new FadeInMotion(MotionDuration, new CircularEaseOut())
                : new FadeOutMotion(MotionDuration, new CubicEaseIn());
        }

        var origin = ResolveMotionOrigin();
        return isOpening
            ? new DialogZoomInMotion(origin, MotionDuration)
            : new DialogZoomOutMotion(origin, MotionDuration);
    }

    private RelativePoint ResolveMotionOrigin()
    {
        var targetCenter = new Point(
            _placementTarget.Bounds.Width / 2,
            _placementTarget.Bounds.Height / 2);
        var presenterPoint = _placementTarget.TranslatePoint(targetCenter, this) ??
                             new Point(Bounds.Width / 2, Bounds.Height / 2);
        return new RelativePoint(presenterPoint, RelativeUnit.Absolute);
    }

    private void HandleSurfaceCloseRequested(object? sender, DialogSurfaceCloseRequestedEventArgs e)
    {
        // 钉住常开只拦截用户发起的关闭入口；外部 IsOpen 关闭走 presenter.CloseAsync，不经过这里。
        if (_dialog.IsPinnedOpen)
        {
            return;
        }

        CloseRequested?.Invoke(this,
            new DialogPresenterCloseRequestedEventArgs(e.Reason, e.Result, e.SourceButton));
    }

    private void HandleSurfaceSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var visibleFrameBounds = ResolveOwnerBounds(_dialogLayer?.AvailableSize ?? Bounds.Size);
        UpdateSurfacePlacement(ResolveDialogBodyOwnerBounds(visibleFrameBounds));
    }

    private void HandleHostCloseRequested(object? sender, EventArgs e)
    {
        // 钉住常开同时覆盖遮罩外点（HandleMaskPointerPressed 复用本处理器）与宿主关闭请求。
        if (_dialog.IsPinnedOpen)
        {
            return;
        }

        CloseRequested?.Invoke(this,
            new DialogPresenterCloseRequestedEventArgs(
                DialogCloseReason.HostCloseRequest,
                _dialog.Result));
    }

    private void HandleMaximizeRequested(object? sender, EventArgs e)
    {
        _restoreSize = new Size(_surface.Width, _surface.Height);
        _resizeOriginBounds = null;
        _surface.IsDialogMaximized = true;
        var visibleFrameBounds = ResolveOwnerBounds(_dialogLayer?.AvailableSize ?? Bounds.Size);
        ApplyMaximizedBounds(ResolveDialogBodyOwnerBounds(visibleFrameBounds));
    }

    private void HandleRestoreRequested(object? sender, EventArgs e)
    {
        _surface.IsDialogMaximized = false;
        _surface.ClearValue(TemplatedControl.CornerRadiusProperty);
        var ownerBounds = ResolveCurrentDialogBodyOwnerBounds();
        ResolveNormalSurfaceSizeConstraints(ownerBounds);
        var restoreSize = _restoreSize ?? new Size(_surface.Width, _surface.Height);
        _surface.Width = ClampExplicitSize(
            restoreSize.Width, _normalSizeConstraints.MinWidth, _normalSizeConstraints.MaxWidth);
        _surface.Height = ClampExplicitSize(
            restoreSize.Height, _normalSizeConstraints.MinHeight, _normalSizeConstraints.MaxHeight);
        _restoreSize = null;
        UpdateSurfacePlacement(ownerBounds);
    }

    private void HandleResizeStarted(object? sender, OverlayDialogResizeEventArgs e)
    {
        if (_surface.IsDialogMaximized)
        {
            return;
        }

        _resizeOriginBounds = ResolveActualSurfaceBounds();
    }

    private void HandleResizeRequested(object? sender, OverlayDialogResizeEventArgs e)
    {
        if (_surface.IsDialogMaximized)
        {
            return;
        }

        var originBounds = _resizeOriginBounds ?? ResolveActualSurfaceBounds();
        _resizeOriginBounds ??= originBounds;
        var ownerBounds = ResolveCurrentDialogBodyOwnerBounds();
        var width = originBounds.Width;
        var height = originBounds.Height;
        var x = originBounds.X;
        var y = originBounds.Y;
        if ((e.Location & ResizeHandleLocation.East) != 0)
        {
            var availableWidth = Math.Max(0, ownerBounds.Right - originBounds.Left);
            width = Math.Clamp(
                originBounds.Width + e.DeltaOffsetX,
                _normalSizeConstraints.MinWidth,
                Math.Min(_normalSizeConstraints.MaxWidth, availableWidth));
        }
        else if ((e.Location & ResizeHandleLocation.West) != 0)
        {
            var availableWidth = Math.Max(0, originBounds.Right - ownerBounds.Left);
            width = Math.Clamp(
                originBounds.Width - e.DeltaOffsetX,
                _normalSizeConstraints.MinWidth,
                Math.Min(_normalSizeConstraints.MaxWidth, availableWidth));
            x = originBounds.Right - width;
        }

        if ((e.Location & ResizeHandleLocation.South) != 0)
        {
            var availableHeight = Math.Max(0, ownerBounds.Bottom - originBounds.Top);
            height = Math.Clamp(
                originBounds.Height + e.DeltaOffsetY,
                _normalSizeConstraints.MinHeight,
                Math.Min(_normalSizeConstraints.MaxHeight, availableHeight));
        }
        else if ((e.Location & ResizeHandleLocation.North) != 0)
        {
            var availableHeight = Math.Max(0, originBounds.Bottom - ownerBounds.Top);
            height = Math.Clamp(
                originBounds.Height - e.DeltaOffsetY,
                _normalSizeConstraints.MinHeight,
                Math.Min(_normalSizeConstraints.MaxHeight, availableHeight));
            y = originBounds.Bottom - height;
        }

        // Only a changed resize axis takes over natural sizing. An already fixed axis still
        // follows the cumulative drag back towards its origin; unrelated axes retain their input.
        var changesWidth = (e.Location & (ResizeHandleLocation.East | ResizeHandleLocation.West)) != 0;
        var changesHeight = (e.Location & (ResizeHandleLocation.North | ResizeHandleLocation.South)) != 0;
        if (changesWidth && (double.IsFinite(_surface.Width) || !MathUtils.AreClose(width, originBounds.Width)))
        {
            _surface.Width = width;
        }

        if (changesHeight && (double.IsFinite(_surface.Height) || !MathUtils.AreClose(height, originBounds.Height)))
        {
            _surface.Height = height;
        }

        var surfaceSize = new Size(width, height);
        var position = ConstrainAndRoundSurfacePosition(ownerBounds, surfaceSize, new Point(x, y));
        ApplyNormalSurfacePlacement(ownerBounds, surfaceSize, position);
    }

    private void HandleResizeCompleted(object? sender, OverlayDialogResizeEventArgs e)
    {
        _resizeOriginBounds = null;
    }

    private void HandleHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_dialog.IsDragMovable ||
            _surface.IsDialogMaximized ||
            !e.Properties.IsLeftButtonPressed ||
            e.Source is Visual source &&
            source.FindAncestorOfType<DialogCaptionButton>(true) is not null)
        {
            return;
        }

        _dragPointerOffset = e.GetPosition(_surface);
        e.Pointer.Capture(_surface.Header);
        e.PreventGestureRecognition();
        e.Handled = true;
    }

    private void HandleHeaderPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragPointerOffset is null || !e.Properties.IsLeftButtonPressed)
        {
            return;
        }

        MoveSurface(e.GetPosition(this));
        e.Handled = true;
    }

    private void MoveSurface(Point pointerPosition)
    {
        var surfaceSize = _surface.Bounds.Size == default
            ? new Size(_surface.Width, _surface.Height)
            : _surface.Bounds.Size;
        var pointerOffset = _dragPointerOffset!.Value;
        var visibleFrameBounds = ResolveOwnerBounds(_dialogLayer?.AvailableSize ?? Bounds.Size);
        var position = ConstrainAndRoundSurfacePosition(
            ResolveDialogBodyOwnerBounds(visibleFrameBounds),
            surfaceSize,
            pointerPosition - new Vector(pointerOffset.X, pointerOffset.Y));

        _dialog.SetCurrentValue(Dialog.OffsetXProperty, _dialog.OffsetX + position.X - _surfacePosition.X);
        _dialog.SetCurrentValue(Dialog.OffsetYProperty, _dialog.OffsetY + position.Y - _surfacePosition.Y);
        SetSurfacePosition(position);
    }

    private void HandleHeaderPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragPointerOffset is null)
        {
            return;
        }

        _dragPointerOffset = null;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void HandleMaskPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.Properties.IsLeftButtonPressed ||
            e.Source is not Visual source ||
            _dialogMask is not { } dialogMask ||
            (!ReferenceEquals(source, dialogMask) &&
             !dialogMask.IsVisualAncestorOf(source)) ||
            _dialogLayer?.IsTopmost(this) != true)
        {
            return;
        }

        e.Handled = true;
        if (!IsMaskClosable)
        {
            return;
        }

        HandleHostCloseRequested(this, EventArgs.Empty);
    }

    private void ReleaseDialogMask()
    {
        if (_dialogMask is not null)
        {
            _dialogMask.PointerPressed -= HandleMaskPointerPressed;
            _dialogMask = null;
        }
    }

    private void HandlePresenterPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ReferenceEquals(e.Source, _dialogMask))
        {
            return;
        }

        _dialogLayer?.Activate(this);
        _surface.Focus(NavigationMethod.Pointer);
    }

}
