using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTitleBarActionToken : AbstractControlDesignToken
{
    public partial Color ActiveBgColor { get; set; }
    public partial Color ActiveColor { get; set; }
    public partial Color ActiveHoverBgColor { get; set; }
    public partial Color ActivePressedBgColor { get; set; }
    public partial Thickness CaptionButtonBackgroundInset { get; set; }
    public partial double CaptionButtonIconSize { get; set; }
    public partial Thickness CaptionButtonPadding { get; set; }
    public partial Color InactiveBgColor { get; set; }
    public partial Color InactiveColor { get; set; }
    public partial Color InactiveHoverBgColor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
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
