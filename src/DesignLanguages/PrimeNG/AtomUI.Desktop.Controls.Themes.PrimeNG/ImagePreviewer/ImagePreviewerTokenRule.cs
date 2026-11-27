using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="ImagePreviewerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ImagePreviewerTokenRule : IControlTokenRule<PrimeNGToken, ImagePreviewerToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ImagePreviewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ImagePreviewerTokenCalculator.Calculate(in context, global, token);
        token.ImagePreviewSwitchSize = global.ControlHeightLG;
    }
}
