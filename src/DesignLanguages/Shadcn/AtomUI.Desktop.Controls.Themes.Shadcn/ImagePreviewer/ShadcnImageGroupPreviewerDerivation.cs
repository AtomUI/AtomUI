using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ImageGroupPreviewerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnImageGroupPreviewerDerivation : IControlDerivation<ShadcnToken, ImageGroupPreviewerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ImageGroupPreviewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractImagePreviewerDerivation.DeriveShared(in context, global, token);
    }
}
