using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public interface ITreeViewItemData : ITreeNode<ITreeViewItemData>
{
    bool? IsChecked { get; set; }
    bool IsSelected { get; set; }
    bool IsExpanded { get; set; }
    bool IsIndicatorEnabled { get; set; }
    string? GroupName { get; }
    bool IsLeaf { get; }
    object? Value { get; set; }

    void UpdateParentNode(ITreeViewItemData? parentNode)
    {
        throw new NotImplementedException();
    }
}
