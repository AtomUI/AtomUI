using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class TreeSelectShowCase : ReactiveUserControl<TreeSelectViewModel>
{
    public TreeSelectShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is TreeSelectViewModel vm)
            {
                InitBasicTreeNodes(vm);
                InitMultiTreeNodes(vm);
                InitItemsSourceTreeNodes(vm);
                InitCheckableTreeNodes(vm);
                InitAsyncLoadTreeNodes(vm);
                InitShowLineTreeNodes(vm);
                InitLeftAddOnTreeNodes(vm);
                InitContentLeftAddOnTreeNodes(vm);
                InitPlacementTreeNodes(vm);
                InitMaxSelectedTreeNodes(vm);
                InitMaxCheckedTreeNodes(vm);
                vm.AsyncLoadTreeNodeLoader = new TreeItemDataLoader();
            }
        });

        InitializeComponent();
    }
    
    public void HandlePlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is TreeSelectViewModel vm)
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

    private void InitBasicTreeNodes(TreeSelectViewModel vm)
    {
        vm.BasicTreeNodes =
        [
            new TreeViewItemData()
            {
                Header = "parent 1",
                Value  = "parent 1",
                Children = [
                    new TreeViewItemData()
                    {
                        Header = "parent 1-0",
                        Value  = "parent 1-0",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header = "leaf1",
                                Value  = "leaf1",
                            },
                            new TreeViewItemData()
                            {
                                Header = "leaf2",
                                Value  = "leaf2",
                            },
                            new TreeViewItemData()
                            {
                                Header = "leaf3",
                                Value  = "leaf3",
                            },
                            new TreeViewItemData()
                            {
                                Header = "leaf4",
                                Value  = "leaf4",
                            },
                            new TreeViewItemData()
                            {
                                Header = "leaf5",
                                Value  = "leaf5",
                            },
                            new TreeViewItemData()
                            {
                                Header = "leaf6",
                                Value  = "leaf6",
                            },
                        ]
                    },
                    new TreeViewItemData()
                    {
                        Header = "parent 1-1",
                        Value  = "parent 1-1",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header = "leaf11",
                                Value  = "leaf11",
                            }
                        ]
                    }
                ]
            }
        ];
    }

    private void InitMultiTreeNodes(TreeSelectViewModel vm)
    {
        vm.MultiSelectionTreeNodes =
        [
            new TreeViewItemData()
            {
                Header = "parent 1",
                Value  = "parent 1",
                Children = [
                    new TreeViewItemData()
                    {
                        Header = "parent 1-0",
                        Value  = "parent 1-0",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header = "my leaf",
                                Value  = "leaf1",
                            },
                            new TreeViewItemData()
                            {
                                Header = "your leaf",
                                Value  = "leaf2",
                            }
                        ]
                    },
                    new TreeViewItemData()
                    {
                        Header = "parent 1-1",
                        Value  = "parent 1-1",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header = "sss",
                                Value  = "sss",
                            }
                        ]
                    }
                ]
            },
        ];
    }

    private void InitLeftAddOnTreeNodes(TreeSelectViewModel vm)
    {
        vm.LeftAddTreeNodes =
        [
            new TreeViewItemData()
            {
                Header = "parent 1",
                Value  = "parent 1",
                Children = [
                    new TreeViewItemData()
                    {
                        Header = "parent 1-0",
                        Value  = "parent 1-0",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header = "my leaf",
                                Value  = "leaf1",
                            },
                            new TreeViewItemData()
                            {
                                Header = "your leaf",
                                Value  = "leaf2",
                            }
                        ]
                    },
                    new TreeViewItemData()
                    {
                        Header = "parent 1-1",
                        Value  = "parent 1-1",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header = "sss",
                                Value  = "sss",
                            }
                        ]
                    }
                ]
            },
        ];
    }
    
    private void InitContentLeftAddOnTreeNodes(TreeSelectViewModel vm)
    {
        vm.ContentLeftAddTreeNodes =
        [
            new TreeViewItemData()
            {
                Header = "parent 1",
                Value  = "parent 1",
                Children = [
                    new TreeViewItemData()
                    {
                        Header = "parent 1-0",
                        Value  = "parent 1-0",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header = "my leaf",
                                Value  = "leaf1",
                            },
                            new TreeViewItemData()
                            {
                                Header = "your leaf",
                                Value  = "leaf2",
                            }
                        ]
                    },
                    new TreeViewItemData()
                    {
                        Header = "parent 1-1",
                        Value  = "parent 1-1",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header = "sss",
                                Value  = "sss",
                            }
                        ]
                    }
                ]
            },
        ];
    }

    private void InitItemsSourceTreeNodes(TreeSelectViewModel vm)
    {
        vm.ItemsSourceTreeNodes =
        [
            new TreeViewItemData()
            {
                Header = "Node1",
                Value  = "0-0",
                Children = [
                    new TreeViewItemData()
                    {
                        Header = "Child Node1",
                        Value  = "0-0-1",
                    },
                    new TreeViewItemData()
                    {
                        Header = "Child Node2",
                        Value  = "0-0-2",
                    }
                ]
            },
            new TreeViewItemData()
            {
                Header = "Node2",
                Value  = "0-1",
            }
        ];
    }

    private void InitCheckableTreeNodes(TreeSelectViewModel vm)
    {
        vm.CheckableTreeNodes =
        [
            new TreeViewItemData()
            {
                Header = "Node1",
                ItemKey = "0-0",
                Children = [
                    new TreeViewItemData()
                    {
                        Header = "Child Node1",
                        Value  = "0-0-0",
                    }
                ]
            },
            new TreeViewItemData()
            {
                Header = "Node2",
                ItemKey = "0-1",
                Children = [
                    new TreeViewItemData()
                    {
                        Header = "Child Node3",
                        Value  = "0-1-0",
                    },
                    new TreeViewItemData()
                    {
                        Header = "Child Node4",
                        Value  = "0-1-1",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header = "Child Node6",
                                Value  = "0-1-1-0",
                            },
                            new TreeViewItemData()
                            {
                                Header = "Child Node7",
                                Value  = "0-1-1-1",
                            },
                        ]
                    },
                    new TreeViewItemData()
                    {
                        Header = "Child Node5",
                        Value  = "0-1-2",
                    }
                ]
            }
        ];
    }
    
    private void InitAsyncLoadTreeNodes(TreeSelectViewModel viewModel)
    {
        viewModel.AsyncLoadTreeNodes =
        [
            new TreeViewItemData()
            {
                Header  = "Expand to load",
                ItemKey = "0",
            },
            new TreeViewItemData()
            {
                Header  = "Expand to load",
                ItemKey = "1",
            },
            new TreeViewItemData()
            {
                Header  = "Tree Node",
                ItemKey = "2",
                IsLeaf  = true
            }
        ];
    }

    private void InitShowLineTreeNodes(TreeSelectViewModel viewModel)
    {
        viewModel.ShowTreeLineTreeNodes =
        [
            new TreeViewItemData()
            {
                Header  = "parent 1",
                ItemKey = "parent 1",
                Children = [
                    new TreeViewItemData()
                    {
                        Header = "parent 1-0",
                        Value  = "parent 1-0",
                        Icon   = new CarryOutOutlined(),
                        Children = [
                            new TreeViewItemData()
                            {
                                Header  = "Leaf1",
                                ItemKey = "Leaf1",
                                IsLeaf  = true,
                                Icon    = new CarryOutOutlined(),
                            },
                            new TreeViewItemData()
                            {
                                Header  = "Leaf2",
                                ItemKey = "Leaf2",
                                IsLeaf  = true,
                                Icon    = new CarryOutOutlined(),
                            }
                        ]
                    },
                    new TreeViewItemData()
                    {
                        Header  = "parent 1-1",
                        ItemKey = "parent 1-1",
                        Icon    = new CarryOutOutlined(),
                        Children = [
                            new TreeViewItemData()
                            {
                                Header  = "sss",
                                ItemKey = "sss",
                                IsLeaf  = true,
                                Icon    = new CarryOutOutlined(),
                            }
                        ]
                    },
                ]
            }
        ];
    }
    
    private void InitPlacementTreeNodes(TreeSelectViewModel viewModel)
    {
        viewModel.PlacementTreeNodes =
        [
            new TreeViewItemData()
            {
                Header  = "parent 1",
                ItemKey = "parent 1",
                Children = [
                    new TreeViewItemData()
                    {
                        Header = "parent 1-0",
                        Value  = "parent 1-0",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header  = "Leaf1",
                                ItemKey = "Leaf1",
                                IsLeaf  = true,
                            },
                            new TreeViewItemData()
                            {
                                Header  = "Leaf2",
                                ItemKey = "Leaf2",
                                IsLeaf  = true,
                            }
                        ]
                    },
                    new TreeViewItemData()
                    {
                        Header  = "parent 1-1",
                        ItemKey = "parent 1-1",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header  = "sss",
                                ItemKey = "sss",
                                IsLeaf  = true,
                            }
                        ]
                    },
                ]
            }
        ];
    }
    
    private void InitMaxSelectedTreeNodes(TreeSelectViewModel viewModel)
    {
        viewModel.MaxSelectedTreeNodes =
        [
            new TreeViewItemData()
            {
                Header  = "parent 1",
                ItemKey = "parent 1",
                Children = [
                    new TreeViewItemData()
                    {
                        Header = "parent 1-0",
                        Value  = "parent 1-0",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header  = "Leaf1",
                                ItemKey = "Leaf1",
                                IsLeaf  = true,
                            },
                            new TreeViewItemData()
                            {
                                Header  = "Leaf2",
                                ItemKey = "Leaf2",
                                IsLeaf  = true,
                            }
                        ]
                    },
                    new TreeViewItemData()
                    {
                        Header  = "parent 1-1",
                        ItemKey = "parent 1-1",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header  = "sss",
                                ItemKey = "sss",
                                IsLeaf  = true,
                            }
                        ]
                    },
                ]
            }
        ];
    }
    
    private void InitMaxCheckedTreeNodes(TreeSelectViewModel viewModel)
    {
        viewModel.MaxCheckedTreeNodes =
        [
            new TreeViewItemData()
            {
                Header  = "parent 1",
                ItemKey = "parent 1",
                Children = [
                    new TreeViewItemData()
                    {
                        Header = "parent 1-0",
                        Value  = "parent 1-0",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header  = "Leaf1",
                                ItemKey = "Leaf1",
                                IsLeaf  = true,
                            },
                            new TreeViewItemData()
                            {
                                Header  = "Leaf2",
                                ItemKey = "Leaf2",
                                IsLeaf  = true,
                            }
                        ]
                    },
                    new TreeViewItemData()
                    {
                        Header  = "parent 1-1",
                        ItemKey = "parent 1-1",
                        Children = [
                            new TreeViewItemData()
                            {
                                Header  = "sss",
                                ItemKey = "sss",
                                IsLeaf  = true,
                            }
                        ]
                    },
                ]
            }
        ];
    }
}