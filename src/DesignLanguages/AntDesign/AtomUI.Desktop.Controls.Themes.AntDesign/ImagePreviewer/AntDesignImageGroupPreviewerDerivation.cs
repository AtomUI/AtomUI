using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ImageGroupPreviewerToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignImageGroupPreviewerDerivation : IControlDerivation<AntDesignToken, ImageGroupPreviewerToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<ImageGroupPreviewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractImagePreviewerDerivation.DeriveShared(in context, global, token);
    }
}
