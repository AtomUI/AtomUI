using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="CheckBoxToken"/>.</summary>
[ControlTokenRule]
internal sealed class CheckBoxTokenRule : IControlTokenRule<AntDesignToken, CheckBoxToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<CheckBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.CheckIndicatorSize        = global.ControlInteractiveSize;

        token.CheckedMarkSize           = token.CheckIndicatorSize * 0.6;
        token.IndicatorTristateMarkSize = global.FontSizeLG / 2;
        token.IndicatorHoverBorderColor = global.ColorPrimaryHover;
        token.TextMargin                = new Thickness(global.UniformlyMarginXS, 0, global.UniformlyMarginXS, 0);
    }
}
