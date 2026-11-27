using AtomUI.Controls.Commons;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Desktop option button group. It adds nothing to <see cref="AbstractOptionButtonGroup" /> beyond creating
/// <see cref="OptionButton" /> containers, and is the type that the desktop theme targets.
/// </summary>
public class OptionButtonGroup : AbstractOptionButtonGroup
{
    public OptionButtonGroup()
    {
    }
    
    /// <summary>
    /// Creates an <see cref="OptionButton" /> as the container for an item. <paramref name="item" />,
    /// <paramref name="index" /> and <paramref name="recycleKey" /> are not read.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new OptionButton();
    }
}