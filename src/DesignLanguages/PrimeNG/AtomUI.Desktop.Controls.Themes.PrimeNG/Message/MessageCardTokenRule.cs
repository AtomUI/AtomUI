using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="MessageCardToken"/>.</summary>
[ControlTokenRule]
internal sealed class MessageCardTokenRule : IControlTokenRule<PrimeNGToken, MessageCardToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<MessageCardToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides and the severity colours, read by the formulas below.
        var statuses = PrimeNGMessageCardTokens.EvaluateGlobal(in context, global);
        // Every MessageType has a severity shadow slot, so there is no resting shadow: the theme sets the card shadow
        // from the slot matching the current MessageType.
        token.InformationShadows  = global.BoxShadows;
        token.SuccessShadows      = global.BoxShadows;
        token.WarningShadows      = global.BoxShadows;
        token.ErrorShadows        = global.BoxShadows;
        token.LoadingShadows      = global.BoxShadows;
        token.ContentBg = global.ColorBgElevated;
        // The severity surfaces default to the plain card: only the icon carries the severity.
        token.InformationBg    = token.ContentBg;
        token.InformationIconColor = statuses.Primary;
        token.InformationColor = global.ColorText;
        token.SuccessBg    = token.ContentBg;
        token.SuccessIconColor = statuses.Success;
        token.SuccessColor = global.ColorText;
        token.WarningBg    = token.ContentBg;
        token.WarningIconColor = statuses.Warning;
        token.WarningColor = global.ColorText;
        token.ErrorBg    = token.ContentBg;
        token.ErrorIconColor = statuses.Error;
        token.ErrorColor = global.ColorText;
        token.LoadingIconColor = statuses.Primary;
        token.ContentPadding = new Thickness(
            (global.ControlHeightLG -
             global.FontSize * global.RelativeLineHeight) / 2,
            global.UniformlyPaddingXS);
        token.MessageIconMargin = new Thickness(0, 0, global.UniformlyMarginXS, 0);
        token.MessageTopMargin  = new Thickness(global.UniformlyMarginXS);
        token.MessageIconSize   = global.FontSizeSM * global.RelativeLineHeightSM;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGMessageCardTokens.EvaluateOwn(in context, global, token);
    }
}
