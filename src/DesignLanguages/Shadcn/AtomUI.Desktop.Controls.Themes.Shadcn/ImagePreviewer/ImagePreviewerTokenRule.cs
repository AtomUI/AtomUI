using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="ImagePreviewerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ImagePreviewerTokenRule : IControlTokenRule<ShadcnToken, ImagePreviewerToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ImagePreviewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ImagePreviewerTokenCalculator.Calculate(in context, global, token);
        token.ImagePreviewSwitchSize = global.ControlHeightLG;
    }
}
