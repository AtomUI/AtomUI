using AtomUI.Controls;
using AtomUI.Desktop.Controls.Utils;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;

public class ScrollViewer : AvaloniaScrollViewer,
                            IMotionAwareControl,
                            IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly AttachedProperty<bool> IsLiteModeProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, Control, bool>(
            nameof(IsLiteMode));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ScrollViewer>();
    
    public bool IsLiteMode
    {
        get => GetValue(IsLiteModeProperty);
        set => SetValue(IsLiteModeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<double> ScrollBarsSeparatorOpacityProperty =
        AvaloniaProperty.Register<ScrollViewer, double>(nameof(ScrollBarsSeparatorOpacity));
    
    internal static readonly StyledProperty<double> ScrollBarOpacityProperty =
        AvaloniaProperty.Register<ScrollViewer, double>(nameof(ScrollBarOpacity));
    
    internal double ScrollBarsSeparatorOpacity
    {
        get => GetValue(ScrollBarsSeparatorOpacityProperty);
        set => SetValue(ScrollBarsSeparatorOpacityProperty, value);
    }
    
    internal double ScrollBarOpacity
    {
        get => GetValue(ScrollBarOpacityProperty);
        set => SetValue(ScrollBarOpacityProperty, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ScrollViewerToken.ID;

    #endregion
    
    private bool _scrollBarDragging = false;
    private bool _isPointerInside = false;
    private IDisposable? _pointerMoveSubscription;

    static ScrollViewer()
    {
        Thumb.DragStartedEvent.AddClassHandler<ScrollViewer>((view, evt) =>
        {
            view.HandleScrollBarDragStarted();
        });
        Thumb.DragCompletedEvent.AddClassHandler<ScrollViewer>((view, evt) =>
        {
            view.HandleScrollBarDragCompleted();
        });
    }
    
    public ScrollViewer()
    {
        this.RegisterResources();
    }
    
    public static bool GetIsLiteMode(Control control)
    {
        return control.GetValue(IsLiteModeProperty);
    }
    
    public static void SetIsLiteMode(Control control, bool value)
    {
        control.SetValue(IsLiteModeProperty, value);
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<DoubleTransition>(ScrollBarsSeparatorOpacityProperty),
                    TransitionUtils.CreateTransition<DoubleTransition>(ScrollBarOpacityProperty),
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(false);
            }
        }

        if (change.Property == AllowAutoHideProperty)
        {
            if (AllowAutoHide)
            {
                ConfigureInputManager();
            }
            else
            {
                _pointerMoveSubscription?.Dispose();
                _pointerMoveSubscription = null;
            }
        }
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

    private void HandleScrollBarDragStarted()
    {
        _scrollBarDragging = true;
    }
    
    private void HandleScrollBarDragCompleted()
    {
        _scrollBarDragging = false;
        if (AllowAutoHide && !_isPointerInside)
        {
            ScrollBarOpacity = 0.0;
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureInputManager();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _pointerMoveSubscription?.Dispose();
        _pointerMoveSubscription = null;
    }

    private void ConfigureInputManager()
    {
        if (AllowAutoHide)
        {
            var inputManager = AvaloniaLocator.Current.GetService<IInputManager>();
            _pointerMoveSubscription = inputManager?.Process.Subscribe(ListenForMouseEvent);
        }
    }
    
    private void ListenForMouseEvent(RawInputEventArgs e)
    {
        if (e is RawPointerEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.GetInputRoot() != TopLevel.GetTopLevel(this))
            {
                return;
            }
            
            if (mouseEventArgs.Type == RawPointerEventType.Move)
            {
                if (IsMousePointIn(mouseEventArgs))
                {
                    if (!_isPointerInside)
                    {
                        _isPointerInside = true;
                        ScrollBarOpacity = 1.0;
                    }
                }
                else
                {
                    if (_isPointerInside)
                    {
                        _isPointerInside = false;
                        if (!_scrollBarDragging)
                        {
                            ScrollBarOpacity = 0.0;
                        }
                    }
                }
            }
        }
    }
    
    private bool IsMousePointIn(RawPointerEventArgs args)
    {
        var pointRoot        = args.Root as Control;
        var localPoint       = pointRoot?.PointToScreen(args.Position) ?? default;
        var offset           = this.PointToScreen(new Point(0, 0));
        var constraintBounds = new Rect(new Point(offset.X, offset.Y), Bounds.Size);
        if (constraintBounds.Contains(new Point(localPoint.X, localPoint.Y)))
        {
            return true;
        }
        return false;
    }
}