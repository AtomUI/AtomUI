using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;

namespace AtomUI.Toolkits.GalleryBase.TokenRules;

/// <summary>
/// The Generic view of <see cref="ColorItemControlToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class GenericColorItemControlTokenRule : IControlTokenRule<LanguageDesignToken, ColorItemControlToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<ColorItemControlToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.HexFontSize = 10;
        token.HexOpacity = 0.8;
    }
}
