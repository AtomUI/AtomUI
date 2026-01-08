using AtomUI.Desktop.Controls;
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
                InitBasicTreeSelectNodes(vm);
                InitMultiTreeSelectNodes(vm);
            }
        });
        InitializeComponent();
    }

    private void InitBasicTreeSelectNodes(TreeSelectViewModel vm)
    {
        vm.BasicTreeSelectNodes =
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

    private void InitMultiTreeSelectNodes(TreeSelectViewModel vm)
    {
        vm.MultiSelectionTreeSelectNodes =
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
}