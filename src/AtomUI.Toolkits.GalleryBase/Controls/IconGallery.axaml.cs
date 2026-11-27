using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using AtomUI.Toolkits.GalleryBase.Models;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ScrollViewer = AtomUI.Desktop.Controls.ScrollViewer;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Lists the icons of the Ant Design icon package, sorted by name, in a searchable list.
/// Icon instances are created in batches: the first 96 at once, then 96 more whenever 240 or fewer pixels are
/// left to scroll. Typing in the search box filters the list by icon name.
/// The template supplies the scroll viewer <c>PART_ScrollViewer</c> and the search box <c>PART_SearchInput</c>.
/// </summary>
public class IconGallery : TemplatedControl
{
    private const int InitialIconLoadCount = 96;
    private const int IncrementalIconLoadCount = 96;
    private const double LoadMoreScrollThreshold = 240;
    private const string ScrollViewerPart = "PART_ScrollViewer";
    private const string SearchInputPart = "PART_SearchInput";
    private static readonly TimeSpan SearchDebounceDelay = TimeSpan.FromMilliseconds(120);

    /// <summary>
    /// Identifies the <see cref="IconThemeType" /> property.
    /// </summary>
    public static readonly StyledProperty<IconThemeType?> IconThemeTypeProperty =
        AvaloniaProperty.Register<IconGallery, IconThemeType?>(nameof(IconThemeType));

    /// <summary>
    /// Restricts the list to the icons of this theme; <see langword="null" /> lists the icons of every theme.
    /// The list is reloaded when this value changes while the control is attached to a visual tree.
    /// </summary>
    public IconThemeType? IconThemeType
    {
        get => GetValue(IconThemeTypeProperty);
        set => SetValue(IconThemeTypeProperty, value);
    }

    #region 内部属性定义

    internal static readonly StyledProperty<AvaloniaList<PackageIconItem>?> IconInfosProperty =
        AvaloniaProperty.Register<IconGallery, AvaloniaList<PackageIconItem>?>(nameof(IconInfos));

    internal AvaloniaList<PackageIconItem>? IconInfos
    {
        get => GetValue(IconInfosProperty);
        set => SetValue(IconInfosProperty, value);
    }

    #endregion

    private readonly List<PackageIconItem> _matchedIconInfos = new();
    private AvaloniaList<PackageIconItem> _activatedIconInfos = new();
    private ScrollViewer? _scrollViewer;
    private SearchEdit? _searchEdit;
    private DispatcherTimer? _searchDebounceTimer;
    private int _loadedIconCount;
    private bool _templateEventsAttached;

    private bool HasMoreIconInfos => _loadedIconCount < _matchedIconInfos.Count;

    /// <summary>
    /// Takes the scroll viewer and the search box of the applied template, subscribes to them and reloads the icons.
    /// The base method is called first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        DetachTemplateEvents();

        _scrollViewer = e.NameScope.Find<ScrollViewer>(ScrollViewerPart);
        _searchEdit   = e.NameScope.Find<SearchEdit>(SearchInputPart);
        AttachTemplateEvents();

        ReLoadIcons();
    }

    /// <summary>
    /// Subscribes to the scroll viewer and the search box when the control is attached to a visual tree, and loads
    /// the icons when the list is still empty. The base method is called first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachTemplateEvents();
        if (_scrollViewer != null &&
            _matchedIconInfos.Count == 0 &&
            (IconInfos is null || IconInfos.Count == 0))
        {
            ReLoadIcons();
        }
    }

    /// <summary>
    /// Unsubscribes from the scroll viewer and the search box and releases the created icons when the control is
    /// detached from a visual tree. The base method is called last.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DetachTemplateEvents();
        ResetActivatedIconInfos();
        _matchedIconInfos.Clear();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Reloads the icons when <see cref="IconThemeType" /> changes while the control is attached to a visual tree.
    /// The base method is called first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconThemeTypeProperty)
        {
            if (VisualRoot is not null)
            {
                ReloadIconsImmediately();
            }
        }
    }

    private void ReLoadIcons()
    {
        ResetActivatedIconInfos();
        _matchedIconInfos.Clear();

        var iconThemeType = IconThemeType;
        var targetIcons = AntDesignIconCatalog.GetIcons()
                                              .Where(info => iconThemeType is null || info.ThemeType == iconThemeType)
                                              .OrderBy(info => info.Name)
                                              .ToArray();

        var filter = _searchEdit?.Text?.Trim();
        foreach (var iconInfo in targetIcons)
        {
            if (!string.IsNullOrEmpty(filter) &&
                !iconInfo.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            _matchedIconInfos.Add(new PackageIconItem(
                iconInfo.Name,
                iconInfo.IconType,
                iconInfo.Creator));
        }

        LoadMoreIconInfos(InitialIconLoadCount);
    }

    private void HandleScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_scrollViewer == null || !HasMoreIconInfos)
        {
            return;
        }

        var remainingHeight = _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height - _scrollViewer.Offset.Y;
        if (remainingHeight <= LoadMoreScrollThreshold)
        {
            LoadMoreIconInfos(IncrementalIconLoadCount);
        }
    }

    private void HandleSearchRequested(object? sender, SearchRequestedEventArgs e)
    {
        ReloadIconsImmediately();
    }

    private void ReloadIconsImmediately()
    {
        _searchDebounceTimer?.Stop();
        ReLoadIcons();
    }

    private void HandleSearchTextChanged(object? sender, RoutedEventArgs e)
    {
        _searchDebounceTimer ??= CreateSearchDebounceTimer();
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    private DispatcherTimer CreateSearchDebounceTimer()
    {
        var timer = new DispatcherTimer
        {
            Interval = SearchDebounceDelay
        };
        timer.Tick += HandleSearchDebounceTick;
        return timer;
    }

    private void HandleSearchDebounceTick(object? sender, EventArgs e)
    {
        _searchDebounceTimer?.Stop();
        ReLoadIcons();
    }

    private void AttachTemplateEvents()
    {
        if (_templateEventsAttached)
        {
            return;
        }

        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += HandleScrollChanged;
        }

        if (_searchEdit != null)
        {
            _searchEdit.TextChanged     += HandleSearchTextChanged;
            _searchEdit.SearchRequested += HandleSearchRequested;
        }

        _templateEventsAttached = _scrollViewer != null || _searchEdit != null;
    }

    private void DetachTemplateEvents()
    {
        if (!_templateEventsAttached)
        {
            return;
        }

        _searchDebounceTimer?.Stop();

        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged -= HandleScrollChanged;
        }

        if (_searchEdit != null)
        {
            _searchEdit.TextChanged     -= HandleSearchTextChanged;
            _searchEdit.SearchRequested -= HandleSearchRequested;
        }

        _templateEventsAttached = false;
    }

    private void LoadMoreIconInfos(int count)
    {
        if (!HasMoreIconInfos)
        {
            return;
        }

        var targetCount = Math.Min(_loadedIconCount + count, _matchedIconInfos.Count);
        for (var i = _loadedIconCount; i < targetCount; i++)
        {
            var iconInfo = _matchedIconInfos[i];
            iconInfo.Icon = iconInfo.Creator?.Invoke();
            _activatedIconInfos.Add(iconInfo);
        }

        _loadedIconCount = targetCount;
    }

    private void ResetActivatedIconInfos()
    {
        foreach (var iconInfo in _activatedIconInfos)
        {
            iconInfo.Icon = null;
        }

        _loadedIconCount    = 0;
        _activatedIconInfos = new AvaloniaList<PackageIconItem>();
        SetCurrentValue(IconInfosProperty, _activatedIconInfos);
        if (_scrollViewer != null)
        {
            _scrollViewer.Offset = new Vector(0, 0);
        }
    }
}
