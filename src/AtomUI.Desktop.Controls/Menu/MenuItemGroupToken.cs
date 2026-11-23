using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MenuItemGroupToken : AbstractControlDesignToken
{
    /// <summary>Font weight of the heading above a group of menu items.</summary>
    public partial FontWeight TitleFontWeight { get; set; }
}
