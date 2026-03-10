using Avalonia.Controls;

namespace AtomUI.Controls;

public interface ITreeNode<TChild> : IItemKey
    where TChild : class, ITreeNode<TChild>
{
    ITreeNode<TChild>? ParentNode { get; }
    object? Header { get; }
    PathIcon? Icon { get; }
    bool IsEnabled { get; }
    IList<TChild> Children { get; }
}