using AtomUI.Toolkits.GalleryBase.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using AtomUITabStrip = AtomUI.Desktop.Controls.TabStrip;
using AtomUITabStripItem = AtomUI.Desktop.Controls.TabStripItem;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Tabs of a <see cref="GalleryShowCaseHost" /> page.
/// </summary>
public enum GalleryShowCaseTab
{
    /// <summary>
    /// The tab that shows <see cref="GalleryShowCaseHost.ExamplesContent" />. It is the default tab.
    /// </summary>
    Examples,
    /// <summary>
    /// The tab that shows the Semantic Parts previews built from
    /// <see cref="GalleryShowCaseHost.SemanticPartsContentTemplate" />. It is reachable only while that
    /// template is set.
    /// </summary>
    SemanticParts
}

/// <summary>
/// Host of a Gallery showcase page. It lays out a header, a sticky tab strip and two content tabs,
/// Examples and Semantic Parts, on top of a <see cref="GalleryStickyTabsHost" />. The Semantic Parts
/// tab exists only while <see cref="SemanticPartsContentTemplate" /> is set.
/// </summary>
public class GalleryShowCaseHost : TemplatedControl
{
    private const string ExamplesContentHostPart   = "PART_ExamplesContentHost";
    private const string SemanticPartsContentHostPart = "PART_SemanticPartsContentHost";
    private const string StickyHostPart             = "PART_StickyHost";

    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<GalleryShowCaseHost, object?>(nameof(Header));

    /// <summary>
    /// Identifies the <see cref="ExamplesContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ExamplesContentProperty =
        AvaloniaProperty.Register<GalleryShowCaseHost, object?>(nameof(ExamplesContent));

    /// <summary>
    /// Identifies the <see cref="SemanticPartsContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SemanticPartsContentTemplateProperty =
        AvaloniaProperty.Register<GalleryShowCaseHost, IDataTemplate?>(nameof(SemanticPartsContentTemplate));

    /// <summary>
    /// Identifies the <see cref="SelectedTab" /> property.
    /// </summary>
    public static readonly StyledProperty<GalleryShowCaseTab> SelectedTabProperty =
        AvaloniaProperty.Register<GalleryShowCaseHost, GalleryShowCaseTab>(
            nameof(SelectedTab),
            GalleryShowCaseTab.Examples);

    /// <summary>
    /// Identifies the <see cref="StickyContentPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> StickyContentPaddingProperty =
        GalleryStickyTabsHost.StickyContentPaddingProperty.AddOwner<GalleryShowCaseHost>();

    internal static readonly DirectProperty<GalleryShowCaseHost, object?> NavigationContentProperty =
        AvaloniaProperty.RegisterDirect<GalleryShowCaseHost, object?>(
            nameof(NavigationContent),
            host => host.NavigationContent);

    /// <summary>
    /// Identifies the <see cref="SemanticPartsContent" /> property.
    /// </summary>
    public static readonly DirectProperty<GalleryShowCaseHost, Control?> SemanticPartsContentProperty =
        AvaloniaProperty.RegisterDirect<GalleryShowCaseHost, Control?>(
            nameof(SemanticPartsContent),
            host => host.SemanticPartsContent);

    internal static readonly DirectProperty<GalleryShowCaseHost, bool> HasSemanticPartsProperty =
        AvaloniaProperty.RegisterDirect<GalleryShowCaseHost, bool>(
            nameof(HasSemanticParts),
            host => host.HasSemanticParts);

    internal static readonly DirectProperty<GalleryShowCaseHost, bool> IsSemanticPartsContentMaterializedProperty =
        AvaloniaProperty.RegisterDirect<GalleryShowCaseHost, bool>(
            nameof(IsSemanticPartsContentMaterialized),
            host => host.IsSemanticPartsContentMaterialized);

    internal static readonly DirectProperty<GalleryShowCaseHost, bool> IsSemanticPartsContentHeightBoundedProperty =
        AvaloniaProperty.RegisterDirect<GalleryShowCaseHost, bool>(
            nameof(IsSemanticPartsContentHeightBounded),
            host => host.IsSemanticPartsContentHeightBounded);

    internal static readonly DirectProperty<GalleryShowCaseHost, double> SemanticPartsContentMinHeightProperty =
        AvaloniaProperty.RegisterDirect<GalleryShowCaseHost, double>(
            nameof(SemanticPartsContentMinHeight),
            host => host.SemanticPartsContentMinHeight);

    private AtomUITabStrip? _tabStrip;
    private ContentPresenter? _examplesContentHost;
    private ContentPresenter? _semanticPartsContentHost;
    private GalleryStickyTabsHost? _stickyHost;
    private IDisposable? _contentMaxHeightSubscription;
    private Control? _semanticPartsContent;
    private IReadOnlyList<SemanticPartPreview> _semanticPartPreviews = Array.Empty<SemanticPartPreview>();
    private object? _navigationContent;
    private bool _hasSemanticParts;
    private bool _isSemanticPartsContentMaterialized;
    private bool _isSemanticPartsContentHeightBounded;
    private double _semanticPartsContentMinHeight;
    private bool _isSynchronizingSelection;

    /// <summary>
    /// Content shown above the sticky tab strip, normally a <see cref="GalleryShowCaseHeader" />.
    /// The host forwards it to the <see cref="GalleryStickyTabsHost" /> in its template.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Content of the Examples tab. As the XAML content property of the host, it takes the child elements
    /// written directly in the host. When the <c>DataContext</c> of the host changes, it is assigned to a
    /// <see cref="Control" /> value.
    /// </summary>
    [Content]
    public object? ExamplesContent
    {
        get => GetValue(ExamplesContentProperty);
        set => SetValue(ExamplesContentProperty, value);
    }

    /// <summary>
    /// Template that builds the content of the Semantic Parts tab. It is built once with the
    /// <c>DataContext</c> of the host when the tab is first shown, and the result must be a
    /// <see cref="Control" /> that contains at least one <see cref="SemanticPartPreview" />; otherwise an
    /// <see cref="InvalidOperationException" /> is thrown. While it is <see langword="null" /> the host
    /// builds no tab strip, and setting it to <see langword="null" /> discards the built content and
    /// resets <see cref="SelectedTab" /> to <see cref="GalleryShowCaseTab.Examples" />.
    /// </summary>
    public IDataTemplate? SemanticPartsContentTemplate
    {
        get => GetValue(SemanticPartsContentTemplateProperty);
        set => SetValue(SemanticPartsContentTemplateProperty, value);
    }

    /// <summary>
    /// Tab whose content is visible. The default is <see cref="GalleryShowCaseTab.Examples" />.
    /// Setting the value moves the selection of the tab strip, and selecting a tab in the strip sets this
    /// value. <see cref="GalleryShowCaseTab.SemanticParts" /> takes effect only while
    /// <see cref="SemanticPartsContentTemplate" /> is set.
    /// </summary>
    public GalleryShowCaseTab SelectedTab
    {
        get => GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    /// <summary>
    /// Insets the sticky area below the header, which holds the tab strip, from the edges of its surface.
    /// The host forwards the value to the <see cref="GalleryStickyTabsHost" /> in its template; the header
    /// and the tab contents are not affected.
    /// </summary>
    public Thickness StickyContentPadding
    {
        get => GetValue(StickyContentPaddingProperty);
        set => SetValue(StickyContentPaddingProperty, value);
    }

    internal object? NavigationContent
    {
        get => _navigationContent;
        private set => SetAndRaise(NavigationContentProperty, ref _navigationContent, value);
    }

    /// <summary>
    /// Content built from <see cref="SemanticPartsContentTemplate" /> for the Semantic Parts tab, or
    /// <see langword="null" /> until that tab is first shown. It is discarded and becomes
    /// <see langword="null" /> again when the template changes or the host is detached from the visual tree.
    /// </summary>
    public Control? SemanticPartsContent
    {
        get => _semanticPartsContent;
        private set => SetAndRaise(SemanticPartsContentProperty, ref _semanticPartsContent, value);
    }

    internal bool HasSemanticParts
    {
        get => _hasSemanticParts;
        private set => SetAndRaise(HasSemanticPartsProperty, ref _hasSemanticParts, value);
    }

    internal bool IsSemanticPartsContentMaterialized
    {
        get => _isSemanticPartsContentMaterialized;
        private set => SetAndRaise(
            IsSemanticPartsContentMaterializedProperty,
            ref _isSemanticPartsContentMaterialized,
            value);
    }

    internal bool IsSemanticPartsContentHeightBounded
    {
        get => _isSemanticPartsContentHeightBounded;
        private set => SetAndRaise(IsSemanticPartsContentHeightBoundedProperty, ref _isSemanticPartsContentHeightBounded, value);
    }

    /// <summary>
    /// Minimum usable height of the Semantic Parts content while its height is bounded: the minimum stage
    /// height of the preview plus the fixed vertical insets of the inspection panel, the same insets that
    /// the <c>PaneMaxHeight</c> conversion uses. When the viewport leaves less height than this, the clamp
    /// stays at this minimum and the page scrolls vertically instead of clipping the content.
    /// </summary>
    internal double SemanticPartsContentMinHeight
    {
        get => _semanticPartsContentMinHeight;
        private set => SetAndRaise(SemanticPartsContentMinHeightProperty, ref _semanticPartsContentMinHeight, value);
    }

    /// <summary>
    /// Called when a styled property of the host changes. A change of
    /// <see cref="SemanticPartsContentTemplate" /> discards the built Semantic Parts content and rebuilds
    /// the tab strip. A change of <see cref="ExamplesContent" /> or <see cref="SelectedTab" /> moves the
    /// tab strip selection, and each of these changes then updates the tab contents. A change of the
    /// <c>DataContext</c> is assigned to both tab contents. Call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SemanticPartsContentTemplateProperty)
        {
            ReleaseSemanticPartsContent();
            RebuildNavigation();
            UpdateTabContent();
        }
        else if (change.Property == ExamplesContentProperty ||
                 change.Property == SelectedTabProperty)
        {
            SynchronizeTabStripSelection();
            UpdateTabContent();
        }
        else if (change.Property == DataContextProperty)
        {
            SynchronizeContentDataContext();
        }
    }

    /// <summary>
    /// Called when the host is attached to the visual tree. It builds the tab strip when it is missing
    /// and a <see cref="SemanticPartsContentTemplate" /> is set, and then updates the tab contents.
    /// Call the base member first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_tabStrip is null && SemanticPartsContentTemplate is not null)
        {
            RebuildNavigation();
        }
        UpdateTabContent();
    }

    /// <summary>
    /// Called when the control template is applied. It looks up the sticky host and the two content
    /// presenters, forwards the <c>ContentMaxHeight</c> of the sticky host to the presenter of the
    /// Semantic Parts tab, and updates the tab contents. Call the base member first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _contentMaxHeightSubscription?.Dispose();
        _contentMaxHeightSubscription = null;
        _stickyHost = e.NameScope.Find<GalleryStickyTabsHost>(StickyHostPart);
        _examplesContentHost = e.NameScope.Find<ContentPresenter>(ExamplesContentHostPart);
        _semanticPartsContentHost = e.NameScope.Find<ContentPresenter>(SemanticPartsContentHostPart);
        if (_stickyHost is not null)
        {
            _contentMaxHeightSubscription = _stickyHost
                .GetObservable(GalleryStickyTabsHost.ContentMaxHeightProperty)
                .Subscribe(value =>
                {
                    if (_semanticPartsContentHost is not null)
                    {
                        _semanticPartsContentHost.MaxHeight = value;
                    }
                });
            if (_semanticPartsContentHost is not null)
            {
                _semanticPartsContentHost.MaxHeight = _stickyHost.ContentMaxHeight;
            }
        }

        UpdateTabContent();
    }

    /// <summary>
    /// Called when the host is detached from the visual tree. It discards the Semantic Parts content,
    /// releases the tab strip and the maximum height subscription, and then calls the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseSemanticPartsContent();
        ReleaseNavigation();
        _contentMaxHeightSubscription?.Dispose();
        _contentMaxHeightSubscription = null;
        _stickyHost = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void RebuildNavigation()
    {
        ReleaseNavigation();

        HasSemanticParts = SemanticPartsContentTemplate is not null;
        if (!HasSemanticParts)
        {
            if (SelectedTab != GalleryShowCaseTab.Examples)
            {
                SetCurrentValue(SelectedTabProperty, GalleryShowCaseTab.Examples);
            }
            return;
        }

        _tabStrip = new AtomUITabStrip
        {
            SelectedIndex = SelectedTab == GalleryShowCaseTab.SemanticParts ? 1 : 0,
            Items =
            {
                CreateLocalizedTabItem(SemanticPartPreviewLangResourceKind.ExamplesTabLabel),
                CreateLocalizedTabItem(SemanticPartPreviewLangResourceKind.SemanticPartsTabLabel)
            }
        };
        _tabStrip.SelectionChanged += HandleTabSelectionChanged;

        // The sticky host mirrors or moves the TabStrip out of the visual inheritance chain of the page.
        // Bind the text inheritance properties explicitly so that the pinned rendering and the inline
        // rendering use the same font.
        _tabStrip.Bind(TemplatedControl.FontFamilyProperty, this.GetObservable(TemplatedControl.FontFamilyProperty));
        _tabStrip.Bind(TemplatedControl.FontSizeProperty, this.GetObservable(TemplatedControl.FontSizeProperty));
        _tabStrip.Bind(TemplatedControl.FontWeightProperty, this.GetObservable(TemplatedControl.FontWeightProperty));

        NavigationContent = _tabStrip;
    }

    private void ReleaseNavigation()
    {
        if (_tabStrip is not null)
        {
            _tabStrip.SelectionChanged -= HandleTabSelectionChanged;
            _tabStrip = null;
        }

        NavigationContent = null;
    }

    private static AtomUITabStripItem CreateLocalizedTabItem(SemanticPartPreviewLangResourceKind resourceKind)
    {
        var item = new AtomUITabStripItem();
        item.Bind(ContentControl.ContentProperty, new DynamicResourceExtension(resourceKind));
        return item;
    }

    private void HandleTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isSynchronizingSelection || _tabStrip is null)
        {
            return;
        }

        SetCurrentValue(
            SelectedTabProperty,
            _tabStrip.SelectedIndex == 1
                ? GalleryShowCaseTab.SemanticParts
                : GalleryShowCaseTab.Examples);
    }

    private void SynchronizeTabStripSelection()
    {
        if (_tabStrip is null)
        {
            return;
        }

        var selectedIndex = SelectedTab == GalleryShowCaseTab.SemanticParts ? 1 : 0;
        if (_tabStrip.SelectedIndex == selectedIndex)
        {
            return;
        }

        _isSynchronizingSelection = true;
        try
        {
            _tabStrip.SelectedIndex = selectedIndex;
        }
        finally
        {
            _isSynchronizingSelection = false;
        }
    }

    private void UpdateTabContent()
    {
        var isSemanticTab = SelectedTab == GalleryShowCaseTab.SemanticParts && HasSemanticParts;
        if (isSemanticTab)
        {
            EnsureSemanticPartsContent();
            foreach (var preview in _semanticPartPreviews)
            {
                preview.ActivatePreview();
            }
        }
        else
        {
            foreach (var preview in _semanticPartPreviews)
            {
                preview.DeactivatePreview();
            }
        }

        // Both content slots stay mounted and a tab switch only flips their visibility. A subtree with
        // IsVisible=False leaves measure, arrange and render completely, which avoids the full relayout
        // caused by detaching and attaching the whole Examples tree and semantic parts tree on each switch.
        // The presenter rebinds the TemplatedParent of the content slots to GalleryStickyTabsHost, so a
        // TemplateBinding in the template cannot reach the properties of this host. The host drives them here.
        if (_examplesContentHost is not null)
        {
            _examplesContentHost.Content   = ExamplesContent;
            _examplesContentHost.IsVisible = !isSemanticTab;
        }

        if (_semanticPartsContentHost is not null)
        {
            _semanticPartsContentHost.Content   = SemanticPartsContent;
            _semanticPartsContentHost.IsVisible = isSemanticTab;
        }

        // The semantic tab content is bounded to the page viewport remainder only
        // when it materializes a single preview: a lone parts pane then fills the
        // remaining height and scrolls its cards internally. Templates that stack
        // several previews keep the content-sized layout so every preview stays
        // reachable through the page scroll.
        IsSemanticPartsContentHeightBounded =
            isSemanticTab && _semanticPartPreviews.Count == 1;

        // Lower bound of the height clamp. When the viewport leaves too little height, the content keeps this
        // minimum usable height and the page scrolls vertically instead. Without the clamp the lower bound is
        // 0, which leaves the content-sized layout untouched.
        SemanticPartsContentMinHeight = IsSemanticPartsContentHeightBounded
            ? _semanticPartPreviews[0].PreviewStageMinHeight
              + SemanticPartPaneMaxHeightConverter.InspectionPanelVerticalInsets
            : 0;
    }

    private Control? EnsureSemanticPartsContent()
    {
        if (_semanticPartsContent is not null)
        {
            SynchronizeContentDataContext();
            return _semanticPartsContent;
        }

        var template = SemanticPartsContentTemplate;
        if (template is null)
        {
            return null;
        }

        var content = template.Build(DataContext);
        if (content is null)
        {
            throw new InvalidOperationException(
                $"{nameof(SemanticPartsContentTemplate)} must build a Control containing at least one " +
                $"{nameof(SemanticPartPreview)}.");
        }

        var previews = content is SemanticPartPreview preview
            ? [preview]
            : content.GetVisualDescendants().OfType<SemanticPartPreview>().ToArray();
        if (previews.Length == 0)
        {
            throw new InvalidOperationException(
                $"{nameof(SemanticPartsContentTemplate)} must build a Control containing at least one " +
                $"{nameof(SemanticPartPreview)}.");
        }

        SemanticPartsContent = content;
        _semanticPartPreviews = previews;
        content.DataContext = DataContext;
        IsSemanticPartsContentMaterialized = true;
        return content;
    }

    private void SynchronizeContentDataContext()
    {
        if (ExamplesContent is Control examplesControl)
        {
            examplesControl.DataContext = DataContext;
        }

        if (_semanticPartsContent is not null)
        {
            _semanticPartsContent.DataContext = DataContext;
        }
    }

    private void ReleaseSemanticPartsContent()
    {
        if (SemanticPartsContent is null)
        {
            return;
        }

        foreach (var preview in _semanticPartPreviews)
        {
            preview.DeactivatePreview();
        }

        foreach (var preview in _semanticPartPreviews)
        {
            preview.Dispose();
        }
        if (_semanticPartsContentHost is not null)
        {
            _semanticPartsContentHost.Content = null;
        }

        SemanticPartsContent = null;
        _semanticPartPreviews = Array.Empty<SemanticPartPreview>();
        IsSemanticPartsContentMaterialized = false;
    }
}
