using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CalendarToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnCalendarDerivation : IControlDerivation<ShadcnToken, CalendarToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<CalendarToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractCalendarDerivation.DeriveShared(in context, global, token);
        token.YearMonthCellWidth = global.ControlHeightLG * 1.5;
        token.MiniContentHeight = 256;
        var dateContentHeight = (global.FontHeightSM + global.UniformlyMarginXS) * 3 +
                                global.LineWidth * 2;
        token.FullCellMinHeight = global.ControlHeightSM + dateContentHeight +
                            global.UniformlyPaddingXS / 2 + global.LineWidthBold;
        if (context.Variant.IsDark)
        {
            token.ItemActiveBg = ShadcnRecipes.CompositeOver(global.ColorText, 0.14, token.ItemActiveBg);
        }
    }
}
