using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CascaderToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnCascaderDerivation : IControlDerivation<ShadcnToken, CascaderToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<CascaderToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnInputFamily.DeriveGlobal(in context, global);
        ShadcnAbstractSelectInputDerivation.DeriveShared(in context, global, token);
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
