using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="DialogToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnDialogDerivation : IControlDerivation<ShadcnToken, DialogToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<DialogToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // The dialog surface reads the dialog's BoxShadows: the overlay ring plus the style's dialog shadow level (it differs
        // from the card's in Luma, Sera and Rhea).
        global.BoxShadows = context.Language.GetDialogOverlayRecipe(global).ShadowsWithOutline;
        ShadcnAbstractDialogDerivation.DeriveShared(in context, global, token);
        token.HeaderPadding = ShadcnAbstractDialogDerivation.CalculateHeaderPadding(token, global);
        token.MinHeight = ShadcnAbstractDialogDerivation.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 200;
    }
}
