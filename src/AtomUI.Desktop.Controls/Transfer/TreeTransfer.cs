using AtomUI.Controls;
using AtomUI.Controls.Data;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A transfer whose items are tree nodes. The source panel is the <see cref="SourceView" /> tree view; the target
/// panel lists the nodes whose keys are in <see cref="AbstractTransfer.TargetKeys" /> and pass the target filter.
/// </summary>
public partial class TreeTransfer : AbstractTransfer
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="SourceView" /> property.
    /// </summary>
    public static readonly StyledProperty<ITransferTreeView?> SourceViewProperty =
        AvaloniaProperty.Register<TreeTransfer, ITransferTreeView?>(nameof(SourceView));
    
    /// <summary>
    /// The tree view that lists the source items. The default is <see langword="null" />; when the value is still
    /// <see langword="null" /> after the control template is applied, a new <see cref="TransferTreeView" /> is created
    /// for it. The view masks the items whose keys are in <see cref="AbstractTransfer.TargetKeys" />; the mask is
    /// applied when the view is set and when the source panel is rebuilt.
    /// </summary>
    public ITransferTreeView? SourceView
    {
        get => GetValue(SourceViewProperty);
        set => SetValue(SourceViewProperty, value);
    }
    #endregion

    /// <summary>
    /// Calls the base implementation, applies the current mask to the source view and creates a new
    /// <see cref="TransferTreeView" /> when <see cref="SourceView" /> is still <see langword="null" />.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (SourceView == null)
        {
            SetCurrentValue(SourceViewProperty, new TransferTreeView());
        }
        SyncSourceViewMask();
    }

    /// <summary>
    /// Calls the base implementation and applies the current mask to the source view whenever
    /// <see cref="SourceView" /> changes.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourceViewProperty)
        {
            SyncSourceViewMask();
        }
    }

    /// <summary>
    /// Rebuilds the item source of the panels that <paramref name="changeType" /> names. The source view lists
    /// <c>ItemsSource</c> and masks the items whose keys are in <see cref="AbstractTransfer.TargetKeys" />. The target
    /// panel lists the nodes whose keys are in <see cref="AbstractTransfer.TargetKeys" /> and that pass the target
    /// filter. When the item source of a panel changed, the transfer raises
    /// <see cref="AbstractTransfer.SelectionChanged" />.
    /// </summary>
    /// <param name="changeType">
    /// The panels to rebuild: the source, the target or both.
    /// </param>
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
