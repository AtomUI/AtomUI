namespace AtomUI.Controls.Commons;

/// <summary>
/// Reports whether the application, or the part of it a visual subtree belongs to, is active enough to run
/// continuous motion. Inactive covers a backgrounded app or a host that is not currently shown.
/// </summary>
public interface IMotionActivitySource
{
    bool IsActive { get; }

    event EventHandler? IsActiveChanged;
}
