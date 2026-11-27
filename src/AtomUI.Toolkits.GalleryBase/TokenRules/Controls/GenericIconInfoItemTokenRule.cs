using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Toolkits.GalleryBase.Controls;

namespace AtomUI.Toolkits.GalleryBase.TokenRules;

/// <summary>
/// The Generic view of <see cref="IconInfoItemToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class GenericIconInfoItemTokenRule : IControlTokenRule<LanguageDesignToken, IconInfoItemToken>
{
    public void Apply(in TokenEvaluationContext<LanguageDesignToken> context, ControlTokenTarget<IconInfoItemToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.IconNameFontSize = 13;
    }
}
