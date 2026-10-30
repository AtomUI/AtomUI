using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal sealed class FeedbackStackHoverChangedEventArgs(bool isPointerOver, bool isStackInteraction) : EventArgs
{
    internal bool IsPointerOver { get; } = isPointerOver;
    internal bool IsStackInteraction { get; } = isStackInteraction;
}

/// <summary>
/// Stable feedback items host that owns whole-stack hover state while delegating hot-path geometry to
/// <see cref="FeedbackStackPanel"/>.
/// </summary>
internal sealed class FeedbackStackPresenter : ItemsControl
{
    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new FeedbackStackPanel());

    public static readonly StyledProperty<FeedbackStackMode> StackModeProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, FeedbackStackMode>(nameof(StackMode));

    public static readonly StyledProperty<NotificationPosition> PositionProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, NotificationPosition>(
            nameof(Position), NotificationPosition.TopCenter);

    public static readonly StyledProperty<bool> IsStackEnabledProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, bool>(nameof(IsStackEnabled));

    public static readonly StyledProperty<int> StackThresholdProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, int>(nameof(StackThreshold), 3);

    public static readonly StyledProperty<double> ExpandedGapProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, double>(nameof(ExpandedGap), 16);

    public static readonly StyledProperty<double> CollapsedOffsetProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, double>(nameof(CollapsedOffset), 8);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, bool>(nameof(IsMotionEnabled), true);

    public static readonly DirectProperty<FeedbackStackPresenter, bool> IsCollapsedProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, bool>(nameof(IsCollapsed), owner => owner.IsCollapsed);

    public static readonly DirectProperty<FeedbackStackPresenter, int> ActiveItemCountProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, int>(
            nameof(ActiveItemCount), owner => owner.ActiveItemCount);

    public static readonly DirectProperty<FeedbackStackPresenter, double> FirstBackplateWidthProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, double>(
            nameof(FirstBackplateWidth), owner => owner.FirstBackplateWidth);

    public static readonly DirectProperty<FeedbackStackPresenter, double> SecondBackplateWidthProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, double>(
            nameof(SecondBackplateWidth), owner => owner.SecondBackplateWidth);

    public static readonly DirectProperty<FeedbackStackPresenter, Thickness> FirstBackplateMarginProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, Thickness>(
            nameof(FirstBackplateMargin), owner => owner.FirstBackplateMargin);

    public static readonly DirectProperty<FeedbackStackPresenter, Thickness> SecondBackplateMarginProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, Thickness>(
            nameof(SecondBackplateMargin), owner => owner.SecondBackplateMargin);

    // These read-only properties let AXAML bind the two static message backplates.
    public static readonly DirectProperty<FeedbackStackPresenter, bool> IsFirstBackplateVisibleProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, bool>(
            nameof(IsFirstBackplateVisible), owner => owner.IsFirstBackplateVisible);

    public static readonly DirectProperty<FeedbackStackPresenter, bool> IsSecondBackplateVisibleProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, bool>(
            nameof(IsSecondBackplateVisible), owner => owner.IsSecondBackplateVisible);

    private FeedbackStackPanel? _panel;
    private bool _isCollapsed;
    private int _activeItemCount;
    private double _firstBackplateWidth;
    private double _secondBackplateWidth;
    private Thickness _firstBackplateMargin;
    private Thickness _secondBackplateMargin;
    private bool _isFirstBackplateVisible;
    private bool _isSecondBackplateVisible;
    private bool _isPointerOver;
    private bool _isStackInteraction;
    private PixelPoint? _lastPointerScreenPosition;
    private bool _isTrackingLayoutMoves;

    internal event EventHandler<FeedbackStackHoverChangedEventArgs>? StackHoverChanged;

    public FeedbackStackMode StackMode
    {
        get => GetValue(StackModeProperty);
        set => SetValue(StackModeProperty, value);
    }

    public NotificationPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
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

    public double ExpandedGap
    {
        get => GetValue(ExpandedGapProperty);
        set => SetValue(ExpandedGapProperty, value);
    }

    public double CollapsedOffset
    {
        get => GetValue(CollapsedOffsetProperty);
        set => SetValue(CollapsedOffsetProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsCollapsed => _isCollapsed;

    public int ActiveItemCount => _activeItemCount;

    public double FirstBackplateWidth => _firstBackplateWidth;

    public double SecondBackplateWidth => _secondBackplateWidth;

    public Thickness FirstBackplateMargin => _firstBackplateMargin;

    public Thickness SecondBackplateMargin => _secondBackplateMargin;

    public bool IsFirstBackplateVisible => _isFirstBackplateVisible;

    public bool IsSecondBackplateVisible => _isSecondBackplateVisible;

    static FeedbackStackPresenter()
    {
        ItemsPanelProperty.OverrideDefaultValue<FeedbackStackPresenter>(DefaultPanel);
    }

    internal void AttachPanel(FeedbackStackPanel panel)
    {
        _panel = panel;
        ConfigurePanel();
    }

    internal void DetachPanel(FeedbackStackPanel panel)
    {
        if (ReferenceEquals(_panel, panel))
        {
            _panel = null;
        }
    }

    internal void ReportLayoutState(bool isCollapsed, Size collapsedCardSize, int activeItemCount)
    {
        SetAndRaise(IsCollapsedProperty, ref _isCollapsed, isCollapsed);
        SetAndRaise(ActiveItemCountProperty, ref _activeItemCount, activeItemCount);

        if (StackMode != FeedbackStackMode.Message)
        {
            SetAndRaise(IsFirstBackplateVisibleProperty, ref _isFirstBackplateVisible, false);
            SetAndRaise(IsSecondBackplateVisibleProperty, ref _isSecondBackplateVisible, false);
            SetAndRaise(FirstBackplateWidthProperty, ref _firstBackplateWidth, 0);
            SetAndRaise(SecondBackplateWidthProperty, ref _secondBackplateWidth, 0);
            SetAndRaise(FirstBackplateMarginProperty, ref _firstBackplateMargin, default);
            SetAndRaise(SecondBackplateMarginProperty, ref _secondBackplateMargin, default);
            UpdateInteractionState();
            return;
        }

        SetAndRaise(
            IsFirstBackplateVisibleProperty,
            ref _isFirstBackplateVisible,
            isCollapsed && activeItemCount >= 2);
        SetAndRaise(
            IsSecondBackplateVisibleProperty,
            ref _isSecondBackplateVisible,
            isCollapsed && activeItemCount >= 3);

        var firstWidth = Math.Max(0, collapsedCardSize.Width - 16);
        var secondWidth = Math.Max(0, collapsedCardSize.Width - 32);
        SetAndRaise(FirstBackplateWidthProperty, ref _firstBackplateWidth, firstWidth);
        SetAndRaise(SecondBackplateWidthProperty, ref _secondBackplateWidth, secondWidth);

        var isBottom = Position is NotificationPosition.BottomLeft or
            NotificationPosition.BottomCenter or
            NotificationPosition.BottomRight;
        // The non-measuring backplate layer is anchored at the same outer edge as the cards.
        var firstOffset = isBottom ? -collapsedCardSize.Height - 8 : Math.Max(0, collapsedCardSize.Height - 8);
        var secondOffset = isBottom ? -collapsedCardSize.Height - 16 : collapsedCardSize.Height;
        var firstMargin = new Thickness(0, firstOffset, 0, 0);
        var secondMargin = new Thickness(0, secondOffset, 0, 0);
        SetAndRaise(FirstBackplateMarginProperty, ref _firstBackplateMargin, firstMargin);
        SetAndRaise(SecondBackplateMarginProperty, ref _secondBackplateMargin, secondMargin);
        UpdateInteractionState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StackModeProperty ||
            change.Property == PositionProperty ||
            change.Property == IsStackEnabledProperty ||
            change.Property == StackThresholdProperty ||
            change.Property == IsMotionEnabledProperty ||
            change.Property == ExpandedGapProperty ||
            change.Property == CollapsedOffsetProperty)
        {
            ConfigurePanel();
        }
        else if (change.Property == BoundsProperty)
        {
            RevalidatePointerOverAfterLayoutMove();
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        RememberPointerPosition(e);
        SetPointerOverState(true);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        RememberPointerPosition(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        SetPointerOverState(false);
        _lastPointerScreenPosition = null;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopTrackingLayoutMoves();
        SetPointerOverState(false);
        _lastPointerScreenPosition = null;
        base.OnDetachedFromVisualTree(e);
    }

    internal void SetPointerOverState(bool isPointerOver)
    {
        if (!isPointerOver)
        {
            var wasStackInteraction = _isStackInteraction;
            var needsConvergence = _isPointerOver ||
                                   wasStackInteraction ||
                                   _panel is { IsStackExpanded: true };
            _isPointerOver = false;
            _isStackInteraction = false;
            if (_panel is not null)
            {
                _panel.IsStackExpanded = false;
            }
            if (needsConvergence)
            {
                StackHoverChanged?.Invoke(
                    this,
                    new FeedbackStackHoverChangedEventArgs(false, wasStackInteraction));
            }
            StopTrackingLayoutMoves();
            return;
        }

        var needsExpansion = !_isPointerOver ||
                             !_isStackInteraction ||
                             _panel is { IsStackExpanded: false };
        _isPointerOver = isPointerOver;
        StartTrackingLayoutMoves();
        UpdateInteractionState(forceNotification: needsExpansion);
    }

    private void StartTrackingLayoutMoves()
    {
        if (_isTrackingLayoutMoves)
        {
            return;
        }

        LayoutUpdated += OnLayoutUpdatedWhilePointerOver;
        _isTrackingLayoutMoves = true;
    }

    private void StopTrackingLayoutMoves()
    {
        if (!_isTrackingLayoutMoves)
        {
            return;
        }

        LayoutUpdated -= OnLayoutUpdatedWhilePointerOver;
        _isTrackingLayoutMoves = false;
    }

    private void OnLayoutUpdatedWhilePointerOver(object? sender, EventArgs e)
    {
        RevalidatePointerOverAfterLayoutMove();
    }

    private void RememberPointerPosition(PointerEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is not null)
        {
            _lastPointerScreenPosition = topLevel.PointToScreen(e.GetPosition(topLevel));
        }
    }

    private void RevalidatePointerOverAfterLayoutMove()
    {
        if (!_isPointerOver || _lastPointerScreenPosition is not { } screenPosition)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            SetPointerOverState(false);
            _lastPointerScreenPosition = null;
            return;
        }

        var pointerPosition = topLevel.PointToClient(screenPosition);
        var localPosition = topLevel.TranslatePoint(pointerPosition, this);
        if (localPosition is null || !new Rect(Bounds.Size).Contains(localPosition.Value))
        {
            SetPointerOverState(false);
            _lastPointerScreenPosition = null;
        }
    }

    private void ConfigurePanel()
    {
        if (_panel is null)
        {
            return;
        }

        _panel.StackMode = StackMode;
        _panel.Position = Position;
        _panel.IsStackEnabled = IsStackEnabled;
        _panel.StackThreshold = StackThreshold;
        _panel.IsMotionEnabled = IsMotionEnabled;
        _panel.ExpandedGap = ExpandedGap;
        _panel.CollapsedOffset = CollapsedOffset;
        UpdateInteractionState();
    }

    private void UpdateInteractionState(bool forceNotification = false)
    {
        var isStackInteraction = IsStackEnabled && _isPointerOver;
        if (_panel is not null)
        {
            _panel.IsStackExpanded = isStackInteraction;
        }

        if (forceNotification || _isStackInteraction != isStackInteraction)
        {
            _isStackInteraction = isStackInteraction;
            StackHoverChanged?.Invoke(
                this,
                new FeedbackStackHoverChangedEventArgs(_isPointerOver, isStackInteraction));
        }
    }
}
