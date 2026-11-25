using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="DialogToken"/>.</summary>
[ControlTokenRule]
internal sealed class DialogTokenRule : IControlTokenRule<AntDesignToken, DialogToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<DialogToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        DialogTokenCalculator.Calculate(in context, global, token);
        token.HeaderPadding = DialogTokenCalculator.CalculateHeaderPadding(token, global);
        token.MinHeight = DialogTokenCalculator.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 200;
    }
}
