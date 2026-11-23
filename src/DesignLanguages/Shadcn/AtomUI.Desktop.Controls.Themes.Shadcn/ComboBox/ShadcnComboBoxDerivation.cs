using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ComboBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnComboBoxDerivation : IControlDerivation<ShadcnToken, ComboBoxToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ComboBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.PopupContentPadding  = new Thickness(global.UniformlyPaddingXXS, global.BorderRadiusLG.TopLeft / 2);

        var colorTextDisabled  = global.ColorTextDisabled;
        var colorTextSecondary = global.ColorTextSecondary;
        var colorBgContainer   = global.ColorBgElevated;
        var colorBgTextHover   = global.ColorBgTextHover;

        token.ItemColor         = colorTextSecondary;
        token.ItemHoverColor    = colorTextSecondary;
        token.ItemSelectedColor = global.ControlItemColorActive;

        token.ItemBgColor         = colorBgContainer;
        token.ItemHoverBgColor    = colorBgTextHover;
        token.ItemSelectedBgColor = global.ControlItemBgActive;
        token.ItemDisabledColor = colorTextDisabled;
        token.HandleHoverColor = global.ColorPrimary;

        token.ItemPadding = new Thickness(global.UniformlyPaddingSM, 0);
        token.ItemHeight  = global.ControlHeight;
        token.ItemMargin  = new Thickness(0, 0.5);
        if (context.Variant.IsDark)
        {
            // Dark shadcn resolves the two source surfaces to the same shade.
            token.ItemSelectedBgColor = ShadcnRecipes.CompositeOver(global.ColorText, 0.14, token.ItemSelectedBgColor);
        }
    }
}
