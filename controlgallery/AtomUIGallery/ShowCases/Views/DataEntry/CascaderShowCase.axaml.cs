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
                InitBasicCascaderData(vm);
                InitDefaultValueCascaderData(vm);
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
            }
        });
        InitializeComponent();
    }

    private List<ICascaderViewItemData> GenerateCascaderViewItems()
    {
        return [
            new CascaderViewItemData()
            {
                Header = "Zhejiang",
                Value  = "zhejiang",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header = "Hangzhou",
                        Value  = "hangzhou",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header = "West Lake",
                                Value  = "xihu",
                            },
                            new CascaderViewItemData()
                            {
                                Header = "Lingyin shi",
                                Value  = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewItemData()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header = "Nanjing",
                        Value  = "nanjing",
                        Children = [
                            new CascaderViewItemData()
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
    
    private void InitBasicCascaderData(CascaderViewModel viewModel)
    {
        viewModel.BasicCascaderNodes = GenerateCascaderViewItems();
    }

    private void InitDefaultValueCascaderData(CascaderViewModel viewModel)
    {
        viewModel.DefaultSelectItemPath = new TreeNodePath("zhejiang/hangzhou/xihu");
        viewModel.DefaultValueCascaderNodes = [
            new CascaderViewItemData()
            {
                Header = "Zhejiang",
                ItemKey  = "zhejiang",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderViewItemData()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewItemData()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderViewItemData()
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

    private void InitHoverCascaderData(CascaderViewModel viewModel)
    {
        viewModel.HoverCascaderNodes = [
            new CascaderViewItemData()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderViewItemData()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewItemData()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderViewItemData()
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
            new CascaderViewItemData()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderViewItemData()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewItemData()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                IsEnabled = false,
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderViewItemData()
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
            new CascaderViewItemData()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderViewItemData()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewItemData()
            {
                Header    = "Jiangsu",
                ItemKey   = "jiangsu",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderViewItemData()
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

    private List<ICascaderViewItemData> GenerateMultiSelectCascaderNodes()
    {
        var lightNode = new CascaderViewItemData()
        {
            Header = "Light",
            Value  = "light",
        };
        for (var i = 1; i <= 20; i++)
        {
            lightNode.Children.Add(new CascaderViewItemData()
            {
                Header = $"Number {i}",
                Value  = i.ToString()
            });
        }
        return [
            lightNode,
            new CascaderViewItemData()
            {
                Header = "Bamboo",
                Value  = "bamboo",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header = "Little",
                        Value  = "little",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header            = "Toy Fish",
                                Value             = "fish",
                                IsCheckBoxEnabled = false
                            },
                            new CascaderViewItemData()
                            {
                                Header = "Toy Cards",
                                Value  = "cards",
                            },
                            new CascaderViewItemData()
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
        viewModel.CheckStrategy1CascaderNodes = GenerateMultiSelectCascaderNodes();
        viewModel.CheckStrategy2CascaderNodes = GenerateMultiSelectCascaderNodes();
    }

    private void InitSearchCascaderData(CascaderViewModel viewModel)
    {
        viewModel.SearchCascaderNodes = [
            new CascaderViewItemData()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderViewItemData()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            },
                            new CascaderViewItemData()
                            {
                                Header  = "Xia Sha",
                                ItemKey = "xiasha",
                                IsEnabled = false
                            }
                        ]
                    }
                ]
            },
            new CascaderViewItemData()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderViewItemData()
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
            new CascaderViewItemData()
            {
                Header = "Zhejiang",
                Value  = "zhejiang"
            },
            new CascaderViewItemData()
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
            new CascaderViewItemData()
            {
                Header = "Zhejiang",
                Value  = "zhejiang",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header = "Hangzhou",
                        Value  = "hangzhou",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header = "West Lake",
                                Value  = "xihu",
                            },
                            new CascaderViewItemData()
                            {
                                Header = "Lingyin shi",
                                Value  = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderViewItemData()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header = "Nanjing",
                        Value  = "nanjing",
                        Children = [
                            new CascaderViewItemData()
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
            new CascaderViewItemData()
            {
                Header = "Zhejiang",
                Value  = "zhejiang"
            },
            new CascaderViewItemData()
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
            new CascaderViewItemData()
            {
                Header = "Zhejiang",
                Value  = "zhejiang",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header = "Hangzhou",
                        Value  = "hangzhou",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header = "West Lake",
                                Value  = "xihu",
                            },
                            new CascaderViewItemData()
                            {
                                Header = "Lingyin shi",
                                Value  = "lingyinshi",
                            },
                            new CascaderViewItemData()
                            {
                                Header = "XiSha",
                                Value  = "xisha",
                                IsEnabled = false
                            }
                        ]
                    }
                ]
            },
            new CascaderViewItemData()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header = "Nanjing",
                        Value  = "nanjing",
                        Children = [
                            new CascaderViewItemData()
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
            new CascaderViewItemData()
            {
                Header = "Zhejiang",
                ItemKey  = "zhejiang",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderViewItemData()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderViewItemData()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            },
                            new CascaderViewItemData()
                            {
                                Header    = "XiSha",
                                ItemKey   = "xisha",
                                IsEnabled = false
                            }
                        ]
                    }
                ]
            },
            new CascaderViewItemData()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                Children = [
                    new CascaderViewItemData()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderViewItemData()
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
    
    private void HandleFilterCascaderViewClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is SearchEdit searchEdit)
        {
            SearchCascaderView.FilterValue = searchEdit.Text?.Trim();
        }
    }
    
    private void HandleFilterCascaderViewItemsSourceClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is SearchEdit searchEdit)
        {
            SearchCascaderViewItemsSource.FilterValue = searchEdit.Text?.Trim();
        }
    }
    
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