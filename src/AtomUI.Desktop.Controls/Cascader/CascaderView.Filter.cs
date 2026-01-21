using System.Collections;
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
            if (FilterValue is string strFilterValue && string.IsNullOrWhiteSpace(strFilterValue))
            {
                ClearFilter();
                return;
            }

            if (_allPathInfos == null)
            {
                var result = new List<CascaderViewFilterListItemData>();
                if (ItemsSource != null)
                {
                    foreach (var item in Items)
                    {
                        if (item is ICascaderViewItemData cascaderViewItemData)
                        {
                            CollectionPaths(cascaderViewItemData, result);
                        }
                    }
                }
                else
                {
                    foreach (var item in Items)
                    {
                        if (item is CascaderViewItem cascaderViewItem)
                        {
                            CollectionPaths(cascaderViewItem, result);
                        }
                    }
                }
                result.Sort((lhs, rhs) =>
                {
                    var lhsStr = lhs.Content?.ToString() ?? string.Empty;
                    var rhsStr = rhs.Content?.ToString() ?? string.Empty;
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

    private CascaderViewFilterListItemData GetFullPath(object item)
    {
        if (item is ICascaderViewItemData cascaderViewItemData)
        {
            var pathHeaders = new List<string>();
            var current       = cascaderViewItemData;
            var pathNodes = new  List<object>();
            while (current != null)
            {
                pathNodes.Add(current);
                pathHeaders.Add(current.Header?.ToString() ?? string.Empty);
                current = current.ParentNode as ICascaderViewItemData;
            }

            pathNodes.Reverse();
            pathHeaders.Reverse();
            return new CascaderViewFilterListItemData()
            {
                Content     = string.Join('/', pathHeaders),
                ExpandItems = pathNodes,
                IsEnabled = cascaderViewItemData.IsEnabled
            };
        }
        if (item is CascaderViewItem cascaderViewItem)
        {
            var pathHeaders = new List<string>();
            var current     = cascaderViewItem;
            var pathNodes   = new  List<object>();
            while (current != null)
            {
                pathNodes.Add(current);
                pathHeaders.Add(current.Header?.ToString() ?? string.Empty);
                current = current.Parent as CascaderViewItem;
            }

            pathNodes.Reverse();
            pathHeaders.Reverse();
            return new CascaderViewFilterListItemData()
            {
                Content     = string.Join('/', pathHeaders),
                ExpandItems = pathNodes,
                IsEnabled = cascaderViewItem.IsEnabled
            };
        }
        throw new ArgumentException($"Item of type {item.GetType()} is not a CascaderViewItem or ICascaderViewItemData");
    }

    private void CollectionPaths(CascaderViewItem cascaderViewItem, List<CascaderViewFilterListItemData> result)
    {
        Debug.Assert(Filter != null);
        foreach (var item in cascaderViewItem.Items)
        {
            if (item is CascaderViewItem childItem)
            {
                CollectionPaths(childItem, result);
            }
        }

        if (cascaderViewItem.ItemCount == 0)
        {
            result.Add(GetFullPath(cascaderViewItem));
        }
    }
    
    private void CollectionPaths(ICascaderViewItemData cascaderViewItemData, List<CascaderViewFilterListItemData> result)
    {
        Debug.Assert(Filter != null);
        foreach (var childItem in cascaderViewItemData.Children)
        {
            CollectionPaths(childItem, result);
        }

        if (cascaderViewItemData.Children.Count == 0)
        {
            result.Add(GetFullPath(cascaderViewItemData));
        }
    }
    
    private void ClearFilter()
    {
        IsFiltering       = false;
        FilterResultCount = 0;
        FilterValue   = null;
        FilteredPathInfos = null;
        _allPathInfos     = null;
    }

    private void HandleFilterListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        IList? paths = null;
        if (_filterList?.SelectedItem is CascaderViewFilterListItemData itemData)
        {
            paths = itemData.ExpandItems;
        }
        ClearFilter();
        if (paths?.Count > 0)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                foreach (var path in paths)
                {
                    if (path is ICascaderViewItemData cascaderViewItemData)
                    {
                        cascaderViewItemData.IsExpanded = true;
                    }
                    else if (path is CascaderViewItem cascaderViewItem)
                    {
                       await ExpandItemAsync(cascaderViewItem);
                    }
                }
            });
        }
    }
}