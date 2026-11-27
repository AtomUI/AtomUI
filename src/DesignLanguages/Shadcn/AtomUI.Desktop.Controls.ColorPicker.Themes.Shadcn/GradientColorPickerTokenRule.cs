using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="GradientColorPickerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class GradientColorPickerTokenRule : IControlTokenRule<ShadcnToken, GradientColorPickerToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<GradientColorPickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ColorPickerTokenCalculator.Calculate(in context, global, token);
    }
}
