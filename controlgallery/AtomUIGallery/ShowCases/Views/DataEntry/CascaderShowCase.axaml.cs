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
    
    private void InitBasicCascaderViewData(CascaderViewModel viewModel)
    {
        viewModel.BasicCascaderViewNodes = [
            new CascaderViewItemData()
            {
                Header  = "Zhejiang",
                Value = "zhejiang",
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
    
    private void InitBasicCascaderData(CascaderViewModel viewModel)
    {
        viewModel.BasicCascaderNodes = [
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
}