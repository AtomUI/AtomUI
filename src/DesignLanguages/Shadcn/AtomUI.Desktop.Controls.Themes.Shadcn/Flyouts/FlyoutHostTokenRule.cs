using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="FlyoutHostToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class FlyoutHostTokenRule : IControlTokenRule<ShadcnToken, FlyoutHostToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<FlyoutHostToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.MarginToAnchor    = global.UniformlyMarginXXS;
        token.OverlayHostShadow = global.BoxShadowsSecondary;
        token.PopupRootShadow   = global.BoxShadowsSecondary;
        token.HorizontalOffset = global.UniformlyMarginXS;
        token.VerticalOffset   = global.UniformlyMarginXS;
        // The arrow flyouts: the plain popover shadow; the arrow box draws the integrated border.
        token.PopupRootShadow   = context.Language.GetPopoverOverlayRecipe(global).Shadows;
        token.OverlayHostShadow = token.PopupRootShadow;
    }
}
