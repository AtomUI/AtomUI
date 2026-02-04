using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Primitives;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class CascaderShowCase : ReactiveUserControl<CascaderViewModel>
{
    public CascaderShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is CascaderViewModel vm)
            {
                InitHoverCascaderData(vm);
                InitDisabledCascaderData(vm);
                InitSelectParentCascaderData(vm);
                InitMultiSelectCascaderData(vm);
                InitCheckStrategyCascaderData(vm);
                InitSearchCascaderData(vm);
                InitLazyLoadCascaderData(vm);
                InitPrefixAndSuffix1CascaderData(vm);
                InitSizeCascaderData(vm);
                InitPlacementCascaderData(vm);
                
                InitBasicCascaderViewData(vm);
                InitBasicCheckableCascaderViewData(vm);
                InitAsyncLoadCascaderViewData(vm);
                InitSearchCascaderViewData(vm);
                InitDefaultExpandCascaderViewData(vm);
                vm.AsyncCascaderNodeLoader = new CascaderItemDataLoader();
                vm.DefaultExpandPath       = new TreeNodePath("jiangsu/nanjing/zhonghuamen");
                vm.DefaultSelectItemPath   = new TreeNodePath("zhejiang/hangzhou/xihu");
            }
        });
        InitializeComponent();
    }

    private List<ICascaderViewOption> GenerateCascaderViewItems()
    {
        return [
            new CascaderViewOption()
            {
                Header = "Zhejiang",
                Value  = "zhejiang",
                Children = [
                    new CascaderViewOption()
                    {
                        Header = "Hangzhou",
                        Value  = "hangzhou",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header = "West Lake",
                                Value  = "xihu",
                            },
                            new CascaderViewOption()
                            {
                                Header = "Lingyin shi",
                                Value  = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewOption()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
                Children = [
                    new CascaderViewOption()
                    {
                        Header = "Nanjing",
                        Value  = "nanjing",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header = "Zhong Hua Men",
                                Value  = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }
    
    private void InitBasicCascaderViewData(CascaderViewModel viewModel)
    {
        viewModel.BasicCascaderViewNodes = GenerateCascaderViewItems();
    }
    
    private void InitHoverCascaderData(CascaderViewModel viewModel)
    {
        viewModel.HoverCascaderNodes = [
            new CascaderViewOption()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderViewOption()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderViewOption()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewOption()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                Children = [
                    new CascaderViewOption()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header  = "Zhong Hua Men",
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
            new CascaderViewOption()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderViewOption()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderViewOption()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewOption()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                IsEnabled = false,
                Children = [
                    new CascaderViewOption()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header  = "Zhong Hua Men",
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
            new CascaderViewOption()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderViewOption()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderViewOption()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewOption()
            {
                Header    = "Jiangsu",
                ItemKey   = "jiangsu",
                Children = [
                    new CascaderViewOption()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header  = "Zhong Hua Men",
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

    private List<ICascaderViewOption> GenerateMultiSelectCascaderNodes()
    {
        var lightNode = new CascaderViewOption()
        {
            Header = "Light",
            Value  = "light",
        };
        for (var i = 1; i <= 20; i++)
        {
            lightNode.Children.Add(new CascaderViewOption()
            {
                Header = $"Number {i}",
                Value  = i.ToString()
            });
        }
        return [
            lightNode,
            new CascaderViewOption()
            {
                Header = "Bamboo",
                Value  = "bamboo",
                Children = [
                    new CascaderViewOption()
                    {
                        Header = "Little",
                        Value  = "little",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header            = "Toy Fish",
                                Value             = "fish",
                                IsCheckBoxEnabled = false
                            },
                            new CascaderViewOption()
                            {
                                Header = "Toy Cards",
                                Value  = "cards",
                            },
                            new CascaderViewOption()
                            {
                                Header = "Toy Bird",
                                Value  = "bird",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitCheckStrategyCascaderData(CascaderViewModel viewModel)
    {
        viewModel.CheckStrategyCascaderNodes = GenerateMultiSelectCascaderNodes();
    }

    private void InitSearchCascaderData(CascaderViewModel viewModel)
    {
        viewModel.SearchCascaderNodes = [
            new CascaderViewOption()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderViewOption()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderViewOption()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            },
                            new CascaderViewOption()
                            {
                                Header  = "Xia Sha",
                                ItemKey = "xiasha",
                                IsEnabled = false
                            }
                        ]
                    }
                ]
            },
            new CascaderViewOption()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                Children = [
                    new CascaderViewOption()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header  = "Zhong Hua Men",
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }
    
    private void InitLazyLoadCascaderData(CascaderViewModel viewModel)
    {
        viewModel.LazyLoadCascaderNodes =
        [
            new CascaderViewOption()
            {
                Header = "Zhejiang",
                Value  = "zhejiang"
            },
            new CascaderViewOption()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
            }
        ];
    }
    
    private void InitPrefixAndSuffix1CascaderData(CascaderViewModel viewModel)
    {
        viewModel.PrefixAndSuffix1CascaderNodes = GenerateCascaderViewItems();
        viewModel.PrefixAndSuffix2CascaderNodes = GenerateCascaderViewItems();
        viewModel.PrefixAndSuffix3CascaderNodes = GenerateCascaderViewItems();
    }
    
    private void InitSizeCascaderData(CascaderViewModel viewModel)
    {
        viewModel.SizeLargeCascaderNodes = GenerateCascaderViewItems();
        viewModel.SizeMiddleCascaderNodes = GenerateCascaderViewItems();
        viewModel.SizeSmallCascaderNodes = GenerateCascaderViewItems();
    }

    private void InitPlacementCascaderData(CascaderViewModel viewModel)
    {
        viewModel.PlacementCascaderNodes = GenerateCascaderViewItems();
    }

    private void InitBasicCheckableCascaderViewData(CascaderViewModel viewModel)
    {
        viewModel.BasicCheckableCascaderViewNodes = [
            new CascaderViewOption()
            {
                Header = "Zhejiang",
                Value  = "zhejiang",
                Children = [
                    new CascaderViewOption()
                    {
                        Header = "Hangzhou",
                        Value  = "hangzhou",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header = "West Lake",
                                Value  = "xihu",
                            },
                            new CascaderViewOption()
                            {
                                Header = "Lingyin shi",
                                Value  = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewOption()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
                Children = [
                    new CascaderViewOption()
                    {
                        Header = "Nanjing",
                        Value  = "nanjing",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header = "Zhong Hua Men",
                                Value  = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitAsyncLoadCascaderViewData(CascaderViewModel viewModel)
    {
        viewModel.AsyncLoadCascaderViewNodes =
        [
            new CascaderViewOption()
            {
                Header = "Zhejiang",
                Value  = "zhejiang"
            },
            new CascaderViewOption()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
            }
        ];
    }
    
    private void InitSearchCascaderViewData(CascaderViewModel viewModel)
    {
        viewModel.SearchCascaderViewNodes =
        [
            new CascaderViewOption()
            {
                Header = "Zhejiang",
                Value  = "zhejiang",
                Children = [
                    new CascaderViewOption()
                    {
                        Header = "Hangzhou",
                        Value  = "hangzhou",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header = "West Lake",
                                Value  = "xihu",
                            },
                            new CascaderViewOption()
                            {
                                Header = "Lingyin shi",
                                Value  = "lingyinshi",
                            },
                            new CascaderViewOption()
                            {
                                Header = "XiSha",
                                Value  = "xisha",
                                IsEnabled = false
                            }
                        ]
                    }
                ]
            },
            new CascaderViewOption()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
                Children = [
                    new CascaderViewOption()
                    {
                        Header = "Nanjing",
                        Value  = "nanjing",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header = "Zhong Hua Men",
                                Value  = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }
    
    private void InitDefaultExpandCascaderViewData(CascaderViewModel viewModel)
    {
        viewModel.DefaultExpandCascaderViewNodes =
        [
            new CascaderViewOption()
            {
                Header = "Zhejiang",
                ItemKey  = "zhejiang",
                Children = [
                    new CascaderViewOption()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderViewOption()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            },
                            new CascaderViewOption()
                            {
                                Header    = "XiSha",
                                ItemKey   = "xisha",
                                IsEnabled = false
                            }
                        ]
                    }
                ]
            },
            new CascaderViewOption()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                Children = [
                    new CascaderViewOption()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderViewOption()
                            {
                                Header  = "Zhong Hua Men",
                                ItemKey = "zhonghuamen",
                            }
                        ]
                    }
                ]
            }
        ];
    }
    
    // private void HandleFilterCascaderViewClicked(object? sender, RoutedEventArgs e)
    // {
    //     if (sender is SearchEdit searchEdit)
    //     {
    //        SearchCascaderView.FilterValue = searchEdit.Text?.Trim();
    //     }
    // }
    //
    // private void HandleFilterCascaderViewItemsSourceClicked(object? sender, RoutedEventArgs e)
    // {
    //     if (sender is SearchEdit searchEdit)
    //     {
    //        SearchCascaderViewItemsSource.FilterValue = searchEdit.Text?.Trim();
    //     }
    // }
    //
    public void HandlePlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is CascaderViewModel vm)
        {
            if (args.Index == 0)
            {
                vm.Placement = SelectPopupPlacement.TopEdgeAlignedLeft;
            }
            else if (args.Index == 1)
            {
                vm.Placement = SelectPopupPlacement.TopEdgeAlignedRight;
            }
            else if (args.Index == 2)
            {
                vm.Placement = SelectPopupPlacement.BottomEdgeAlignedLeft;
            }
            else
            {
                vm.Placement = SelectPopupPlacement.BottomEdgeAlignedRight;
            }
        }
    }
}