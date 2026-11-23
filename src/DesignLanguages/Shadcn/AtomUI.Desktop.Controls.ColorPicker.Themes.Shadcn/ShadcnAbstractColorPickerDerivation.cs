using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractColorPickerToken"/> layer.</summary>
public static class ShadcnAbstractColorPickerDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractColorPickerToken token)
    {
        token.ColorBlockDisabledOpacity = 0.4;
        token.ColorBlockInnerShadows = new BoxShadows(new BoxShadow { IsInset = true, Spread = global.LineWidth, Color = global.ColorFillSecondary });
        token.TransparentBgSize = global.SizeXS;
        token.TriggerPadding = new Thickness(global.UniformlyPaddingXXS - global.LineWidth);
        token.TriggerTextMargin = new Thickness(global.UniformlyMarginXS, 0, global.UniformlyMarginXS - global.UniformlyPaddingXXS + global.LineWidth, 0);
    }
}
