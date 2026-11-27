using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="ComboBoxToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ComboBoxTokenRule : IControlTokenRule<PrimeNGToken, ComboBoxToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ComboBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGComboBoxTokens.EvaluateGlobal(in context, global);
        token.PopupContentPadding  = new Thickness(global.UniformlyPaddingXXS, global.BorderRadiusLG.TopLeft / 2);

        var colorTextDisabled  = global.ColorTextDisabled;
        var colorTextSecondary = global.ColorTextSecondary;
        var colorBgContainer   = global.ColorBgElevated;
        var colorBgTextHover   = global.ColorBgTextHover;

        // Ant Design formula kept as is; the PrimeNG combo box stage (PrimeNGComboBoxTokens.EvaluateOwn)
        // replaces the item height and padding, the item and hover text colors, the hover background and
        // the popup padding. The item margin and the selected, disabled and handle colors stay.
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
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGComboBoxTokens.EvaluateOwn(in context, global, token);
    }
}
