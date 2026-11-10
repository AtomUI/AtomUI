using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class ContextMenuToken : AbstractMenuPopupToken
{
    public double ContextMenuOffsetX { get; set; }
    public double ContextMenuOffsetY { get; set; }
    public override void CalculateTokenValues(bool isDarkMode)
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
