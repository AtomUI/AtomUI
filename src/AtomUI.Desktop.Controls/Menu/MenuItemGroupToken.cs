using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MenuItemGroupToken : AbstractControlDesignToken
{
    /// <summary>
    /// 分组标题字重
    /// Font weight of the group title
    /// </summary>
    public partial FontWeight TitleFontWeight { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TitleFontWeight = FontWeight.Normal;
    }
}
