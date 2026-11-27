using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="PaginationToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class PaginationTokenRule : IControlTokenRule<PrimeNGToken, PaginationToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<PaginationToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (UniformlyMarginXS), read by the formulas below
        // (PaginationTokenCalculator) and by the Pagination theme (ItemSpacing).
        PrimeNGPaginationTokens.EvaluateGlobal(in context, global);
        PaginationTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: the PrimeNG paginator stage at the end of the method replaces the item
        // size, radius and colors (ItemColor, ItemHover*, ItemActive*).
        token.PaginationItemPaddingInline = new Thickness(global.UniformlyMarginXXS * 1.5, 0);
        token.ItemBg                    = global.ColorBgContainer;
        token.ItemActiveBg              = global.ColorBgContainer;
        token.ItemLinkBg                = global.ColorBgContainer;
        token.ItemActiveBgDisabled      = global.ControlItemBgActiveDisabled;
        token.ItemActiveColor            = global.ColorPrimary;
        token.ItemActiveBorderColor      = global.ColorPrimary;
        token.ItemActiveHoverColor       = global.ColorPrimaryHover;
        token.ItemActiveHoverBorderColor = global.ColorPrimaryHover;
        token.ItemActiveFontWeight       = global.FontWeightStrong;

        token.InputOutlineOffset                  = new Thickness(0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGPaginationTokens.EvaluateOwn(in context, global, token);
    }
}
