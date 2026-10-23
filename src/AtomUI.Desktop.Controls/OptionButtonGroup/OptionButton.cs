using AtomUI.Controls.Commons;
using AtomUI.Theme.Resources;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public class OptionButton : AbstractOptionButton,
    IThemeInheritanceSource
{
    public OptionButton()
    {
    }

    // AXAML cannot read AvaloniaObject's protected, potentially non-logical parent.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;
}