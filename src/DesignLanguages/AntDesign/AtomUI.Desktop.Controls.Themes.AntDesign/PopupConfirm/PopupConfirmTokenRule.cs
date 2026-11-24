using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="PopupConfirmToken"/>.</summary>
[ControlTokenRule]
internal sealed class PopupConfirmTokenRule : IControlTokenRule<AntDesignToken, PopupConfirmToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<PopupConfirmToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.ButtonSpacing          = global.UniformlyMarginXS;
        token.IconMargin             = new Thickness(0, global.UniformlyMarginXS / 2, global.UniformlyMarginXS, 0);
        token.ContentContainerMargin = new Thickness(0, 0, 0, global.UniformlyMarginXS);
        token.TitleMargin            = new Thickness(0, 0, 0, global.UniformlyMarginXS);
        token.ButtonContainerMargin  = new Thickness(0, global.UniformlyMarginXS, 0, 0);
    }
}
