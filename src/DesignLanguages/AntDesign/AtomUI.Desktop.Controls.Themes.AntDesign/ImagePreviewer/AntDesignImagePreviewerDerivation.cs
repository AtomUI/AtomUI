using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ImagePreviewerToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignImagePreviewerDerivation : IControlDerivation<AntDesignToken, ImagePreviewerToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<ImagePreviewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractImagePreviewerDerivation.DeriveShared(in context, global, token);
        token.ImagePreviewSwitchSize = global.ControlHeightLG;
    }
}
