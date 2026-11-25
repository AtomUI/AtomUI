using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TreeViewFlyoutPresenterToken"/>.</summary>
[ControlTokenRule]
internal sealed class TreeViewFlyoutPresenterTokenRule : IControlTokenRule<ShadcnToken, TreeViewFlyoutPresenterToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TreeViewFlyoutPresenterToken> target)
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
