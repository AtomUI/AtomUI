using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class CascaderView
{
    internal static readonly DirectProperty<CascaderView, bool> IsFilteringProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, bool>(nameof(IsFiltering),
            o => o.IsFiltering,
            (o, v) => o.IsFiltering = v);
    
    internal static readonly DirectProperty<CascaderView, List<CascaderViewFilterListItemData>?> FilteredPathInfosProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, List<CascaderViewFilterListItemData>?>(nameof(FilteredPathInfos),
            o => o.FilteredPathInfos,
            (o, v) => o.FilteredPathInfos = v);
    
    private bool _isFiltering;

    internal bool IsFiltering
    {
        get => _isFiltering;
        set => SetAndRaise(IsFilteringProperty, ref _isFiltering, value);
    }
    
    private List<CascaderViewFilterListItemData>? _filteredPathInfos;

    internal List<CascaderViewFilterListItemData>? FilteredPathInfos
    {
        get => _filteredPathInfos;
        set => SetAndRaise(FilteredPathInfosProperty, ref _filteredPathInfos, value);
    }
    
    private List<CascaderViewFilterListItemData>? _allPathInfos;
    private CascaderViewFilterList? _filterList;
    
    public void FilterItems()
    {
        if (Filter != null && FilterValue != null && IsLoaded)
        {
            if (_allPathInfos == null)
            {
                var result = new List<CascaderViewFilterListItemData>();
                foreach (var item in Items)
                {
                    if (item is ICascaderViewOption viewOption)
                    {
                        CollectionPaths(viewOption, result);
                    }
                }
                result.Sort((lhs, rhs) =>
                {
                    var lhsStr = lhs.Value?.ToString() ?? string.Empty;
                    var rhsStr = rhs.Value?.ToString() ?? string.Empty;
                    return string.Compare(lhsStr, rhsStr, StringComparison.OrdinalIgnoreCase);
                });
                _allPathInfos = result;
            }
            
            FilteredPathInfos = _allPathInfos.Where(data => Filter.Filter(this, data, FilterValue)).ToList();
            IsFiltering       = true;
            FilterResultCount = FilteredPathInfos.Count;
        }
        else
        {
            ClearFilter();
        }
    }

    private CascaderViewFilterListItemData GetFullPath(ICascaderViewOption option)
    {
        var pathHeaders = new List<string>();
        var current     = option;
        var pathNodes   = new  List<ICascaderViewOption>();
        while (current != null)
        {
            pathNodes.Add(current);
            pathHeaders.Add(current.Header?.ToString() ?? string.Empty);
            current = current.ParentNode as ICascaderViewOption;
        }

        pathNodes.Reverse();
        pathHeaders.Reverse();
        return new CascaderViewFilterListItemData()
        {
            Value       = string.Join('/', pathHeaders),
            ExpandItems = pathNodes,
            IsEnabled   = option.IsEnabled
        };
    }
    
    private void CollectionPaths(ICascaderViewOption option, List<CascaderViewFilterListItemData> result)
    {
        Debug.Assert(Filter != null);
        foreach (var childItem in option.Children)
        {
            CollectionPaths(childItem, result);
        }

        if (option.Children.Count == 0)
        {
            result.Add(GetFullPath(option));
        }
    }
    
    private void ClearFilter()
    {
        IsFiltering       = false;
        FilterResultCount = 0;
        SetCurrentValue(FilterValueProperty, null);
        FilteredPathInfos = null;
        _allPathInfos     = null;
    }

    private void HandleFilterListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        IList<ICascaderViewOption>? paths = null;
        if (_filterList?.SelectedItem is CascaderViewFilterListItemData itemData)
        {
            paths = itemData.ExpandItems;
        }
        ClearFilter();
        if (paths?.Count > 0)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var targetNode = paths[^1];
                await ExpandItemAsync(targetNode);
        
                // if (!IsCheckable)
                // {
                //     SelectedItem = targetNode;
                //     ItemSelected?.Invoke(this, new CascaderItemSelectedEventArgs(targetNode));
                // }
            });
        }
    }
}