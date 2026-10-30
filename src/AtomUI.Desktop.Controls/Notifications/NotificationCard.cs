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

[PseudoClasses(StdPseudoClass.Error, StdPseudoClass.Information, StdPseudoClass.Success, StdPseudoClass.Warning)]
public partial class NotificationCard : ContentControl,
                                IMotionAwareControl,
                                IFeedbackStackItem,
                                IFeedbackStackTransitionSnapshotItem
{
    #region 公共属性定义
    
    public static readonly DirectProperty<NotificationCard, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<NotificationCard, bool>(nameof(IsClosing), o => o.IsClosing);
    
    public static readonly StyledProperty<bool> IsClosedProperty =
        AvaloniaProperty.Register<NotificationCard, bool>(nameof(IsClosed));

    public static readonly StyledProperty<bool> IsShowProgressProperty =
        AvaloniaProperty.Register<NotificationCard, bool>(nameof(IsShowProgress));
    
    public static readonly StyledProperty<NotificationType> NotificationTypeProperty =
        AvaloniaProperty.Register<NotificationCard, NotificationType>(
            nameof(NotificationType),
            NotificationType.Default);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NotificationCard>();
    
    public static readonly RoutedEvent<RoutedEventArgs> NotificationClosedEvent =
        RoutedEvent.Register<NotificationCard, RoutedEventArgs>(nameof(NotificationClosed), RoutingStrategies.Bubble);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<NotificationCard, string>(nameof(Title));

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<NotificationCard, PathIcon?>(nameof(Icon));

    /// <summary>
    /// 操作组内容，显示在通知卡片描述下方的操作区。
    /// </summary>
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<NotificationCard, object?>(nameof(Actions));

    /// <summary>
    /// 操作组内容的数据模板，用于自定义操作区呈现。
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ActionsTemplateProperty =
        AvaloniaProperty.Register<NotificationCard, IDataTemplate?>(nameof(ActionsTemplate));

    /// <summary>
    /// 卡片表面阴影。root 外观由 owner 属性投影到模板表面 Border，便于 owner-scoped Semantic Style 定制。
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<NotificationCard>();
    
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
    /// 操作组内容。非空时模板中的 actions 区域可见。
    /// </summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <summary>
    /// 操作组内容模板。
    /// </summary>
    public IDataTemplate? ActionsTemplate
    {
        get => GetValue(ActionsTemplateProperty);
        set => SetValue(ActionsTemplateProperty, value);
    }

    /// <summary>
    /// 卡片表面阴影，投影到模板表面 Border。
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

    /// <summary>回调：用户点击通知卡片时触发，由 WindowNotificationManager 设置以避免 lambda 闭包。</summary>
    internal Action? OnClick { get; set; }

    /// <summary>回调：通知卡片关闭时触发，由 WindowNotificationManager 设置以避免 lambda 闭包。</summary>
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
    /// Initializes a new instance of the <see cref="NotificationCard" /> class.
    /// </summary>
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

    void IFeedbackStackItem.RequestClose()
    {
        Close();
    }

    void IFeedbackStackItem.UpdateRemaining(TimeSpan remaining)
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
            // progress 是覆盖在卡片底部的运行时 Part：显式注入 semantic class，随进度条一同创建与释放。
            _progressBar.Classes.Add(NotificationCardSemanticParts.ProgressClass);
            _progressBar.SetTemplatedParent(this);
            // 列 0 是 Auto，未跨列时会以无限宽测量并令 Measure 返回非法尺寸；跨双列对齐卡片内容行。
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

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _notificationManager?.SetItemPaused(this, true);
    }

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
