using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractTagToken" /> layer.
/// </summary>
public static class TagTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values into the shared tag layer: the font size, weight and line height of the tag, the
    /// icon size and the padding of the tag and of its text.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractTagToken token)
    {
        token.TagFontSize          = global.FontSizeSM;
        token.TagFontWeight        = FontWeight.Normal;
        token.TagLineHeight        = global.FontHeightSM;
        token.TagIconSize          = global.FontSizeIcon;
        token.TagPadding           = new Thickness(global.SizeXS - 1, 0);
        token.TagTextPaddingInline = new Thickness(global.UniformlyPaddingXXS, 0);
    }
}
