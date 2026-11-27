using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="MessageBoxToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class MessageBoxTokenRule : IControlTokenRule<ShadcnToken, MessageBoxToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<MessageBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        DialogTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: no registry value is merged into the message box tokens.
        token.HeaderPadding = DialogTokenCalculator.CalculateHeaderPadding(token, global);
        token.MinHeight = DialogTokenCalculator.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 410;
        token.StyleIconSize = global.SizeLG * 1.2;
    }
}
