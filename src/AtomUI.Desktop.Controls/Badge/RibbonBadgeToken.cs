using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class RibbonBadgeToken : AbstractControlDesignToken
{
    public partial Point Offset { get; set; }

    public partial ImmutableTransform? CornerTransform { get; set; }

    public partial int CornerDarkenAmount { get; set; }

    public partial Thickness TextPadding { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        Offset             = new Point(EffectiveGlobalToken.UniformlyMarginXS, EffectiveGlobalToken.UniformlyMarginXS);
        CornerTransform    = new ScaleTransform(1, 0.75).ToImmutable();
        CornerDarkenAmount = 15;
        TextPadding        = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, 0);
    }
}
