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

    private List<ICascaderOption> GenerateCascaderViewItems()
    {
        return [
            new CascaderOption()
            {
                Header = "Zhejiang",
                Value  = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header = "Hangzhou",
                        Value  = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header = "West Lake",
                                Value  = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header = "Lingyin shi",
                                Value  = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header = "Nanjing",
                        Value  = "nanjing",
                        Children = [
                            new CascaderOption()
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
            new CascaderOption()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
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
            new CascaderOption()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                IsEnabled = false,
                Children = [
                    new CascaderOption()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
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
            new CascaderOption()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header    = "Jiangsu",
                ItemKey   = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
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

    private List<ICascaderOption> GenerateMultiSelectCascaderNodes()
    {
        var lightNode = new CascaderOption()
        {
            Header = "Light",
            Value  = "light",
        };
        for (var i = 1; i <= 20; i++)
        {
            lightNode.Children.Add(new CascaderOption()
            {
                Header = $"Number {i}",
                Value  = i.ToString()
            });
        }
        return [
            lightNode,
            new CascaderOption()
            {
                Header = "Bamboo",
                Value  = "bamboo",
                Children = [
                    new CascaderOption()
                    {
                        Header = "Little",
                        Value  = "little",
                        Children = [
                            new CascaderOption()
                            {
                                Header            = "Toy Fish",
                                Value             = "fish",
                                IsCheckBoxEnabled = false
                            },
                            new CascaderOption()
                            {
                                Header = "Toy Cards",
                                Value  = "cards",
                            },
                            new CascaderOption()
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
            new CascaderOption()
            {
                Header  = "Zhejiang",
                ItemKey = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            },
                            new CascaderOption()
                            {
                                Header  = "Xia Sha",
                                ItemKey = "xiasha",
                                IsEnabled = false
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
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
    
    private void InitPrefixAndSuffix1CascaderData(CascaderViewModel viewModel)
    {
        viewModel.PrefixAndSuffixCascaderNodes  = GenerateCascaderViewItems();
    }
    
    private void InitSizeCascaderData(CascaderViewModel viewModel)
    {
        viewModel.SizeCascaderNodes = GenerateCascaderViewItems();
    }

    private void InitPlacementCascaderData(CascaderViewModel viewModel)
    {
        viewModel.PlacementCascaderNodes = GenerateCascaderViewItems();
    }

    private void InitBasicCheckableCascaderViewData(CascaderViewModel viewModel)
    {
        viewModel.BasicCheckableCascaderViewNodes = [
            new CascaderOption()
            {
                Header = "Zhejiang",
                Value  = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header = "Hangzhou",
                        Value  = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header = "West Lake",
                                Value  = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header = "Lingyin shi",
                                Value  = "lingyinshi",
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header = "Nanjing",
                        Value  = "nanjing",
                        Children = [
                            new CascaderOption()
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
            new CascaderOption()
            {
                Header = "Zhejiang",
                Value  = "zhejiang"
            },
            new CascaderOption()
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
            new CascaderOption()
            {
                Header = "Zhejiang",
                Value  = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header = "Hangzhou",
                        Value  = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header = "West Lake",
                                Value  = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header = "Lingyin shi",
                                Value  = "lingyinshi",
                            },
                            new CascaderOption()
                            {
                                Header = "XiSha",
                                Value  = "xisha",
                                IsEnabled = false
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header = "Jiangsu",
                Value  = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header = "Nanjing",
                        Value  = "nanjing",
                        Children = [
                            new CascaderOption()
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
            new CascaderOption()
            {
                Header = "Zhejiang",
                ItemKey  = "zhejiang",
                Children = [
                    new CascaderOption()
                    {
                        Header  = "Hangzhou",
                        ItemKey = "hangzhou",
                        Children = [
                            new CascaderOption()
                            {
                                Header  = "West Lake",
                                ItemKey = "xihu",
                            },
                            new CascaderOption()
                            {
                                Header  = "Lingyin shi",
                                ItemKey = "lingyinshi",
                            },
                            new CascaderOption()
                            {
                                Header    = "XiSha",
                                ItemKey   = "xisha",
                                IsEnabled = false
                            }
                        ]
                    }
                ]
            },
            new CascaderOption()
            {
                Header  = "Jiangsu",
                ItemKey = "jiangsu",
                Children = [
                    new CascaderOption()
                    {
                        Header  = "Nanjing",
                        ItemKey = "nanjing",
                        Children = [
                            new CascaderOption()
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