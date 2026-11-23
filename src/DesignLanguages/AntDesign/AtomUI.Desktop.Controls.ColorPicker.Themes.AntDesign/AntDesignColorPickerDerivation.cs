using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ColorPickerToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignColorPickerDerivation : IControlDerivation<AntDesignToken, ColorPickerToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<ColorPickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractColorPickerDerivation.DeriveShared(in context, global, token);
    }
}
