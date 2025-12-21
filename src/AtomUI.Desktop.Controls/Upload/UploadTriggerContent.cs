using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal class UploadTriggerContent : ContentControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<UploadListType> ListTypeProperty =
        Upload.ListTypeProperty.AddOwner<UploadTriggerContent>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<UploadTriggerContent>();
    
    public UploadListType ListType
    {
        get => GetValue(ListTypeProperty);
        set => SetValue(ListTypeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> FileSelectRequestEvent =
        RoutedEvent.Register<AbstractUploadListItem, RoutedEventArgs>(nameof(FileSelectRequest), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? FileSelectRequest
    {
        add => AddHandler(FileSelectRequestEvent, value);
        remove => RemoveHandler(FileSelectRequestEvent, value);
    }

    #endregion
    
    private IDisposable? _clickSubscription;
    private Point? _latestClickPosition;
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureInputManager();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _clickSubscription?.Dispose();
    }

    private void ConfigureInputManager()
    {
        var inputManager = AvaloniaLocator.Current.GetService<IInputManager>();
        _clickSubscription = inputManager?.Process.Subscribe(ListenForMouseEvent);
    }
    
    private void ListenForMouseEvent(RawInputEventArgs e)
    {
        if (e is RawPointerEventArgs mouseEventArgs)
        {
            var point            = mouseEventArgs.Point;
            var localPoint       = TopLevel.GetTopLevel(this)?.TranslatePoint(point.Position, this) ?? default;
            var constraintOffset = this.TranslatePoint(new Point(0, 0), this) ?? default;
            var constraintBounds = new Rect(constraintOffset, Bounds.Size);
            if (constraintBounds.Contains(localPoint))
            {
                if (mouseEventArgs.Type == RawPointerEventType.LeftButtonDown)
                {
                    _latestClickPosition = localPoint;
                }
                else if (mouseEventArgs.Type == RawPointerEventType.LeftButtonUp)
                {
                    if (_latestClickPosition != null && constraintBounds.Contains(_latestClickPosition.Value))
                    {
                        RaiseEvent(new RoutedEventArgs(FileSelectRequestEvent, this));
                    }

                    _latestClickPosition = null;
                }
                else if (mouseEventArgs.Type == RawPointerEventType.Move)
                {
                    
                }
            }
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }

        if (change.Property == ListTypeProperty)
        {
            if (ListType == UploadListType.PictureCircle)
            {
                var radius= Math.Max(Width, Height);
                if (double.IsNaN(radius))
                {
                    radius = Math.Min(DesiredSize.Width, DesiredSize.Height);
                }
                ConfigureEffectiveCornerRadius(radius);
            }
        }
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BorderBrushProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ConfigureEffectiveCornerRadius(Math.Max(e.NewSize.Width, e.NewSize.Height));
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

    private void ConfigureEffectiveCornerRadius(double cornerRadius)
    {
        if (ListType == UploadListType.PictureCircle)
        {
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(cornerRadius));
        }
    }
}