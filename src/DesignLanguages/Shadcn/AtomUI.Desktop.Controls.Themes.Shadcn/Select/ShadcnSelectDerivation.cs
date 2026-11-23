using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="SelectToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnSelectDerivation : IControlDerivation<ShadcnToken, SelectToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<SelectToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        var item   = context.Language.GetListItemRecipe(global);
        var frame = ShadcnInputFamily.DeriveFieldGlobal(in context, global);
        ShadcnAbstractSelectInputDerivation.DeriveShared(in context, global, token);
        token.TransparentOutlineBorder = false;
        token.BottomOutlineBorder      = false;
        token.MutedSelectIndicator     = false;
        token.OptionSelectedColor      = item.SelectedForeground;
        token.OptionSelectedIndicatorColor = global.InputActiveBorderColor;
        token.OptionSelectedFontWeight = global.FontWeightStrong;
        token.OptionSelectedBg         = item.SelectedBackground;
        token.OptionActiveBg           = item.FocusBackground;
        token.OptionPadding            = new Thickness(global.ControlPaddingHorizontal, (global.ControlHeight - global.FontHeight) / 2);
        token.OptionHeight = global.ControlHeight;
        token.SelectAffixPadding = global.PaddingXXS;
        token.FixedItemMargin = new Thickness(Math.Floor(global.UniformlyPaddingXXS / 2));
        token.PopupContentPadding = new Thickness(global.UniformlyPaddingXXS / 2);
        // Ant Design's option highlight fills the row without an inset or rounding.
        token.OptionBackgroundInset = new Thickness(0);
        token.OptionBorderRadius    = new CornerRadius(0);
        token.OptionBackgroundInset = new Thickness(0, global.LineWidth);
        (token.TransparentOutlineBorder, token.BottomOutlineBorder) = frame;
        token.MutedSelectIndicator = context.Variant.Style == ShadcnStyles.Luma;
    }
}
