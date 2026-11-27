using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="ResetButtonToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ResetButtonTokenRule : IControlTokenRule<ShadcnToken, ResetButtonToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ResetButtonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ButtonTokenCalculator.CalculateGlobal(in context, global);
        StandardButtonTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: the registry `button` values are merged by ButtonTokenCalculator, before
        // and after this geometry.
        token.ContentFontSize = global.FontSize;
        token.ContentLineHeight = ButtonGeometryTokenCalculator.CalculateContentLineHeight(token.ContentFontSize);
        token.ContentPadding = ButtonGeometryTokenCalculator.CalculateContentPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.IconOnyPadding = ButtonGeometryTokenCalculator.CalculateIconPadding(token, global, token.ContentLineHeight, CustomizableSizeType.Middle);
        token.ContentFontSizeSM = global.FontSize;
        token.ContentLineHeightSM = ButtonGeometryTokenCalculator.CalculateContentLineHeight(token.ContentFontSizeSM);
        token.ContentPaddingSM = ButtonGeometryTokenCalculator.CalculateContentPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.IconOnyPaddingSM = ButtonGeometryTokenCalculator.CalculateIconPadding(token, global, token.ContentLineHeightSM, CustomizableSizeType.Small);
        token.ContentFontSizeLG = global.FontSizeLG;
        token.ContentLineHeightLG = ButtonGeometryTokenCalculator.CalculateContentLineHeight(token.ContentFontSizeLG);
        token.ContentPaddingLG = ButtonGeometryTokenCalculator.CalculateContentPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.IconOnyPaddingLG = ButtonGeometryTokenCalculator.CalculateIconPadding(token, global, token.ContentLineHeightLG, CustomizableSizeType.Large);
        token.CirclePadding = new Thickness(token.ContentPaddingSM.Left / 2);
        ButtonTokenCalculator.CalculateOwn(in context, global, token);
    }
}
