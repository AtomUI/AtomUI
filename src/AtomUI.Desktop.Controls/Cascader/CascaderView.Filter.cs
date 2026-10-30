using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public partial class CascaderView
{
    #region 内部属性定义

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
    
    #endregion

    private List<CascaderViewFilterListItemData>? _allPathInfos;
    private CascaderViewFilterList? _filterList;
    
    public void FilterItems()
    {
        var hasFilterValue = FilterValue switch
        {
            null        => false,
            string text => !string.IsNullOrWhiteSpace(text),
            _           => true
        };
        if (Filter != null && hasFilterValue && IsLoaded)
        {
            if (_allPathInfos == null)
            {
                var result = new List<CascaderViewFilterListItemData>(_options.Count);
                foreach (var item in _options)
                {
                    if (item is ICascaderOption viewOption)
                    {
                        CollectionPaths(viewOption, result);
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
            
            var selector = FilterValueSelector ?? DefaultCascaderFilterValueSelector;
            var filteredPathInfos = new List<CascaderViewFilterListItemData>(_allPathInfos.Count);
            foreach (var pathInfo in _allPathInfos)
            {
                if (Filter.Filter(selector(pathInfo), FilterValue))
                {
                    filteredPathInfos.Add(pathInfo);
                }
            }
            var previousCandidate = _filterList?.CandidateSelectedItem as CascaderViewFilterListItemData;
            SetKeyboardCandidateOption(null);
            _filterList?.ClearCandidate();
            FilteredPathInfos = filteredPathInfos;
            IsFiltering       = true;
            FilterResultCount = FilteredPathInfos.Count;
            _filterList?.RestoreCandidate(previousCandidate);
        }
        else
        {
            ClearFilter();
        }
    }

    private CascaderViewFilterListItemData GetFullPath(ICascaderOption option)
    {
        var pathDepth   = CountPathDepth(option);
        var pathHeaders = new string[pathDepth];
        var current     = option;
        var pathNodes   = new ICascaderOption[pathDepth];
        var isEnabled   = true;
        for (var i = pathDepth - 1; current != null; i--)
        {
            pathNodes[i]   = current;
            pathHeaders[i] = current.Header?.ToString() ?? string.Empty;
            isEnabled     &= current.IsEnabled;
            current        = current.ParentNode as ICascaderOption;
        }

        return new CascaderViewFilterListItemData()
        {
            Content     = string.Join('/', pathHeaders),
            ExpandItems = pathNodes,
            IsEnabled   = isEnabled
        };
    }

    private static int CountPathDepth(ICascaderOption option)
    {
        var count   = 0;
        var current = option;
        while (current != null)
        {
            count++;
            current = current.ParentNode as ICascaderOption;
        }

        return count;
    }
    
    private void CollectionPaths(ICascaderOption option, List<CascaderViewFilterListItemData> result)
    {
        Debug.Assert(Filter != null);
        if (!option.HasChildren() || IsAllowSelectParent || IsCheckable)
        {
            result.Add(GetFullPath(option));
        }
        foreach (var childItem in option.Children)
        {
            CollectionPaths(childItem, result);
        }
    }
    
    public void ClearFilter()
    {
        _filterList?.ClearCandidate();
        IsFiltering       = false;
        FilterResultCount = 0;
        SetCurrentValue(FilterValueProperty, null);
        FilteredPathInfos = null;
        _allPathInfos     = null;
    }

    #region 内部协作 API

    internal bool TryMoveFilterCandidate(int delta)
    {
        if (!IsFiltering || _filterList == null)
        {
            return false;
        }

        return _filterList.TryMoveCandidate(delta);
    }

    internal bool TryCommitFilterCandidate()
    {
        if (!IsFiltering || _filterList == null)
        {
            return false;
        }

        var itemData = _filterList.CandidateSelectedItem as CascaderViewFilterListItemData;
        return itemData != null && TrySelectFilterResult(itemData);
    }

    #endregion

    private void HandleFilterListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_filterList?.SelectedItem is CascaderViewFilterListItemData itemData)
        {
            TrySelectFilterResult(itemData);
        }
    }

    private bool TrySelectFilterResult(CascaderViewFilterListItemData itemData)
    {
        var paths = itemData.ExpandItems;
        if (!itemData.IsEnabled || paths == null || paths.Count == 0)
        {
            return false;
        }
        var target = paths[^1];
        if (!IsOptionInSource(target) || !IsOptionEnabled(target))
        {
            return false;
        }
        if (IsCheckable)
        {
            if (!TryCommitOption(target))
            {
                return false;
            }
            ClearFilter();
            SetKeyboardCandidateOption(target);
            return true;
        }
        if (!target.IsEffectiveLeaf() && !IsAllowSelectParent)
        {
            return false;
        }
        ClearFilter();
        if (!TryCommitOption(target))
        {
            return false;
        }
        CloseRequested?.Invoke(this, EventArgs.Empty);
        return true;
    }
}
