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
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(MessageCardPseudoClass.Error, 
    MessageCardPseudoClass.Information,
    MessageCardPseudoClass.Success, 
    MessageCardPseudoClass.Warning, 
    MessageCardPseudoClass.Loading)]
public class MessageCard : TemplatedControl,
                           IMotionAwareControl,
                           IControlSharedTokenResourcesHost
{
    internal const double AnimationMaxOffsetY = 100d;

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
        AvaloniaProperty.Register<MessageCard, MessageType>(nameof(NotificationType));

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

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<MessageCard, TimeSpan> OpenCloseMotionDurationProperty =
        AvaloniaProperty.RegisterDirect<MessageCard, TimeSpan>(nameof(OpenCloseMotionDuration),
            o => o.OpenCloseMotionDuration,
            (o, v) => o.OpenCloseMotionDuration = v);

    private TimeSpan _openCloseMotionDuration;

    internal TimeSpan OpenCloseMotionDuration
    {
        get => _openCloseMotionDuration;
        set => SetAndRaise(OpenCloseMotionDurationProperty, ref _openCloseMotionDuration, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => MessageToken.ID;

    #endregion

    private bool _isClosing;
    private BaseMotionActor? _motionActor;
    
    public MessageCard()
    {
        this.RegisterResources();
    }
    
    public void Close()
    {
        if (IsClosing)
        {
            return;
        }

        IsClosing = true;
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
                ApplyHideMotion();
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _motionActor = e.NameScope.Find<BaseMotionActor>(MessageCardThemeConstants.MotionActorPart);
        ApplyShowMotion();
        UpdatePseudoClasses();
        SetupDefaultMessageIcon();
    }

    private void ApplyShowMotion()
    {
        if (_motionActor is not null)
        {
            if (IsMotionEnabled)
            {
                _motionActor.IsVisible = false;
                var motion = new MoveUpInMotion(AnimationMaxOffsetY, _openCloseMotionDuration, new CubicEaseOut());
                motion.Run(_motionActor, () => { _motionActor.IsVisible = true; });
            }
            else
            {
                _motionActor.IsVisible = true;
            }
        }
    }

    private void ApplyHideMotion()
    {
        if (_motionActor is not null)
        {
            if (IsMotionEnabled)
            {
                var motion =
                    new MoveUpOutMotion(AnimationMaxOffsetY, _openCloseMotionDuration, new CubicEaseIn());
                motion.Run(_motionActor, null, () => { IsClosed = true; });
            }
            else
            {
                IsClosed = true;
            }
        }
    }

    private void UpdatePseudoClasses()
    {
        switch (MessageType)
        {
            case MessageType.Error:
                PseudoClasses.Add(MessageCardPseudoClass.Error);
                break;

            case MessageType.Information:
                PseudoClasses.Add(MessageCardPseudoClass.Information);
                break;

            case MessageType.Success:
                PseudoClasses.Add(MessageCardPseudoClass.Success);
                break;

            case MessageType.Warning:
                PseudoClasses.Add(MessageCardPseudoClass.Warning);
                break;

            case MessageType.Loading:
                PseudoClasses.Add(MessageCardPseudoClass.Loading);
                break;
        }
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