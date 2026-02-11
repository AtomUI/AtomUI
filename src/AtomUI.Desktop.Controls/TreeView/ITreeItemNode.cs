using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public interface ITreeItemNode : ITreeNode<ITreeItemNode>
{
    bool? IsChecked { get; set; }
    bool IsSelected { get; set; }
    bool IsExpanded { get; set; }
    bool IsIndicatorEnabled { get; set; }
    string? GroupName { get; }
    bool IsLeaf { get; }
    object? Value { get; set; }

    void UpdateParentNode(ITreeItemNode? parentNode)
    {
        throw new NotImplementedException();
    }
}
