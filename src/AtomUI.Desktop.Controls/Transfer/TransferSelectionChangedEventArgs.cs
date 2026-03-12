using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public class TransferSelectionChangedEventArgs : EventArgs
{
    public IList<EntityKey>? SourceSelectedKeys { get; }
    public IList<EntityKey>? TargetSelectedKeys { get; }

    public TransferSelectionChangedEventArgs(IList<EntityKey>? sourceSelectedKeys, IList<EntityKey>? targetSelectedKeys)
    {
        SourceSelectedKeys = sourceSelectedKeys;
        TargetSelectedKeys = targetSelectedKeys;
    }
}