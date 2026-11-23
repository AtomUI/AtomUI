using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="LunarCalendarToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnLunarCalendarDerivation : IControlDerivation<ShadcnToken, LunarCalendarToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<LunarCalendarToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractCalendarDerivation.DeriveShared(in context, global, token);

        if ((context.Variant.Appearance == ThemeAppearance.Dark))
        {
            token.ItemActiveColor = global.ColorText;
        }
        token.MiniDateCellSize = global.ControlHeightLG;

        token.MiniMonthCellWidth = global.ControlHeightLG * 2;
        token.SecondaryTextFontSize = global.FontSizeSM;
        token.SecondaryTextLineHeight = global.FontHeightSM;
        token.MiniDateCellPadding = new Thickness(0, 3);
        token.MiniDateCellCornerRadius = new CornerRadius(6);
        token.MiniDateCellHeight = Math.Max(token.MiniDateCellSize, Math.Ceiling(
                global.FontHeight + token.SecondaryTextLineHeight +
                token.MiniDateCellPadding.Top + token.MiniDateCellPadding.Bottom));

        token.MiniContentHeight = global.FontHeightSM +
                            (token.MiniDateCellHeight + global.UniformlyMarginXS) * 6;

        token.SecondaryTextColor = global.ColorTextTertiary;
        token.WeekendTextColor = global.ColorError;
        token.HolidayMarkerColor = global.ColorError;
        token.WorkdayMarkerColor = global.ColorTextTertiary;
        token.MarkerSize = global.UniformlyMarginXXS;

        token.MarkerMargin = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        token.MarkerCornerRadius = new CornerRadius(token.MarkerSize / 2);

        var calendarContentHeight =
            (global.FontHeightSM + global.UniformlyMarginXS) * 3 +
            global.LineWidth * 2;
        token.FullCellMinHeight = global.ControlHeightSM +
                            calendarContentHeight +
                            global.UniformlyPaddingXS / 2 +
                            global.LineWidthBold +
                            token.SecondaryTextLineHeight +
                            global.UniformlyMarginXXS;
        token.RangeBarTopOffset = global.ControlHeightSM +
                            token.SecondaryTextLineHeight +
                            global.UniformlyMarginXXS;
        if (context.Variant.IsDark)
        {
            token.ItemActiveBg = ShadcnRecipes.CompositeOver(global.ColorText, 0.14, token.ItemActiveBg);
        }
    }
}
