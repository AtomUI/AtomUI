using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TreeViewFlyoutPresenterToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTreeViewFlyoutPresenterDerivation : IControlDerivation<PrimeNGToken, TreeViewFlyoutPresenterToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TreeViewFlyoutPresenterToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractPopupCornerDerivation.DeriveShared(in context, global, token);
        token.PopupMinWidth     = 120;
        token.PopupMaxWidth = 800;

        token.PopupMinHeight = global.ControlHeightSM * 3;
        token.PopupMaxHeight = global.ControlHeightSM * 30;

        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS, global.BorderRadiusLG.TopLeft / 2);
        token.PopupBgColor        = global.ColorBgElevated;
    }
}
