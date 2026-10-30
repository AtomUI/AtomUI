using AtomUI.Controls;
using AtomUI.Controls.Data;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public partial class TreeTransfer : AbstractTransfer
{
    #region 公共属性定义
    public static readonly StyledProperty<ITransferTreeView?> SourceViewProperty =
        AvaloniaProperty.Register<TreeTransfer, ITransferTreeView?>(nameof(SourceView));
    
    public ITransferTreeView? SourceView
    {
        get => GetValue(SourceViewProperty);
        set => SetValue(SourceViewProperty, value);
    }
    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (SourceView == null)
        {
            SetCurrentValue(SourceViewProperty, new TransferTreeView());
        }
        SyncSourceViewMask();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourceViewProperty)
        {
            SyncSourceViewMask();
        }
    }

    protected override void ConfigurePanelItemsSourceForFilter(FilterChangeType changeType)
    {
        var               sourcePanelSourceChanged = false;
        var               targetPanelSourceChanged = false;
        IList<EntityKey>? sourceItemKeys           = null;
        IList<EntityKey>? targetItemKeys           = null;
        var               targetKeySet             = BuildTargetKeySet(TargetKeys);
        var               sourceChanged            = (changeType & FilterChangeType.Source) == FilterChangeType.Source;
        var               targetChanged            = (changeType & FilterChangeType.Target) == FilterChangeType.Target;
        if (sourceChanged)
        {
            var sourcePanelSource = ItemsSource;
            sourcePanelSourceChanged = SourceViewSource != sourcePanelSource;
            SourceViewSource         = sourcePanelSource;
            sourceItemKeys           = BuildItemKeyList(sourcePanelSource);
            SourceView?.SetMaskedItems(TargetKeys);
        }

        if (targetChanged)
        {
            IReadOnlyList<IListItemData> targetPanelSource = targetKeySet == null || ItemsSource == null
                ? Array.Empty<IListItemData>()
                : CalculateTargetItemsSource(ItemsSource, targetKeySet);
            targetPanelSourceChanged = TargetViewSource != targetPanelSource;
            TargetViewSource         = targetPanelSource;
            targetItemKeys           = BuildItemKeyList(targetPanelSource);
        }

        if (sourcePanelSourceChanged || targetPanelSourceChanged)
        {
            NotifySelectionChanged(sourceItemKeys, targetItemKeys);
        }
    }

    private List<IListItemData> CalculateTargetItemsSource(IEnumerable<IItemKey>? itemNodes, ISet<EntityKey> targetKeySet)
    {
        var results = new List<IListItemData>(targetKeySet.Count);
        CollectTargetItemsSource(itemNodes, targetKeySet, results);
        return results;
    }

    private void CollectTargetItemsSource(
        IEnumerable<IItemKey>? itemNodes,
        ISet<EntityKey> targetKeySet,
        IList<IListItemData> results)
    {
        if (itemNodes != null)
        {
            foreach (var itemNode in itemNodes)
            {
                var node = (ITreeItemNode)itemNode;
                if (targetKeySet.Contains(node.ItemKey ?? default))
                {
                    var item = new ListItemData
                    {
                        ItemKey = node.ItemKey ?? default,
                        Content = node.Header
                    };
                    if (IsTargetFilterMatched(item))
                    {
                        results.Add(item);
                    }
                }
                CollectTargetItemsSource(node.Children, targetKeySet, results);
            }
        }
    }

    private bool IsTargetFilterMatched(IListItemData item)
    {
        return !IsFilterEnabled ||
               string.IsNullOrEmpty(TargetFilterValue) ||
               (Filter?.Filter(FilterValueSelector != null ? FilterValueSelector(item) : item,
                   TargetFilterValue) ?? false);
    }

    private void SyncSourceViewMask()
    {
        SourceView?.SetMaskedItems(TargetKeys);
    }
    
}
