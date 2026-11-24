using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractTagToken"/> layer.</summary>
public static class TagTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractTagToken token)
    {
        token.TagFontSize          = global.FontSizeSM;
        token.TagFontWeight        = FontWeight.Normal;
        token.TagLineHeight        = global.FontHeightSM;
        token.TagIconSize          = global.FontSizeIcon;
        token.TagPadding           = new Thickness(global.SizeXS - 1, 0);
        token.TagTextPaddingInline = new Thickness(global.UniformlyPaddingXXS, 0);
    }
}
