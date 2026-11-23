using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="PopupToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignPopupDerivation : IControlDerivation<AntDesignToken, PopupToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<PopupToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractPopupCornerDerivation.DeriveShared(in context, global, token);

        token.OverlayHostShadow = global.BoxShadowsSecondary;
        // 窗口型与 Overlay Popup 使用同一阴影；定位按残余阴影补偿，内容相对 anchor 的位置与阴影厚度无关
        token.PopupRootShadow = global.BoxShadowsSecondary;
        token.MarginToAnchor = global.UniformlyMarginXXS;
    }
}
