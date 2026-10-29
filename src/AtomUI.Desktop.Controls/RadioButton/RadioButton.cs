using AtomUI.Controls.Commons;
using AtomUI.Theme.Resources;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public partial class RadioButton : AbstractRadioButton,
    IThemeInheritanceSource
{
    public RadioButton()
    {
    }

    // AXAML cannot read AvaloniaObject's protected, potentially non-logical parent.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;
}