using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CalendarToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGCalendarDerivation : IControlDerivation<PrimeNGToken, CalendarToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<CalendarToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGCalendarTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractCalendarDerivation.DeriveShared(in context, global, token);
        token.YearMonthCellWidth = global.ControlHeightLG * 1.5;
        token.MiniContentHeight = 256;
        var dateContentHeight = (global.FontHeightSM + global.UniformlyMarginXS) * 3 +
                                global.LineWidth * 2;
        token.FullCellMinHeight = global.ControlHeightSM + dateContentHeight +
                            global.UniformlyPaddingXS / 2 + global.LineWidthBold;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGCalendarTokens.EvaluateOwn(in context, global, token);
    }
}
