using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="ImageGroupPreviewerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ImageGroupPreviewerTokenRule : IControlTokenRule<ShadcnToken, ImageGroupPreviewerToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ImageGroupPreviewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ImagePreviewerTokenCalculator.Calculate(in context, global, token);
    }
}
