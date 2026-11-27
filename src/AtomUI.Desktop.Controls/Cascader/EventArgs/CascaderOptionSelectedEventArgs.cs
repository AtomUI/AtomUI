namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="CascaderView.OptionSelected" /> event, carrying the option that was picked through the
/// view.
/// </summary>
public class CascaderOptionSelectedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="ICascaderOption" /> that was picked through the view.
    /// </summary>
    public ICascaderOption Option { get; }

    /// <summary>
    /// Creates the event data that reports <paramref name="option" /> as the picked option.
    /// </summary>
    public CascaderOptionSelectedEventArgs(ICascaderOption option)
    {
        Option = option;
    }
}