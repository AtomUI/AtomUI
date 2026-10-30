using System.Collections.Specialized;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal sealed class TabScrollViewer : ScrollViewer, ITabOverflowPopupActionTarget
{
    private const string DefaultPopupTemplateResourceKey = "TabOverflowPopupDefaultTemplate";
    private const long ClosingSessionId = -1;

    internal static readonly DirectProperty<TabScrollViewer, Dock> TabStripPlacementProperty =
        AvaloniaProperty.RegisterDirect<TabScrollViewer, Dock>(
            nameof(TabStripPlacement),
            control => control.TabStripPlacement,
            (control, value) => control.TabStripPlacement = value);

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Flyout.IsPopupPinnedOpenProperty.AddOwner<TabScrollViewer>();

    internal static readonly StyledProperty<IDataTemplate?> OverflowPopupTemplateProperty =
        BaseTabControl.OverflowPopupTemplateProperty.AddOwner<TabScrollViewer>();

    internal static readonly DirectProperty<TabScrollViewer, bool> IsOverflowPopupOpenProperty =
        AvaloniaProperty.RegisterDirect<TabScrollViewer, bool>(
            nameof(IsOverflowPopupOpen),
            control => control.IsOverflowPopupOpen);

    internal static readonly DirectProperty<TabScrollViewer, TabOverflowPopupContext?> OverflowPopupContextProperty =
        AvaloniaProperty.RegisterDirect<TabScrollViewer, TabOverflowPopupContext?>(
            nameof(OverflowPopupContext),
            control => control.OverflowPopupContext);

    internal static readonly DirectProperty<TabScrollViewer, PlacementMode> OverflowPopupPlacementProperty =
        AvaloniaProperty.RegisterDirect<TabScrollViewer, PlacementMode>(
            nameof(OverflowPopupPlacement),
            control => control.OverflowPopupPlacement);

    private Dock _tabStripPlacement;
    private bool _isOverflowPopupOpen;
    private TabOverflowPopupContext? _overflowPopupContext;
    private PlacementMode _overflowPopupPlacement = PlacementMode.RightEdgeAlignedBottom;
    private IconButton? _menuIndicator;
    private TabOverflowEdgeIndicator? _startEdgeIndicator;
    private TabOverflowEdgeIndicator? _endEdgeIndicator;
    private Popup? _overflowPopup;
    private ContentPresenter? _overflowPopupPresenter;
    private ITabOverflowOwner? _overflowOwner;
    private readonly Dictionary<TabOverflowItem, OverflowEntry> _overflowEntries =
        new(ReferenceEqualityComparer.Instance);
    private readonly NotifyCollectionChangedEventHandler _overflowItemsChangedHandler;
    private readonly EventHandler _overflowSelectionChangedHandler;
    private bool _isOwnerChangeSubscribed;
    private long _nextSessionId;
    private long _openSessionId;

    static TabScrollViewer()
    {
        AffectsMeasure<TabScrollViewer>(TabStripPlacementProperty);
    }

    public TabScrollViewer()
    {
        _overflowItemsChangedHandler = HandleOverflowItemsChanged;
        _overflowSelectionChangedHandler = HandleOverflowSelectionChanged;
    }

    internal Dock TabStripPlacement
    {
        get => _tabStripPlacement;
        set => SetAndRaise(TabStripPlacementProperty, ref _tabStripPlacement, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    internal IDataTemplate? OverflowPopupTemplate
    {
        get => GetValue(OverflowPopupTemplateProperty);
        set => SetValue(OverflowPopupTemplateProperty, value);
    }

    internal bool IsOverflowPopupOpen
    {
        get => _isOverflowPopupOpen;
        private set => SetAndRaise(IsOverflowPopupOpenProperty, ref _isOverflowPopupOpen, value);
    }

    internal TabOverflowPopupContext? OverflowPopupContext
    {
        get => _overflowPopupContext;
        private set => SetAndRaise(OverflowPopupContextProperty, ref _overflowPopupContext, value);
    }

    internal PlacementMode OverflowPopupPlacement
    {
        get => _overflowPopupPlacement;
        private set => SetAndRaise(OverflowPopupPlacementProperty, ref _overflowPopupPlacement, value);
    }

    internal Popup? OverflowPopup => _overflowPopup;

    internal Control? OverflowPopupRoot => _overflowPopupPresenter?.Child;

    internal ITabOverflowOwner? OverflowOwner => _overflowOwner;

    internal void AttachOverflowOwner(ITabOverflowOwner owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (ReferenceEquals(_overflowOwner, owner))
        {
            TryOpenPinnedPopup();
            return;
        }

        FullTeardown(clearOwner: true);
        _overflowOwner = owner;
        TryOpenPinnedPopup();
    }

    internal void DetachOverflowOwner(ITabOverflowOwner owner)
    {
        if (ReferenceEquals(_overflowOwner, owner))
        {
            FullTeardown(clearOwner: true);
        }
    }

    internal void OpenOverflowPopup()
    {
        if (_openSessionId != 0 || IsOverflowPopupOpen ||
            _overflowOwner is not { } owner ||
            _overflowPopup is not { } popup ||
            _menuIndicator is not { IsEffectivelyEnabled: true, IsVisible: true })
        {
            return;
        }

        var snapshot = BuildOverflowSnapshot(owner);
        if (snapshot.Items.Count == 0)
        {
            return;
        }

        var context = OverflowPopupContext ?? new TabOverflowPopupContext();
        var sessionId = NextSessionId();
        _openSessionId = sessionId;
        SubscribeOwnerChanges(owner);
        try
        {
            OverflowPopupContext = context;
            if (!IsCurrentSession(sessionId, owner, context))
            {
                return;
            }

            // Publishing and constructing application content can synchronously dismiss
            // this session, replace its template or mutate/detach its owner.
            context.OpenSession(sessionId, snapshot.Items, snapshot.SelectedItem, this);
            if (!IsCurrentSession(sessionId, owner, context) ||
                !EnsurePopupContent(sessionId, owner, context))
            {
                return;
            }

            IsOverflowPopupOpen = true;
            if (!IsCurrentSession(sessionId, owner, context))
            {
                return;
            }
            popup.SetCurrentValue(Popup.IsPopupPinnedOpenProperty, IsPopupPinnedOpen);
            if (!IsCurrentSession(sessionId, owner, context))
            {
                return;
            }
            popup.SetCurrentValue(Popup.RequestedPlacementProperty, OverflowPopupPlacement);
            if (!IsCurrentSession(sessionId, owner, context))
            {
                return;
            }
            popup.SetCurrentValue(Avalonia.Controls.Primitives.Popup.IsOpenProperty, true);
            if (!IsCurrentSession(sessionId, owner, context) && _openSessionId == 0 && popup.IsOpen)
            {
                popup.CloseForLifecycle();
            }
        }
        catch
        {
            if (_openSessionId == sessionId)
            {
                ReleasePopupContent();
            }
            throw;
        }
    }

    internal void CloseOverflowPopup()
    {
        ClosePopupCore(forceLifecycleClose: false);
    }

    internal void CloseForLifecycle()
    {
        FullTeardown(clearOwner: false);
    }

    bool ITabOverflowPopupActionTarget.TryActivate(long sessionId, TabOverflowItem item)
    {
        if (!TryGetCurrentEntry(sessionId, item, out var owner, out var entry) || !item.IsEnabled)
        {
            return false;
        }

        return owner.TryActivateOverflowItem(entry.Index, entry.LogicalItem, entry.Container);
    }

    bool ITabOverflowPopupActionTarget.TryClose(long sessionId, TabOverflowItem item)
    {
        if (!TryGetCurrentEntry(sessionId, item, out var owner, out var entry) || !item.IsClosable)
        {
            return false;
        }

        return owner.TryCloseOverflowItem(entry.Index, entry.LogicalItem, entry.Container);
    }

    void ITabOverflowPopupActionTarget.Dismiss(long sessionId)
    {
        if (sessionId == _openSessionId)
        {
            CloseOverflowPopup();
        }
    }

    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (presenter is TabScrollContentPresenter tabScrollContentPresenter)
        {
            tabScrollContentPresenter.TabStripPlacement = TabStripPlacement;
        }

        return base.RegisterContentPresenter(presenter);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ReleaseTemplateParts();
        base.OnApplyTemplate(e);

        _menuIndicator = e.NameScope.Find<IconButton>("PART_ScrollMenuIndicator");
        _startEdgeIndicator = e.NameScope.Find<TabOverflowEdgeIndicator>("PART_ScrollStartEdgeIndicator");
        _endEdgeIndicator = e.NameScope.Find<TabOverflowEdgeIndicator>("PART_ScrollEndEdgeIndicator");
        _overflowPopup = e.NameScope.Find<Popup>("PART_OverflowPopup");

        if (_menuIndicator is not null)
        {
            _menuIndicator.Click += HandleMenuIndicatorClicked;
        }
        if (_overflowPopup is not null)
        {
            _overflowPopup.Closed += HandlePopupClosed;
            _overflowPopup.PlacementTarget = _menuIndicator;
            // Presses on the indicator must bypass the light-dismiss layer: without this
            // the layer treats an open popup's indicator press as an outside press (the
            // dismiss closes the popup, then the re-raised press reaches the button whose
            // Click on release reopens it). With the pass-through element the press is
            // delivered straight to the indicator and the popup stays open.
            _overflowPopup.OverlayInputPassThroughElement = _menuIndicator;
            _overflowPopup.SetCurrentValue(Popup.RequestedPlacementProperty, OverflowPopupPlacement);
        }

        SetupIndicatorsVisibility();
        TryOpenPinnedPopup();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        TryOpenPinnedPopup();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        FullTeardown(clearOwner: true);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == VerticalScrollBarVisibilityProperty ||
            change.Property == OffsetProperty ||
            change.Property == ExtentProperty ||
            change.Property == ViewportProperty)
        {
            SetupIndicatorsVisibility();
        }
        else if (change.Property == TabStripPlacementProperty)
        {
            UpdateOverflowPopupPlacement();
        }
        else if (change.Property == OverflowPopupTemplateProperty)
        {
            ReleasePopupContent();
        }
        else if (change.Property == IsPopupPinnedOpenProperty)
        {
            if (_overflowPopup is not null)
            {
                _overflowPopup.SetCurrentValue(Popup.IsPopupPinnedOpenProperty, change.GetNewValue<bool>());
            }
            if (change.GetNewValue<bool>())
            {
                TryOpenPinnedPopup();
            }
        }
        else if ((change.Property == IsEffectivelyEnabledProperty || change.Property == IsVisibleProperty) &&
                 IsPopupPinnedOpen)
        {
            TryOpenPinnedPopup();
        }
    }

    private void HandleMenuIndicatorClicked(object? sender, RoutedEventArgs e)
    {
        OpenOverflowPopup();
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        CompleteOrdinaryClose();
    }

    private void HandleOverflowItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ClosePopupCore(forceLifecycleClose: true);
    }

    private void HandleOverflowSelectionChanged(object? sender, EventArgs e)
    {
        if (!IsOverflowPopupOpen || _overflowOwner is not { } owner || OverflowPopupContext is null)
        {
            return;
        }

        var snapshot = BuildOverflowSnapshot(owner);
        if (snapshot.Items.Count == 0)
        {
            ClosePopupCore(forceLifecycleClose: true);
            return;
        }

        OverflowPopupContext.Publish(snapshot.Items, snapshot.SelectedItem);
    }

    private OverflowSnapshot BuildOverflowSnapshot(ITabOverflowOwner owner)
    {
        var overflowCount = 0;
        for (var index = 0; index < owner.OverflowItemCount; index++)
        {
            if (owner.GetOverflowContainer(index) is { } container && IsOverflowed(container.Bounds))
            {
                overflowCount++;
            }
        }

        _overflowEntries.Clear();
        if (overflowCount == 0)
        {
            return new OverflowSnapshot(Array.Empty<TabOverflowItem>(), null);
        }

        var items = new TabOverflowItem[overflowCount];
        TabOverflowItem? selectedItem = null;
        var targetIndex = 0;
        for (var index = 0; index < owner.OverflowItemCount; index++)
        {
            if (owner.GetOverflowContainer(index) is not { } container || !IsOverflowed(container.Bounds))
            {
                continue;
            }

            var logicalItem = owner.GetOverflowLogicalItem(index);
            var item = owner.CreateOverflowItem(index, logicalItem, container);
            items[targetIndex++] = item;
            _overflowEntries.Add(item, new OverflowEntry(index, logicalItem, container));
            if (item.IsSelected)
            {
                selectedItem = item;
            }
        }

        return new OverflowSnapshot(Array.AsReadOnly(items), selectedItem);
    }

    private bool IsOverflowed(Rect itemBounds)
    {
        if (TabStripPlacement is Dock.Top or Dock.Bottom)
        {
            var left = Math.Floor(itemBounds.Left - Offset.X);
            var right = Math.Floor(itemBounds.Right - Offset.X);
            return left < 0 || right > Viewport.Width;
        }

        var top = Math.Floor(itemBounds.Top - Offset.Y);
        var bottom = Math.Floor(itemBounds.Bottom - Offset.Y);
        return top < 0 || bottom > Viewport.Height;
    }

    private bool TryGetCurrentEntry(
        long sessionId,
        TabOverflowItem item,
        out ITabOverflowOwner owner,
        out OverflowEntry entry)
    {
        owner = null!;
        entry = default;
        return sessionId != 0 &&
               sessionId == _openSessionId &&
               IsOverflowPopupOpen &&
               _overflowOwner is { } currentOwner &&
               _overflowEntries.TryGetValue(item, out entry) &&
               (owner = currentOwner) is not null;
    }

    private bool IsCurrentSession(long sessionId, ITabOverflowOwner owner, TabOverflowPopupContext context)
    {
        return _openSessionId == sessionId &&
               ReferenceEquals(_overflowOwner, owner) &&
               ReferenceEquals(OverflowPopupContext, context);
    }

    private bool EnsurePopupContent(long sessionId, ITabOverflowOwner owner, TabOverflowPopupContext context)
    {
        if (_overflowPopup is not { } popup)
        {
            return false;
        }
        if (_overflowPopupPresenter is not null)
        {
            return true;
        }

        var template = OverflowPopupTemplate ?? FindDefaultPopupTemplate();
        // Build application content before attachment. A callback may dismiss the
        // session or replace/detach its owner without mutating an attaching tree.
        var root = template.Build(context);
        if (!IsCurrentSession(sessionId, owner, context))
        {
            return false;
        }

        var presenter = new ContentPresenter
        {
            Name = "PART_OverflowPopupContentPresenter",
            Content = context,
            ContentTemplate = new PreparedPopupTemplate(root)
        };
        try
        {
            presenter.UpdateChild();
            if (!IsCurrentSession(sessionId, owner, context))
            {
                return false;
            }

            _overflowPopupPresenter = presenter;
            popup.Child = presenter;
            return IsCurrentSession(sessionId, owner, context);
        }
        finally
        {
            if (!ReferenceEquals(_overflowPopupPresenter, presenter))
            {
                presenter.Content = null;
                presenter.ContentTemplate = null;
                presenter.UpdateChild();
            }
        }
    }

    private IDataTemplate FindDefaultPopupTemplate()
    {
        if (this.TryFindResource(DefaultPopupTemplateResourceKey, out var resource) &&
            resource is IDataTemplate template)
        {
            return template;
        }

        throw new InvalidOperationException(
            $"The required {DefaultPopupTemplateResourceKey} resource was not registered.");
    }

    private void SubscribeOwnerChanges(ITabOverflowOwner owner)
    {
        if (_isOwnerChangeSubscribed)
        {
            return;
        }

        owner.OverflowItems.CollectionChanged += _overflowItemsChangedHandler;
        owner.OverflowSelectionChanged += _overflowSelectionChangedHandler;
        _isOwnerChangeSubscribed = true;
    }

    private void UnsubscribeOwnerChanges()
    {
        if (!_isOwnerChangeSubscribed || _overflowOwner is not { } owner)
        {
            _isOwnerChangeSubscribed = false;
            return;
        }

        owner.OverflowItems.CollectionChanged -= _overflowItemsChangedHandler;
        owner.OverflowSelectionChanged -= _overflowSelectionChangedHandler;
        _isOwnerChangeSubscribed = false;
    }

    private void ClosePopupCore(bool forceLifecycleClose)
    {
        if (forceLifecycleClose)
        {
            var wasClosing = _openSessionId == ClosingSessionId;
            _openSessionId = ClosingSessionId;
            try
            {
                _overflowPopup?.CloseForLifecycle();
                CompleteOrdinaryClose();
            }
            finally
            {
                if (!wasClosing && _openSessionId == ClosingSessionId)
                {
                    _openSessionId = 0;
                }
            }
            return;
        }

        if (_overflowPopup is { } popup)
        {
            popup.Close();
            if (popup.IsOpen)
            {
                return;
            }
        }

        CompleteOrdinaryClose();
    }

    private void CompleteOrdinaryClose()
    {
        var wasClosing = _openSessionId == ClosingSessionId;
        _openSessionId = ClosingSessionId;
        try
        {
            UnsubscribeOwnerChanges();
            _overflowEntries.Clear();
            IsOverflowPopupOpen = false;
            OverflowPopupContext?.CloseSession();
        }
        finally
        {
            if (!wasClosing && _openSessionId == ClosingSessionId)
            {
                _openSessionId = 0;
            }
        }
    }

    private void ReleasePopupContent()
    {
        var wasClosing = _openSessionId == ClosingSessionId;
        _openSessionId = ClosingSessionId;
        try
        {
            ClosePopupCore(forceLifecycleClose: true);
            if (_overflowPopup is not null)
            {
                _overflowPopup.Child = null;
            }
            if (_overflowPopupPresenter is not null)
            {
                _overflowPopupPresenter.Content = null;
                _overflowPopupPresenter.ContentTemplate = null;
            }
            _overflowPopupPresenter = null;
            OverflowPopupContext = null;
        }
        finally
        {
            if (!wasClosing && _openSessionId == ClosingSessionId)
            {
                _openSessionId = 0;
            }
        }
    }

    private void FullTeardown(bool clearOwner)
    {
        ReleasePopupContent();
        if (clearOwner)
        {
            _overflowOwner = null;
        }
    }

    private void ReleaseTemplateParts()
    {
        ReleasePopupContent();
        if (_menuIndicator is not null)
        {
            _menuIndicator.Click -= HandleMenuIndicatorClicked;
        }
        if (_overflowPopup is not null)
        {
            _overflowPopup.Closed -= HandlePopupClosed;
            _overflowPopup.PlacementTarget = null;
            _overflowPopup.OverlayInputPassThroughElement = null;
        }
        _menuIndicator = null;
        _startEdgeIndicator = null;
        _endEdgeIndicator = null;
        _overflowPopup = null;
    }

    private void TryOpenPinnedPopup()
    {
        if (IsPopupPinnedOpen &&
            !IsOverflowPopupOpen &&
            this.IsAttachedToVisualTree() &&
            IsEffectivelyEnabled &&
            IsVisible &&
            _menuIndicator is { IsEffectivelyEnabled: true, IsVisible: true } indicator &&
            indicator.IsAttachedToVisualTree() &&
            TopLevel.GetTopLevel(indicator) is not null)
        {
            OpenOverflowPopup();
        }
    }

    private long NextSessionId()
    {
        _nextSessionId++;
        if (_nextSessionId <= 0)
        {
            _nextSessionId = 1;
        }
        return _nextSessionId;
    }

    private void SetupIndicatorsVisibility()
    {
        bool startVisible;
        bool endVisible;
        if (TabStripPlacement is Dock.Top or Dock.Bottom)
        {
            startVisible = CalculateEdgeIndicatorVisibility(Offset.X, Extent.Width, Viewport.Width, 0d);
            endVisible = CalculateEdgeIndicatorVisibility(Offset.X, Extent.Width, Viewport.Width, 100d);
        }
        else
        {
            startVisible = CalculateEdgeIndicatorVisibility(Offset.Y, Extent.Height, Viewport.Height, 0d);
            endVisible = CalculateEdgeIndicatorVisibility(Offset.Y, Extent.Height, Viewport.Height, 100d);
        }

        if (_startEdgeIndicator is not null)
        {
            _startEdgeIndicator.IsVisible = startVisible;
        }
        if (_endEdgeIndicator is not null)
        {
            _endEdgeIndicator.IsVisible = endVisible;
        }
        if (_menuIndicator is not null)
        {
            _menuIndicator.IsVisible = startVisible || endVisible;
        }

        TryOpenPinnedPopup();
    }

    private void UpdateOverflowPopupPlacement()
    {
        OverflowPopupPlacement = TabStripPlacement switch
        {
            Dock.Top => PlacementMode.BottomEdgeAlignedLeft,
            Dock.Bottom => PlacementMode.TopEdgeAlignedLeft,
            Dock.Right => PlacementMode.LeftEdgeAlignedBottom,
            _ => PlacementMode.RightEdgeAlignedBottom
        };
        if (_overflowPopup is not null)
        {
            _overflowPopup.SetCurrentValue(Popup.RequestedPlacementProperty, OverflowPopupPlacement);
        }
    }

    private static bool CalculateEdgeIndicatorVisibility(
        double offset,
        double extent,
        double viewport,
        double target)
    {
        if (MathUtils.AreClose(extent, viewport))
        {
            return false;
        }

        var percent = Math.Clamp(offset * 100.0 / (extent - viewport), 0, 100);
        return !MathUtils.AreClose(percent, target);
    }

    // ContentPresenter resets its child creation state when attaching. Preserve its
    // DataContext semantics while keeping the application template a one-time build.
    private sealed class PreparedPopupTemplate(Control? root) : IDataTemplate
    {
        public bool Match(object? data) => data is TabOverflowPopupContext;

        public Control? Build(object? param) => root;
    }

    private readonly record struct OverflowEntry(int Index, object? LogicalItem, Control Container);
    private readonly record struct OverflowSnapshot(
        IReadOnlyList<TabOverflowItem> Items,
        TabOverflowItem? SelectedItem);
}
