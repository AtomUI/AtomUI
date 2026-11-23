using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="GradientColorPickerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnGradientColorPickerDerivation : IControlDerivation<ShadcnToken, GradientColorPickerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<GradientColorPickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractColorPickerDerivation.DeriveShared(in context, global, token);
    }
}
