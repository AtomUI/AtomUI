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

public enum GalleryShowCaseTab
{
    Examples,
    SemanticParts
}

public class GalleryShowCaseHost : TemplatedControl
{
    private const string ExamplesContentHostPart   = "PART_ExamplesContentHost";
    private const string SemanticPartsContentHostPart = "PART_SemanticPartsContentHost";
    private const string StickyHostPart             = "PART_StickyHost";

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<GalleryShowCaseHost, object?>(nameof(Header));

    public static readonly StyledProperty<object?> ExamplesContentProperty =
        AvaloniaProperty.Register<GalleryShowCaseHost, object?>(nameof(ExamplesContent));

    public static readonly StyledProperty<IDataTemplate?> SemanticPartsContentTemplateProperty =
        AvaloniaProperty.Register<GalleryShowCaseHost, IDataTemplate?>(nameof(SemanticPartsContentTemplate));

    public static readonly StyledProperty<GalleryShowCaseTab> SelectedTabProperty =
        AvaloniaProperty.Register<GalleryShowCaseHost, GalleryShowCaseTab>(
            nameof(SelectedTab),
            GalleryShowCaseTab.Examples);

    public static readonly StyledProperty<Thickness> StickyContentPaddingProperty =
        GalleryStickyTabsHost.StickyContentPaddingProperty.AddOwner<GalleryShowCaseHost>();

    internal static readonly DirectProperty<GalleryShowCaseHost, object?> NavigationContentProperty =
        AvaloniaProperty.RegisterDirect<GalleryShowCaseHost, object?>(
            nameof(NavigationContent),
            host => host.NavigationContent);

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

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    [Content]
    public object? ExamplesContent
    {
        get => GetValue(ExamplesContentProperty);
        set => SetValue(ExamplesContentProperty, value);
    }

    public IDataTemplate? SemanticPartsContentTemplate
    {
        get => GetValue(SemanticPartsContentTemplateProperty);
        set => SetValue(SemanticPartsContentTemplateProperty, value);
    }

    public GalleryShowCaseTab SelectedTab
    {
        get => GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

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
    /// 限高模式下语义内容的最小可用高度：舞台配置下限加检查面板固定上下内边距
    /// （与 PaneMaxHeight 换算共用同一组内边距）。视口剩余高度不足该值时，宿主
    /// 钳制保持在下限，页面出现垂直滚动条而不是裁切内容。
    /// </summary>
    internal double SemanticPartsContentMinHeight
    {
        get => _semanticPartsContentMinHeight;
        private set => SetAndRaise(SemanticPartsContentMinHeightProperty, ref _semanticPartsContentMinHeight, value);
    }

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_tabStrip is null && SemanticPartsContentTemplate is not null)
        {
            RebuildNavigation();
        }
        UpdateTabContent();
    }

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

        // TabStrip 会被 sticky 镜像/搬移呈现，离开页面视觉继承链；
        // 显式绑定文字继承属性，保证钉住呈现与内联呈现字号一致。
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

        // 双内容槽常驻挂载，tab 切换只翻转可见性：IsVisible=False 的子树
        // 完全退出 measure/arrange/render，避免整棵 Examples 树与语义树
        // 来回 detach/attach 造成的整树重布局卡顿。
        // 内容槽的 TemplatedParent 会被呈现器重绑定为 GalleryStickyTabsHost，
        // 模板内 TemplateBinding 解析不到宿主属性，因此这里直接驱动。
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

        // 限高钳制的下限：视口剩余高度不足时内容保持最小可用高度，由页面
        // 垂直滚动条兜底；未限高时下限为 0，不影响内容尺寸布局。
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
