using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ImageGroupPreviewerToken"/>.</summary>
[ControlTokenRule]
internal sealed class ImageGroupPreviewerTokenRule : IControlTokenRule<AntDesignToken, ImageGroupPreviewerToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ImageGroupPreviewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ImagePreviewerTokenCalculator.Calculate(in context, global, token);
    }
}
