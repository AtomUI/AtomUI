using System.Collections.ObjectModel;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

[TemplatePart("PART_Items", typeof(FeedbackStackPresenter))]
[PseudoClasses(NotificationPseudoClass.TopLeft,
    NotificationPseudoClass.TopRight,
    NotificationPseudoClass.BottomLeft,
    NotificationPseudoClass.BottomRight,
    NotificationPseudoClass.TopCenter,
    NotificationPseudoClass.BottomCenter)]
public partial class WindowMessageManager : TemplatedControl, IMessageManager, IMotionAwareControl, IDisposable
{
    public static readonly StyledProperty<NotificationPosition> PositionProperty =
        AvaloniaProperty.Register<WindowMessageManager, NotificationPosition>(
            nameof(Position), NotificationPosition.TopCenter);

    public static readonly StyledProperty<int> MaxItemsProperty =
        AvaloniaProperty.Register<WindowMessageManager, int>(nameof(MaxItems));

    public static readonly StyledProperty<bool> IsStackEnabledProperty =
        AvaloniaProperty.Register<WindowMessageManager, bool>(nameof(IsStackEnabled));

    public static readonly StyledProperty<int> StackThresholdProperty =
        AvaloniaProperty.Register<WindowMessageManager, int>(nameof(StackThreshold), 3);

    public static readonly StyledProperty<bool> IsPauseOnHoverProperty =
        AvaloniaProperty.Register<WindowMessageManager, bool>(nameof(IsPauseOnHover), true);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<WindowMessageManager>();

    private readonly ObservableCollection<MessageCard> _cards = new();
    private TopLevel? _topLevel;
    private bool _isDisposed;
    private Panel? _hostLayer;
    private bool _hostLayerUsesNativeAdorner;
    private IDisposable? _safeAreaMarginSubscription;
    private FeedbackStackPresenter? _presenter;
    private FeedbackLifetimeScheduler? _lifetimeScheduler;
    private bool _isLifecyclePaused = true;
    private bool _hasBeenAttached;
    private bool _isStackPaused;
    private const int MaxHostLayerRetryCount = 30;
    private bool _hostLayerRetryScheduled;
    private int _hostLayerRetryCount;

    public NotificationPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public int MaxItems
    {
        get => GetValue(MaxItemsProperty);
        set => SetValue(MaxItemsProperty, value);
    }

    public bool IsStackEnabled
    {
        get => GetValue(IsStackEnabledProperty);
        set => SetValue(IsStackEnabledProperty, value);
    }

    public int StackThreshold
    {
        get => GetValue(StackThresholdProperty);
        set => SetValue(StackThresholdProperty, value);
    }

    public bool IsPauseOnHover
    {
        get => GetValue(IsPauseOnHoverProperty);
        set => SetValue(IsPauseOnHoverProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal IReadOnlyList<MessageCard> Cards => _cards;

    internal bool HasLifetimeScheduler => _lifetimeScheduler is not null;

    internal int LifetimeEntryCount => _lifetimeScheduler?.Count ?? 0;

    internal bool IsLifetimePaused => _lifetimeScheduler?.IsAllPaused ?? _isLifecyclePaused;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowMessageManager" /> class without a host.
    /// The manager is not installed into any layer; it renders inline wherever the caller places it,
    /// which is the AtomUI equivalent of rendering a message list in a local container instead of the
    /// window feedback layer. This also makes the control declaratively usable from XAML, mirroring
    /// <see cref="WindowNotificationManager" />.
    /// </summary>
    public WindowMessageManager()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowMessageManager" /> class.
    /// </summary>
    /// <param name="host">The TopLevel that will host the control. Pass <c>null</c> to skip installing
    /// the manager into a TopLevel layer; the manager then renders inline wherever the caller places it.</param>
    public WindowMessageManager(TopLevel? host) : this()
    {
        if (host is not null)
        {
            _topLevel = host;
            InstallFromTopLevel(host);
        }
    }

    static WindowMessageManager()
    {
        HorizontalAlignmentProperty.OverrideDefaultValue<WindowMessageManager>(HorizontalAlignment.Stretch);
        VerticalAlignmentProperty.OverrideDefaultValue<WindowMessageManager>(VerticalAlignment.Stretch);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_presenter is not null)
        {
            _presenter.StackHoverChanged -= OnStackHoverChanged;
            _presenter.ItemsSource = null;
        }

        base.OnApplyTemplate(e);
        _presenter = e.NameScope.Find<FeedbackStackPresenter>("PART_Items");
        if (_presenter is not null)
        {
            _presenter[!FeedbackStackPresenter.PositionProperty] = this[!PositionProperty];
            _presenter[!FeedbackStackPresenter.IsStackEnabledProperty] = this[!IsStackEnabledProperty];
            _presenter[!FeedbackStackPresenter.StackThresholdProperty] = this[!StackThresholdProperty];
            _presenter[!FeedbackStackPresenter.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
            _presenter.StackMode = FeedbackStackMode.Message;
            _presenter.ItemsSource = _cards;
            _presenter.StackHoverChanged += OnStackHoverChanged;
        }
        UpdatePseudoClasses(Position);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isLifecyclePaused = false;
        _hasBeenAttached = true;
        UpdateSchedulerPauseState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isLifecyclePaused = true;
        UpdateSchedulerPauseState();
        base.OnDetachedFromVisualTree(e);
        ScheduleReleaseCardsOnHostDetach();
    }

    // 宿主层释放契约：manager 运行途中离开视觉树即释放其上的全部卡片（对齐上游 message-list 卸载行为）。
    // 必须推迟到 detach 级联完成之后：在 detach 过程中同步关闭卡片会让 presenter 重建容器树，
    // 触发 Avalonia 视觉树内部集合越界。从未 attach 过的 manager 不适用——release/6.0 允许 attach 前 Show。
    private void ScheduleReleaseCardsOnHostDetach()
    {
        if (!_hasBeenAttached || _isDisposed)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            // 执行时已重新入树（反馈层迁移等瞬时 detach）则不释放。
            if (_isDisposed || !_isLifecyclePaused)
            {
                return;
            }

            // 关闭回调可能同步修改 _cards，先拷贝快照。
            var cards = _cards.ToArray();
            for (var i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                _lifetimeScheduler?.Remove(card);
                card.Close();
            }
        });
    }

    /// <summary>
    /// Shows a Message
    /// </summary>
    /// <param name="message">the content of the message</param>
    /// <param name="classes">style classes to apply</param>
    public void Show(IMessage message, string[]? classes = null)
    {
        Dispatcher.VerifyAccess();
        if (_isDisposed)
        {
            return;
        }

        var card = new MessageCard
        {
            Icon = message.Icon,
            Message = message.Content,
            MessageType = message.Type,
            OnClose = message.OnClose,
            HoverChanged = OnCardHoverChanged
        };
        card[!MessageCard.PositionProperty] = this[!PositionProperty];
        card[!MessageCard.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        if (classes is not null)
        {
            for (var i = 0; i < classes.Length; i++)
            {
                card.Classes.Add(classes[i]);
            }
        }

        card.MessageClosed += OnMessageClosed;
        _cards.Add(card);
        WindowFeedbackLayer.Activate(_hostLayer, this);
        if (message.Expiration > TimeSpan.Zero)
        {
            var scheduler = _lifetimeScheduler ??= new FeedbackLifetimeScheduler();
            scheduler.Register(card, message.Expiration);
            UpdateSchedulerPauseState();
        }
        RemoveExcessMessages();
    }

    public void DestroyAll()
    {
        Dispatcher.VerifyAccess();
        // Close callbacks can synchronously change the queue or dispose its owner.
        var cards = _cards.ToArray();
        for (var i = cards.Length - 1; i >= 0 && !_isDisposed; i--)
        {
            var card = cards[i];
            _lifetimeScheduler?.Remove(card);
            card.Close();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PositionProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<NotificationPosition>());
        }
        else if (change.Property == IsPauseOnHoverProperty && !change.GetNewValue<bool>())
        {
            _isStackPaused = false;
            UpdateSchedulerPauseState();
            for (var i = 0; i < _cards.Count; i++)
            {
                _lifetimeScheduler?.SetPaused(_cards[i], false);
            }
        }
        else if (change.Property == IsStackEnabledProperty && !change.GetNewValue<bool>())
        {
            _isStackPaused = false;
            UpdateSchedulerPauseState();
        }
    }

    private void OnCardHoverChanged(IFeedbackStackItem item, bool isPointerOver)
    {
        if (IsPauseOnHover)
        {
            _lifetimeScheduler?.SetPaused(item, isPointerOver);
        }
    }

    private void OnStackHoverChanged(object? sender, FeedbackStackHoverChangedEventArgs e)
    {
        _isStackPaused = e.IsStackInteraction && e.IsPointerOver && IsPauseOnHover;
        UpdateSchedulerPauseState();
        _lifetimeScheduler?.Refresh();
    }

    private void UpdateSchedulerPauseState()
    {
        _lifetimeScheduler?.SetAllPaused(_isLifecyclePaused || _isStackPaused);
    }

    private void OnMessageClosed(object? sender, RoutedEventArgs e)
    {
        if (sender is not MessageCard card)
        {
            return;
        }

        _lifetimeScheduler?.Remove(card);
        card.MessageClosed -= OnMessageClosed;
        _cards.Remove(card);
        var callback = card.OnClose;
        card.ReleaseOwner();
        try
        {
            callback?.Invoke();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Message close callback failed: {exception.Message}");
        }
    }

    private void RemoveExcessMessages()
    {
        if (MaxItems <= 0)
        {
            return;
        }

        var activeCount = 0;
        for (var i = 0; i < _cards.Count; i++)
        {
            if (!_cards[i].IsClosing)
            {
                activeCount++;
            }
        }

        var excessCount = activeCount - MaxItems;
        if (excessCount <= 0)
        {
            return;
        }

        // Select the oldest active batch before synchronous close callbacks mutate the queue.
        var cards = new MessageCard[excessCount];
        var count = 0;
        for (var i = 0; i < _cards.Count && count < excessCount; i++)
        {
            if (!_cards[i].IsClosing)
            {
                cards[count++] = _cards[i];
            }
        }
        for (var i = 0; i < count && !_isDisposed; i++)
        {
            _lifetimeScheduler?.Remove(cards[i]);
            cards[i].Close();
        }
    }

    private void InstallFromTopLevel(TopLevel topLevel)
    {
        topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        topLevel.TemplateApplied += TopLevelOnTemplateApplied;
        TryInstallHostLayer(topLevel);
        _safeAreaMarginSubscription?.Dispose();
        _safeAreaMarginSubscription = TopLevelMarginBinder.BindHostMargin(topLevel, margin => Margin = margin);
    }

    private void TryInstallHostLayer(TopLevel topLevel)
    {
        _hostLayer = WindowFeedbackLayer.GetLayer(topLevel);
        _hostLayerUsesNativeAdorner = false;
        if (_hostLayer is null)
        {
            _hostLayer = AdornerLayer.GetAdornerLayer(topLevel);
            _hostLayerUsesNativeAdorner = _hostLayer is not null;
        }
        if (_hostLayer is null)
        {
            ScheduleHostLayerRetry(topLevel);
            return;
        }

        _hostLayerRetryCount = 0;
        _hostLayerRetryScheduled = false;
        if (!_hostLayer.Children.Contains(this))
        {
            _hostLayer.Children.Add(this);
        }
        AdornerLayer.SetAdornedElement(this, _hostLayerUsesNativeAdorner ? _hostLayer : null);
    }

    private void ScheduleHostLayerRetry(TopLevel topLevel)
    {
        if (_hostLayerRetryScheduled || _isDisposed || _hostLayerRetryCount >= MaxHostLayerRetryCount)
        {
            return;
        }
        _hostLayerRetryScheduled = true;
        if (_hostLayerRetryCount == 0)
        {
            Dispatcher.UIThread.Post(() => RetryInstallFromTopLevel(topLevel), DispatcherPriority.Loaded);
        }
        else
        {
            DispatcherTimer.RunOnce(() => RetryInstallFromTopLevel(topLevel), TimeSpan.FromMilliseconds(16));
        }
    }

    private void RetryInstallFromTopLevel(TopLevel topLevel)
    {
        if (!_hostLayerRetryScheduled)
        {
            return;
        }
        _hostLayerRetryScheduled = false;
        if (_isDisposed || _hostLayer is not null || !ReferenceEquals(_topLevel, topLevel))
        {
            return;
        }
        _hostLayerRetryCount++;
        TryInstallHostLayer(topLevel);
    }

    private void TopLevelOnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        RemoveFromHostLayer();
        _hostLayerRetryScheduled = false;
        _hostLayerRetryCount = 0;
        var topLevel = (TopLevel)sender!;
        topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        InstallFromTopLevel(topLevel);
    }

    private void RemoveFromHostLayer()
    {
        _safeAreaMarginSubscription?.Dispose();
        _safeAreaMarginSubscription = null;
        if (_hostLayer is null)
        {
            return;
        }
        _hostLayer.Children.Remove(this);
        if (_hostLayerUsesNativeAdorner)
        {
            AdornerLayer.SetAdornedElement(this, null);
        }
        _hostLayer = null;
        _hostLayerUsesNativeAdorner = false;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        if (_topLevel is not null)
        {
            _topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        }
        if (_presenter is not null)
        {
            _presenter.StackHoverChanged -= OnStackHoverChanged;
            _presenter.ItemsSource = null;
            _presenter = null;
        }
        _lifetimeScheduler?.Dispose();
        _lifetimeScheduler = null;
        for (var i = 0; i < _cards.Count; i++)
        {
            var card = _cards[i];
            card.MessageClosed -= OnMessageClosed;
            card.ReleaseOwner();
        }
        _cards.Clear();
        _isLifecyclePaused = true;
        _isStackPaused = false;
        _hostLayerRetryScheduled = false;
        _hostLayerRetryCount = 0;
        RemoveFromHostLayer();
        _topLevel = null;
    }

    private void UpdatePseudoClasses(NotificationPosition position)
    {
        PseudoClasses.Set(NotificationPseudoClass.TopLeft, position == NotificationPosition.TopLeft);
        PseudoClasses.Set(NotificationPseudoClass.TopRight, position == NotificationPosition.TopRight);
        PseudoClasses.Set(NotificationPseudoClass.BottomLeft, position == NotificationPosition.BottomLeft);
        PseudoClasses.Set(NotificationPseudoClass.BottomRight, position == NotificationPosition.BottomRight);
        PseudoClasses.Set(NotificationPseudoClass.TopCenter, position == NotificationPosition.TopCenter);
        PseudoClasses.Set(NotificationPseudoClass.BottomCenter, position == NotificationPosition.BottomCenter);
    }
}
