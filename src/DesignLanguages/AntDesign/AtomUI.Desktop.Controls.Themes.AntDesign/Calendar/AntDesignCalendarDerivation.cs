using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="CalendarToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignCalendarDerivation : IControlDerivation<AntDesignToken, CalendarToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<CalendarToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractCalendarDerivation.DeriveShared(in context, global, token);
        token.YearMonthCellWidth = global.ControlHeightLG * 1.5;
        token.MiniContentHeight = 256;
        var dateContentHeight = (global.FontHeightSM + global.UniformlyMarginXS) * 3 +
                                global.LineWidth * 2;
        token.FullCellMinHeight = global.ControlHeightSM + dateContentHeight +
                            global.UniformlyPaddingXS / 2 + global.LineWidthBold;
    }
}
