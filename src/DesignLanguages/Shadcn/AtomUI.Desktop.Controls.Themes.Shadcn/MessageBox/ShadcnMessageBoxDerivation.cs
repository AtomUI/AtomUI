using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="MessageBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnMessageBoxDerivation : IControlDerivation<ShadcnToken, MessageBoxToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<MessageBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractDialogDerivation.DeriveShared(in context, global, token);
        token.HeaderPadding = ShadcnAbstractDialogDerivation.CalculateHeaderPadding(token, global);
        token.MinHeight = ShadcnAbstractDialogDerivation.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 410;
        token.StyleIconSize = global.SizeLG * 1.2;
    }
}
