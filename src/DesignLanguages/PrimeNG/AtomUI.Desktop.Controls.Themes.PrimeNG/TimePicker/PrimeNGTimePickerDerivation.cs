using Avalonia;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TimePickerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTimePickerDerivation : IControlDerivation<PrimeNGToken, TimePickerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TimePickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTimePickerTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractTimePickerDerivation.DeriveShared(in context, global, token);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = PrimeNGAbstractTimeViewDerivation.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
        // Ant Design aligns the items to the start and fills the whole row.
        token.CenterItemContent   = false;
        token.ItemBackgroundInset = new Thickness(0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTimePickerTokens.EvaluateOwn(in context, global, token);
    }
}
