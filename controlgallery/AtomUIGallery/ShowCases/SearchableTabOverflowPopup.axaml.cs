using System.ComponentModel;
using System.Globalization;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AtomButton = AtomUI.Desktop.Controls.Button;

namespace AtomUIGallery.ShowCases;

public partial class SearchableTabOverflowPopup : UserControl
{
    public static readonly StyledProperty<TabOverflowPopupContext?> ContextProperty =
        AvaloniaProperty.Register<SearchableTabOverflowPopup, TabOverflowPopupContext?>(nameof(Context));

    public static readonly StyledProperty<string?> QueryProperty =
        AvaloniaProperty.Register<SearchableTabOverflowPopup, string?>(nameof(Query));

    public static readonly StyledProperty<string?> SearchPlaceholderProperty =
        AvaloniaProperty.Register<SearchableTabOverflowPopup, string?>(nameof(SearchPlaceholder));

    public static readonly StyledProperty<string?> EmptyTextProperty =
        AvaloniaProperty.Register<SearchableTabOverflowPopup, string?>(nameof(EmptyText));

    private static readonly DirectProperty<SearchableTabOverflowPopup, bool> HasMatchesProperty =
        AvaloniaProperty.RegisterDirect<SearchableTabOverflowPopup, bool>(
            nameof(HasMatches),
            control => control.HasMatches);

    private readonly AvaloniaList<TabOverflowItem> _filteredItems = [];
    private TabOverflowPopupContext? _subscribedContext;
    private bool _hasMatches;
    private bool _isAttached;

    public TabOverflowPopupContext? Context
    {
        get => GetValue(ContextProperty);
        set => SetValue(ContextProperty, value);
    }

    public string? Query
    {
        get => GetValue(QueryProperty);
        set => SetValue(QueryProperty, value);
    }

    public string? SearchPlaceholder
    {
        get => GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    public string? EmptyText
    {
        get => GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public IReadOnlyList<TabOverflowItem> FilteredItems => _filteredItems;

    public bool HasMatches
    {
        get => _hasMatches;
        private set => SetAndRaise(HasMatchesProperty, ref _hasMatches, value);
    }

    public SearchableTabOverflowPopup()
    {
        InitializeComponent();
        AddHandler(AtomButton.ClickEvent, HandleItemClick);
        AddHandler(KeyDownEvent, HandleKeyDown, RoutingStrategies.Tunnel);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        ReplaceContextSubscription(Context);
        RefreshFilter();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        ReplaceContextSubscription(null);
        _filteredItems.Clear();
        HasMatches = false;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContextProperty)
        {
            if (_isAttached)
            {
                ReplaceContextSubscription(Context);
                RefreshFilter();
            }
        }
        else if (change.Property == QueryProperty && _isAttached)
        {
            RefreshFilter();
        }
    }

    private void ReplaceContextSubscription(TabOverflowPopupContext? context)
    {
        if (ReferenceEquals(_subscribedContext, context))
        {
            return;
        }

        if (_subscribedContext is { } oldContext)
        {
            oldContext.PropertyChanged -= HandleContextPropertyChanged;
        }

        _subscribedContext = context;
        if (context is not null)
        {
            context.PropertyChanged += HandleContextPropertyChanged;
        }
    }

    private void HandleContextPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(TabOverflowPopupContext.Items) or
            nameof(TabOverflowPopupContext.SelectedItem))
        {
            RefreshFilter();
        }
    }

    private void RefreshFilter()
    {
        _filteredItems.Clear();

        var query = Query;
        if (Context is { } context)
        {
            foreach (var item in context.Items)
            {
                var header = Convert.ToString(item.Header, CultureInfo.CurrentCulture) ?? string.Empty;
                if (string.IsNullOrEmpty(query) ||
                    header.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    _filteredItems.Add(item);
                }
            }
        }

        HasMatches = _filteredItems.Count != 0;
    }

    private void HandleItemClick(object? sender, RoutedEventArgs args)
    {
        if (args.Source is not AtomButton { Tag: TabOverflowItem item } ||
            Context is not { } context)
        {
            return;
        }

        Query = string.Empty;
        if (context.TryActivate(item))
        {
            context.Dismiss();
        }
    }

    private void HandleKeyDown(object? sender, KeyEventArgs args)
    {
        if (args.Key == Key.Escape)
        {
            Context?.Dismiss();
            args.Handled = true;
            return;
        }

        var buttons = PART_Items.GetVisualDescendants()
            .OfType<AtomButton>()
            .Where(button => button.IsEffectivelyEnabled && button.Tag is TabOverflowItem)
            .ToArray();
        if (buttons.Length == 0)
        {
            return;
        }

        var targetIndex = -1;
        if (ReferenceEquals(args.Source, PART_SearchInput))
        {
            targetIndex = args.Key switch
            {
                Key.Down => 0,
                Key.Up   => buttons.Length - 1,
                _        => -1
            };
        }
        else if (args.Source is AtomButton focusedButton)
        {
            var currentIndex = Array.IndexOf(buttons, focusedButton);
            targetIndex = args.Key switch
            {
                Key.Down => Math.Min(currentIndex + 1, buttons.Length - 1),
                Key.Up   => Math.Max(currentIndex - 1, 0),
                Key.Home => 0,
                Key.End  => buttons.Length - 1,
                _        => -1
            };
        }

        if (targetIndex >= 0)
        {
            buttons[targetIndex].Focus();
            args.Handled = true;
        }
    }
}
