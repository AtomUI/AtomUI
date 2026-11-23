using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="DatePickerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGDatePickerDerivation : IControlDerivation<PrimeNGToken, DatePickerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<DatePickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGDatePickerTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractDatePickerDerivation.DeriveShared(in context, global, token);
        token.HeaderBorderThickness = new Thickness(0, 0, 0, global.LineWidth);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = PrimeNGAbstractTimeViewDerivation.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGDatePickerTokens.EvaluateOwn(in context, global, token);
    }
}
