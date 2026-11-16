using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MenuFlyoutPresenterToken : AbstractMenuPopupToken
{
    /// <summary>
    /// The border of the menu flyout's arrow box, drawn when the arrow box has a border
    /// (<c>ArrowDecoratedBox</c> <c>ContentBorderThickness</c>). Defaults to <c>ColorBorderSecondary</c>, the
    /// ArrowDecoratedBox's own <c>ContentBorderColor</c> default, so a theme can give menus a border colour of their own.
    /// </summary>
    public partial Color MenuPopupBorderColor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        MenuPopupBorderColor = EffectiveGlobalToken.ColorBorderSecondary;
        if (!IsExplicit(MenuFlyoutPresenterTokenKind.ItemColor))
        {
            ItemColor = EffectiveGlobalToken.ColorText;
        }
        ItemHoverColor = ItemColor;
    }
}
