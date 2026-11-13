using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ContextMenuToken : AbstractMenuPopupToken
{
    public partial double ContextMenuOffsetX { get; set; }
    public partial double ContextMenuOffsetY { get; set; }
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        if (!IsExplicit(ContextMenuTokenKind.ItemColor))
        {
            ItemColor = EffectiveGlobalToken.ColorText;
        }
        ItemHoverColor = ItemColor;
        ContextMenuOffsetX = EffectiveGlobalToken.SpacingXXS;
        ContextMenuOffsetY = EffectiveGlobalToken.SpacingXXS;
    }
}
