using AtomUI.Controls;
using AtomUI.Controls.Data;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public class TreeTransfer : AbstractTransfer
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
    }

    protected override void ConfigurePanelItemsSourceForFilter(FilterChangeType changeType)
    {
        var               sourcePanelSourceChanged = false;
        var               targetPanelSourceChanged = false;
        IList<EntityKey>? sourceItemKeys           = null;
        IList<EntityKey>? targetItemKeys           = null;
        if (changeType.HasFlag(FilterChangeType.Source))
        {
            var sourcePanelSource = ItemsSource?.Cast<ITreeItemNode>().ToArray();
            sourcePanelSourceChanged = SourceViewSource != sourcePanelSource;
            SourceViewSource        = sourcePanelSource;
            sourceItemKeys           = sourcePanelSource?.Select(item => item.ItemKey ?? default).ToList();
        }

        if (changeType.HasFlag(FilterChangeType.Target))
        {
            var targetPanelSource = CalculateTargetItemsSource(ItemsSource?.Cast<ITreeItemNode>().ToList())
                .Where(item => !IsFilterEnabled || string.IsNullOrEmpty(TargetFilterValue) || 
                               (Filter?.Filter(FilterValueSelector != null ? FilterValueSelector(item) : item, TargetFilterValue) ?? false))
                .ToArray();
            TargetViewSource        = targetPanelSource;
            targetPanelSourceChanged = TargetViewSource != targetPanelSource;
            targetItemKeys           = targetPanelSource.Select(item => item.ItemKey ?? default).ToList();
        }

        if (sourcePanelSourceChanged || targetPanelSourceChanged)
        {
            NotifySelectionChanged(sourceItemKeys, targetItemKeys);
        }
    }

    private List<IListItemData> CalculateTargetItemsSource(IList<ITreeItemNode>? itemNodes)
    {
        var results = new List<IListItemData>();
        if (itemNodes != null)
        {
            foreach (var node in itemNodes)
            {
                if (TargetKeys?.Contains(node.ItemKey ?? default) ?? false)
                {
                    results.Add(new ListItemData
                    {
                        ItemKey = node.ItemKey ?? default,
                        Content = node.Header
                    });
                }
                var children = CalculateTargetItemsSource(node.Children);
                results.AddRange(children);
            }
        }
        return results;
    }
}