// Keep the CLR name used by existing Desktop clients.
namespace AtomUI.Desktop.Controls;

public interface ICollapseItemData
{
    bool IsSelected { get; }
    bool IsEnabled { get; }
    bool IsShowExpandIcon { get; }
    object? Header { get; }
}
