using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="CascaderToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class CascaderTokenRule : IControlTokenRule<PrimeNGToken, CascaderToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<CascaderToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides of PrimeNGInputStage.Apply (form-field values): the formulas below
        // read ColorTextDisabled; the Cascader theme and its input frame (Foreground, CornerRadius, Background) too.
        PrimeNGCascaderTokens.EvaluateGlobal(in context, global);
        SelectInputTokenCalculator.Calculate(in context, global, token);
        var itemPaddingVertical = Math.Round((global.ControlHeight - global.FontHeight) / 2);
        // Ant Design formula kept as is; the PrimeNG cascader stage (PrimeNGCascaderTokens.EvaluateOwn)
        // replaces the option padding, radius, margin, hover background and selected font weight, the menu
        // padding and the chip background. The widths 184 and 111 and the drop-down height 180 stay.
        token.ControlWidth             = 184;
        token.ControlItemWidth         = 111;
        token.DropdownHeight           = 180;
        token.OptionHoverBg            = global.ControlItemBgHover;
        token.OptionSelectedBg         = global.ControlItemBgActive;
        token.OptionSelectedFontWeight = global.FontWeightStrong;
        token.OptionPadding            = new Thickness(global.UniformlyPaddingSM, itemPaddingVertical);
        token.MenuPadding              = global.PaddingXXS;
        token.OptionMargin             = default;
        token.OptionBorderRadius       = global.BorderRadius;
        token.OptionSelectedColor      = global.ControlItemColorActive;
        token.HeaderHeight             = global.ControlHeightSM;
        token.FilterHighlightColor     = global.ColorError;
        token.ItemHeaderSpacing        = global.SpacingXXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGCascaderTokens.EvaluateOwn(in context, global, token);
    }
}
