using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="MessageCardToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class MessageCardTokenRule : IControlTokenRule<ShadcnToken, MessageCardToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<MessageCardToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
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
        token.InformationIconColor = global.ColorPrimary;
        token.InformationColor = global.ColorText;
        token.SuccessBg    = token.ContentBg;
        token.SuccessIconColor = global.ColorSuccess;
        token.SuccessColor = global.ColorText;
        token.WarningBg    = token.ContentBg;
        token.WarningIconColor = global.ColorWarning;
        token.WarningColor = global.ColorText;
        token.ErrorBg    = token.ContentBg;
        token.ErrorIconColor = global.ColorError;
        token.ErrorColor = global.ColorText;
        token.LoadingIconColor = global.ColorPrimary;
        // Ant Design formula kept as is: shadcn draws toasts with sonner, which has no registry class, so no registry
        // value is merged into the message card tokens.
        token.ContentPadding = new Thickness(
            (global.ControlHeightLG -
             global.FontSize * global.RelativeLineHeight) / 2,
            global.UniformlyPaddingXS);
        token.MessageIconMargin = new Thickness(0, 0, global.UniformlyMarginXS, 0);
        token.MessageTopMargin  = new Thickness(global.UniformlyMarginXS);
        token.MessageIconSize   = global.FontSizeSM * global.RelativeLineHeightSM;
    }
}
