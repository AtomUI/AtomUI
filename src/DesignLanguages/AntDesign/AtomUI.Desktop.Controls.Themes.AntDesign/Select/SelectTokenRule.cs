using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="SelectToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SelectTokenRule : IControlTokenRule<AntDesignToken, SelectToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<SelectToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        SelectInputTokenCalculator.Calculate(in context, global, token);
        // antd draws the full frame border and a regular indicator color, so the three frame switches stay off.
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.MutedSelectIndicator     = false;
        token.OptionSelectedColor      = global.ControlItemColorActive;
        token.OptionSelectedIndicatorColor = global.ColorPrimary;
        token.OptionSelectedFontWeight = global.FontWeightStrong;
        token.OptionSelectedBg         = global.ControlItemBgActive;
        token.OptionActiveBg           = global.ControlItemBgHover;
        token.OptionPadding            = new Thickness(global.ControlPaddingHorizontal, (global.ControlHeight - global.FontHeight) / 2);
        token.OptionHeight = global.ControlHeight;
        token.SelectAffixPadding = global.PaddingXXS;
        token.FixedItemMargin = new Thickness(Math.Floor(global.UniformlyPaddingXXS / 2));
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        // Ant Design's option highlight fills the row without an inset or rounding.
        token.OptionBackgroundInset = new Thickness(0);
        token.OptionBorderRadius    = new CornerRadius(0);
    }
}
