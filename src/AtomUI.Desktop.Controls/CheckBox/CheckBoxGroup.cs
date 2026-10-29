using AtomUI.Controls.Commons;
using AtomUI.Theme.Resources;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public class CheckBoxGroup : AbstractCheckBoxGroup,
    IThemeInheritanceSource
{
    public CheckBoxGroup()
    {
    }

    // AXAML cannot read AvaloniaObject's protected, potentially non-logical parent.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;
}