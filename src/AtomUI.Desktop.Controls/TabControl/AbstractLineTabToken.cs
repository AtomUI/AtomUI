using AtomUI.Theme.DesignTokens;
using AtomUI.Media;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractLineTabToken : AbstractTabToken
{
    public partial Color InkBarColor { get; set; }
    public partial double InkBarThickness { get; set; }
    public partial double HorizontalItemGutter { get; set; }
    public partial Thickness HorizontalItemPadding { get; set; }
    public partial Thickness HorizontalItemPaddingLG { get; set; }
    public partial Thickness HorizontalItemPaddingSM { get; set; }
    public partial double VerticalItemGutter { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        InkBarColor = EffectiveGlobalToken.ColorPrimary;
        InkBarThickness = EffectiveGlobalToken.LineWidthBold;
        HorizontalItemGutter = 32;
        HorizontalItemPadding = new Thickness(0, EffectiveGlobalToken.UniformlyPaddingSM);
        HorizontalItemPaddingLG = new Thickness(0, EffectiveGlobalToken.UniformlyPadding);
        HorizontalItemPaddingSM = new Thickness(0, EffectiveGlobalToken.UniformlyPaddingXS);
        VerticalItemGutter = EffectiveGlobalToken.SpacingXXS;
    }
}
