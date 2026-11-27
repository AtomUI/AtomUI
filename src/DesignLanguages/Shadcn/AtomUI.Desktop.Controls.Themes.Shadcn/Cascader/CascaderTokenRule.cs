using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="CascaderToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class CascaderTokenRule : IControlTokenRule<ShadcnToken, CascaderToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<CascaderToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        InputTokenCalculator.CalculateGlobal(in context, global);
        SelectInputTokenCalculator.Calculate(in context, global, token);
        // InputTokenCalculator.CalculateGlobal merges the registry input values into the control's global view;
        // the cascader sizes below keep the Ant formula.
        var itemPaddingVertical = Math.Round((global.ControlHeight - global.FontHeight) / 2);
        token.ControlWidth             = 184;
        token.ControlItemWidth         = 111;
        token.DropdownHeight           = 180;
        token.OptionHoverBg            = global.ControlItemBgHover;
        token.OptionSelectedBg         = global.ControlItemBgActive;
        token.OptionSelectedFontWeight = global.FontWeightStrong;
        token.OptionPadding            = new Thickness(global.UniformlyPaddingSM, itemPaddingVertical);
        token.MenuPadding              = global.PaddingXXS;
        token.OptionMargin             = default;
        token.OptionBorderRadius       = global.BorderRadiusMD;
        token.OptionSelectedColor      = global.ControlItemColorActive;
        token.HeaderHeight             = global.ControlHeightSM;
        token.FilterHighlightColor     = global.ColorError;
        token.ItemHeaderSpacing        = global.SpacingXXS;
    }
}
