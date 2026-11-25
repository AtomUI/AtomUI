using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TreeViewFlyoutPresenterToken"/>.</summary>
[ControlTokenRule]
internal sealed class TreeViewFlyoutPresenterTokenRule : IControlTokenRule<PrimeNGToken, TreeViewFlyoutPresenterToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TreeViewFlyoutPresenterToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        PopupCornerTokenCalculator.Calculate(in context, global, token);
        token.PopupMinWidth     = 120;
        token.PopupMaxWidth = 800;

        token.PopupMinHeight = global.ControlHeightSM * 3;
        token.PopupMaxHeight = global.ControlHeightSM * 30;

        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS, global.BorderRadiusLG.TopLeft / 2);
        token.PopupBgColor        = global.ColorBgElevated;
    }
}
