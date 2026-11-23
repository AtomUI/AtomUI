using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The increment/decrement handle of ButtonSpinner and NumericUpDown under the PrimeNG presets: PrimeUIX 3.1.1
/// <c>inputnumber.button</c> — a transparent resting fill, <c>surface.100</c> / <c>800</c> on hover,
/// <c>surface.200</c> / <c>700</c> while pressed, icon <c>surface.400</c> / <c>500</c> / <c>600</c> (dark
/// <c>300</c> / <c>200</c>) and no divider (the stacked buttons carry <c>border: 0 none</c>); the icon and the column
/// width are tuned to the field (see <see cref="PrimeNGSpinnerHandle"/>). Both hosts share the handle theme, so both token identities are written.
/// </summary>
internal static class PrimeNGButtonSpinnerTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, ButtonSpinnerToken token)
    {
        PrimeNGSpinnerHandle.Apply(context, effectiveGlobal, token);
    }
}

/// <summary>
/// NumericUpDown's spinner owns <see cref="NumericUpDownToken"/> (a sibling of <see cref="ButtonSpinnerToken"/>), so the
/// same PrimeUIX <c>inputnumber.button</c> values are written through its own identity.
/// </summary>
internal static class PrimeNGNumericUpDownTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, NumericUpDownToken token)
    {
        PrimeNGSpinnerHandle.Apply(context, effectiveGlobal, token);
        // PrimeUIX form.field.sm.font.size 0.75rem.
        token.InputFontSizeSM = effectiveGlobal.FontSizeSM;
    }
}

/// <summary>The shared <c>inputnumber.button</c> mapping, written for both spinner token identities.</summary>
internal static class PrimeNGSpinnerHandle
{
    internal static void Apply(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, AbstractButtonSpinnerToken token)
    {
        var language = context.Language;
        var dark    = context.Variant.Appearance == ThemeAppearance.Dark;
        token.HandleBg          = Colors.Transparent;
        token.FilledHandleBg    = Colors.Transparent;
        token.HandleHoverBg     = dark ? language.Surface800 : language.Surface100;
        token.HandleActiveBg    = dark ? language.Surface700 : language.Surface200;
        token.HandleIconColor   = language.Surface400;
        token.HandleHoverColor  = dark ? language.Surface300 : language.Surface500;
        token.HandlePressedColor = dark ? language.Surface200 : language.Surface600;
        // The stacked buttons have no border of their own in PrimeUIX (border: 0 none): the input frame is the only
        // outline, so the two dividers AtomUI draws turn invisible.
        token.HandleBorderColor = Colors.Transparent;
        // .p-inputnumber-button .p-icon is form.field.font.size (0.875rem, 14); one quarter step smaller (10.5) keeps the
        // stacked pair inside the small field and still reads as an icon next to the text.
        token.HandleIconSize = 10.5;
        // inputnumber.button.width is 2.25rem, which leaves the chevrons floating in a wide empty strip; the column
        // instead keeps the field's horizontal padding on both sides of the icon, so the icon sits as far from the
        // right edge as the text from the left one.
        token.HandleWidth = token.HandleIconSize + 2 * effectiveGlobal.InputPadding.Left;
    }
}
