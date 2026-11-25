using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ComboBoxToken"/>.</summary>
[ControlTokenRule]
internal sealed class ComboBoxTokenRule : IControlTokenRule<AntDesignToken, ComboBoxToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ComboBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
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
    }
}
