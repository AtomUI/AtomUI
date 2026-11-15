using System.Collections;
using System.Collections.Specialized;
using Avalonia;

namespace AtomUI.Desktop.Controls;

// SelectedItems owns the selection. Node data mirrors it: a virtualizing panel prepares containers from the
// node data, and a node without a container cannot receive Avalonia's container marking, so a selection
// changed while a node is not realized (keyboard ranges, code) must reach the node itself. Compared by
// reference: TreeItemNode is a record whose equality includes its mutable state.
public partial class TreeView
{
    private readonly HashSet<object> _selectedNodeMirror = new(ReferenceEqualityComparer.Instance);
    private INotifyCollectionChanged? _observedSelectedItems;

    // Attach-scoped so that a SelectedItems collection owned by the application never keeps a detached
    // TreeView alive; changes made while detached are reconciled on the next attach.
    private void ObserveSelectedItems()
    {
        var selectedItems = base.SelectedItems as INotifyCollectionChanged;
        if (!ReferenceEquals(selectedItems, _observedSelectedItems))
        {
            StopObservingSelectedItems();
            _observedSelectedItems = selectedItems;
            if (selectedItems is not null)
            {
                selectedItems.CollectionChanged += HandleSelectedItemsCollectionChanged;
            }
        }

        ReconcileSelectedNodeMirror();
    }

    private void StopObservingSelectedItems()
    {
        if (_observedSelectedItems is not null)
        {
            _observedSelectedItems.CollectionChanged -= HandleSelectedItemsCollectionChanged;
            _observedSelectedItems = null;
        }
    }

    private void HandleSelectedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                MirrorSelectedNodes(e.NewItems, true);
                break;
            case NotifyCollectionChangedAction.Remove:
                MirrorSelectedNodes(e.OldItems, false);
                break;
            case NotifyCollectionChangedAction.Replace:
                MirrorSelectedNodes(e.OldItems, false);
                MirrorSelectedNodes(e.NewItems, true);
                break;
            default:
                ReconcileSelectedNodeMirror();
                break;
        }
    }

    private void MirrorSelectedNodes(IList? items, bool isSelected)
    {
        if (items is null)
        {
            return;
        }

        for (var i = 0; i < items.Count; i++)
        {
            if (items[i] is { } item)
            {
                MirrorSelectedNode(item, isSelected);
            }
        }
    }

    private void ReconcileSelectedNodeMirror()
    {
        var current = new HashSet<object>(ReferenceEqualityComparer.Instance);
        foreach (var item in base.SelectedItems)
        {
            if (item is not null)
            {
                current.Add(item);
            }
        }

        foreach (var item in _selectedNodeMirror.ToArray())
        {
            if (!current.Contains(item))
            {
                MirrorSelectedNode(item, false);
            }
        }

        foreach (var item in current)
        {
            MirrorSelectedNode(item, true);
        }
    }

    private void MirrorSelectedNode(object item, bool isSelected)
    {
        if (isSelected)
        {
            _selectedNodeMirror.Add(item);
        }
        else
        {
            _selectedNodeMirror.Remove(item);
        }

        // Item containers (TreeViewItem items) are marked by Avalonia directly.
        if (item is ITreeItemNode node and not Visual && node.IsSelected != isSelected)
        {
            node.IsSelected = isSelected;
        }
    }
}
