using AtomUI.Controls.Commons;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public class OptionButtonGroup : AbstractOptionButtonGroup,
    IThemeInheritanceSource
{
    public OptionButtonGroup()
    {
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new OptionButton();
    }

    // AXAML cannot read AvaloniaObject's protected, potentially non-logical parent.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;
}