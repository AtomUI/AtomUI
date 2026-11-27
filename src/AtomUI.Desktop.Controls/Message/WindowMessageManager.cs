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

/// <summary>
/// Shows transient messages at an edge of a window. Each message becomes a <see cref="MessageCard" /> that is
/// listed or folded into a stack with the other cards, and that closes by itself after its expiration. A manager
/// created with a <see cref="TopLevel" /> installs itself into the feedback layer of that window.
/// </summary>
[TemplatePart("PART_Items", typeof(FeedbackStackPresenter))]
[PseudoClasses(NotificationPseudoClass.TopLeft,
    NotificationPseudoClass.TopRight,
    NotificationPseudoClass.BottomLeft,
    NotificationPseudoClass.BottomRight,
    NotificationPseudoClass.TopCenter,
    NotificationPseudoClass.BottomCenter)]
public partial class WindowMessageManager : TemplatedControl, IMessageManager, IMotionAwareControl, IDisposable
{
    /// <summary>
    /// Identifies the <see cref="Position" /> property.
    /// </summary>
    public static readonly StyledProperty<NotificationPosition> PositionProperty =
        AvaloniaProperty.Register<WindowMessageManager, NotificationPosition>(
            nameof(Position), NotificationPosition.TopCenter);

    /// <summary>
    /// Identifies the <see cref="MaxItems" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxItemsProperty =
        AvaloniaProperty.Register<WindowMessageManager, int>(nameof(MaxItems));

    /// <summary>
    /// Identifies the <see cref="IsStackEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsStackEnabledProperty =
        AvaloniaProperty.Register<WindowMessageManager, bool>(nameof(IsStackEnabled));

    /// <summary>
    /// Identifies the <see cref="StackThreshold" /> property.
    /// </summary>
    public static readonly StyledProperty<int> StackThresholdProperty =
        AvaloniaProperty.Register<WindowMessageManager, int>(nameof(StackThreshold), 3);

    /// <summary>
    /// Identifies the <see cref="IsPauseOnHover" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPauseOnHoverProperty =
        AvaloniaProperty.Register<WindowMessageManager, bool>(nameof(IsPauseOnHover), true);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
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

    /// <summary>
    /// The window edge at which the messages appear; the cards are laid out from that edge. The item presenter and
    /// every card shown receive the value, and the card uses it for its entry and close motion. The default is
    /// <see cref="NotificationPosition.TopCenter" />.
    /// </summary>
    public NotificationPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>
    /// The greatest number of cards that may be open at the same time. When a message is shown and more cards are
    /// open, the oldest ones that are not closing are closed until the number fits. A value of <c>0</c> or less
    /// removes the limit; the default is <c>0</c>.
    /// </summary>
    public int MaxItems
    {
        get => GetValue(MaxItemsProperty);
        set => SetValue(MaxItemsProperty, value);
    }

    /// <summary>
    /// Lets the cards pile up into a stack while more than <see cref="StackThreshold" /> of them are open, instead
    /// of listing every card. The value is passed to the item presenter. The default is <see langword="false" />.
    /// </summary>
    public bool IsStackEnabled
    {
        get => GetValue(IsStackEnabledProperty);
        set => SetValue(IsStackEnabledProperty, value);
    }

    /// <summary>
    /// The number of open cards that may be listed before they pile up into a stack, which is used while more cards
    /// than this are open and <see cref="IsStackEnabled" /> is <see langword="true" />. Values below <c>1</c> count
    /// as <c>1</c>. The default is <c>3</c>.
    /// </summary>
    public int StackThreshold
    {
        get => GetValue(StackThresholdProperty);
        set => SetValue(StackThresholdProperty, value);
    }

    /// <summary>
    /// Pauses the expiry of a card while the pointer is over it, and of all cards while the pointer is over the
    /// folded stack. Turning it off resumes every paused card at once. The default is <see langword="true" />.
    /// </summary>
    public bool IsPauseOnHover
    {
        get => GetValue(IsPauseOnHoverProperty);
        set => SetValue(IsPauseOnHoverProperty, value);
    }

    /// <summary>
    /// Controls whether the cards animate. The value is relayed to the item presenter and to every card shown,
    /// which use it for their motion. The theme takes the value from the <c>EnableMotion</c> design Token; without a
    /// value from a style or the control it is <see langword="false" />.
    /// </summary>
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
    /// <param name="host">
    /// The TopLevel that will host the control. Pass <c>null</c> to skip installing
    /// the manager into a TopLevel layer; the manager then renders inline wherever the caller places it.
    /// </param>
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

    /// <summary>
    /// Binds the template's <c>PART_Items</c> presenter: it receives the cards and the values of
    /// <see cref="Position" />, <see cref="IsStackEnabled" />, <see cref="StackThreshold" /> and
    /// <see cref="IsMotionEnabled" />, and it reports the hover state of the stack here. The base method is called
    /// before the new presenter is looked up.
    /// </summary>
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

    /// <summary>
    /// Resumes the expiry of the cards, which is paused while the manager is not in the tree, and records that the
    /// manager was attached, so that a later detach releases its cards. Calls the base method first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isLifecyclePaused = false;
        _hasBeenAttached = true;
        UpdateSchedulerPauseState();
    }

    /// <summary>
    /// Pauses the expiry of the cards and schedules the release of every card once the detach has finished, unless
    /// the manager is attached again before that. The base method is called after the expiry is paused.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isLifecyclePaused = true;
        UpdateSchedulerPauseState();
        base.OnDetachedFromVisualTree(e);
        ScheduleReleaseCardsOnHostDetach();
    }

    // Host layer release contract: a manager that leaves the visual tree while it is running releases every card
    // it holds, which matches the upstream message list when it unmounts.
    // The release must wait until the detach cascade has finished: closing cards synchronously while they are being
    // detached makes the presenter rebuild its container tree and run past the end of a collection inside the
    // Avalonia visual tree. A manager that was never attached is not affected: Show is allowed before attach.
    private void ScheduleReleaseCardsOnHostDetach()
    {
        if (!_hasBeenAttached || _isDisposed)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            // Back in the tree by now, for example after a transient detach such as a feedback layer migration.
            if (_isDisposed || !_isLifecyclePaused)
            {
                return;
            }

            // Close callbacks may change _cards synchronously, so work on a snapshot.
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
    /// Shows a message as a new <see cref="MessageCard" /> and closes the oldest cards beyond
    /// <see cref="MaxItems" />. A message with a positive expiration closes by itself after that time. Does nothing
    /// after <see cref="Dispose" />.
    /// </summary>
    /// <param name="message">
    /// The message to show: its type, icon and text, how long it stays open and the callback to run once it closed.
    /// </param>
    /// <param name="classes">
    /// Style classes to add to the card, or <see langword="null" /> to add none.
    /// </param>
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

    /// <summary>
    /// Closes every card, the newest first. A card leaves the manager when its close has completed. Must be called
    /// on the UI thread; does nothing after <see cref="Dispose" />.
    /// </summary>
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

    /// <summary>
    /// Updates the pseudo-classes when <see cref="Position" /> changes, and resumes the expiry of all cards when
    /// <see cref="IsPauseOnHover" /> or <see cref="IsStackEnabled" /> is turned off. Calls the base method first.
    /// </summary>
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
        _safeAreaMarginSubscription = TopLevelMarginBinder.BindHostMargin(topLevel, margin => SetCurrentValue(MarginProperty, margin));
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

    /// <summary>
    /// Releases the manager. Every card is dropped without a close motion and without running its close callback,
    /// the expiry timers stop, and the manager is removed from the host layer. Later calls of <see cref="Show" />
    /// and <see cref="DestroyAll" /> do nothing.
    /// </summary>
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
