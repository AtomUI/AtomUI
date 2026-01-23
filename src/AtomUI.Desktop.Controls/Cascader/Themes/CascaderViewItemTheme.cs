using AtomUI.Desktop.Controls.Converters;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

internal class CascaderViewItemTheme : ControlTheme
{
    public static readonly CascaderViewItemIndicatorEnabledConverter CascaderViewItemIndicatorEnabledConverter = new ();
    public static readonly CascaderViewItemExpanderIsVisibleConverter CascaderViewItemExpanderIsVisibleConverter = new ();
}