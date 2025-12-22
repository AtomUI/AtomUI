using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUI.MotionScene;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Error, StdPseudoClass.Information, StdPseudoClass.Success, StdPseudoClass.Warning)]
public class NotificationCard : ContentControl,
                                IMotionAwareControl,
                                IControlSharedTokenResourcesHost
{
    internal const double AnimationMaxOffsetY = 150d;
    internal const double AnimationMaxOffsetX = 500d;

    #region 公共属性定义
    
    public static readonly DirectProperty<NotificationCard, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<NotificationCard, bool>(nameof(IsClosing), o => o.IsClosing);
    
    public static readonly StyledProperty<bool> IsClosedProperty =
        AvaloniaProperty.Register<NotificationCard, bool>(nameof(IsClosed));

    public static readonly StyledProperty<bool> IsShowProgressProperty =
        AvaloniaProperty.Register<NotificationCard, bool>(nameof(IsShowProgress));
    
    public static readonly StyledProperty<NotificationType> NotificationTypeProperty =
        AvaloniaProperty.Register<NotificationCard, NotificationType>(nameof(NotificationType));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NotificationCard>();
    
    public static readonly RoutedEvent<RoutedEventArgs> NotificationClosedEvent =
        RoutedEvent.Register<NotificationCard, RoutedEventArgs>(nameof(NotificationClosed), RoutingStrategies.Bubble);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<NotificationCard, string>(nameof(Title));

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<NotificationCard, PathIcon?>(nameof(Icon));
    
    public static readonly StyledProperty<TimeSpan?> ExpirationProperty =
        AvaloniaProperty.Register<NotificationCard, TimeSpan?>(nameof(Expiration));
    
    public bool IsClosing
    {
        get => _isClosing;
        private set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
    }
    
    public bool IsClosed
    {
        get => GetValue(IsClosedProperty);
        set => SetValue(IsClosedProperty, value);
    }

    public bool IsShowProgress
    {
        get => GetValue(IsShowProgressProperty);
        set => SetValue(IsShowProgressProperty, value);
    }

    public NotificationType NotificationType
    {
        get => GetValue(NotificationTypeProperty);
        set => SetValue(NotificationTypeProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    /// <summary>
    /// Gets the expiration time of the notification after which it will automatically close.
    /// If the value is null then the notification will remain open until the user closes it.
    /// </summary>
    public TimeSpan? Expiration
    {
        get => GetValue(ExpirationProperty);
        set => SetValue(ExpirationProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? NotificationClosed
    {
        add => AddHandler(NotificationClosedEvent, value);
        remove => RemoveHandler(NotificationClosedEvent, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<NotificationCard, NotificationPosition> PositionProperty =
        AvaloniaProperty.RegisterDirect<NotificationCard, NotificationPosition>(
            nameof(Position),
            o => o.Position,
            (o, v) => o.Position = v);

    internal static readonly DirectProperty<NotificationCard, TimeSpan> OpenCloseMotionDurationProperty =
        AvaloniaProperty.RegisterDirect<NotificationCard, TimeSpan>(nameof(OpenCloseMotionDuration),
            o => o.OpenCloseMotionDuration,
            (o, v) => o.OpenCloseMotionDuration = v);

    private NotificationPosition _position;

    internal NotificationPosition Position
    {
        get => _position;
        set => SetAndRaise(PositionProperty, ref _position, value);
    }

    private TimeSpan _openCloseMotionDuration;

    internal TimeSpan OpenCloseMotionDuration
    {
        get => _openCloseMotionDuration;
        set => SetAndRaise(OpenCloseMotionDurationProperty, ref _openCloseMotionDuration, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => NotificationToken.ID;

    #endregion

    private bool _isClosing;
    private readonly WindowNotificationManager _notificationManager;
    private IconButton? _closeButton;
    private BaseMotionActor? _motionActor;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationCard" /> class.
    /// </summary>
    public NotificationCard(WindowNotificationManager manager)
    {
        this.RegisterResources();
        _notificationManager = manager;
    }
    
    public void Close()
    {
        if (IsClosing)
        {
            return;
        }

        IsClosing = true;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupPositionPseudoClasses(Position);
        SetupNotificationTypePseudoClasses();
        SetupDefaultNotificationIcon();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<IconButton>(NotificationCardThemeConstants.CloseButtonPart);
        _motionActor = e.NameScope.Find<BaseMotionActor>(BaseMotionActor.MotionActorPart);

        if (_closeButton is not null)
        {
            _closeButton.Click += HandleCloseButtonClose;
        }

        ApplyShowMotion();
    }

    private void ApplyShowMotion()
    {
        if (_motionActor is null)
        {
            return;
        }
        
        if (IsMotionEnabled)
        {
            AbstractMotion? motion;
            if (Position == NotificationPosition.TopLeft || Position == NotificationPosition.BottomLeft)
            {
                motion = new NotificationMoveLeftInMotion(AnimationMaxOffsetX,
                    _openCloseMotionDuration, new CubicEaseOut());
            }
            else if (Position == NotificationPosition.TopRight || Position == NotificationPosition.BottomRight)
            {
                motion = new NotificationMoveRightInMotion(AnimationMaxOffsetX, _openCloseMotionDuration, new CubicEaseOut());
            }
            else if (Position == NotificationPosition.TopCenter)
            {
                motion = new NotificationMoveUpInMotion(AnimationMaxOffsetY, _openCloseMotionDuration,
                    new CubicEaseOut());
            }
            else
            {
                motion = new NotificationMoveDownInMotion(AnimationMaxOffsetY, _openCloseMotionDuration,
                    new CubicEaseOut());
            }
            motion.Run(_motionActor);
        }
    }

    private void ApplyHideMotion()
    {
        if (_motionActor is null)
        {
            return;
        }
        
        if (IsMotionEnabled)
        {
            AbstractMotion? motion;
            if (Position == NotificationPosition.TopLeft || Position == NotificationPosition.BottomLeft)
            {
                motion = new NotificationMoveLeftOutMotion(AnimationMaxOffsetX, _openCloseMotionDuration,
                    new CubicEaseIn());
            }
            else if (Position == NotificationPosition.TopRight || Position == NotificationPosition.BottomRight)
            {
                motion = new NotificationMoveRightOutMotion(AnimationMaxOffsetX, _openCloseMotionDuration,
                    new CubicEaseIn());
            }
            else if (Position == NotificationPosition.TopCenter)
            {
                motion = new NotificationMoveUpOutMotion(AnimationMaxOffsetY, _openCloseMotionDuration,
                    new CubicEaseIn());
            }
            else
            {
                motion = new NotificationMoveDownOutMotion(AnimationMaxOffsetY, _openCloseMotionDuration,
                    new CubicEaseIn());
            }
        
            motion.Run(_motionActor, null, () => { IsClosed = true; });
        }
        else
        {
            IsClosed = true;
        }
    }

    private void HandleCloseButtonClose(object? sender, EventArgs args)
    {
        Close();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == NotificationTypeProperty)
            {
                SetupNotificationTypePseudoClasses();
            }
        }
        
        if (change.Property == IsClosedProperty)
        {
            if (!IsClosing && !IsClosed)
            {
                return;
            }

            RaiseEvent(new RoutedEventArgs(NotificationClosedEvent));
        }
        else if (change.Property == PositionProperty)
        {
            SetupPositionPseudoClasses(change.GetNewValue<NotificationPosition>());
        } 
        else if (change.Property == IsClosingProperty)
        {
            if (IsClosing)
            {
                ApplyHideMotion();
            }
        } 
        else if (change.Property == IconProperty)
        {
            if (Icon is null)
            {
                SetupDefaultNotificationIcon();
            }
        }
    }

    private void SetupNotificationTypePseudoClasses()
    {
        switch (NotificationType)
        {
            case NotificationType.Error:
                PseudoClasses.Add(StdPseudoClass.Error);
                break;

            case NotificationType.Information:
                PseudoClasses.Add(StdPseudoClass.Information);
                break;

            case NotificationType.Success:
                PseudoClasses.Add(StdPseudoClass.Success);
                break;

            case NotificationType.Warning:
                PseudoClasses.Add(StdPseudoClass.Warning);
                break;
        }
    }

    private void SetupDefaultNotificationIcon()
    {
        if (Icon is null)
        {
            Icon? icon = null;
            if (NotificationType == NotificationType.Information)
            {
                icon = new InfoCircleFilled();
            }
            else if (NotificationType == NotificationType.Success)
            {
                icon = new CheckCircleFilled();
            }
            else if (NotificationType == NotificationType.Error)
            {
                icon = new CloseCircleFilled();
            }
            else if (NotificationType == NotificationType.Warning)
            {
                icon = new ExclamationCircleFilled();
            }
        
            ClearValue(IconProperty);
            SetValue(IconProperty, icon, BindingPriority.Template);
        }
    }

    internal bool NotifyCloseTick(TimeSpan cycleDuration)
    {
        InvalidateVisual();
        if (Expiration is null)
        {
            return false;
        }

        Expiration -= cycleDuration;

        if (Expiration.Value.TotalMilliseconds < 0)
        {
            return true;
        }

        return false;
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (_notificationManager.IsPauseOnHover)
        {
            _notificationManager.StopExpiredTimer();
        }
    }
    
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_notificationManager.IsPauseOnHover)
        {
            _notificationManager.StopExpiredTimer();
        }
    }
    
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (_notificationManager.IsPauseOnHover)
        {
            _notificationManager.StartExpiredTimer();
        }
    }

    private void SetupPositionPseudoClasses(NotificationPosition position)
    {
        PseudoClasses.Set(NotificationPseudoClass.TopLeft, position == NotificationPosition.TopLeft);
        PseudoClasses.Set(NotificationPseudoClass.TopRight, position == NotificationPosition.TopRight);
        PseudoClasses.Set(NotificationPseudoClass.BottomLeft, position == NotificationPosition.BottomLeft);
        PseudoClasses.Set(NotificationPseudoClass.BottomRight, position == NotificationPosition.BottomRight);
        PseudoClasses.Set(NotificationPseudoClass.TopCenter, position == NotificationPosition.TopCenter);
        PseudoClasses.Set(NotificationPseudoClass.BottomCenter, position == NotificationPosition.BottomCenter);
    }
}