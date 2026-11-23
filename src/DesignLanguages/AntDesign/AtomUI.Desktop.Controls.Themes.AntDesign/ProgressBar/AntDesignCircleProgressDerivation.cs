using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="CircleProgressToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignCircleProgressDerivation : IControlDerivation<AntDesignToken, CircleProgressToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<CircleProgressToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractCircleProgressDerivation.DeriveShared(in context, global, token);
    }
}
