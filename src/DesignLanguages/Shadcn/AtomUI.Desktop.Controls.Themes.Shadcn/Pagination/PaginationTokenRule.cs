using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="PaginationToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class PaginationTokenRule : IControlTokenRule<ShadcnToken, PaginationToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<PaginationToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        PaginationTokenCalculator.Calculate(in context, global, token);
        // Ant Design formula kept as is: no registry value is merged into the pagination tokens.
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
    }
}
