using AtomUI.Theme.DesignTokens;
using AtomUI.Media;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractLineTabToken : AbstractTabToken
{
    public Color InkBarColor { get; set; }
    public double InkBarThickness { get; set; }
    public double HorizontalItemGutter { get; set; }
    public Thickness HorizontalItemPadding { get; set; }
    public Thickness HorizontalItemPaddingLG { get; set; }
    public Thickness HorizontalItemPaddingSM { get; set; }
    public double VerticalItemGutter { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
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
