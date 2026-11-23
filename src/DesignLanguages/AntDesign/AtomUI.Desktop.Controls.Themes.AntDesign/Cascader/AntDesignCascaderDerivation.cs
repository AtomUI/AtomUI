using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="CascaderToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignCascaderDerivation : IControlDerivation<AntDesignToken, CascaderToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<CascaderToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractSelectInputDerivation.DeriveShared(in context, global, token);
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
        token.OptionBorderRadius       = global.BorderRadius;
        token.OptionSelectedColor      = global.ControlItemColorActive;
        token.HeaderHeight             = global.ControlHeightSM;
        token.FilterHighlightColor     = global.ColorError;
        token.ItemHeaderSpacing        = global.SpacingXXS;
    }
}
