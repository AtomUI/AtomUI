using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractTitleBarActionToken : AbstractControlDesignToken
{
    public Color ActiveBgColor { get; set; }
    public Color ActiveColor { get; set; }
    public Color ActiveHoverBgColor { get; set; }
    public Color ActivePressedBgColor { get; set; }
    public Thickness CaptionButtonBackgroundInset { get; set; }
    public double CaptionButtonIconSize { get; set; }
    public Thickness CaptionButtonPadding { get; set; }
    public Color InactiveBgColor { get; set; }
    public Color InactiveColor { get; set; }
    public Color InactiveHoverBgColor { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ActiveBgColor = EffectiveGlobalToken.ColorFillTertiary;
        ActiveColor = EffectiveGlobalToken.ColorTextSecondary;
        ActiveHoverBgColor = EffectiveGlobalToken.ColorFillSecondary;
        ActivePressedBgColor = EffectiveGlobalToken.ColorFill;
        CaptionButtonBackgroundInset = new Thickness(EffectiveGlobalToken.SizeUnit / 2);
        InactiveBgColor = EffectiveGlobalToken.ColorFillQuaternary;
        InactiveColor = EffectiveGlobalToken.ColorTextQuaternary;
        InactiveHoverBgColor = EffectiveGlobalToken.ColorFillTertiary;
    }

    protected static CornerRadius CalculateCaptionCornerRadius(double iconSize, Thickness padding) =>
        new((iconSize + padding.Top + padding.Bottom) / 2);
}
