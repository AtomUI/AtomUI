using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="NumericUpDownToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignNumericUpDownDerivation : IControlDerivation<AntDesignToken, NumericUpDownToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<NumericUpDownToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractButtonSpinnerDerivation.DeriveShared(in context, global, token);
        token.InputFontSizeSM = global.FontSize;
        token.HandleBg = global.ColorBgContainer;
        token.FilledHandleBg = AntDesignAbstractButtonSpinnerDerivation.CalculateFilledHandleBackground(token, global, token.HandleBg);
    }
}
