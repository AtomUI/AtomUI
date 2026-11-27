using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="OptionButtonToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class OptionButtonTokenRule : IControlTokenRule<AntDesignToken, OptionButtonToken>
{
    private const string GroupDrawnFrame =
        "OptionButtonGroup draws its separators and the selection border behind the options, so an opaque option " +
        "background would cover them; options stay transparent until the group draws the frame above its options.";
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<OptionButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.ButtonSolidCheckedColor            = global.ColorTextLightSolid;
        token.ButtonSolidCheckedBackground       = global.ColorPrimary;
        token.ButtonSolidCheckedHoverBackground  = global.ColorPrimaryHover;
        token.ButtonSolidCheckedActiveBackground = global.ColorPrimaryActive;
        token.ButtonBackground                   = global.ColorBgContainer;
        token.ButtonOutlineCheckedBackground     = Colors.Transparent;
        token.ButtonOutlineCheckedColor          = global.ColorPrimary;
        token.ButtonOutlineCheckedHoverColor     = global.ColorPrimaryHover;
        token.ButtonOutlineCheckedActiveColor    = global.ColorPrimaryActive;
        token.ButtonCheckedBackground            = global.ColorBgContainer;
        token.ButtonColor                        = global.ColorText;
        token.ButtonCheckedBgDisabled            = global.ControlItemBgActiveDisabled;
        token.ButtonCheckedColorDisabled         = global.ColorTextDisabled;

        var fontSize   = global.FontSize;
        var fontSizeLG = global.FontSizeLG;
        token.ContentFontSize = fontSize;
        token.ContentFontSizeSM = fontSize;
        token.ContentFontSizeLG = fontSizeLG;
        token.ContentLineHeight = CalculatorUtils.CalculateLineHeight(token.ContentFontSize);
        token.ContentLineHeightSM = CalculatorUtils.CalculateLineHeight(token.ContentFontSizeSM);
        token.ContentLineHeightLG = CalculatorUtils.CalculateLineHeight(token.ContentFontSizeLG);

        var controlHeight   = global.ControlHeight;
        var controlHeightSM = global.ControlHeightSM;
        var controlHeightLG = global.ControlHeightLG;
        var lineWidth       = global.LineWidth;
        token.ButtonPadding = new Thickness(global.PaddingContentHorizontal - lineWidth, 0);

        // Kept as direct reads in the assignments: the Own Token reference analysis follows derivations, not locals.
        token.ContentPadding = new Thickness(
            token.ButtonPadding.Left,
            Math.Max((controlHeight - token.ContentFontSize * token.ContentLineHeight) / 2 - lineWidth, 0),
            token.ButtonPadding.Right,
            Math.Max((controlHeight - token.ContentFontSize * token.ContentLineHeight) / 2 - lineWidth, 0));
        token.ContentPaddingLG = new Thickness(
            token.ButtonPadding.Left,
            Math.Max((controlHeightLG - token.ContentFontSizeLG * token.ContentLineHeightLG) / 2 - lineWidth, 0),
            token.ButtonPadding.Right,
            Math.Max((controlHeightLG - token.ContentFontSizeLG * token.ContentLineHeightLG) / 2 - lineWidth, 0));
        token.ContentPaddingSM = new Thickness(8 - lineWidth,
            Math.Max((controlHeightSM - token.ContentFontSizeSM * token.ContentLineHeightSM) / 2 - lineWidth, 0));
    }
}
