using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.SourceCode;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Column span of a <see cref="ShowCaseItem" /> in <see cref="ShowCaseMasonryPanel" />.
/// </summary>
public enum ShowCaseItemSpan
{
    /// <summary>
    /// The item takes the width of one column of the masonry grid. This is the default value.
    /// </summary>
    Auto,
    /// <summary>
    /// The item takes the full width of the masonry grid, like <see cref="ShowCaseItem.IsOccupyEntireRow" />.
    /// </summary>
    Full
}

/// <summary>
/// How <see cref="ShowCaseZoomStagePanel" /> lays out the content of a zoomed <see cref="ShowCaseItem" />.
/// </summary>
public enum ShowCaseZoomContentLayout
{
    /// <summary>
    /// The content is measured with the width of the stage and centered at its desired size. This is the
    /// default value.
    /// </summary>
    Natural,
    /// <summary>
    /// The content is measured with the whole slot of the stage and arranged in that slot; its own size and
    /// alignment constraints still apply.
    /// </summary>
    Stretch
}

/// <summary>
/// One entry of a Gallery showcase page: a card that shows the <see cref="ContentControl.Content" /> above
/// a title row with the zoom and show-source buttons, followed by the description. <see cref="ShowCasePanel" />
/// arranges its items in a masonry grid and decides when their deferred content is built.
/// </summary>
public class ShowCaseItem : ContentControl
{
    private const string ShowSourceButtonPart = "PART_ShowSourceButton";
    private const string ZoomButtonPart        = "PART_ZoomButton";

    /// <summary>
    /// Identifies the <see cref="SourceCodeRequested" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<ShowCaseSourceCodeRequestedEventArgs> SourceCodeRequestedEvent =
        RoutedEvent.Register<ShowCaseItem, ShowCaseSourceCodeRequestedEventArgs>(
            nameof(SourceCodeRequested),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="ZoomRequested" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<ShowCaseZoomRequestedEventArgs> ZoomRequestedEvent =
        RoutedEvent.Register<ShowCaseItem, ShowCaseZoomRequestedEventArgs>(
            nameof(ZoomRequested),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ShowCaseItem, string>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="IsZoomEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsZoomEnabledProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsZoomEnabled), true);

    /// <summary>
    /// Identifies the <see cref="ZoomContentLayout" /> property.
    /// </summary>
    public static readonly StyledProperty<ShowCaseZoomContentLayout> ZoomContentLayoutProperty =
        AvaloniaProperty.Register<ShowCaseItem, ShowCaseZoomContentLayout>(nameof(ZoomContentLayout));

    /// <summary>
    /// Identifies the <see cref="Description" /> property.
    /// </summary>
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<ShowCaseItem, string>(nameof(Description));

    /// <summary>
    /// Identifies the <see cref="IsOccupyEntireRow" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOccupyEntireRowProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsOccupyEntireRow));

    /// <summary>
    /// Identifies the <see cref="Span" /> property.
    /// </summary>
    public static readonly StyledProperty<ShowCaseItemSpan> SpanProperty =
        AvaloniaProperty.Register<ShowCaseItem, ShowCaseItemSpan>(nameof(Span));

    internal static readonly StyledProperty<bool> IsFakeProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsFake), false);

    /// <summary>
    /// Identifies the <see cref="IsDeferredContentEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDeferredContentEnabledProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsDeferredContentEnabled), false);

    /// <summary>
    /// Identifies the <see cref="DeferredContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> DeferredContentTemplateProperty =
        AvaloniaProperty.Register<ShowCaseItem, IDataTemplate?>(nameof(DeferredContentTemplate));

    /// <summary>
    /// Identifies the <see cref="DeferredContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> DeferredContentProperty =
        AvaloniaProperty.Register<ShowCaseItem, object?>(nameof(DeferredContent));

    /// <summary>
    /// Identifies the <see cref="DeferredPlaceholderHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> DeferredPlaceholderHeightProperty =
        AvaloniaProperty.Register<ShowCaseItem, double>(nameof(DeferredPlaceholderHeight), 160);

    /// <summary>
    /// Identifies the <see cref="BadgeText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> BadgeTextProperty =
        AvaloniaProperty.Register<ShowCaseItem, string?>(nameof(BadgeText));

    /// <summary>
    /// Identifies the <see cref="BadgeColor" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> BadgeColorProperty =
        AvaloniaProperty.Register<ShowCaseItem, string?>(nameof(BadgeColor));

    /// <summary>
    /// Identifies the <see cref="SourceKey" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> SourceKeyProperty =
        AvaloniaProperty.Register<ShowCaseItem, string?>(nameof(SourceKey));

    /// <summary>
    /// Identifies the <see cref="IsDeferredContentMaterialized" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDeferredContentMaterializedProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsDeferredContentMaterialized), false);

    internal static readonly StyledProperty<bool> IsCodeActionVisibleProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsCodeActionVisible), false);

    internal static readonly StyledProperty<bool> IsZoomActionVisibleProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsZoomActionVisible), false);

    internal static readonly StyledProperty<bool> IsDeferredPlaceholderVisibleProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsDeferredPlaceholderVisible), false);

    internal static readonly StyledProperty<bool> IsBadgeVisibleProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsBadgeVisible), false);

    /// <summary>
    /// Raised when the show-source button is clicked and the source snippet of the item can be identified. That
    /// needs a named <see cref="ShowCasePanel" /> with the item as a direct child and a showcase view above it.
    /// The arguments carry the snippet key and the <see cref="Title" />. The event bubbles up from the item;
    /// the click is marked handled afterwards.
    /// </summary>
    public event EventHandler<ShowCaseSourceCodeRequestedEventArgs> SourceCodeRequested
    {
        add => AddHandler(SourceCodeRequestedEvent, value);
        remove => RemoveHandler(SourceCodeRequestedEvent, value);
    }

    /// <summary>
    /// Raised when the zoom button is clicked while <see cref="IsZoomEnabled" /> is <see langword="true" /> and
    /// zoom is enabled in the Gallery configuration. The deferred content is built first. The arguments carry
    /// this item, its <see cref="Title" /> and its <see cref="Description" />. The event bubbles up from the
    /// item; the click is marked handled afterwards.
    /// </summary>
    public event EventHandler<ShowCaseZoomRequestedEventArgs> ZoomRequested
    {
        add => AddHandler(ZoomRequestedEvent, value);
        remove => RemoveHandler(ZoomRequestedEvent, value);
    }

    public ShowCaseItem()
    {
        AddHandler(Button.ClickEvent, HandleButtonClick);
        UpdateCodeActionVisibility();
        UpdateZoomActionVisibility();
    }

    /// <summary>
    /// The title shown at the left of the button row, below the content. It is also carried by
    /// <see cref="SourceCodeRequested" /> and <see cref="ZoomRequested" />. The default is
    /// <see langword="null" />.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The description shown as wrapped text below the button row. It is also carried by
    /// <see cref="ZoomRequested" />. The default is <see langword="null" />.
    /// </summary>
    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> makes <see cref="ShowCaseMasonryPanel" /> give the item the full width of the
    /// grid instead of one column. This has the same effect as <see cref="Span" /> set to
    /// <see cref="ShowCaseItemSpan.Full" />. The default is <see langword="false" />.
    /// </summary>
    public bool IsOccupyEntireRow
    {
        get => GetValue(IsOccupyEntireRowProperty);
        set => SetValue(IsOccupyEntireRowProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> shows the zoom button and lets a click on it raise <see cref="ZoomRequested" />.
    /// The button is hidden when zoom is disabled in the Gallery configuration as well. The default is
    /// <see langword="true" />.
    /// </summary>
    public bool IsZoomEnabled
    {
        get => GetValue(IsZoomEnabledProperty);
        set => SetValue(IsZoomEnabledProperty, value);
    }

    /// <summary>
    /// How the zoom stage lays out the content of the item while it is zoomed; see
    /// <see cref="ShowCaseZoomContentLayout" />. The zoom overlay forwards the value to
    /// <see cref="ShowCaseZoomStagePanel" />. The default is <see cref="ShowCaseZoomContentLayout.Natural" />.
    /// </summary>
    public ShowCaseZoomContentLayout ZoomContentLayout
    {
        get => GetValue(ZoomContentLayoutProperty);
        set => SetValue(ZoomContentLayoutProperty, value);
    }

    /// <summary>
    /// Column span of the item in <see cref="ShowCaseMasonryPanel" />. The default is
    /// <see cref="ShowCaseItemSpan.Auto" />, one column; <see cref="ShowCaseItemSpan.Full" /> gives the item
    /// the full width of the grid, like <see cref="IsOccupyEntireRow" />.
    /// </summary>
    public ShowCaseItemSpan Span
    {
        get => GetValue(SpanProperty);
        set => SetValue(SpanProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> renders the item without card chrome: the card loses its background, border and
    /// shadow, and the title and the zoom button are hidden. The content, the show-source button and the
    /// description are kept. The default is <see langword="false" />.
    /// </summary>
    public bool IsFake
    {
        get => GetValue(IsFakeProperty);
        set => SetValue(IsFakeProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> keeps the content of the item unbuilt until <see cref="ShowCasePanel" /> brings
    /// the item near the viewport, a zoom is requested or <see cref="MaterializeDeferredContent" /> is called.
    /// This needs <see cref="DeferredContentTemplate" />; until then a skeleton placeholder of
    /// <see cref="DeferredPlaceholderHeight" /> is shown instead of the content. Deferred loading is skipped
    /// when <c>GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled</c> is <see langword="true" />. The
    /// default is <see langword="false" />.
    /// </summary>
    public bool IsDeferredContentEnabled
    {
        get => GetValue(IsDeferredContentEnabledProperty);
        set => SetValue(IsDeferredContentEnabledProperty, value);
    }

    /// <summary>
    /// The template that builds the deferred content. It takes effect only while
    /// <see cref="IsDeferredContentEnabled" /> is <see langword="true" />. The default is
    /// <see langword="null" />: nothing is built and no placeholder is shown.
    /// </summary>
    public IDataTemplate? DeferredContentTemplate
    {
        get => GetValue(DeferredContentTemplateProperty);
        set => SetValue(DeferredContentTemplateProperty, value);
    }

    /// <summary>
    /// The data given to <see cref="DeferredContentTemplate" /> when the deferred content is built. When it is
    /// <see langword="null" />, the <c>DataContext</c> of the item is used instead and the built content follows
    /// later changes of that <c>DataContext</c>. The default is <see langword="null" />.
    /// </summary>
    public object? DeferredContent
    {
        get => GetValue(DeferredContentProperty);
        set => SetValue(DeferredContentProperty, value);
    }

    /// <summary>
    /// The minimum height, in device-independent units, of the skeleton placeholder shown while the deferred
    /// content is not built yet. The theme sets it from the <c>DeferredPlaceholderHeight</c> token of the item;
    /// the registration default is 160.
    /// </summary>
    public double DeferredPlaceholderHeight
    {
        get => GetValue(DeferredPlaceholderHeightProperty);
        set => SetValue(DeferredPlaceholderHeightProperty, value);
    }

    /// <summary>
    /// The text of the ribbon badge on the card, typically a version such as <c>v6.0.8</c>.
    /// <see langword="null" />, empty or white space hides the badge. The default is <see langword="null" />.
    /// </summary>
    public string? BadgeText
    {
        get => GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    /// <summary>
    /// The color of the ribbon badge, forwarded to the badge as its ribbon color. The default is
    /// <see langword="null" />, which leaves the color to the ribbon badge itself.
    /// </summary>
    public string? BadgeColor
    {
        get => GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }

    /// <summary>
    /// Selects the source snippet group to show for this item when it has several. The key matches a group
    /// that declares the same key; <see langword="null" /> (the default) or an empty value matches a group
    /// that declares none.
    /// </summary>
    public string? SourceKey
    {
        get => GetValue(SourceKeyProperty);
        set => SetValue(SourceKeyProperty, value);
    }

    /// <summary>
    /// <see langword="true" /> once the deferred content has been built and assigned to
    /// <see cref="ContentControl.Content" />; <see langword="false" /> while the content is still deferred or
    /// deferred content is not enabled. <see cref="MaterializeDeferredContent" /> sets it; the setter is private.
    /// </summary>
    public bool IsDeferredContentMaterialized
    {
        get => GetValue(IsDeferredContentMaterializedProperty);
        private set => SetValue(IsDeferredContentMaterializedProperty, value);
    }

    internal bool IsCodeActionVisible
    {
        get => GetValue(IsCodeActionVisibleProperty);
        set => SetValue(IsCodeActionVisibleProperty, value);
    }

    internal bool IsZoomActionVisible
    {
        get => GetValue(IsZoomActionVisibleProperty);
        set => SetValue(IsZoomActionVisibleProperty, value);
    }

    internal bool IsDeferredPlaceholderVisible
    {
        get => GetValue(IsDeferredPlaceholderVisibleProperty);
        set => SetValue(IsDeferredPlaceholderVisibleProperty, value);
    }

    internal bool IsBadgeVisible
    {
        get => GetValue(IsBadgeVisibleProperty);
        set => SetValue(IsBadgeVisibleProperty, value);
    }

    /// <summary>
    /// Builds the deferred content from <see cref="DeferredContentTemplate" /> and assigns it to
    /// <see cref="ContentControl.Content" /> with <c>SetCurrentValue</c>. The template gets
    /// <see cref="DeferredContent" /> as data, or the <c>DataContext</c> of the item when that is
    /// <see langword="null" />; when the data is not <see langword="null" />, a built <c>StyledElement</c>
    /// gets it as its <c>DataContext</c>. <see cref="IsDeferredContentMaterialized" /> becomes
    /// <see langword="true" />. Nothing happens when the item is already materialized, when
    /// <see cref="IsDeferredContentEnabled" /> is <see langword="false" /> or when
    /// <see cref="DeferredContentTemplate" /> is <see langword="null" />.
    /// </summary>
    public void MaterializeDeferredContent()
    {
        if (IsDeferredContentMaterialized ||
            !IsDeferredContentEnabled ||
            DeferredContentTemplate is null)
        {
            return;
        }

        var data    = DeferredContent ?? DataContext;
        var content = DeferredContentTemplate.Build(data);
        if (data is not null && content is StyledElement styledElement)
        {
            styledElement.DataContext = data;
        }

        SetCurrentValue(ContentProperty, content);
        IsDeferredContentMaterialized = true;
    }

    /// <summary>
    /// Called when the <c>DataContext</c> of the item changes. The base member is called first, then the new
    /// value is forwarded to the materialized deferred content when that content was built without
    /// <see cref="DeferredContent" /> and is a <c>StyledElement</c>. Overrides must call the base member.
    /// </summary>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (IsDeferredContentMaterialized &&
            DeferredContent is null &&
            Content is StyledElement styledElement)
        {
            styledElement.DataContext = DataContext;
        }
    }

    /// <summary>
    /// Called when the item is attached to a visual tree. The base member is called first, then the visibility
    /// of the zoom and show-source buttons is updated from the current Gallery configuration. Overrides must
    /// call the base member.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateCodeActionVisibility();
        UpdateZoomActionVisibility();
    }

    /// <summary>
    /// Called when a property of the item changes. The base member is called first. A change of
    /// <see cref="IsDeferredContentEnabled" />, <see cref="DeferredContentTemplate" /> or
    /// <see cref="IsDeferredContentMaterialized" /> then builds the deferred content at once when
    /// <c>GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled</c> is <see langword="true" /> and updates
    /// the placeholder visibility; a change of <see cref="BadgeText" /> updates the badge visibility; a change
    /// of <see cref="IsZoomEnabled" /> updates the zoom button visibility. Overrides must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDeferredContentEnabledProperty ||
            change.Property == DeferredContentTemplateProperty ||
            change.Property == IsDeferredContentMaterializedProperty)
        {
            MaterializeDeferredContentIfDiagnosticsDisabled();
            UpdateDeferredPlaceholderVisibility();
        }

        if (change.Property == BadgeTextProperty)
        {
            UpdateBadgeVisibility();
        }

        if (change.Property == IsZoomEnabledProperty)
        {
            UpdateZoomActionVisibility();
        }
    }

    private void MaterializeDeferredContentIfDiagnosticsDisabled()
    {
        if (GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled)
        {
            MaterializeDeferredContent();
        }
    }

    private void UpdateDeferredPlaceholderVisibility()
    {
        IsDeferredPlaceholderVisible =
            IsDeferredContentEnabled &&
            !GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled &&
            DeferredContentTemplate is not null &&
            !IsDeferredContentMaterialized;
    }

    private void UpdateBadgeVisibility()
    {
        IsBadgeVisible = !string.IsNullOrWhiteSpace(BadgeText);
    }

    private void UpdateCodeActionVisibility()
    {
        IsCodeActionVisible = GalleryBaseConfigurationProvider.Current?.SourceCodeDisplay.CanShowSourceCode == true;
    }

    private void UpdateZoomActionVisibility()
    {
        IsZoomActionVisible = IsZoomEnabled &&
                              (GalleryBaseConfigurationProvider.Current?.ShowCaseZoom.IsEnabled ?? true);
    }

    private void HandleButtonClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Button button)
        {
            return;
        }

        if (button.Name == ZoomButtonPart)
        {
            HandleZoomButtonClick(button, e);
            return;
        }

        if (button.Name == ShowSourceButtonPart)
        {
            if (TryCreateSnippetKey(out var key))
            {
                RaiseEvent(new ShowCaseSourceCodeRequestedEventArgs(SourceCodeRequestedEvent, key, Title));
                e.Handled = true;
            }
        }
    }

    private void HandleZoomButtonClick(Button button, RoutedEventArgs e)
    {
        if (!IsZoomEnabled ||
            (GalleryBaseConfigurationProvider.Current?.ShowCaseZoom.IsEnabled ?? true) != true)
        {
            return;
        }

        MaterializeDeferredContent();
        RaiseEvent(new ShowCaseZoomRequestedEventArgs(
            ZoomRequestedEvent, this, Title, Description));
        e.Handled = true;
    }

    private bool TryCreateSnippetKey(out ShowCaseCodeSnippetKey key)
    {
        key = default;
        var panel = this.FindAncestorOfType<ShowCasePanel>();
        if (panel is null || string.IsNullOrWhiteSpace(panel.Name))
        {
            return false;
        }

        var itemIndex = panel.Children.OfType<ShowCaseItem>().ToList().IndexOf(this);
        if (itemIndex < 0)
        {
            return false;
        }

        var viewTypeName = ResolveViewTypeName();
        if (string.IsNullOrWhiteSpace(viewTypeName))
        {
            return false;
        }

        key = new ShowCaseCodeSnippetKey(viewTypeName, panel.Name!, itemIndex, SourceKey);
        return true;
    }

    private string? ResolveViewTypeName()
    {
        return this.GetVisualAncestors()
                   .OfType<UserControl>()
                   .Select(static control => control.GetType())
                   .FirstOrDefault(static type => type.Namespace?.Contains(".ShowCases.") == true)
                   ?.FullName;
    }
}
