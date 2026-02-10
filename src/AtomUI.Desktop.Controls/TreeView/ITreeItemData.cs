using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public interface ITreeItemData : ITreeNode<ITreeItemData>
{
    bool? IsChecked { get; set; }
    bool IsSelected { get; set; }
    bool IsExpanded { get; set; }
    bool IsIndicatorEnabled { get; set; }
    string? GroupName { get; }
    bool IsLeaf { get; }
    object? Value { get; set; }

    void UpdateParentNode(ITreeItemData? parentNode)
    {
        throw new NotImplementedException();
    }
}
