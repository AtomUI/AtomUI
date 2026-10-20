using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using AtomTabControl = AtomUI.Desktop.Controls.TabControl;
using AtomTabItem = AtomUI.Desktop.Controls.TabItem;

namespace AtomUIGallery.ShowCases.TabControl;

public class MyTabItemData : TabItemData
{
    public object? Content { get; init; }
}

public partial class TabControlShowCase : GalleryReactiveUserControl<TabControlViewModel>
{
    public const string LanguageId = nameof(TabControlShowCase);

    private readonly List<WeakReference<CardTabControl>> _dynamicAddTabControls = [];
    private ILanguageManager? _subscribedLanguageManager;
    private EventHandler<LanguageChangedEventArgs>? _languageChangedHandler;

    public TabControlShowCase()
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
        _dynamicAddTabControls.Clear();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        RefreshViewModelData();
    }

    private void HandlePlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is TabControlViewModel viewModel)
        {
            viewModel.HandlePlacementOptionCheckedChanged(sender, args);
        }
    }

    private void HandleCardPlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is TabControlViewModel viewModel)
        {
            viewModel.HandleCardPlacementOptionCheckedChanged(sender, args);
        }
    }

    private void HandleReorderPlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is TabControlViewModel viewModel)
        {
            viewModel.HandleReorderPlacementOptionCheckedChanged(sender, args);
        }
    }

    private void HandleSizeTypeOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is TabControlViewModel viewModel)
        {
            viewModel.HandleSizeTypeOptionCheckedChanged(sender, args);
        }
    }

    private void HandleAddTabRequest(object? sender, RoutedEventArgs args)
    {
        if (sender is not CardTabControl tabControl)
        {
            return;
        }

        TrackDynamicAddTabControl(tabControl);
        var index = tabControl.ItemCount;
        tabControl.Items.Add(new AtomTabItem
        {
            Header     = Format(TabControlShowCaseLangResourceKind.P2HeaderNewTabFormat, "new tab {0}", index),
            Content    = Format(TabControlShowCaseLangResourceKind.P2ContentNewTabContentFormat, "new tab content {0}", index),
            IsClosable = true,
            Tag        = index
        });
    }

    private static void HandleSemanticStyleDemoTabControlInitialized(object? sender, EventArgs e)
    {
        if (sender is not AtomTabControl tabControl)
        {
            return;
        }

        // The selected ink bar is not a public Semantic Part; its color and thickness
        // are customized through the TabControl InkBarColor and InkBarThickness tokens
        // (see semantic-part.md §5).
        tabControl.Resources[TabControlTokenKind.InkBarColor]     = Color.FromArgb(0x4D, 0xFF, 0x4D, 0x4F);
        tabControl.Resources[TabControlTokenKind.InkBarThickness] = 4.0;
    }

    private void TrackDynamicAddTabControl(CardTabControl tabControl)
    {
        foreach (var reference in _dynamicAddTabControls)
        {
            if (reference.TryGetTarget(out var tracked) &&
                ReferenceEquals(tracked, tabControl))
            {
                return;
            }
        }

        _dynamicAddTabControls.Add(new WeakReference<CardTabControl>(tabControl));
    }

    private void RefreshViewModelData()
    {
        if (DataContext is TabControlViewModel viewModel)
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

    private static void RefreshItemsSourceData(TabControlViewModel viewModel)
    {
        viewModel.TabItemDataSource.Clear();
        viewModel.TabItemDataSource.Add(new MyTabItemData()
        {
            Header  = Lang(TabControlShowCaseLangResourceKind.P2HeaderTabN1, "Tab 1"),
            Content = Lang(TabControlShowCaseLangResourceKind.P2ContentDynamicTabContentN1, "Tab Content 1"),
            Icon    = new WechatFilled()
        });

        viewModel.TabItemDataSource.Add(new MyTabItemData()
        {
            Header     = Lang(TabControlShowCaseLangResourceKind.P2HeaderTabN2, "Tab 2"),
            Content    = Lang(TabControlShowCaseLangResourceKind.P2ContentDynamicTabContentN2, "Tab Content 2"),
            IsClosable = true,
            Icon       = new LinuxOutlined()
        });
    }

    private void RefreshDynamicAddedTabs()
    {
        for (var i = _dynamicAddTabControls.Count - 1; i >= 0; i--)
        {
            if (!_dynamicAddTabControls[i].TryGetTarget(out var tabControl))
            {
                _dynamicAddTabControls.RemoveAt(i);
                continue;
            }

            foreach (var item in tabControl.Items)
            {
                if (item is AtomTabItem { Tag: int index } tabItem)
                {
                    tabItem.Header  = Format(TabControlShowCaseLangResourceKind.P2HeaderNewTabFormat, "new tab {0}", index);
                    tabItem.Content = Format(TabControlShowCaseLangResourceKind.P2ContentNewTabContentFormat, "new tab content {0}", index);
                }
            }
        }
    }

    private static string Lang(TabControlShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }

    private static string Format(TabControlShowCaseLangResourceKind resourceKind, string fallback, params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }
}
