using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Utilities;
using Avalonia.VisualTree;
using AtomUI.Desktop.Controls.DialogPositioning;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Desktop.Controls.Themes;
using Avalonia.Animation.Easings;
using Avalonia.Media.Transformation;

namespace AtomUI.Desktop.Controls;

internal class OverlayDialogHost : ContentControl,
                                   IInputRoot,
                                   IDialogHost,
                                   IMotionAwareControl,
                                   IManagedDialogPositionerDialog
{
    #region 公共属性定义
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<OverlayDialogHost, string?>(nameof (Title));
    
    public static readonly StyledProperty<PathIcon?> TitleIconProperty =
        AvaloniaProperty.Register<OverlayDialogHost, PathIcon?>(nameof (TitleIcon));
    
    public static readonly StyledProperty<bool> IsResizableProperty =
        Dialog.IsResizableProperty.AddOwner<OverlayDialogHost>();
        
    public static readonly StyledProperty<bool> IsClosableProperty =
        Dialog.IsClosableProperty.AddOwner<OverlayDialogHost>();
    
    public static readonly StyledProperty<bool> IsMaximizableProperty =
        Dialog.IsMaximizableProperty.AddOwner<OverlayDialogHost>();
    
    public static readonly StyledProperty<bool> IsDragMovableProperty =
        Dialog.IsDragMovableProperty.AddOwner<OverlayDialogHost>();
    
    public static readonly StyledProperty<bool> IsModalProperty =
        Dialog.IsModalProperty.AddOwner<OverlayDialogHost>();
    
    public static readonly StyledProperty<OverlayDialogState> WindowStateProperty =
        AvaloniaProperty.Register<OverlayDialogHost, OverlayDialogState>(nameof(WindowState));
    
    public static readonly StyledProperty<Transform?> TransformProperty =
        AvaloniaProperty.Register<OverlayDialogHost, Transform?>(nameof (Transform));
    
    public static readonly StyledProperty<DialogStandardButtons> StandardButtonsProperty =
        DialogButtonBox.StandardButtonsProperty.AddOwner<OverlayDialogHost>();
    
    public static readonly StyledProperty<DialogStandardButton> DefaultStandardButtonProperty =
        DialogButtonBox.DefaultStandardButtonProperty.AddOwner<OverlayDialogHost>();
    
    public static readonly StyledProperty<DialogStandardButton> EscapeStandardButtonProperty =
        DialogButtonBox.EscapeStandardButtonProperty.AddOwner<OverlayDialogHost>();
    
    public static readonly StyledProperty<bool> IsFooterVisibleProperty =
        Dialog.IsFooterVisibleProperty.AddOwner<OverlayDialogHost>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<OverlayDialogHost>();
    
    public static readonly StyledProperty<bool> IsLoadingProperty = Dialog.IsLoadingProperty.AddOwner<OverlayDialogHost>();
    public static readonly StyledProperty<bool> IsConfirmLoadingProperty = Dialog.IsConfirmLoadingProperty.AddOwner<OverlayDialogHost>();
    
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public PathIcon? TitleIcon
    {
        get => GetValue(TitleIconProperty);
        set => SetValue(TitleIconProperty, value);
    }
    
    public bool IsResizable
    {
        get => GetValue(IsResizableProperty);
        set => SetValue(IsResizableProperty, value);
    }
    
    public bool IsMaximizable
    {
        get => GetValue(IsMaximizableProperty);
        set => SetValue(IsMaximizableProperty, value);
    }
    
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }
    
    public bool IsDragMovable
    {
        get => GetValue(IsDragMovableProperty);
        set => SetValue(IsDragMovableProperty, value);
    }
    
    public bool IsModal
    {
        get => GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
    }
    
    public OverlayDialogState WindowState
    {
        get => GetValue(WindowStateProperty);
        set => SetValue(WindowStateProperty, value);
    }
    
    public Transform? Transform
    {
        get => GetValue(TransformProperty);
        set => SetValue(TransformProperty, value);
    }
    
    public DialogStandardButtons StandardButtons
    {
        get => GetValue(StandardButtonsProperty);
        set => SetValue(StandardButtonsProperty, value);
    }
    
    public DialogStandardButton DefaultStandardButton
    {
        get => GetValue(DefaultStandardButtonProperty);
        set => SetValue(DefaultStandardButtonProperty, value);
    }
    
    public DialogStandardButton EscapeStandardButton
    {
        get => GetValue(EscapeStandardButtonProperty);
        set => SetValue(EscapeStandardButtonProperty, value);
    }

    public bool IsFooterVisible
    {
        get => GetValue(IsFooterVisibleProperty);
        set => SetValue(IsFooterVisibleProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    
    public bool IsConfirmLoading
    {
        get => GetValue(IsConfirmLoadingProperty);
        set => SetValue(IsConfirmLoadingProperty, value);
    }
    
    public AvaloniaList<DialogButton> CustomButtons { get; } = new ();
    
    #endregion
    
    internal event EventHandler? HeaderPressed;
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<OverlayDialogHost, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<OverlayDialogHost, bool>(nameof(IsDragging),
            o => o.IsDragging,
            (o, v) => o.IsDragging = v);
    
    internal static readonly DirectProperty<OverlayDialogHost, bool> IsEffectiveFooterVisibleProperty =
        AvaloniaProperty.RegisterDirect<OverlayDialogHost, bool>(
            nameof(IsEffectiveFooterVisible),
            o => o.IsEffectiveFooterVisible,
            (o, v) => o.IsEffectiveFooterVisible = v);
    
    internal static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<OverlayDialogHost, TimeSpan>(nameof(AnimationDuration));
    
    internal static readonly DirectProperty<OverlayDialogHost, bool> IsHostAnimatingProperty =
        AvaloniaProperty.RegisterDirect<OverlayDialogHost, bool>(
            nameof(IsHostAnimating),
            o => o.IsHostAnimating,
            (o, v) => o.IsHostAnimating = v);
    
    private bool _isDragging;

    internal bool IsDragging
    {
        get => _isDragging;
        set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }
    
    private bool _isEffectiveFooterVisible;

    internal bool IsEffectiveFooterVisible
    {
        get => _isEffectiveFooterVisible;
        set => SetAndRaise(IsEffectiveFooterVisibleProperty, ref _isEffectiveFooterVisible, value);
    }
    
    internal TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }
    
    private bool _isHostAnimating;

    internal bool IsHostAnimating
    {
        get => _isHostAnimating;
        set => SetAndRaise(IsHostAnimatingProperty, ref _isHostAnimating, value);
    }
    
    internal bool IsHidden => _dialogLayer.Children.Contains(this);
    
    #endregion

    private IInputRoot? InputRoot => TopLevel.GetTopLevel(this);
    private readonly DialogLayer _dialogLayer;
    private readonly ManagedDialogPositioner _positioner;
    private readonly IKeyboardNavigationHandler? _keyboardNavigationHandler;
    private Point _lastRequestedPosition;
    private DialogPositionRequest? _dialogPositionRequest;
    private Size _dialogSize;
    private bool _needsUpdate;
    private OverlayDialogMask _dialogMask;
    
    // 用于最大化
    private Point _originPosition;
    private OverlayDialogHeader? _header;
    private OverlayDialogResizer? _resizer;
    private readonly List<Action> _disposeActions = new();
    private Dialog _dialog;
    private DialogButtonBox? _buttonBox;
    private CompositeDisposable? _confirmLoadingBindings;
    
    // 拖动
    private Size? _lastestSize;
    private Point? _lastestPoint;
    static OverlayDialogHost()
    {
        KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<OverlayDialogHost>(KeyboardNavigationMode.Cycle);
        AffectsMeasure<OverlayDialogHost>(WindowStateProperty);
    }

    public OverlayDialogHost(DialogLayer dialogLayer, Dialog dialog)
    {
        _dialog                    = dialog;
        _dialogLayer               = dialogLayer;
        _positioner                = new ManagedDialogPositioner(this);
        _keyboardNavigationHandler = AvaloniaLocator.Current.GetService<IKeyboardNavigationHandler>();
        _keyboardNavigationHandler?.SetOwner(this);
        CustomButtons.CollectionChanged +=  new NotifyCollectionChangedEventHandler(HandleCustomButtonsChanged);
        _dialogMask                     ??= new OverlayDialogMask(_dialogLayer, _dialog);
    }
    
    IKeyboardNavigationHandler? IInputRoot.KeyboardNavigationHandler => _keyboardNavigationHandler;
    
    IFocusManager? IInputRoot.FocusManager => InputRoot?.FocusManager;
    
    IPlatformSettings? IInputRoot.PlatformSettings => InputRoot?.PlatformSettings;
    
    IInputElement? IInputRoot.PointerOverElement
    {
        get => InputRoot?.PointerOverElement;
        set
        {
            if (InputRoot is { } inputRoot)
            {
                inputRoot.PointerOverElement = value;
            }
        }
    }

    bool IInputRoot.ShowAccessKeys
    {
        get => InputRoot?.ShowAccessKeys ?? false;
        set
        {
            if (InputRoot is { } inputRoot)
            {
                inputRoot.ShowAccessKeys = value;
            }
        }
    }
    
    public void SetChild(Control? control)
    {
        Content = control;
    }
    
    public Visual? HostedVisualTreeRoot => null;
    
    // 多个 overlay 中始终置顶也可以有的
    bool IDialogHost.Topmost { get; set; }

    public void Show()
    {
        _dialogLayer.Children.Add(this);
        _dialogLayer.SizeChanged +=  HandleDialogLayerSizeChanged;
        if (IsModal)
        {

            _dialogMask.Show(this);
        }
        if (Content is Visual visual && !visual.IsAttachedToVisualTree())
        {
            // We need to force a measure pass so any descendants are built, for focus to work.
            UpdateLayout();
        }

        IsHostAnimating = false;
        if (IsMotionEnabled)
        {
            Opacity     =   0.0;
            {
                var builder = new TransformOperations.Builder(2);
                var offset  = CalculateOffsetFromPlacement();
                builder.AppendScale(0.75, 0.75);
                builder.AppendTranslate(-offset.X, -offset.Y);
           
                RenderTransform = builder.Build();
            }
            
            Dispatcher.UIThread.Post(() =>
            {
                IsHostAnimating = true;
                Opacity         = 1.0;
                {
                    var builder = new TransformOperations.Builder(2);
                    builder.AppendScale(1.0, 1.0);
                    builder.AppendTranslate(0, 0);
                    RenderTransform = builder.Build();
                }
                DispatcherTimer.RunOnce(() =>
                {
                    IsHostAnimating = false;
                }, AnimationDuration);
            });
        }
    }

    private Point CalculateOffsetFromPlacement()
    {
        var offset = new Point();
        if (_dialog.PlacementTarget != null)
        {
            var sourceOffset = _dialog.PlacementTarget.TranslatePoint(new Point(0, 0), _dialogLayer);
            var targetOffset = Bounds.TopLeft;
            if (sourceOffset != null)
            {
                var offsetX = targetOffset.X - sourceOffset.Value.X;
                var offsetY = targetOffset.Y - sourceOffset.Value.Y;
                offset = new Point(offsetX, offsetY);
            }
        }
        return offset;
    }

    public void Close(Action? callback = null)
    {
        IsHostAnimating = false;
        if (IsMotionEnabled)
        {
            IsHostAnimating = true;
            Opacity         = 0.0;
            
            {
                var builder = new TransformOperations.Builder(2);
                var offset  = CalculateOffsetFromPlacement();
                builder.AppendTranslate(-offset.X, -offset.Y);
                builder.AppendScale(0.75, 0.75);
                RenderTransform = builder.Build();
            }
            DispatcherTimer.RunOnce(() =>
            {
                IsHostAnimating = false;
                HandleClosed();
                callback?.Invoke();
            }, AnimationDuration * 1.1);
        }
        else
        {
            HandleClosed();
            callback?.Invoke();
        }
        if (IsModal)
        {
            Debug.Assert(_dialogMask != null);
            _dialogMask.Hide();
        }
    }

    private void HandleClosed()
    {
        _dialogLayer.SizeChanged -= HandleDialogLayerSizeChanged;
        _dialogLayer.Children.Remove(this);
        _dialog.ClearValue(Dialog.OffsetXProperty);
        _dialog.ClearValue(Dialog.OffsetYProperty);
        foreach (var disposeAction in _disposeActions)
        {
            disposeAction.Invoke();
        }
        _disposeActions.Clear();
    }

    private void HandleDialogLayerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        _needsUpdate = true;
        UpdatePosition();
        
        var offsetX = Canvas.GetLeft(this);
        var offsetY = Canvas.GetTop(this);

        var bounds     = _dialogLayer.Bounds;
        var maxOffsetX = bounds.Width - DesiredSize.Width;
        var maxOffsetY =  bounds.Height - DesiredSize.Height;
        
        var deltaX = maxOffsetX - offsetX;
        if (deltaX < 0)
        {
            offsetX += deltaX;
        }
        
        _dialog.SetCurrentValue(Dialog.OffsetXProperty, Math.Min(Math.Max(offsetX, 0), maxOffsetX));
        _dialog.SetCurrentValue(Dialog.OffsetYProperty, Math.Min(Math.Max(offsetY, 0), maxOffsetY));
    }
    
    void IDialogHost.ConfigurePosition(DialogPositionRequest positionRequest)
    {
        _dialogPositionRequest = positionRequest;
        _needsUpdate           = true;
        UpdatePosition();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        if (WindowState == OverlayDialogState.Maximized)
        {
            return _dialogLayer.Bounds.Size;
        }
        return size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_dialogSize != finalSize)
        {
            _dialogSize  = finalSize;
            _dialog.NotifyDialogHostMeasured(_dialogSize, new Rect(_dialogLayer.DesiredSize));
            _needsUpdate = true;
            UpdatePosition();
        }
        return base.ArrangeOverride(finalSize);
    }

    private void UpdatePosition()
    {
        if (_needsUpdate && _dialogPositionRequest is not null)
        {
            _needsUpdate = false;
            _positioner.Update(_dialogPositionRequest, _dialogSize);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsModalProperty)
        {
            if (!IsHidden)
            {
                if (change.OldValue is bool oldValue && oldValue)
                {
                    _dialogMask.Hide();
                }

                if (change.NewValue is bool newValue && newValue)
                {
                    _dialogMask.Show(this);
                }
            }
        }
        else if (change.Property == WindowStateProperty)
        {
            HandleWindowStateChanged(change.GetOldValue<OverlayDialogState>(), change.GetNewValue<OverlayDialogState>());
        }
        else if (change.Property == StandardButtonsProperty ||
                 change.Property == IsLoadingProperty)
        {
            ConfigureEffectiveFooterVisible();
        }
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }

    private void ConfigureEffectiveFooterVisible()
    {
        if (IsFooterVisible)
        {
            if (!IsLoading)
            {
                SetCurrentValue(IsEffectiveFooterVisibleProperty, StandardButtons.Count > 0 || CustomButtons.Count > 0);
            }
            else
            {
                SetCurrentValue(IsEffectiveFooterVisibleProperty, false);
            }
        }
        else
        {
            SetCurrentValue(IsEffectiveFooterVisibleProperty, false);
        }
    }
    
    IReadOnlyList<ManagedDialogPositionerScreenInfo> IManagedDialogPositionerDialog.ScreenInfos
    {
        get
        {
            var rc       = new Rect(default, _dialogLayer.AvailableSize);
            var topLevel = TopLevel.GetTopLevel(this);
            if(topLevel != null)
            {
                var padding = topLevel.InsetsManager?.SafeAreaPadding ?? default;
                rc = rc.Deflate(padding);
            }
    
            return [new ManagedDialogPositionerScreenInfo(rc, rc)];
        }
    }
    
    Rect IManagedDialogPositionerDialog.ParentClientAreaScreenGeometry => new Rect(default, _dialogLayer.Bounds.Size);
    
    void IManagedDialogPositionerDialog.Move(Point devicePoint)
    {
        _lastRequestedPosition = devicePoint;
        Canvas.SetLeft(this, _lastRequestedPosition.X);
        Canvas.SetTop(this, _lastRequestedPosition.Y);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _resizer = e.NameScope.Find<OverlayDialogResizer>(OverlayDialogThemeConstants.ResizerPart);
        if (_resizer != null)
        {
            _resizer.TargetDialog  =  this;
            _resizer.ResizeRequest += HandleResizeRequest;
            _resizer.AboutToResize += HandleAboutToResize;
        }
        _header    = e.NameScope.Find<OverlayDialogHeader>(OverlayDialogThemeConstants.HeaderPart);
        _buttonBox = e.NameScope.Find<DialogButtonBox>(DialogThemeConstants.ButtonBoxPart);
        ConfigureHeaderHandlers();
        if (_header != null)
        {
            _header.SetCurrentValue(OverlayDialogHeader.IsDialogMaximizedProperty, WindowState == OverlayDialogState.Maximized);
            _header.Pressed += (sender, args) =>
            {
                HeaderPressed?.Invoke(this, EventArgs.Empty);
            };
        }
        if (_buttonBox != null)
        {
            _buttonBox.CustomButtons.AddRange(CustomButtons);
            _buttonBox.Clicked             += HandleButtonBoxClicked;
            _buttonBox.ButtonsSynchronized += HandleButtonsSynchronized;
        }
    }
    
    private void HandleButtonBoxClicked(object? sender, DialogButtonClickedEventArgs args)
    {
        _dialog.NotifyDialogButtonBoxClicked(args.SourceButton);
    }
    
    private void HandleButtonsSynchronized(object? sender, DialogBoxButtonSyncEventArgs args)
    {
        _dialog.NotifyDialogButtonSynchronized(args.Buttons);
        _confirmLoadingBindings?.Dispose();
        _confirmLoadingBindings = new CompositeDisposable(args.Buttons.Count);
        foreach (var button in args.Buttons)
        {
            if (button.Role == DialogButtonRole.AcceptRole ||
                button.Role == DialogButtonRole.YesRole ||
                button.Role == DialogButtonRole.ApplyRole)
            {
                _confirmLoadingBindings.Add(BindUtils.RelayBind(this, IsConfirmLoadingProperty, button, Button.IsLoadingProperty));
            }
        }
    }

    private void HandleAboutToResize(object? sender, OverlayDialogResizeEventArgs args)
    {
        _lastestSize  = DesiredSize;
        _lastestPoint = _lastRequestedPosition;
    }

    private void HandleResizeRequest(object? sender, OverlayDialogResizeEventArgs args)
    {
        if (_lastestSize != null && _lastestPoint != null)
        {
            var originWidth  = _lastestSize.Value.Width;
            var originHeight = _lastestSize.Value.Height;
            if (args.Location == ResizeHandleLocation.East)
            {
                var width    = originWidth + args.DeltaOffsetX;
                var minWidth = 0d;
                if (_dialog.IsSet(Dialog.MinWidthProperty))
                {
                    minWidth =  _dialog.MinWidth;
                }
                width = Math.Max(width, minWidth);
                if (_dialog.IsSet(Dialog.MaxWidthProperty))
                {
                    width = Math.Min(width, MaxWidth);
                }
                _dialog.SetCurrentValue(Dialog.WidthProperty, width);
            }
            else if (args.Location == ResizeHandleLocation.West)
            {
                var minWidthReached = false;
                var width           = originWidth - args.DeltaOffsetX;
                if (_dialog.IsSet(Dialog.MinWidthProperty))
                {
                    if (MathUtilities.LessThanOrClose(width, _dialog.MinWidth))
                    {
                        minWidthReached = true;
                    }
                    width = Math.Max(width, _dialog.MinWidth);
                }
                
                if (_dialog.IsSet(Dialog.MaxWidthProperty))
                {
                    width = Math.Min(width, MaxWidth);
                }
                
                if (!minWidthReached)
                {
                    _dialog.SetCurrentValue(Dialog.OffsetXProperty, _lastestPoint.Value.X + args.DeltaOffsetX);
                    _dialog.SetCurrentValue(Dialog.WidthProperty, width);
                }
            }
            else if (args.Location == ResizeHandleLocation.North)
            {
                var minHeightReached = false;
                var height           = originHeight - args.DeltaOffsetY;
                if (_dialog.IsSet(Dialog.MinWidthProperty))
                {
                    if (MathUtilities.LessThanOrClose(height, _dialog.MinHeight))
                    {
                        minHeightReached = true;
                    }
                    height = Math.Max(height, _dialog.MinHeight);
                }
                
                if (_dialog.IsSet(Dialog.MaxHeightProperty))
                {
                    height = Math.Min(height, MaxHeight);
                }
                
                if (!minHeightReached)
                {
                    _dialog.SetCurrentValue(Dialog.OffsetYProperty, _lastestPoint.Value.Y + args.DeltaOffsetY);
                    _dialog.SetCurrentValue(Dialog.HeightProperty, height);
                }
                
            }
            else if (args.Location == ResizeHandleLocation.South)
            {
                var height    = originHeight + args.DeltaOffsetY;
                var minHeight = 0d;
                if (_dialog.IsSet(MinHeightProperty))
                {
                    minHeight =  _dialog.MinHeight;
                }
                height = Math.Max(height, minHeight);
                if (_dialog.IsSet(MaxHeightProperty))
                {
                    height = Math.Min(height, MaxHeight);
                }
                _dialog.SetCurrentValue(HeightProperty, height);
            }
        }
    }

    private void ConfigureHeaderHandlers()
    {
        if (_header != null)
        {
            _header.DoubleTapped     += HandleHeaderDoubleClicked;
            _header.MaximizeRequest  += HandleHeaderMaximizeRequest;
            _header.NormalizeRequest += HandleHeaderNormalizeRequest;
            _header.CloseRequest     += HandleHeaderCloseRequest;
            _header.PointerPressed   += HandleHeaderPointerPressed;
            _header.PointerReleased  += HandleHeaderPointerReleased;
            _header.PointerMoved     += HandleHeaderPointerMoved;
            _disposeActions.Add(() =>
            {
                _header.DoubleTapped     -= HandleHeaderDoubleClicked;
                _header.MaximizeRequest  -= HandleHeaderMaximizeRequest;
                _header.NormalizeRequest -= HandleHeaderNormalizeRequest;
                _header.CloseRequest     -= HandleHeaderCloseRequest;
                _header.PointerPressed   -= HandleHeaderPointerPressed;
                _header.PointerReleased  -= HandleHeaderPointerReleased;
                _header.PointerMoved     -= HandleHeaderPointerMoved;
            });
        }
    }
    
    private void HandleHeaderDoubleClicked(object? sender, RoutedEventArgs e)
    {
        if (!IsMaximizable || !IsResizable)
        {
            return;
        }

        WindowState = WindowState == OverlayDialogState.Normal ? OverlayDialogState.Maximized : OverlayDialogState.Normal;
    }
    
    private void HandleHeaderNormalizeRequest(object? sender, EventArgs e)
    {
        SetCurrentValue(WindowStateProperty, OverlayDialogState.Normal);
    }

    private void HandleHeaderMaximizeRequest(object? sender, EventArgs e)
    {
        SetCurrentValue(WindowStateProperty, OverlayDialogState.Maximized);
    }
    
    private void HandleHeaderCloseRequest(object? sender, EventArgs e)
    {
        if (Parent is Dialog dialog)
        {
            dialog.Done();
        }
    }

    private void HandleHeaderPointerPressed(object? sender, PointerEventArgs e)
    {
        if (IsDragMovable && e.Properties.IsLeftButtonPressed)
        {
            e.Handled     = true;
            _lastestPoint = e.GetPosition(this);
            e.PreventGestureRecognition();
        }
    }

    private void HandleHeaderPointerReleased(object? sender, PointerEventArgs e)
    {
        if (_lastestPoint.HasValue)
        {
            e.Handled     = true;
            _lastestPoint = null;
            SetCurrentValue(IsDraggingProperty, false);
        }
    }

    private void HandleHeaderPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_lastestPoint.HasValue && e.Properties.IsLeftButtonPressed)
        {
            var delta             = e.GetPosition(this) - _lastestPoint.Value;
            var manhattanDistance = Math.Abs(delta.X) + Math.Abs(delta.Y);
            if (manhattanDistance > Constants.DragThreshold)
            {
                if (!IsDragging)
                {
                    SetCurrentValue(IsDraggingProperty, true);
                }

                HandleDragging(e.GetPosition(this), delta);
            }
        }
    }
    
    private void HandleDragging(Point position, Point delta)
    {
        var offsetX = Canvas.GetLeft(this);
        var offsetY = Canvas.GetTop(this);

        offsetX += delta.X;
        offsetY += delta.Y;

        var bounds     = _dialogLayer.Bounds;
        var maxOffsetX = bounds.Width - DesiredSize.Width;
        var maxOffsetY =  bounds.Height - DesiredSize.Height;
        
        _dialog.SetCurrentValue(Dialog.OffsetXProperty, Math.Min(Math.Max(offsetX, 0), maxOffsetX));
        _dialog.SetCurrentValue(Dialog.OffsetYProperty, Math.Min(Math.Max(offsetY, 0), maxOffsetY));
    }
    
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_lastestPoint.HasValue)
        {
            _lastestPoint = null;
            IsDragging    = false;
        }

        base.OnPointerCaptureLost(e);
    }

    private void HandleWindowStateChanged(OverlayDialogState oldState, OverlayDialogState newState)
    {
        if (oldState == OverlayDialogState.Normal)
        {
            _originPosition = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
        }

        if (newState == OverlayDialogState.Maximized)
        {
            _dialog.SetCurrentValue(Dialog.OffsetXProperty, 0);
            _dialog.SetCurrentValue(Dialog.OffsetYProperty, 0);
        }
        else if (newState == OverlayDialogState.Normal)
        {
            _dialog.SetCurrentValue(Dialog.OffsetXProperty, _originPosition.X);
            _dialog.SetCurrentValue(Dialog.OffsetYProperty, _originPosition.Y);
        }
        if (_header != null)
        {
            _header.SetCurrentValue(OverlayDialogHeader.IsDialogMaximizedProperty, WindowState == OverlayDialogState.Maximized);
        }
    }
    
    private void HandleCustomButtonsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_buttonBox != null)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItems = e.NewItems!.OfType<DialogButton>();
                    _buttonBox.CustomButtons.AddRange(newItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldItems = e.OldItems!.OfType<DialogButton>();
                    _buttonBox.CustomButtons.RemoveAll(oldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
        }

        ConfigureEffectiveFooterVisible();
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }

    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                var easing = new CircularEaseOut();
                Transitions = [
                    TransitionUtils.CreateTransition<DoubleTransition>(OpacityProperty, AnimationDuration, easing),
                    TransitionUtils.CreateTransition<TransformOperationsTransition>(RenderTransformProperty, AnimationDuration, easing),
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    internal void NotifyChangeZIndex(int zindex)
    {
        _dialogMask.SetCurrentValue(OverlayDialogMask.ZIndexProperty, zindex - 1);
        SetCurrentValue(ZIndexProperty, zindex);
    }
}