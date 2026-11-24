using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="DialogToken"/>.</summary>
[ControlTokenRule]
internal sealed class DialogTokenRule : IControlTokenRule<ShadcnToken, DialogToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<DialogToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // The dialog surface reads the dialog's BoxShadows: the overlay ring plus the style's dialog shadow level (it differs
        // from the card's in Luma, Sera and Rhea).
        global.BoxShadows = context.Language.GetDialogOverlayRecipe(global).ShadowsWithOutline;
        DialogTokenCalculator.Calculate(in context, global, token);
        token.HeaderPadding = DialogTokenCalculator.CalculateHeaderPadding(token, global);
        token.MinHeight = DialogTokenCalculator.CalculateMinimumHeight(token, global, token.HeaderPadding);
        token.MinWidth = 200;
    }
}
