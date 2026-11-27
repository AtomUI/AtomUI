using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractTagToken" /> layer.
/// </summary>
public static class TagTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the shared <see cref="AbstractTagToken" /> layer to
    /// <paramref name="token" />: the tag font, icon size and paddings. The Tag and CheckableTag rules call it.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractTagToken token)
    {
        token.TagFontSize          = global.FontSizeSM;
        token.TagFontWeight        = FontWeight.Normal;
        token.TagLineHeight        = global.FontHeightSM;
        token.TagIconSize          = global.FontSizeIcon;
        token.TagPadding           = new Thickness(global.SizeXS - 1, 0);
        token.TagTextPaddingInline = new Thickness(global.UniformlyPaddingXXS, 0);
    }
}
