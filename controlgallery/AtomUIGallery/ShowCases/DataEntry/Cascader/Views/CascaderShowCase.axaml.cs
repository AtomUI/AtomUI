using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUIGallery.ShowCases.Cascader;

public partial class CascaderShowCase : GalleryReactiveUserControl<CascaderViewModel>
{
    public const string LanguageId = nameof(CascaderShowCase);

    private SearchEdit? _enterPressedSearchInput;

    public CascaderShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is CascaderViewModel viewModel)
            {
                RefreshCascaderData(viewModel);

                var languageManager = GalleryLocalization.GetLanguageManager();
                if (languageManager != null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshCascaderData(viewModel);
                    languageManager.LanguageChanged += handler;
                    disposables.Add(Disposable.Create(() => languageManager.LanguageChanged -= handler));
                }

                disposables.Add(Disposable.Create(() =>
                {
                    viewModel.BasicCascaderViewNodes             = null;
                    viewModel.DefaultSelectOptionPath            = null;
                    viewModel.BoundSelectedCascaderOption        = null;
                    viewModel.BoundSelectedCascaderOptions       = null;
                    viewModel.BasicCheckableCascaderViewNodes    = null;
                    viewModel.HoverCascaderNodes                 = null;
                    viewModel.DisabledCascaderNodes              = null;
                    viewModel.SelectParentCascaderNodes          = null;
                    viewModel.MultipleSelectCascaderNodes        = null;
                    viewModel.CheckStrategyShowParentCascaderNodes = null;
                    viewModel.CheckStrategyShowAllCascaderNodes  = null;
                    viewModel.PrefixAndSuffixCascaderNodes       = null;
                    viewModel.PlacementCascaderNodes             = null;
                    viewModel.SearchCascaderNodes                = null;
                    viewModel.SizeCascaderNodes                  = null;
                    viewModel.AsyncLoadCascaderViewNodes         = null;
                    viewModel.SearchCascaderViewNodes            = null;
                    viewModel.AsyncCascaderNodeLoader            = null;
                    viewModel.DefaultExpandCascaderViewNodes     = null;
                    viewModel.DefaultExpandPath                  = null;
                    viewModel.SemanticPreviewOptions             = null;
                    viewModel.SemanticPreviewSelectedOptions     = null;
                    viewModel.StyleClassOptions                  = null;
                }));
            }
        });
        InitializeComponent();
        AddHandler(KeyDownEvent, HandleExternalSearchKeyDown, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, HandleExternalSearchKeyUp, RoutingStrategies.Tunnel);
        AddHandler(LostFocusEvent, HandleExternalSearchLostFocus, RoutingStrategies.Bubble);
    }

    private void HandlePlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is not CascaderViewModel viewModel)
        {
            return;
        }

        viewModel.Placement = args.Index switch
        {
            0 => SelectPopupPlacement.TopEdgeAlignedLeft,
            1 => SelectPopupPlacement.TopEdgeAlignedRight,
            2 => SelectPopupPlacement.BottomEdgeAlignedLeft,
            _ => SelectPopupPlacement.BottomEdgeAlignedRight
        };
    }

    private void HandleSelectFirstBoundCascaderOptionClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CascaderViewModel viewModel)
        {
            viewModel.SelectFirstBoundCascaderOption();
        }
    }

    private void HandleSelectSecondBoundCascaderOptionClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CascaderViewModel viewModel)
        {
            viewModel.SelectSecondBoundCascaderOption();
        }
    }

    private void HandleClearBoundCascaderOptionSelectionClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CascaderViewModel viewModel)
        {
            viewModel.ClearBoundCascaderOptionSelection();
        }
    }

    private void HandleSelectFirstBoundCascaderOptionsClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CascaderViewModel viewModel)
        {
            viewModel.SelectFirstBoundCascaderOptions();
        }
    }

    private void HandleSelectSecondBoundCascaderOptionsClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CascaderViewModel viewModel)
        {
            viewModel.SelectSecondBoundCascaderOptions();
        }
    }

    private void HandleSelectBothBoundCascaderOptionsClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CascaderViewModel viewModel)
        {
            viewModel.SelectBothBoundCascaderOptions();
        }
    }

    private void HandleClearBoundCascaderOptionsSelectionClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CascaderViewModel viewModel)
        {
            viewModel.ClearBoundCascaderOptionsSelection();
        }
    }

    private static bool TryGetExternalSearchTarget(object? source, out SearchEdit search, out CascaderView view)
    {
        var input = source as SearchEdit ?? (source as Visual)?.FindAncestorOfType<SearchEdit>();
        if (input?.Parent is Panel parent &&
            parent.Children.OfType<CascaderView>().FirstOrDefault(candidate =>
                candidate.Name is "SearchCascaderView" or "SearchCascaderViewItemsSource") is { } target)
        {
            search = input;
            view = target;
            return true;
        }
        search = null!;
        view = null!;
        return false;
    }

    private void HandleExternalSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled || e.KeyModifiers != KeyModifiers.None ||
            e.Key is not (Key.Up or Key.Down or Key.Enter) ||
            !TryGetExternalSearchTarget(e.Source, out var search, out var view) ||
            search.GetVisualDescendants().OfType<TextPresenter>()
                  .Any(presenter => !string.IsNullOrEmpty(presenter.PreeditText)))
        {
            return;
        }
        if (e.Key == Key.Enter)
        {
            if (ReferenceEquals(_enterPressedSearchInput, search))
            {
                e.Handled = true;
                return;
            }
            _enterPressedSearchInput = search;
        }
        var query = search.Text?.Trim() ?? string.Empty;
        var hasCurrentResults = !string.IsNullOrEmpty(query) && Equals(view.FilterValue, query);
        if (e.Key == Key.Enter && !hasCurrentResults)
        {
            view.FilterValue = query;
            e.Handled = true;
        }
        else if (hasCurrentResults)
        {
            view.HandleKeyDown(e);
        }
    }

    private void HandleExternalSearchKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _enterPressedSearchInput = null;
            if (TryGetExternalSearchTarget(e.Source, out _, out var view))
            {
                // Complete the forwarded key cycle on the sibling view's routed input surface.
                view.RaiseEvent(new KeyEventArgs { RoutedEvent = KeyUpEvent, Key = e.Key });
            }
        }
    }

    private void HandleExternalSearchLostFocus(object? sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(e.Source, _enterPressedSearchInput))
        {
            if (TryGetExternalSearchTarget(e.Source, out _, out var view))
            {
                view.RaiseEvent(new KeyEventArgs { RoutedEvent = KeyUpEvent, Key = Key.Enter });
            }
            _enterPressedSearchInput = null;
        }
    }

    private void HandleFilterCascaderViewSearchRequested(object? sender, SearchRequestedEventArgs args)
    {
        if (sender is SearchEdit searchEdit &&
            TryFindTemplateCascaderView(searchEdit, "SearchCascaderView", out var cascaderView))
        {
            cascaderView.FilterValue = args.Query.Trim();
        }
    }

    private void HandleFilterCascaderViewItemsSourceSearchRequested(object? sender, SearchRequestedEventArgs args)
    {
        if (sender is SearchEdit searchEdit &&
            TryFindTemplateCascaderView(searchEdit, "SearchCascaderViewItemsSource", out var cascaderView))
        {
            cascaderView.FilterValue = args.Query.Trim();
        }
    }

    private void HandleSearchCascaderViewOptionSelected(object? sender, CascaderOptionSelectedEventArgs args)
    {
        if (sender is CascaderView cascaderView &&
            cascaderView.Parent is Control parent &&
            parent.GetVisualDescendants().OfType<SearchEdit>().FirstOrDefault() is { } searchEdit)
        {
            searchEdit.Clear();
        }
    }

    private static bool TryFindTemplateCascaderView(Control source, string cascaderViewName, out CascaderView cascaderView)
    {
        var parent = source.Parent as Control;
        while (parent is not null)
        {
            if (parent is CascaderView directCascaderView &&
                directCascaderView.Name == cascaderViewName)
            {
                cascaderView = directCascaderView;
                return true;
            }

            var descendantCascaderView = parent.GetVisualDescendants()
                                                .OfType<CascaderView>()
                                                .FirstOrDefault(candidate => candidate.Name == cascaderViewName);
            if (descendantCascaderView is not null)
            {
                cascaderView = descendantCascaderView;
                return true;
            }

            parent = parent.Parent as Control;
        }

        cascaderView = null!;
        return false;
    }

    private void RefreshCascaderData(CascaderViewModel viewModel)
    {
        InitBasicCascaderData(viewModel);
        InitDefaultValueCascaderData(viewModel);
        InitSelectionBindingData(viewModel);
        InitHoverCascaderData(viewModel);
        InitDisabledCascaderData(viewModel);
        InitSelectParentCascaderData(viewModel);
        InitMultiSelectCascaderData(viewModel);
        InitCheckStrategyCascaderData(viewModel);
        InitPrefixAndSuffixCascaderData(viewModel);
        InitPlacementCascaderData(viewModel);
        InitSearchCascaderData(viewModel);
        InitSizeCascaderData(viewModel);
        InitCascaderViewData(viewModel);
        InitCascaderViewAsyncLoadData(viewModel);
        InitCascaderViewSearchData(viewModel);
        InitCascaderViewDefaultExpandData(viewModel);
        InitSemanticPreviewData(viewModel);
        InitStyleClassData(viewModel);
    }

    private static string Lang(CascaderShowCaseLangResourceKind resourceKind, string fallback)
    {
        return CascaderShowCaseLanguage.Get(resourceKind, fallback);
    }

    private void InitBasicCascaderData(CascaderViewModel viewModel)
    {
        viewModel.BasicCascaderViewNodes = [
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitDefaultValueCascaderData(CascaderViewModel viewModel)
    {
        viewModel.DefaultSelectOptionPath = new TreeNodePath(["zhejiang", "hangzhou", "xihu"]);
    }

    private static void InitSelectionBindingData(CascaderViewModel viewModel)
    {
        viewModel.ResetBoundCascaderSelection();
    }

    private void InitHoverCascaderData(CascaderViewModel viewModel)
    {
        viewModel.HoverCascaderNodes = [
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitDisabledCascaderData(CascaderViewModel viewModel)
    {
        viewModel.DisabledCascaderNodes = [
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                ItemKey = "lingyinshi",
                            },
                            new CascaderOption()
                            {
                                IsEnabled = false,
                                Header    = Lang(CascaderShowCaseLangResourceKind.P2HeaderHefangJie, "Hefang jie"),
                                ItemKey   = "Hefang jie",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header    = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey   = "jiangsu",
                IsEnabled = false,
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitSelectParentCascaderData(CascaderViewModel viewModel)
    {
        viewModel.SelectParentCascaderNodes = [
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitMultiSelectCascaderData(CascaderViewModel viewModel)
    {
        viewModel.MultipleSelectCascaderNodes = GenerateMultiSelectCascaderNodes();
    }

    private void InitCheckStrategyCascaderData(CascaderViewModel viewModel)
    {
        viewModel.CheckStrategyShowAllCascaderNodes    = GenerateCheckStrategyCascaderNodes();
        viewModel.CheckStrategyShowParentCascaderNodes = GenerateCheckStrategyCascaderNodes();
    }

    private List<ICascaderOption> GenerateCheckStrategyCascaderNodes()
    {
        return [
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),  ItemKey = "xihu" },
                            new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"), ItemKey = "lingyinshi" }
                        ]
                    },
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNingbo, "Ningbo"),
                        ItemKey = "ningbo",
                        Children = [
                            new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderTianyiPavilion, "Tianyi Pavilion"), ItemKey = "tianyige" },
                            new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderMoonLake, "Moon Lake"),       ItemKey = "yuehu" }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),         ItemKey = "zhonghuamen" },
                            new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderSunYatSenMausoleum, "Sun Yat-sen Mausoleum"), ItemKey = "zhongshanling" }
                        ]
                    },
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderSuzhou, "Suzhou"),
                        ItemKey = "suzhou",
                        Children = [
                            new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderHumbleAdministratorsGarden, "Humble Administrator's Garden"), ItemKey = "zhuozhengyuan" },
                            new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingeringGarden, "Lingering Garden"),              ItemKey = "liuyuan" }
                        ]
                    }
                ]
            }
        ];
    }

    private List<ICascaderOption> GenerateMultiSelectCascaderNodes()
    {
        var lightNode = new CascaderOption()
        {
            Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderLight, "Light"),
            Value  = "light",
            Children = [
                new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderBamboo, "Bamboo"), Value = "bamboo" },
                new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderLittle, "Little"), Value = "little" }
            ]
        };
        var boyNode = new CascaderOption()
        {
            Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderBambooBoy, "Bamboo Boy"),
            Value  = "bambooBoy",
            Children = [
                new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderLittle, "Little"), Value = "little" },
                new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderTadpole, "Tadpole"), Value = "tadpole" }
            ]
        };
        var mergeNode = new CascaderOption()
        {
            Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderLittleTadpole, "Little Tadpole"),
            Value  = "littleTadpole",
        };

        var zhangSanNode = new CascaderOption()
        {
            Header   = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhangSan, "Zhang San"),
            Value    = "zhangsan",
            Children = [lightNode, boyNode, mergeNode]
        };

        var greenNode = new CascaderOption()
        {
            Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderGreen, "Green"),
            Value  = "green",
            Children = [
                new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderWildWolf, "Wild Wolf"), Value = "wildWolf" },
                new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderGrayWolf, "Gray Wolf"), Value = "grayWolf" }
            ]
        };

        var yellowNode = new CascaderOption()
        {
            Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderYellow, "Yellow"),
            Value  = "yellow",
            Children = [
                new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiaoTailang, "Jiao Tailang"), Value = "jiaoTailang" },
                new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderBanban, "Banban"), Value = "banban" }
            ]
        };

        var bigNode = new CascaderOption()
        {
            Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderBig, "Big"),
            Value  = "big",
            Children = [
                new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderFeifei, "Feifei"), Value = "feifei" },
                new CascaderOption() { Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderXiaoXingxing, "Xiao Xingxing"), Value = "xiaoxingxing" }
            ]
        };

        var xiaoHuihuiNode = new CascaderOption()
        {
            Header   = Lang(CascaderShowCaseLangResourceKind.P2HeaderXiaoHuihui, "Xiao Huihui"),
            Value    = "xiaoHuihui",
            Children = [greenNode, yellowNode, bigNode]
        };
        return [zhangSanNode, xiaoHuihuiNode];
    }

    private void InitPrefixAndSuffixCascaderData(CascaderViewModel viewModel)
    {
        viewModel.PrefixAndSuffixCascaderNodes = [
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitPlacementCascaderData(CascaderViewModel viewModel)
    {
        viewModel.Placement              = SelectPopupPlacement.TopEdgeAlignedLeft;
        viewModel.PlacementCascaderNodes = [
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitSearchCascaderData(CascaderViewModel viewModel)
    {
        viewModel.SearchCascaderNodes = [
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                ItemKey = "lingyinshi",
                            },
                            new CascaderOption()
                            {
                                Header    = Lang(CascaderShowCaseLangResourceKind.P2HeaderXisha, "Xisha"),
                                ItemKey   = "xisha",
                                IsEnabled = false,
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitSizeCascaderData(CascaderViewModel viewModel)
    {
        viewModel.SizeCascaderNodes = [
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitCascaderViewData(CascaderViewModel viewModel)
    {
        viewModel.BasicCascaderViewNodes = [
            new CascaderOption()
            {
                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                Value  = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        Value  = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                Value  = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                Value  = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                Value  = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        Value  = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                Value  = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
        viewModel.BasicCheckableCascaderViewNodes = [
            new CascaderOption()
            {
                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                Value  = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        Value  = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                Value  = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                Value  = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                Value  = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        Value  = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                Value  = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitCascaderViewAsyncLoadData(CascaderViewModel viewModel)
    {
        viewModel.AsyncLoadCascaderViewNodes = [
            new CascaderOption()
            {
                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                Value  = "zhejiang",
                IsLeaf = false,
            },
            new CascaderOption()
            {
                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                Value  = "jiangsu",
                IsLeaf = false,
            }
        ];
        viewModel.AsyncCascaderNodeLoader = new CascaderItemDataLoader();
    }

    private void InitCascaderViewSearchData(CascaderViewModel viewModel)
    {
        viewModel.SearchCascaderViewNodes = [
            new CascaderOption()
            {
                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                Value  = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        Value  = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                Value  = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                Value  = "lingyinshi",
                            },
                            new CascaderOption()
                            {
                                Header    = Lang(CascaderShowCaseLangResourceKind.P2HeaderXisha, "Xisha"),
                                Value     = "xisha",
                                IsEnabled = false,
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                Value  = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        Value  = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                Value  = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitStyleClassData(CascaderViewModel viewModel)
    {
        viewModel.StyleClassOptions = [
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        ItemKey = "hangzhou",
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                    }
                ]
            }
        ];
    }

    private void InitSemanticPreviewData(CascaderViewModel viewModel)
    {
        var zhejiang = new CascaderOption()
        {
            Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
            ItemKey = "zhejiang",
            Children = [
                new CascaderOption()
                {
                    Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                    ItemKey = "hangzhou",
                    Children = [
                        new CascaderOption()
                        {
                            Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                            ItemKey = "xihu",
                        }
                    ]
                }
            ]
        };
        viewModel.SemanticPreviewSelectedOptions = [zhejiang.Children.First().Children.First()];
        viewModel.SemanticPreviewOptions = [
            zhejiang,
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitCascaderViewDefaultExpandData(CascaderViewModel viewModel)
    {
        viewModel.DefaultExpandPath = new TreeNodePath(["zhejiang", "hangzhou", "lingyinshi"]);
        viewModel.DefaultExpandCascaderViewNodes = [
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang"),
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou"),
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderWestLake, "West Lake"),
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderLingyinShi, "Lingyin shi"),
                                ItemKey = "lingyinshi",
                            },
                            new CascaderOption()
                            {
                                Header    = Lang(CascaderShowCaseLangResourceKind.P2HeaderXisha, "Xisha"),
                                ItemKey   = "xisha",
                                IsEnabled = false,
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu"),
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing"),
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = Lang(CascaderShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men"),
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

}

internal static class CascaderShowCaseLanguage
{
    public static string Get(CascaderShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }

    public static string Format(CascaderShowCaseLangResourceKind resourceKind, string fallback, params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }

    public static string FormatDynamicOption(ICascaderOption targetCascaderItem, int index)
    {
        var parentHeader = targetCascaderItem.Header?.ToString()
                           ?? targetCascaderItem.Value?.ToString()
                           ?? string.Empty;
        return Format(CascaderShowCaseLangResourceKind.P2HeaderDynamicOptionFormat,
            "{0} Dynamic {1}",
            parentHeader,
            index);
    }
}
