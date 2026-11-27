using AtomUI.Controls.Commons;
using Avalonia.Interactivity;

namespace AtomUI.Controls;

/// <summary>
/// Describes the option that became checked in an option button group.
/// </summary>
public class OptionCheckedChangedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the arguments for the <see cref="AbstractOptionButtonGroup.OptionCheckedChanged" /> event.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event to raise, <see cref="AbstractOptionButtonGroup.OptionCheckedChangedEvent" />.
    /// </param>
    /// <param name="option">
    /// The option button that became checked.
    /// </param>
    /// <param name="index">
    /// The index of the option among the items of the group.
    /// </param>
    public OptionCheckedChangedEventArgs(RoutedEvent routedEvent, AbstractOptionButton option, int index)
        : base(routedEvent)
    {
        CheckedOption = option;
        Index         = index;
    }

    /// <summary>
    /// The option button that became checked.
    /// </summary>
    public AbstractOptionButton CheckedOption { get; }
    /// <summary>
    /// The index of <see cref="CheckedOption" /> among the items of the group.
    /// </summary>
    public int Index { get; }
}