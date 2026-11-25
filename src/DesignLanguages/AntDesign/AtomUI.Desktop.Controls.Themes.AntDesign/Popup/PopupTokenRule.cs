using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="PopupToken"/>.</summary>
[ControlTokenRule]
internal sealed class PopupTokenRule : IControlTokenRule<AntDesignToken, PopupToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<PopupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        PopupCornerTokenCalculator.Calculate(in context, global, token);

        token.OverlayHostShadow = global.BoxShadowsSecondary;
        // 窗口型与 Overlay Popup 使用同一阴影；定位按残余阴影补偿，内容相对 anchor 的位置与阴影厚度无关
        token.PopupRootShadow = global.BoxShadowsSecondary;
        token.MarginToAnchor = global.UniformlyMarginXXS;
    }
}
