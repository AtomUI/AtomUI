using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="FlyoutHostToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class FlyoutHostTokenRule : IControlTokenRule<AntDesignToken, FlyoutHostToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<FlyoutHostToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.MarginToAnchor    = global.UniformlyMarginXXS;
        token.OverlayHostShadow = global.BoxShadowsSecondary;
        token.PopupRootShadow   = global.BoxShadowsSecondary;
        token.HorizontalOffset = global.UniformlyMarginXS;
        token.VerticalOffset   = global.UniformlyMarginXS;
    }
}
