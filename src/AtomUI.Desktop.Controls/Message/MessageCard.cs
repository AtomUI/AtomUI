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

/// <summary>
/// One message of a <see cref="WindowMessageManager" />: a status icon and a text that enter, stack and close
/// with a motion.
/// </summary>
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
    /// Identifies the <see cref="MessageType" /> property.
    /// </summary>
    public static readonly StyledProperty<MessageType> MessageTypeProperty =
        AvaloniaProperty.Register<MessageCard, MessageType>(nameof(MessageType));

    /// <summary>
    /// Defines the <see cref="MessageClosed" /> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> MessageClosedEvent =
        RoutedEvent.Register<MessageCard, RoutedEventArgs>(nameof(MessageClosed), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty = 
        AvaloniaProperty.Register<MessageCard, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="Message" /> property.
    /// </summary>
    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<MessageCard, string>(nameof(Message));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<MessageCard>();

    /// <summary>
    /// Defines the <see cref="BoxShadow" /> property. The card frame projects it onto the template's
    /// frame border, so the implicit <c>root</c> Semantic Part can express the upstream card shadow.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<MessageCard>();

    /// <summary>
    /// Tells that the card is closing. <see cref="Close" /> sets it and nothing resets it: it turns
    /// <see langword="true" /> when the close starts and stays so after the card is closed.
    /// </summary>
    public bool IsClosing
    {
        get => _isClosing;
        private set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
    }

    /// <summary>
    /// Tells that the card is closed and hidden. The card sets it when its close motion has finished, or at once
    /// when <see cref="IsMotionEnabled" /> is <see langword="false" />. When it becomes <see langword="true" />,
    /// <see cref="MessageClosed" /> is raised.
    /// </summary>
    public bool IsClosed
    {
        get => GetValue(IsClosedProperty);
        set => SetValue(IsClosedProperty, value);
    }

    /// <summary>
    /// The kind of message. It selects the status icon that the card shows while <see cref="Icon" /> is
    /// <see langword="null" />, and the colors of the card.
    /// </summary>
    public MessageType MessageType
    {
        get => GetValue(MessageTypeProperty);
        set => SetValue(MessageTypeProperty, value);
    }

    /// <summary>
    /// Raised after <see cref="IsClosed" /> became <see langword="true" />, that is, when the close has completed.
    /// <see cref="WindowMessageManager" /> handles it by removing the card and then running the close callback of
    /// the message. The event bubbles up.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? MessageClosed
    {
        add => AddHandler(MessageClosedEvent, value);
        remove => RemoveHandler(MessageClosedEvent, value);
    }

    /// <summary>
    /// The icon shown at the left of the message text. While the value is <see langword="null" />, the card shows
    /// the status icon that <see cref="MessageType" /> selects. That icon is written at
    /// <see cref="BindingPriority.Template" />, so a value set on the card wins, and setting the property to
    /// <see langword="null" /> returns to the built-in icon.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// The text shown in the body of the card.
    /// </summary>
    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Controls whether the card plays an entry motion when it appears and a close motion when it closes; both use
    /// <see cref="Position" /> and <see cref="OpenCloseMotionDuration" />. With <see langword="false" /> the card is
    /// shown and closed at once. The theme takes the value from the <c>EnableMotion</c> design Token, and
    /// <see cref="WindowMessageManager" /> relays its own value to every card it shows.
    /// </summary>
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

    /// <summary>
    /// Callback that runs once the card has closed. <see cref="WindowMessageManager" /> sets it to avoid a lambda
    /// closure.
    /// </summary>
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
    
    /// <summary>
    /// Starts closing the card. With <see cref="IsMotionEnabled" /> the close motion plays, otherwise the card is
    /// closed at once; either way <see cref="MessageClosed" /> is raised once the close has completed. The call
    /// returns before that, and further calls have no effect.
    /// </summary>
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

    void IFeedbackLifetimeItem.RequestClose()
    {
        Close();
    }

    void IFeedbackLifetimeItem.UpdateRemaining(TimeSpan remaining)
    {
    }

    internal void ReleaseOwner()
    {
        _motionCoordinator.Dispose();
        HoverChanged = null;
        OnClose = null;
    }

    /// <summary>
    /// Reports through <see cref="HoverChanged" /> that the pointer is over the card, which lets the owner pause the
    /// expiry of the card. Calls the base method first.
    /// </summary>
    protected override void OnPointerEntered(Avalonia.Input.PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        HoverChanged?.Invoke(this, true);
    }

    /// <summary>
    /// Reports through <see cref="HoverChanged" /> that the pointer left the card. Calls the base method first.
    /// </summary>
    protected override void OnPointerExited(Avalonia.Input.PointerEventArgs e)
    {
        base.OnPointerExited(e);
        HoverChanged?.Invoke(this, false);
    }

    /// <summary>
    /// Reacts to property changes after the base method: updates the pseudo-classes for <see cref="MessageType" />,
    /// restores the built-in icon for a <see langword="null" /> <see cref="Icon" />, starts or completes the close
    /// and raises <see cref="MessageClosed" /> for <see cref="IsClosing" /> and <see cref="IsClosed" />, and hands
    /// <see cref="IsMotionEnabled" />, <see cref="Position" /> and <see cref="OpenCloseMotionDuration" /> to the
    /// motion coordinator.
    /// </summary>
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

    /// <summary>
    /// Takes the template's <c>PART_Frame</c> as the target of the card motion, then applies the pseudo-classes and
    /// the built-in status icon. Calls the base method first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _motionTarget = e.NameScope.Find<Control>("PART_Frame");
        ApplyMotionTarget();
        UpdatePseudoClasses();
        SetupDefaultMessageIcon();
    }

    /// <summary>
    /// Hands the motion target back to the motion coordinator, because detaching released it. Calls the base
    /// method first.
    /// </summary>
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

    /// <summary>
    /// Releases the motion target before the base method is called. A card that is closing is closed at once here.
    /// </summary>
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
