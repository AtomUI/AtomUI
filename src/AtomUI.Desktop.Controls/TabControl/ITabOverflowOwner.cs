using System.Collections.Specialized;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal interface ITabOverflowOwner
{
    int OverflowItemCount { get; }
    int OverflowSelectedIndex { get; }
    INotifyCollectionChanged OverflowItems { get; }
    event EventHandler? OverflowSelectionChanged;

    Control? GetOverflowContainer(int index);
    object? GetOverflowLogicalItem(int index);
    TabOverflowItem CreateOverflowItem(int index, object? logicalItem, Control container);
    bool TryActivateOverflowItem(int index, object? logicalItem, Control container);
    bool TryCloseOverflowItem(int index, object? logicalItem, Control container);
}
