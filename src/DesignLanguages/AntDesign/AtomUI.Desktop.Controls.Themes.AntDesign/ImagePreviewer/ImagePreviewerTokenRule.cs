using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ImagePreviewerToken"/>.</summary>
[ControlTokenRule]
internal sealed class ImagePreviewerTokenRule : IControlTokenRule<AntDesignToken, ImagePreviewerToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ImagePreviewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ImagePreviewerTokenCalculator.Calculate(in context, global, token);
        token.ImagePreviewSwitchSize = global.ControlHeightLG;
    }
}
