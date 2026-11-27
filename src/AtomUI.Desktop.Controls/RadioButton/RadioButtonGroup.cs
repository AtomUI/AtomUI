using AtomUI.Controls.Commons;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Desktop radio button group. It adds nothing to <see cref="AbstractRadioButtonGroup" /> beyond creating
/// <see cref="RadioButton" /> containers, and is the type that the desktop theme targets.
/// </summary>
public class RadioButtonGroup : AbstractRadioButtonGroup
{
    public RadioButtonGroup()
    {
    }
    
    /// <summary>
    /// Creates a <see cref="RadioButton" /> as the container for an item. <paramref name="item" />,
    /// <paramref name="index" /> and <paramref name="recycleKey" /> are not read.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new RadioButton();
    }
}