using AtomUI.Desktop.Controls.Converters;
using Avalonia.Styling;
using MarginMultiplierConverter = Avalonia.Controls.Converters.MarginMultiplierConverter;

namespace AtomUI.Desktop.Controls.Themes;

internal class TreeViewItemTheme : ControlTheme
{
    public static readonly MarginMultiplierConverter MarginMultiplierConverter = new ()
    {
        Left   = true,
        Indent = 16
    };

    public static readonly TreeViewItemIndicatorEnabledConverter TreeViewItemIndicatorEnabledConverter = new ();
    
}