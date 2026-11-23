using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="GradientColorPickerToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignGradientColorPickerDerivation : IControlDerivation<AntDesignToken, GradientColorPickerToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<GradientColorPickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractColorPickerDerivation.DeriveShared(in context, global, token);
    }
}
