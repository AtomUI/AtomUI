using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="ImageGroupPreviewerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ImageGroupPreviewerTokenRule : IControlTokenRule<PrimeNGToken, ImageGroupPreviewerToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ImageGroupPreviewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ImagePreviewerTokenCalculator.Calculate(in context, global, token);
    }
}
