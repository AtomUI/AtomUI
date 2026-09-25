using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.SourceCode;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Toolkits.GalleryBase.Controls;

public enum ShowCaseItemSpan
{
    Auto,
    Full
}

public class ShowCaseItem : ContentControl
{
    private const string ShowSourceButtonPart = "PART_ShowSourceButton";
    private const string ZoomButtonPart        = "PART_ZoomButton";

    public static readonly RoutedEvent<ShowCaseSourceCodeRequestedEventArgs> SourceCodeRequestedEvent =
        RoutedEvent.Register<ShowCaseItem, ShowCaseSourceCodeRequestedEventArgs>(
            nameof(SourceCodeRequested),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<ShowCaseZoomRequestedEventArgs> ZoomRequestedEvent =
        RoutedEvent.Register<ShowCaseItem, ShowCaseZoomRequestedEventArgs>(
            nameof(ZoomRequested),
            RoutingStrategies.Bubble);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ShowCaseItem, string>(nameof(Title));

    public static readonly StyledProperty<bool> IsZoomEnabledProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsZoomEnabled), true);

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<ShowCaseItem, string>(nameof(Description));

    public static readonly StyledProperty<bool> IsOccupyEntireRowProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsOccupyEntireRow));

    public static readonly StyledProperty<ShowCaseItemSpan> SpanProperty =
        AvaloniaProperty.Register<ShowCaseItem, ShowCaseItemSpan>(nameof(Span));

    internal static readonly StyledProperty<bool> IsFakeProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsFake), false);

    public static readonly StyledProperty<bool> IsDeferredContentEnabledProperty =
        AvaloniaProperty.Register<ShowCaseItem, bool>(nameof(IsDeferredContentEnabled), false);

    public static readonly StyledProperty<IDataTemplate?> DeferredContentTemplateProperty =
        AvaloniaProperty.Register<ShowCaseItem, IDataTemplate?>(nameof(DeferredContentTemplate));

    public static readonly StyledProperty<object?> DeferredContentProperty =
        AvaloniaProperty.Register<ShowCaseItem, object?>(nameof(DeferredContent));

    public static readonly StyledProperty<double> DeferredPlaceholderHeightProperty =
        AvaloniaProperty.Register<ShowCaseItem, double>(nameof(DeferredPlaceholderHeight), 160);

    public static readonly StyledProperty<string?> BadgeTextProperty =
        AvaloniaProperty.Register<ShowCaseItem, string?>(nameof(BadgeText));

    public static readonly StyledProperty<string?> BadgeColorProperty =
        AvaloniaProperty.Register<ShowCaseItem, string?>(nameof(BadgeColor), "blue");

    public static readonly StyledProperty<string?> SourceKeyProperty =
        AvaloniaProperty.Register<ShowCaseItem, string?>(nameof(SourceKey));

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

    public event EventHandler<ShowCaseSourceCodeRequestedEventArgs> SourceCodeRequested
    {
        add => AddHandler(SourceCodeRequestedEvent, value);
        remove => RemoveHandler(SourceCodeRequestedEvent, value);
    }

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

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public bool IsOccupyEntireRow
    {
        get => GetValue(IsOccupyEntireRowProperty);
        set => SetValue(IsOccupyEntireRowProperty, value);
    }

    public bool IsZoomEnabled
    {
        get => GetValue(IsZoomEnabledProperty);
        set => SetValue(IsZoomEnabledProperty, value);
    }

    public ShowCaseItemSpan Span
    {
        get => GetValue(SpanProperty);
        set => SetValue(SpanProperty, value);
    }

    public bool IsFake
    {
        get => GetValue(IsFakeProperty);
        set => SetValue(IsFakeProperty, value);
    }

    public bool IsDeferredContentEnabled
    {
        get => GetValue(IsDeferredContentEnabledProperty);
        set => SetValue(IsDeferredContentEnabledProperty, value);
    }

    public IDataTemplate? DeferredContentTemplate
    {
        get => GetValue(DeferredContentTemplateProperty);
        set => SetValue(DeferredContentTemplateProperty, value);
    }

    public object? DeferredContent
    {
        get => GetValue(DeferredContentProperty);
        set => SetValue(DeferredContentProperty, value);
    }

    public double DeferredPlaceholderHeight
    {
        get => GetValue(DeferredPlaceholderHeightProperty);
        set => SetValue(DeferredPlaceholderHeightProperty, value);
    }

    public string? BadgeText
    {
        get => GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    public string? BadgeColor
    {
        get => GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }

    public string? SourceKey
    {
        get => GetValue(SourceKeyProperty);
        set => SetValue(SourceKeyProperty, value);
    }

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
        if (content is StyledElement styledElement)
        {
            styledElement.DataContext = data;
        }

        SetCurrentValue(ContentProperty, content);
        IsDeferredContentMaterialized = true;
    }

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateCodeActionVisibility();
        UpdateZoomActionVisibility();
    }

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
