using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Interactivity;
using AtomTabStripItem = AtomUI.Desktop.Controls.TabStripItem;

namespace AtomUIGallery.ShowCases.TabStrip;

public partial class TabStripShowCase : GalleryReactiveUserControl<TabStripViewModel>
{
    public const string LanguageId = nameof(TabStripShowCase);

    private readonly List<WeakReference<CardTabStrip>> _dynamicAddTabStrips = [];
    private ILanguageManager? _subscribedLanguageManager;
    private EventHandler<LanguageChangedEventArgs>? _languageChangedHandler;

    public TabStripShowCase()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RefreshViewModelData();
        SubscribeLanguageChanged();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        UnsubscribeLanguageChanged();
        _dynamicAddTabStrips.Clear();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        RefreshViewModelData();
    }

    private void HandlePlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is TabStripViewModel viewModel)
        {
            viewModel.HandlePlacementOptionCheckedChanged(sender, args);
        }
    }

    private void HandleCardPlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is TabStripViewModel viewModel)
        {
            viewModel.HandleCardPlacementOptionCheckedChanged(sender, args);
        }
    }

    private void HandleReorderPlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is TabStripViewModel viewModel)
        {
            viewModel.HandleReorderPlacementOptionCheckedChanged(sender, args);
        }
    }

    private void HandleSizeTypeOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is TabStripViewModel viewModel)
        {
            viewModel.HandleSizeTypeOptionCheckedChanged(sender, args);
        }
    }

    private void HandleAddTabRequest(object? sender, RoutedEventArgs args)
    {
        if (sender is not CardTabStrip tabStrip)
        {
            return;
        }

        TrackDynamicAddTabStrip(tabStrip);
        var index = tabStrip.ItemCount;
        tabStrip.Items.Add(new AtomTabStripItem
        {
            Content    = Format(TabStripShowCaseLangResourceKind.P2ContentNewTabFormat, "new tab {0}", index),
            IsClosable = true,
            Tag        = index
        });
    }

    private void TrackDynamicAddTabStrip(CardTabStrip tabStrip)
    {
        foreach (var reference in _dynamicAddTabStrips)
        {
            if (reference.TryGetTarget(out var tracked) &&
                ReferenceEquals(tracked, tabStrip))
            {
                return;
            }
        }

        _dynamicAddTabStrips.Add(new WeakReference<CardTabStrip>(tabStrip));
    }

    private void RefreshViewModelData()
    {
        if (DataContext is TabStripViewModel viewModel)
        {
            RefreshItemsSourceData(viewModel);
        }
    }

    private void SubscribeLanguageChanged()
    {
        if (_subscribedLanguageManager is not null)
        {
            return;
        }

        _subscribedLanguageManager = GalleryLocalization.GetLanguageManager();
        if (_subscribedLanguageManager is null)
        {
            return;
        }

        _languageChangedHandler = (_, _) =>
        {
            RefreshViewModelData();
            RefreshDynamicAddedTabs();
        };
        _subscribedLanguageManager.LanguageChanged += _languageChangedHandler;
    }

    private void UnsubscribeLanguageChanged()
    {
        if (_subscribedLanguageManager is null || _languageChangedHandler is null)
        {
            return;
        }

        _subscribedLanguageManager.LanguageChanged -= _languageChangedHandler;
        _subscribedLanguageManager = null;
        _languageChangedHandler = null;
    }

    private static void RefreshItemsSourceData(TabStripViewModel viewModel)
    {
        viewModel.TabStripItemDataSource.Clear();
        viewModel.TabStripItemDataSource.Add(new TabItemData()
        {
            Header = Lang(TabStripShowCaseLangResourceKind.P2ContentTabN1, "Tab 1")
        });
        viewModel.TabStripItemDataSource.Add(new TabItemData()
        {
            Header = Lang(TabStripShowCaseLangResourceKind.P2ContentTabN2, "Tab 2")
        });
    }

    private void RefreshDynamicAddedTabs()
    {
        for (var i = _dynamicAddTabStrips.Count - 1; i >= 0; i--)
        {
            if (!_dynamicAddTabStrips[i].TryGetTarget(out var tabStrip))
            {
                _dynamicAddTabStrips.RemoveAt(i);
                continue;
            }

            foreach (var item in tabStrip.Items)
            {
                if (item is AtomTabStripItem { Tag: int index } tabStripItem)
                {
                    tabStripItem.Content = Format(TabStripShowCaseLangResourceKind.P2ContentNewTabFormat, "new tab {0}", index);
                }
            }
        }
    }

    private static string Lang(TabStripShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }

    private static string Format(TabStripShowCaseLangResourceKind resourceKind, string fallback, params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }
}
