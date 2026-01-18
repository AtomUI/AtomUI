using AtomUI.Desktop.Controls.Converters;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

internal class CascaderViewItemTheme : ControlTheme
{
    public static readonly TreeViewItemIndicatorEnabledConverter CascaderViewItemIndicatorEnabledConverter = new ();
}