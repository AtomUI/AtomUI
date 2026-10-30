using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IListItemData = AtomUI.Controls.Data.IListItemData;
using ListItemData = AtomUI.Controls.Data.ListItemData;

namespace AtomUIGallery.ShowCases.List;

public partial class ListShowCase : GalleryReactiveUserControl<ListViewModel>
{
    public const string LanguageId = nameof(ListShowCase);

    public ListShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is ListViewModel viewModel)
            {
                RefreshLocalizedListItems(viewModel);
                viewModel.SelectionMode            = SelectionMode.Single;
                viewModel.OrderedSortDescriptions  = [ListSortDescription.FromPath(nameof(IListItemData.Content))];

                var languageManager = GalleryLocalization.GetLanguageManager();
                if (languageManager != null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshLocalizedListItems(viewModel);
                    languageManager.LanguageChanged += handler;
                    Disposable.Create(() => languageManager.LanguageChanged -= handler)
                              .DisposeWith(disposables);
                }

                Disposable.Create(() =>
                {
                    viewModel.ListItems              = null;
                    viewModel.SelectionListItems     = null;
                    viewModel.GroupListItems         = null;
                    viewModel.ListItemsWidthDisabled = null;
                    viewModel.EmptyDemoItems         = null;
                    viewModel.FilteredGroupListItems = null;
                    viewModel.OrderedGroupListItems  = null;
                    viewModel.BasicListBoxItems      = null;
                    viewModel.PaginationListItems    = null;
                    viewModel.SemanticListItems      = null;
                    viewModel.SemanticListBoxItems   = null;
                    viewModel.OrderedSortDescriptions = null;
                    viewModel.SearchFilterValue       = null;
                    viewModel.ClearBoundSelectedItems();
                }).DisposeWith(disposables);
            }
        });
        InitializeComponent();
    }

    private void HandleSelectionModeOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs e)
    {
        if (DataContext is ListViewModel viewModel &&
            e.CheckedOption.IsChecked == true &&
            e.CheckedOption.Tag is SelectionMode selectionMode)
        {
            viewModel.SelectionMode = selectionMode;
        }
    }

    private void HandleAddEmptyItemClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ListViewModel viewModel)
        {
            return;
        }

        var items = viewModel.EmptyDemoItems != null
            ? new List<IListItemData>(viewModel.EmptyDemoItems)
            : new List<IListItemData>();

        items.Add(CreateDynamicItem());
        viewModel.EmptyDemoItems = items;
    }

    private void HandleRemoveEmptyItemClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ListViewModel viewModel)
        {
            return;
        }

        if (viewModel.EmptyDemoItems is null || viewModel.EmptyDemoItems.Count <= 1)
        {
            viewModel.EmptyDemoItems = [];
            return;
        }

        var items = new List<IListItemData>(viewModel.EmptyDemoItems);
        items.RemoveAt(items.Count - 1);
        viewModel.EmptyDemoItems = items;
    }

    private void HandleFilterListBoxSearchRequested(object? sender, SearchRequestedEventArgs e)
    {
        if (DataContext is ListViewModel viewModel)
        {
            viewModel.SearchFilterValue = e.Query.Trim();
        }
    }

    private void RefreshLocalizedListItems(ListViewModel viewModel)
    {
        var emptyDemoItemCount = viewModel.EmptyDemoItems?.Count ?? 0;
        InitBasicListItems(viewModel);
        InitSelectionListItems(viewModel);
        InitializeGroupItems(viewModel);
        InitializeDisabledItems(viewModel);
        InitializeFilteredGroupItems(viewModel);
        InitializeOrderedGroupItems(viewModel);
        InitializeEmptyDemoItems(viewModel, emptyDemoItemCount);
        InitializeBasicListBoxItems(viewModel);
        InitializePaginationListBoxItems(viewModel);
        InitializeSemanticListItems(viewModel);
        InitializeSemanticListBoxItems(viewModel);
    }

    internal static string Lang(ListShowCaseLangResourceKind resourceKind, string fallback)
    {
        return ListShowCaseLanguage.Get(resourceKind, fallback);
    }

    private List<IListItemData> BuildBasicListItems()
    {
        return [
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorBlue, "Blue")
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorGreen, "Green")
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorRed, "Red")
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorYellow, "Yellow")
            }
        ];
    }

    private void InitBasicListItems(ListViewModel viewModel)
    {
        viewModel.ListItems = BuildBasicListItems();
    }

    private void InitSelectionListItems(ListViewModel viewModel)
    {
        viewModel.SelectionListItems = BuildBasicListItems();
        viewModel.ResetBoundSelectedItems();
    }

    private void InitializeDisabledItems(ListViewModel viewModel)
    {
        viewModel.ListItemsWidthDisabled = [
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorBlue, "Blue")
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorGreen, "Green")
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorRed, "Red")
            },
            new ListItemData()
            {
                Content   = Lang(ListShowCaseLangResourceKind.P2ColorYellow, "Yellow"),
                IsEnabled = false
            }
        ];
    }

    private List<IListItemData> BuildGroupItems()
    {
        var basicColors   = Lang(ListShowCaseLangResourceKind.P2GroupBasicColors, "Basic Colors");
        var neutralColors = Lang(ListShowCaseLangResourceKind.P2GroupNeutralColors, "Neutral Colors");
        var specificShades = Lang(ListShowCaseLangResourceKind.P2GroupSpecificShades,
            "Specific Shades");

        return [
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorRed, "Red"),
                Group   = basicColors
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorOrange, "Orange"),
                Group   = basicColors
            },

            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorGreen, "Green"),
                Group   = basicColors
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorBlue, "Blue"),
                Group   = basicColors
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorPurple, "Purple"),
                Group   = basicColors
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorPink, "Pink"),
                Group   = basicColors
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorYellow, "Yellow"),
                Group   = basicColors
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorBrown, "Brown"),
                Group   = neutralColors
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorWhite, "White"),
                Group   = neutralColors
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorBlack, "Black"),
                Group   = neutralColors
            },

            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorGray, "Gray"),
                Group   = neutralColors
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorTurquoise, "Turquoise"),
                Group   = specificShades
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorViolet, "Violet"),
                Group   = specificShades
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorMagenta, "Magenta"),
                Group   = specificShades
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorMaroon, "Maroon"),
                Group   = specificShades
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorNavy, "Navy"),
                Group   = specificShades
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorBeige, "Beige"),
                Group   = specificShades
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorCyan, "Cyan"),
                Group   = specificShades
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorLavender, "Lavender"),
                Group   = specificShades
            },
            new ListItemData()
            {
                Content = Lang(ListShowCaseLangResourceKind.P2ColorOlive, "Olive"),
                Group   = specificShades
            },
        ];
    }

    private void InitializeGroupItems(ListViewModel viewModel)
    {
        viewModel.GroupListItems = BuildGroupItems();
    }

    private void InitializeFilteredGroupItems(ListViewModel viewModel)
    {
        viewModel.FilteredGroupListItems = BuildGroupItems();
    }

    private void InitializeOrderedGroupItems(ListViewModel viewModel)
    {
        viewModel.OrderedGroupListItems = BuildGroupItems();
    }

    private void InitializeEmptyDemoItems(ListViewModel viewModel, int itemCount)
    {
        var items = new List<IListItemData>(itemCount);
        for (var i = 0; i < itemCount; i++)
        {
            items.Add(CreateDynamicItem());
        }

        viewModel.EmptyDemoItems = items;
    }

    private void InitializeBasicListBoxItems(ListViewModel viewModel)
    {
        viewModel.BasicListBoxItems = [
            new ListItemData()
            {
                Content = Lang(
                    ListShowCaseLangResourceKind.P2ContentRacingCarSpraysBurningFuelIntoCrowd,
                    "Racing car sprays burning fuel into crowd.")
            },
            new ListItemData()
            {
                Content = Lang(
                    ListShowCaseLangResourceKind.P2ContentJapanesePrincessToWedCommoner,
                    "Japanese princess to wed commoner.")
            },
            new ListItemData()
            {
                Content = Lang(
                    ListShowCaseLangResourceKind.P2ContentAustralianWalksN100kmAfterOutbackCrash,
                    "Australian walks 100km after outback crash.")
            },
            new ListItemData()
            {
                Content = Lang(
                    ListShowCaseLangResourceKind.P2ContentManChargedOverMissingWeddingGirl,
                    "Man charged over missing wedding girl.")
            },
            new ListItemData()
            {
                Content = Lang(
                    ListShowCaseLangResourceKind.P2ContentLosAngelesBattlesHugeWildfires,
                    "Los Angeles battles huge wildfires.")
            },
        ];
    }

    internal static ListItemData CreateDynamicItem()
    {
        return new ListItemData()
        {
            Content = Lang(ListShowCaseLangResourceKind.P2ContentDynamicItem, "Dynamic item")
        };
    }

    private void InitializePaginationListBoxItems(ListViewModel viewModel)
    {
        var list = new List<IListItemData>();
        for (var i = 0; i < 2000; i++)
        {
            list.Add(new ListItemData()
            {
                ItemKey = $"{i}",
                Content = GalleryLocalization.Format(
                    ListShowCaseLangResourceKind.P2ContentPaginationItemFormat,
                    "Content {0}",
                    i)
            });
        }

        viewModel.PaginationListItems = list;
    }

    private void InitializeSemanticListItems(ListViewModel viewModel)
    {
        viewModel.SemanticListItems =
        [
            new ListItemData { Content = "Olivia", Group = "Design" },
            new ListItemData { Content = "Liam", Group = "Design" },
            new ListItemData { Content = "Emma", Group = "Design" },
            new ListItemData { Content = "Noah", Group = "Engineering" },
            new ListItemData { Content = "Ava", Group = "Engineering" },
            new ListItemData { Content = "Ethan", Group = "Engineering" }
        ];
    }

    private void InitializeSemanticListBoxItems(ListViewModel viewModel)
    {
        viewModel.SemanticListBoxItems =
        [
            new ListItemData { Content = "Olivia" },
            new ListItemData { Content = "Liam" },
            new ListItemData { Content = "Emma" },
            new ListItemData { Content = "Noah" },
            new ListItemData { Content = "Ava" },
            new ListItemData { Content = "Ethan" }
        ];
    }
}

internal static class ListShowCaseLanguage
{
    public static string Get(ListShowCaseLangResourceKind resourceKind, string fallback)
    {
        if (Application.Current is null)
        {
            return fallback;
        }

        return GalleryLocalization.Get(resourceKind, fallback);
    }
}
