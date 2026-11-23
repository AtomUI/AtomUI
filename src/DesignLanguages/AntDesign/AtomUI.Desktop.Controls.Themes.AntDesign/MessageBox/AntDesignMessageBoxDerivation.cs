using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="MessageBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignMessageBoxDerivation : IControlDerivation<AntDesignToken, MessageBoxToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<MessageBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractDialogDerivation.DeriveShared(in context, global, token);
        token.HeaderPadding = AntDesignAbstractDialogDerivation.CalculateHeaderPadding(token, global);
        token.MinHeight = AntDesignAbstractDialogDerivation.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 410;
        token.StyleIconSize = global.SizeLG * 1.2;
    }
}
