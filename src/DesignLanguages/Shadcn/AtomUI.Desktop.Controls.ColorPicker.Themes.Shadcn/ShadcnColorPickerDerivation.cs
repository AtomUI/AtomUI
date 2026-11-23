using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ColorPickerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnColorPickerDerivation : IControlDerivation<ShadcnToken, ColorPickerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ColorPickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractColorPickerDerivation.DeriveShared(in context, global, token);
    }
}
