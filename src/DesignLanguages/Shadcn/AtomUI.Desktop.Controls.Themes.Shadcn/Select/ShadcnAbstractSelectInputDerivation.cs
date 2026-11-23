using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractSelectInputToken"/> layer.</summary>
public static class ShadcnAbstractSelectInputDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractSelectInputToken token)
    {

        // Item height default use `controlHeight - 2 * paddingXXS`,
        // but some case `paddingXXS=0`.
        // Let's fallback it.
        double dblPaddingXXS      = global.UniformlyPaddingXXS * 2;
        double dblLineWidth       = global.LineWidth * 2;
        double multipleItemHeight = Math.Min(global.ControlHeight - dblPaddingXXS, global.ControlHeight - dblLineWidth);
        double multipleItemHeightSM = Math.Min(global.ControlHeightSM - dblPaddingXXS, global.ControlHeightSM - dblLineWidth);
        double multipleItemHeightLG = Math.Min(global.ControlHeightLG - dblPaddingXXS, global.ControlHeightLG - dblLineWidth);

        token.OptionFontSize =  global.FontSize;
        token.InputFontSizeSM = global.FontSize;
        token.MultipleItemBg = global.ColorFillSecondary;
        token.MultipleItemHeight = multipleItemHeight - 2;
        token.MultipleItemHeightSM = multipleItemHeightSM + 4;
        token.MultipleItemHeightLG = multipleItemHeightLG;
        token.MultipleSelectorBgDisabled = global.ColorBgContainerDisabled;
        token.MultipleItemColorDisabled = global.ColorTextDisabled;


        var lineWidth    = global.LineWidth;

        var multiPaddingVertical = Math.Round((global.ControlHeight - global.FontHeight) / 2 * 10) / 10 - lineWidth;
        var multiPaddingVerticalSM = Math.Round((global.ControlHeightSM - global.FontHeight) / 2 * 10) / 10 - lineWidth;
        var multiPaddingVerticalLG = Math.Ceiling((global.ControlHeightLG - global.FontHeightLG) / 2 * 10) / 10 -
                                     lineWidth;

        var multiPaddingRight = global.UniformlyPaddingSM - lineWidth;
        token.MultiModePadding = new Thickness(multiPaddingVertical, multiPaddingVertical, multiPaddingRight, multiPaddingVertical);
        token.MultiModePrefixIndent = new Thickness(Math.Max(0, multiPaddingRight - multiPaddingVertical), 0, 0, 0);

        var multiPaddingRightSM = global.ControlPaddingHorizontalSM - lineWidth;
        token.MultiModePaddingSM = new Thickness(multiPaddingVerticalSM, multiPaddingVerticalSM, multiPaddingRightSM, multiPaddingVerticalSM);
        token.MultiModePrefixIndentSM = new Thickness(Math.Max(0, multiPaddingRightSM - multiPaddingVerticalSM), 0, 0, 0);

        var multiPaddingRightLG = global.ControlPaddingHorizontal - lineWidth;
        token.MultiModePaddingLG = new Thickness(multiPaddingVerticalLG, multiPaddingVerticalLG, multiPaddingRightLG, multiPaddingVerticalLG);
        token.MultiModePrefixIndentLG = new Thickness(Math.Max(0, multiPaddingRightLG - multiPaddingVerticalLG), 0, 0, 0);

        token.SingleModePadding   = new Thickness(multiPaddingRight, multiPaddingVertical);
        token.SingleModePaddingSM = new Thickness(multiPaddingRightSM, multiPaddingVerticalSM);
        token.SingleModePaddingLG = new Thickness(multiPaddingRightLG,  multiPaddingVerticalLG);
    }
}
