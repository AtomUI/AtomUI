using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ColorPickerToken"/>.</summary>
[ControlTokenRule]
internal sealed class ColorPickerTokenRule : IControlTokenRule<ShadcnToken, ColorPickerToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ColorPickerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ColorPickerTokenCalculator.Calculate(in context, global, token);
    }
}
