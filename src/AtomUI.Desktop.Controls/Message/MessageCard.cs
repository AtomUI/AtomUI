using AtomUI.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(MessageCardPseudoClass.Error, 
    MessageCardPseudoClass.Information,
    MessageCardPseudoClass.Success, 
    MessageCardPseudoClass.Warning, 
    MessageCardPseudoClass.Loading)]
public partial class MessageCard : TemplatedControl, IMotionAwareControl, IFeedbackStackItem
{
    #region 公共属性定义

    /// <summary>
    /// Defines the <see cref="IsClosing" /> property.
    /// </summary>
    public static readonly DirectProperty<MessageCard, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<MessageCard, bool>(nameof(IsClosing), o => o.IsClosing);

    /// <summary>
    /// Defines the <see cref="IsClosed" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosedProperty =
        AvaloniaProperty.Register<MessageCard, bool>(nameof(IsClosed));

    /// <summary>
    /// Defines the <see cref="NotificationType" /> property
    /// </summary>
    public static readonly StyledProperty<MessageType> MessageTypeProperty =
        AvaloniaProperty.Register<MessageCard, MessageType>(nameof(MessageType));

    /// <summary>
    /// Defines the <see cref="MessageClosed" /> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> MessageClosedEvent =
        RoutedEvent.Register<MessageCard, RoutedEventArgs>(nameof(MessageClosed), RoutingStrategies.Bubble);

    public static readonly StyledProperty<PathIcon?> IconProperty = 
        AvaloniaProperty.Register<MessageCard, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<MessageCard, string>(nameof(Message));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<MessageCard>();

    /// <summary>
    /// Defines the <see cref="BoxShadow" /> property. The card frame projects it onto the template's
    /// frame border, so the implicit <c>root</c> Semantic Part can express the upstream card shadow.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<MessageCard>();

    /// <summary>
    /// Determines if the notification is already closing.
    /// </summary>
    public bool IsClosing
    {
        get => _isClosing;
        private set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
    }

    /// <summary>
    /// Determines if the notification is closed.
    /// </summary>
    public bool IsClosed
    {
        get => GetValue(IsClosedProperty);
        set => SetValue(IsClosedProperty, value);
    }

    /// <summary>
    /// Gets or sets the type of the notification
    /// </summary>
    public MessageType MessageType
    {
        get => GetValue(MessageTypeProperty);
        set => SetValue(MessageTypeProperty, value);
    }

    /// <summary>
    /// Raised when the <see cref="MessageCard" /> has closed.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? MessageClosed
    {
        add => AddHandler(MessageClosedEvent, value);
        remove => RemoveHandler(MessageClosedEvent, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the shadow painted on the card frame.
    /// </summary>
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    #endregion

    #region 内部属性定义

    /// <summary>回调：消息卡片关闭时触发，由 WindowMessageManager 设置以避免 lambda 闭包。</summary>
    internal Action? OnClose { get; set; }

    internal Action<IFeedbackStackItem, bool>? HoverChanged { get; set; }

    internal static readonly DirectProperty<MessageCard, NotificationPosition> PositionProperty =
        AvaloniaProperty.RegisterDirect<MessageCard, NotificationPosition>(
            nameof(Position),
            card => card.Position,
            (card, value) => card.Position = value);

    internal static readonly DirectProperty<MessageCard, TimeSpan> OpenCloseMotionDurationProperty =
        AvaloniaProperty.RegisterDirect<MessageCard, TimeSpan>(nameof(OpenCloseMotionDuration),
            o => o.OpenCloseMotionDuration,
            (o, v) => o.OpenCloseMotionDuration = v);

    private NotificationPosition _position = NotificationPosition.TopCenter;

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
    
    #endregion

    private bool _isClosing;
    private bool _isStackVisible = true;
    private readonly FeedbackCardMotionCoordinator _motionCoordinator;
    private Control? _motionTarget;
    
    public MessageCard()
    {
        _motionCoordinator = new FeedbackCardMotionCoordinator(CompleteCloseMotion);
    }
    
    public void Close()
    {
        if (IsClosing)
        {
            return;
        }

        IsClosing = true;
    }

    bool IFeedbackStackItem.IsStackVisible
    {
        get => _isStackVisible;
        set => _isStackVisible = value;
    }

    bool IFeedbackStackItem.IsProgressVisible => false;

    void IFeedbackStackItem.RequestClose()
    {
        Close();
    }

    void IFeedbackStackItem.UpdateRemaining(TimeSpan remaining)
    {
    }

    internal void ReleaseOwner()
    {
        _motionCoordinator.Dispose();
        HoverChanged = null;
        OnClose = null;
    }

    protected override void OnPointerEntered(Avalonia.Input.PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        HoverChanged?.Invoke(this, true);
    }

    protected override void OnPointerExited(Avalonia.Input.PointerEventArgs e)
    {
        base.OnPointerExited(e);
        HoverChanged?.Invoke(this, false);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == MessageTypeProperty)
            {
                SetupDefaultMessageIcon();
                UpdatePseudoClasses();
            }
        }

        if (change.Property == IconProperty)
        {
            if (Icon is null)
            {
                SetupDefaultMessageIcon();
            }
        }

        if (change.Property == IsClosedProperty)
        {
            _motionCoordinator.UpdateConfiguration(
                IsClosing,
                IsClosed,
                IsMotionEnabled,
                Position,
                OpenCloseMotionDuration);
            if (!IsClosing && !IsClosed)
            {
                return;
            }

            RaiseEvent(new RoutedEventArgs(MessageClosedEvent));
        } 
        else if (change.Property == IsClosingProperty)
        {
            if (IsClosing)
            {
                InvalidateFeedbackStackMeasure();
                _motionCoordinator.StartClose(IsClosed, IsMotionEnabled, Position, OpenCloseMotionDuration);
            }
        }
        else if (change.Property == IsMotionEnabledProperty ||
                 change.Property == PositionProperty ||
                 change.Property == OpenCloseMotionDurationProperty)
        {
            _motionCoordinator.UpdateConfiguration(
                IsClosing,
                IsClosed,
                IsMotionEnabled,
                Position,
                OpenCloseMotionDuration);
        }
    }

    private void InvalidateFeedbackStackMeasure()
    {
        InvalidateMeasure();
        if (this.GetVisualParent() is Control parent)
        {
            parent.InvalidateMeasure();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _motionTarget = e.NameScope.Find<Control>("PART_Frame");
        ApplyMotionTarget();
        UpdatePseudoClasses();
        SetupDefaultMessageIcon();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ApplyMotionTarget();
    }

    private void ApplyMotionTarget()
    {
        _motionCoordinator.ApplyTarget(
            _motionTarget,
            IsClosing,
            IsClosed,
            IsMotionEnabled,
            Position,
            OpenCloseMotionDuration);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _motionCoordinator.DetachTarget(IsClosing, IsClosed);
        base.OnDetachedFromVisualTree(e);
    }

    private void CompleteCloseMotion()
    {
        IsClosed = true;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(MessageCardPseudoClass.Error, MessageType == MessageType.Error);
        PseudoClasses.Set(MessageCardPseudoClass.Information, MessageType == MessageType.Information);
        PseudoClasses.Set(MessageCardPseudoClass.Success, MessageType == MessageType.Success);
        PseudoClasses.Set(MessageCardPseudoClass.Warning, MessageType == MessageType.Warning);
        PseudoClasses.Set(MessageCardPseudoClass.Loading, MessageType == MessageType.Loading);
    }

    private void SetupDefaultMessageIcon()
    {
        Icon? icon = null;
        if (MessageType == MessageType.Information)
        {
            icon = new InfoCircleFilled();
        }
        else if (MessageType == MessageType.Success)
        {
            icon = new CheckCircleFilled();
        }
        else if (MessageType == MessageType.Error)
        {
            icon = new CloseCircleFilled();
        }
        else if (MessageType == MessageType.Warning)
        {
            icon = new ExclamationCircleFilled();
        }
        else if (MessageType == MessageType.Loading)
        {
            icon                  = new LoadingOutlined();
            icon.LoadingAnimation = IconAnimation.Spin;
        }

        if (Icon is null)
        {
            ClearValue(IconProperty);
            SetValue(IconProperty, icon, BindingPriority.Template);
        }
    }
    
}
