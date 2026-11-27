using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Icons.AntDesign;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The card that <see cref="WindowNotificationManager" /> shows for one notification: a status icon, a title, the
/// description and an optional actions area, with a close button at the top right corner and an optional progress
/// bar at the bottom. A card created without a manager is standalone and gets no hover pause.
/// </summary>
[PseudoClasses(StdPseudoClass.Error, StdPseudoClass.Information, StdPseudoClass.Success, StdPseudoClass.Warning)]
public partial class NotificationCard : ContentControl,
                                IMotionAwareControl,
                                IFeedbackStackItem,
                                IFeedbackStackTransitionSnapshotItem
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="IsClosing" /> property.
    /// </summary>
    public static readonly DirectProperty<NotificationCard, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<NotificationCard, bool>(nameof(IsClosing), o => o.IsClosing);
    
    /// <summary>
    /// Identifies the <see cref="IsClosed" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosedProperty =
        AvaloniaProperty.Register<NotificationCard, bool>(nameof(IsClosed));

    /// <summary>
    /// Identifies the <see cref="IsShowProgress" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowProgressProperty =
        AvaloniaProperty.Register<NotificationCard, bool>(nameof(IsShowProgress));
    
    /// <summary>
    /// Identifies the <see cref="NotificationType" /> property.
    /// </summary>
    public static readonly StyledProperty<NotificationType> NotificationTypeProperty =
        AvaloniaProperty.Register<NotificationCard, NotificationType>(
            nameof(NotificationType),
            NotificationType.Default);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NotificationCard>();
    
    /// <summary>
    /// Identifies the <see cref="NotificationClosed" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> NotificationClosedEvent =
        RoutedEvent.Register<NotificationCard, RoutedEventArgs>(nameof(NotificationClosed), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<NotificationCard, string>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<NotificationCard, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="Actions" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<NotificationCard, object?>(nameof(Actions));

    /// <summary>
    /// Identifies the <see cref="ActionsTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ActionsTemplateProperty =
        AvaloniaProperty.Register<NotificationCard, IDataTemplate?>(nameof(ActionsTemplate));

    /// <summary>
    /// Identifies the <see cref="BoxShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<NotificationCard>();
    
    /// <summary>
    /// Identifies the <see cref="Expiration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan?> ExpirationProperty =
        AvaloniaProperty.Register<NotificationCard, TimeSpan?>(nameof(Expiration));
    
    /// <summary>
    /// Reports that the card has started closing. It becomes <see langword="true" /> when <see cref="Close" /> is
    /// called and stays so; the close motion starts at that moment, and <see cref="IsClosed" /> is set when it has
    /// finished.
    /// </summary>
    public bool IsClosing
    {
        get => _isClosing;
        private set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
    }
    
    /// <summary>
    /// Reports that the card has closed. The card sets it when the close motion has finished, then raises
    /// <see cref="NotificationClosed" />. The theme uses it to drop the card's margin.
    /// </summary>
    public bool IsClosed
    {
        get => GetValue(IsClosedProperty);
        set => SetValue(IsClosedProperty, value);
    }

    /// <summary>
    /// Shows a progress bar along the bottom edge of the card that runs down while the card is shown. The bar exists
    /// only while <see cref="Expiration" /> is not <see langword="null" />: its total is the <see cref="Expiration" />
    /// value present when the bar is created, and later values are taken as the remaining time.
    /// </summary>
    public bool IsShowProgress
    {
        get => GetValue(IsShowProgressProperty);
        set => SetValue(IsShowProgressProperty, value);
    }

    /// <summary>
    /// Severity of the notification; <c>Default</c> when not set. It selects the shadow and status icon color of the
    /// theme, and the card supplies a matching filled status icon unless the application sets <see cref="Icon" />.
    /// </summary>
    public NotificationType NotificationType
    {
        get => GetValue(NotificationTypeProperty);
        set => SetValue(NotificationTypeProperty, value);
    }

    /// <summary>
    /// The title shown above the description in the content area of the card.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Icon shown in front of the title and description. While the application sets none, the card supplies the
    /// filled status icon of <see cref="NotificationType" /> (none for <c>Default</c>) and replaces it when the type
    /// changes. Setting an icon here shows that one; setting <see langword="null" /> restores the status icon.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Content of the actions area at the bottom of the card, aligned to the right. The area is visible only while
    /// the value is not <see langword="null" />.
    /// </summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <summary>
    /// The data template used to present <see cref="Actions" />.
    /// </summary>
    public IDataTemplate? ActionsTemplate
    {
        get => GetValue(ActionsTemplateProperty);
        set => SetValue(ActionsTemplateProperty, value);
    }

    /// <summary>
    /// Shadow of the card surface, painted by the frame of the template. The frame reads the value from the card,
    /// so a value set on the card, including one from an owner-scoped semantic style, reaches the frame. The theme
    /// supplies the shadow of <see cref="NotificationType" />.
    /// </summary>
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
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

    /// <summary>
    /// Raised after <see cref="IsClosed" /> changes while the card is closing or closed, which is normally the end
    /// of the close motion. The event bubbles and cannot be canceled; <see cref="WindowNotificationManager" />
    /// handles it and invokes <see cref="OnClose" />.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? NotificationClosed
    {
        add => AddHandler(NotificationClosedEvent, value);
        remove => RemoveHandler(NotificationClosedEvent, value);
    }

    /// <summary>
    /// Controls the motion of the card: the transitions of the theme, the close motion and the stack collapse
    /// snapshot. The theme sets it from the <c>EnableMotion</c> token when no value is set on the card.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 内部属性定义

    /// <summary>
    /// Callback invoked when the user clicks the card. <see cref="WindowNotificationManager" /> assigns it from the
    /// notification, so the manager needs no lambda closure per card.
    /// </summary>
    internal Action? OnClick { get; set; }

    /// <summary>
    /// Callback invoked when the card closes. <see cref="WindowNotificationManager" /> assigns it from the
    /// notification, so the manager needs no lambda closure per card.
    /// </summary>
    internal Action? OnClose { get; set; }

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
    
    #endregion

    private bool _isClosing;
    private bool _isStackVisible = true;
    private readonly FeedbackCardMotionCoordinator _motionCoordinator;
    private WindowNotificationManager? _notificationManager;
    private Grid? _layout;
    private IconButton? _closeButton;
    private NotificationProgressBar? _progressBar;
    private PathIcon? _templateNotificationIcon;
    private bool _isApplyingTemplateNotificationIcon;
    private TimeSpan _progressBarTotalExpiration;
    private Control? _motionTarget;
    private FeedbackStackTransitionSnapshotHost? _stackTransitionSnapshotHost;
    private Matrix _stackCollapseSnapshotTarget;
    private bool _isStackCollapseSnapshotArmed;

    /// <summary>
    /// Initializes a card that is hosted by a manager, so that it pauses while the pointer is over it.
    /// </summary>
    /// <param name="manager">
    /// The manager that shows the card and pauses it while the pointer is over it.
    /// </param>
    public NotificationCard(WindowNotificationManager manager)
    {
        _notificationManager = manager;
        _motionCoordinator = new FeedbackCardMotionCoordinator(CompleteCloseMotion);
    }

    /// <summary>
    /// Initializes a standalone notification card that is not hosted by a
    /// <see cref="WindowNotificationManager" />. Hover-pause feedback is a manager
    /// capability and does not apply to standalone cards.
    /// </summary>
    public NotificationCard()
    {
        _motionCoordinator = new FeedbackCardMotionCoordinator(CompleteCloseMotion);
    }
    
    /// <summary>
    /// Starts closing the card by setting <see cref="IsClosing" />, which starts the close motion. When that motion
    /// has finished, the card sets <see cref="IsClosed" /> and raises <see cref="NotificationClosed" />. A call
    /// while the card is already closing returns at once.
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

    bool IFeedbackStackItem.IsProgressVisible => IsShowProgress;

    void IFeedbackLifetimeItem.RequestClose()
    {
        Close();
    }

    void IFeedbackLifetimeItem.UpdateRemaining(TimeSpan remaining)
    {
        Expiration = remaining;
    }

    bool IFeedbackStackTransitionSnapshotItem.TryBeginStackCollapseSnapshot()
    {
        ReleaseStackTransitionSnapshot();
        return _stackTransitionSnapshotHost?.TryBeginSnapshot() == true;
    }

    void IFeedbackStackTransitionSnapshotItem.ArmStackCollapseSnapshot(ITransform targetTransform)
    {
        if (_stackTransitionSnapshotHost?.IsSnapshotActive != true)
        {
            return;
        }

        if (!HasStackTransformTransition())
        {
            ReleaseStackTransitionSnapshot();
            return;
        }

        _stackCollapseSnapshotTarget = targetTransform.Value;
        _isStackCollapseSnapshotArmed = true;
    }

    void IFeedbackStackTransitionSnapshotItem.ReleaseStackTransitionSnapshot()
    {
        ReleaseStackTransitionSnapshot();
    }

    internal void ReleaseOwner()
    {
        ReleaseStackTransitionSnapshot();
        _motionCoordinator.Dispose();
        _notificationManager = null;
        OnClick = null;
        OnClose = null;
    }

    /// <summary>
    /// When the card enters the visual tree, updates the pseudo-classes of <see cref="Position" /> and
    /// <see cref="NotificationType" />, supplies the default status icon, sets the frame of the template as the target
    /// of the close motion and subscribes to the close button. An override must call this member.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupPositionPseudoClasses(Position);
        SetupNotificationTypePseudoClasses();
        SetupDefaultNotificationIcon();
        ApplyMotionTarget();
        if (_closeButton != null)
        {
            _closeButton.Click += HandleCloseButtonClose;
        }
    }

    /// <summary>
    /// When the card leaves the visual tree, releases the resources that depend on the template: the stack transition
    /// snapshot, the motion target of the close motion, the close button subscription and the progress bar.
    /// An override must call this member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseStackTransitionSnapshot();
        _motionCoordinator.DetachTarget(IsClosing, IsClosed);
        base.OnDetachedFromVisualTree(e);
        if (_closeButton != null)
        {
            _closeButton.Click -= HandleCloseButtonClose;
        }
        ClearProgressBar();
        _layout = null;
    }

    /// <summary>
    /// Binds the template parts (layout grid, close button, frame as motion target and snapshot host), subscribes to
    /// the close button and rebuilds the progress bar for the current <see cref="IsShowProgress" /> and
    /// <see cref="Expiration" />. An override must call this member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ReleaseStackTransitionSnapshot();
        base.OnApplyTemplate(e);

        if (_closeButton is not null)
        {
            _closeButton.Click -= HandleCloseButtonClose;
        }
        ClearProgressBar();
        _layout      = e.NameScope.Find<Grid>("PART_Layout");
        _closeButton = e.NameScope.Find<IconButton>("PART_CloseButton");
        _motionTarget = e.NameScope.Find<Control>("Frame");
        _stackTransitionSnapshotHost = e.NameScope.Find<FeedbackStackTransitionSnapshotHost>(
            FeedbackStackTransitionSnapshotHost.SnapshotHostPart);

        if (_closeButton is not null)
        {
            _closeButton.Click += HandleCloseButtonClose;
        }

        ApplyMotionTarget();
        ConfigureProgressBar();
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

    private void CompleteCloseMotion()
    {
        IsClosed = true;
    }

    private void HandleCloseButtonClose(object? sender, EventArgs args)
    {
        Close();
    }

    /// <summary>
    /// Keeps the card in step with its properties after each change. It updates the pseudo-classes after
    /// <see cref="Position" /> and <see cref="NotificationType" /> changes, the default status icon after
    /// <see cref="NotificationType" /> and <see cref="Icon" /> changes, and the progress bar after
    /// <see cref="IsShowProgress" /> and <see cref="Expiration" /> changes. It starts the close motion when
    /// <see cref="IsClosing" /> becomes <see langword="true" />, raises <see cref="NotificationClosed" /> when
    /// <see cref="IsClosed" /> changes while the card is closing or closed, and updates the motion configuration
    /// and the stack transition snapshot after changes of <see cref="IsClosed" />, <see cref="Position" />,
    /// <see cref="IsMotionEnabled" /> and <see cref="OpenCloseMotionDuration" />. An override must call this member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == NotificationTypeProperty)
            {
                SetupNotificationTypePseudoClasses();
                SetupDefaultNotificationIcon();
            }
        }
        
        if (change.Property == RenderTransformProperty)
        {
            TryCompleteStackTransitionSnapshot(change.GetNewValue<ITransform?>());
        }
        else if (change.Property == IsClosedProperty)
        {
            ReleaseStackTransitionSnapshot();
            UpdateMotionConfiguration();
            if (!IsClosing && !IsClosed)
            {
                return;
            }

            RaiseEvent(new RoutedEventArgs(NotificationClosedEvent));
        }
        else if (change.Property == PositionProperty)
        {
            ReleaseStackTransitionSnapshot();
            SetupPositionPseudoClasses(change.GetNewValue<NotificationPosition>());
            UpdateMotionConfiguration();
        } 
        else if (change.Property == IsClosingProperty)
        {
            if (IsClosing)
            {
                ReleaseStackTransitionSnapshot();
                InvalidateFeedbackStackMeasure();
                _motionCoordinator.StartClose(IsClosed, IsMotionEnabled, Position, OpenCloseMotionDuration);
            }
        } 
        else if (change.Property == IconProperty)
        {
            if (!_isApplyingTemplateNotificationIcon &&
                (Icon is null || ReferenceEquals(Icon, _templateNotificationIcon)))
            {
                SetupDefaultNotificationIcon();
            }
        }
        else if (change.Property == IsShowProgressProperty ||
                 change.Property == ExpirationProperty)
        {
            ConfigureProgressBar();
        }
        else if (change.Property == IsMotionEnabledProperty ||
                 change.Property == OpenCloseMotionDurationProperty)
        {
            if (!IsMotionEnabled)
            {
                ReleaseStackTransitionSnapshot();
            }
            UpdateMotionConfiguration();
        }
    }

    private bool HasStackTransformTransition()
    {
        if (!IsMotionEnabled || Transitions is null)
        {
            return false;
        }

        for (var i = 0; i < Transitions.Count; i++)
        {
            var transition = Transitions[i];
            if (transition.Property == RenderTransformProperty &&
                transition is TransformOperationsTransition { Duration: var duration } &&
                duration > TimeSpan.Zero)
            {
                return true;
            }
        }

        return false;
    }

    private void TryCompleteStackTransitionSnapshot(ITransform? currentTransform)
    {
        if (!_isStackCollapseSnapshotArmed ||
            currentTransform is null ||
            !MatricesAreClose(currentTransform.Value, _stackCollapseSnapshotTarget))
        {
            return;
        }

        ReleaseStackTransitionSnapshot();
    }

    private void ReleaseStackTransitionSnapshot()
    {
        _isStackCollapseSnapshotArmed = false;
        _stackCollapseSnapshotTarget = default;
        _stackTransitionSnapshotHost?.ReleaseSnapshot();
    }

    private static bool MatricesAreClose(Matrix first, Matrix second)
    {
        const double tolerance = 0.0001;
        return Math.Abs(first.M11 - second.M11) < tolerance &&
               Math.Abs(first.M12 - second.M12) < tolerance &&
               Math.Abs(first.M21 - second.M21) < tolerance &&
               Math.Abs(first.M22 - second.M22) < tolerance &&
               Math.Abs(first.M31 - second.M31) < tolerance &&
               Math.Abs(first.M32 - second.M32) < tolerance;
    }

    private void UpdateMotionConfiguration()
    {
        _motionCoordinator.UpdateConfiguration(
            IsClosing,
            IsClosed,
            IsMotionEnabled,
            Position,
            OpenCloseMotionDuration);
    }

    private void InvalidateFeedbackStackMeasure()
    {
        InvalidateMeasure();
        if (this.GetVisualParent() is Control parent)
        {
            parent.InvalidateMeasure();
        }
    }

    private void ConfigureProgressBar()
    {
        if (_layout is null ||
            !IsShowProgress ||
            Expiration is null)
        {
            ClearProgressBar();
            return;
        }

        if (_progressBar is null)
        {
            _progressBarTotalExpiration = Expiration.Value;
            _progressBar = new NotificationProgressBar
            {
                Name = "ProgressBar"
            };
            // progress is a runtime part over the card's bottom edge. Its semantic class is injected explicitly; it is
            // created and released together with the bar.
            _progressBar.Classes.Add(NotificationCardSemanticParts.ProgressClass);
            _progressBar.SetTemplatedParent(this);
            // Column 0 is Auto: without a span the bar is measured with infinite width and Measure returns an invalid
            // size. Spanning both columns aligns the bar with the card's content row.
            Grid.SetRow(_progressBar, 1);
            Grid.SetColumn(_progressBar, 0);
            Grid.SetColumnSpan(_progressBar, 2);
            _layout.Children.Add(_progressBar);
        }

        _progressBar.SetCurrentValue(NotificationProgressBar.ExpirationProperty, _progressBarTotalExpiration);
        _progressBar.SetCurrentValue(NotificationProgressBar.CurrentExpirationProperty, Expiration.Value);
    }

    private void ClearProgressBar()
    {
        if (_progressBar is null)
        {
            return;
        }

        _layout?.Children.Remove(_progressBar);
        _progressBar.SetTemplatedParent(null);
        _progressBar                = null;
        _progressBarTotalExpiration = default;
    }

    private void SetupNotificationTypePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Error, NotificationType == NotificationType.Error);
        PseudoClasses.Set(StdPseudoClass.Information, NotificationType == NotificationType.Information);
        PseudoClasses.Set(StdPseudoClass.Success, NotificationType == NotificationType.Success);
        PseudoClasses.Set(StdPseudoClass.Warning, NotificationType == NotificationType.Warning);
    }

    private void SetupDefaultNotificationIcon()
    {
        if (_isApplyingTemplateNotificationIcon)
        {
            return;
        }

        var currentIcon = Icon;
        if (currentIcon is not null &&
            !ReferenceEquals(currentIcon, _templateNotificationIcon))
        {
            return;
        }

        var icon = CreateDefaultNotificationIcon();
        _isApplyingTemplateNotificationIcon = true;
        try
        {
            if (currentIcon is null)
            {
                ClearValue(IconProperty);
            }

            _templateNotificationIcon = icon;
            SetValue(IconProperty, icon, BindingPriority.Template);
        }
        finally
        {
            _isApplyingTemplateNotificationIcon = false;
        }
    }

    private PathIcon? CreateDefaultNotificationIcon()
    {
        if (NotificationType == NotificationType.Information)
        {
            return new InfoCircleFilled();
        }

        if (NotificationType == NotificationType.Success)
        {
            return new CheckCircleFilled();
        }

        if (NotificationType == NotificationType.Error)
        {
            return new CloseCircleFilled();
        }

        if (NotificationType == NotificationType.Warning)
        {
            return new ExclamationCircleFilled();
        }

        return null;
    }

    /// <summary>
    /// Asks the manager that shows the card to pause it while the pointer is over it, after the base member has run.
    /// A standalone card has no manager and is not paused. An override must call this member.
    /// </summary>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _notificationManager?.SetItemPaused(this, true);
    }

    /// <summary>
    /// Asks the manager that shows the card to resume it, after the base member has run. A standalone card has no
    /// manager. An override must call this member.
    /// </summary>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _notificationManager?.SetItemPaused(this, false);
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
