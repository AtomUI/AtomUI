using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Data;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class ListShowCase : ReactiveUserControl<ListViewModel>
{
    
    public ListShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is ListViewModel viewModel)
            {
         
                viewModel.ListItemsWidthDisabled = [
                    new ListItemData()
                    {
                        Content = "Blue"
                    },
                    new ListItemData()
                    {
                        Content = "Green"
                    },
                    new ListItemData()
                    {
                        Content = "Red"
                    },
                    new ListItemData()
                    {
                        Content   = "Yellow",
                        IsEnabled = false
                    }
                ];
                InitBasicListItems(viewModel);
                InitSelectionListItems(viewModel);
                InitializeGroupItems(viewModel);
                InitializeFilteredGroupItems(viewModel);
                InitializeOrderedGroupItems(viewModel);
                InitializeEmptyDemoItems(viewModel);
                InitializeBasicListBoxItems(viewModel);
                viewModel.SelectionMode = SelectionMode.Single;
            }
        });
        InitializeComponent();
        SelectionModeOptionGroup.OptionCheckedChanged += HandleSelectionModeOptionCheckedChanged;
        FilteredList.CollectionViewChanged            += HandleFilterCollectionViewChanged;
        OrderedList.CollectionViewChanged             += HandleOrderedCollectionViewChanged;
    }

    private void HandleSelectionModeOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs e)
    {
        if (DataContext is ListViewModel viewModel)
        {
            if (e.CheckedOption.IsChecked == true && e.CheckedOption.Tag is SelectionMode selectionMode)
            {
                viewModel.SelectionMode = selectionMode;
            }
        }
    }

    private List<IListItemData> BuildBasicListItems()
    {
        return [
            new ListItemData()
            {
                Content = "Blue"
            },
            new ListItemData()
            {
                Content = "Green"
            },
            new ListItemData()
            {
                Content = "Red"
            },
            new ListItemData()
            {
                Content = "Yellow"
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
    }
    
    private List<IListItemData> BuildGroupItems()
    {
        return [
            new ListItemData()
            {
                Content = "Red",
                Group   = "Basic Colors"
            },
            new ListItemData()
            {
                Content = "Orange",
                Group   = "Basic Colors"
            },
            
            new ListItemData()
            {
                Content = "Green",
                Group   = "Basic Colors"
            },
            new ListItemData()
            {
                Content = "Blue",
                Group   = "Basic Colors"
            },
            new ListItemData()
            {
                Content = "Purple",
                Group   = "Basic Colors"
            },
            new ListItemData()
            {
                Content = "Pink",
                Group   = "Basic Colors"
            },
            new ListItemData()
            {
                Content = "Yellow",
                Group   = "Basic Colors"
            },
            new ListItemData()
            {
                Content = "Brown",
                Group   = "Neutral Colors"
            },
            new ListItemData()
            {
                Content = "White",
                Group   = "Neutral Colors"
            },
            new ListItemData()
            {
                Content = "Black",
                Group   = "Neutral Colors"
            },
            
            new ListItemData()
            {
                Content = "Gray",
                Group   = "Neutral Colors"
            },
            new ListItemData()
            {
                Content = "Turquoise",
                Group   = "Specific Shades"
            },
            new ListItemData()
            {
                Content = "Violet",
                Group   = "Specific Shades"
            },
            new ListItemData()
            {
                Content = "Magenta",
                Group   = "Specific Shades"
            },
            new ListItemData()
            {
                Content = "Maroon",
                Group   = "Specific Shades"
            },
            new ListItemData()
            {
                Content = "Navy",
                Group   = "Specific Shades"
            },
            new ListItemData()
            {
                Content = "Beige",
                Group   = "Specific Shades"
            },
            new ListItemData()
            {
                Content = "Cyan",
                Group   = "Specific Shades"
            },
            new ListItemData()
            {
                Content = "Lavender",
                Group   = "Specific Shades"
            },
            new ListItemData()
            {
                Content = "Olive",
                Group   = "Specific Shades"
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
    
    private void InitializeEmptyDemoItems(ListViewModel viewModel)
    {
        viewModel.EmptyDemoItems = [];
    }
    
    private void InitializeBasicListBoxItems(ListViewModel viewModel)
    {
        viewModel.BasicListBoxItems = [
            new ListBoxItemData()
            {
                Value = "Racing car sprays burning fuel into crowd."
            },
            new ListBoxItemData()
            {
                Value = "Japanese princess to wed commoner."
            },
            new ListBoxItemData()
            {
                Value = "Australian walks 100km after outback crash."
            },
            new ListBoxItemData()
            {
                Value = "Man charged over missing wedding girl."
            },
            new ListBoxItemData()
            {
                Value = "Los Angeles battles huge wildfires."
            },
        ];
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
    
        items.Add(new ListItemData()
        {
            Content = $"Dynamic item "
        });
    
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
    
    private void HandleFilterCollectionViewChanged(object? sender, ListCollectionViewChangedEventArgs e)
    {
        if (FilteredList.CollectionView != null)
        {
            FilteredList.CollectionView.FilterDescriptions.Add(new ListFilterDescription()
            {
                FilterPropertySelector = data =>
                {
                    if (data is IListItemData listItemData)
                    {
                        return listItemData.Content;
                    }

                    return null;
                },
                FilterConditions       = ["a"]
            });
        }
    }
    
    private void HandleOrderedCollectionViewChanged(object? sender, ListCollectionViewChangedEventArgs e)
    {
        if (OrderedList.CollectionView != null)
        {
            OrderedList.CollectionView.SortDescriptions.Add(ListSortDescription.FromPath("Content"));
        }
    }
    
    private void HandleFilterListBoxClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is SearchEdit searchEdit)
        {
            SearchListBox.ItemFilterValue = searchEdit.Text?.Trim();
        }
    }
}