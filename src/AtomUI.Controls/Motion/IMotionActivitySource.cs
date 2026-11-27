namespace AtomUI.Controls.Commons;

/// <summary>
/// Reports whether the application, or the part of it a visual subtree belongs to, is active enough to run
/// continuous motion. Inactive covers a backgrounded app or a host that is not currently shown.
/// </summary>
public interface IMotionActivitySource
{
    /// <summary>
    /// Whether continuous motion may run in the scope of this source. A <see cref="MotionGate" /> lets its owner's
    /// animation run only while the value is <see langword="true" />.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Occurs after <see cref="IsActive" /> changed, with the new value already stored. The arguments carry no data,
    /// so a handler reads <see cref="IsActive" />.
    /// </summary>
    event EventHandler? IsActiveChanged;
}
