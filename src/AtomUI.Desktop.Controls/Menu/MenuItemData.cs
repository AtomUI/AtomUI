using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

public interface IMenuItemData : ITreeNode<IMenuItemData>
{
    KeyGesture? InputGesture { get; }
}

public class MenuItemData : IMenuItemData
{
    public ITreeNode<IMenuItemData>? ParentNode { get; internal set; }
    public EntityKey? ItemKey { get; init; }
    public object? Header { get; init; }
    public PathIcon? Icon { get; init; }
    public bool IsEnabled { get; init; } = true;
    public KeyGesture? InputGesture { get; init; }
    
    private IList<IMenuItemData> _children = [];
    public IList<IMenuItemData> Children
    {
        get => _children;
        init
        {
            _children = value;
            foreach (var child in _children)
            {
                if (child is MenuItemData item)
                {
                    item.ParentNode = this;
                }
            }
        }
    }
}