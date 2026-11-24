using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="MessageBoxToken"/>.</summary>
[ControlTokenRule]
internal sealed class MessageBoxTokenRule : IControlTokenRule<AntDesignToken, MessageBoxToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<MessageBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        DialogTokenCalculator.Calculate(in context, global, token);
        token.HeaderPadding = DialogTokenCalculator.CalculateHeaderPadding(token, global);
        token.MinHeight = DialogTokenCalculator.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 410;
        token.StyleIconSize = global.SizeLG * 1.2;
    }
}
