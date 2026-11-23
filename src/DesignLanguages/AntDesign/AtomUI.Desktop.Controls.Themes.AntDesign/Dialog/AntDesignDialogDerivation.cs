using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="DialogToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignDialogDerivation : IControlDerivation<AntDesignToken, DialogToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<DialogToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractDialogDerivation.DeriveShared(in context, global, token);
        token.HeaderPadding = AntDesignAbstractDialogDerivation.CalculateHeaderPadding(token, global);
        token.MinHeight = AntDesignAbstractDialogDerivation.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 200;
    }
}
