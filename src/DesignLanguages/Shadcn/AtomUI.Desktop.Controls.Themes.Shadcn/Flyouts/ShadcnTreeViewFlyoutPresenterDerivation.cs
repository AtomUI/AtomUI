using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TreeViewFlyoutPresenterToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTreeViewFlyoutPresenterDerivation : IControlDerivation<ShadcnToken, TreeViewFlyoutPresenterToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TreeViewFlyoutPresenterToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractPopupCornerDerivation.DeriveShared(in context, global, token);
        token.PopupMinWidth     = 120;
        token.PopupMaxWidth = 800;

        token.PopupMinHeight = global.ControlHeightSM * 3;
        token.PopupMaxHeight = global.ControlHeightSM * 30;

        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS, global.BorderRadiusLG.TopLeft / 2);
        token.PopupBgColor        = global.ColorBgElevated;
    }
}
