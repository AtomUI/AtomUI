namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="CascaderView.SelectedOptionsChanged" /> event. Both lists are the instances that the
/// <c>SelectedOptions</c> property held before and after the change, not copies.
/// </summary>
public class CascaderOptionsSelectedChangedEventArgs : EventArgs
{
    /// <summary>
    /// The selected options before the change, or <see langword="null" /> when no list was set before it.
    /// </summary>
    public IList<ICascaderOption>? OldOptions { get; }
    /// <summary>
    /// The selected options after the change, or <see langword="null" /> when no list is set after it.
    /// </summary>
    public IList<ICascaderOption>? NewOptions { get; }
    
    /// <summary>
    /// Creates the event data for a change from <paramref name="oldOptions" /> to <paramref name="newOptions" />.
    /// </summary>
    public CascaderOptionsSelectedChangedEventArgs(IList<ICascaderOption>? oldOptions, IList<ICascaderOption>? newOptions)
    {
        OldOptions = oldOptions;
        NewOptions = newOptions;
    }
}