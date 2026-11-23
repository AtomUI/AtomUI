using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TreeViewFlyoutPresenterToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTreeViewFlyoutPresenterDerivation : IControlDerivation<AntDesignToken, TreeViewFlyoutPresenterToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TreeViewFlyoutPresenterToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractPopupCornerDerivation.DeriveShared(in context, global, token);
        token.PopupMinWidth     = 120;
        token.PopupMaxWidth = 800;

        token.PopupMinHeight = global.ControlHeightSM * 3;
        token.PopupMaxHeight = global.ControlHeightSM * 30;

        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS, global.BorderRadiusLG.TopLeft / 2);
        token.PopupBgColor        = global.ColorBgElevated;
    }
}
