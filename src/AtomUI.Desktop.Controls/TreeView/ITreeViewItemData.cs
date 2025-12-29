using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public interface ITreeViewItemData : ITreeNode<ITreeViewItemData>
{
    bool? IsChecked { get; }
    bool IsSelected { get; }
    bool IsExpanded { get; }
    bool IsIndicatorEnabled { get; }
    string? GroupName { get; }
    bool IsLeaf { get; }

    void UpdateParentNode(ITreeViewItemData? parentNode)
    {
        throw new NotImplementedException();
    }
}
