using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="ArrowDecoratedBoxToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ArrowDecoratedBoxTokenRule : IControlTokenRule<ShadcnToken, ArrowDecoratedBoxToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ArrowDecoratedBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // antd genRoundedArrow: sizePopupArrow wide, half as high, borderRadiusXS at the tip and borderRadiusOuter
        // where the sides meet the box.
        token.ArrowSize            = global.SizePopupArrow;
        token.ArrowTipRadius       = global.BorderRadiusXS.TopLeft;
        token.ArrowBaseRadius      = global.BorderRadiusOuter.TopLeft;
        token.ContentPadding       = global.PaddingXS;
        token.ArrowStrokeColor     = ColorUtils.FromRgbF(0.07, 0, 0, 0);
        token.ArrowStrokeThickness = 1;
        token.ContentBorderColor     = global.ColorBorderSecondary;
        token.ContentBorderThickness = default;
        token.ContentMinHeight       = global.ControlHeight;
        // The outline ring color as an integrated one-line border along the arrow's sides too, the popover padding, and
        // the tooltip's arrow (shadcn popovers have no arrow of their own).
        var popover = context.Language.GetPopoverOverlayRecipe(global);
        token.ContentBorderColor     = popover.BorderColor;
        token.ContentBorderThickness = new Thickness(1);
        token.ContentPadding         = popover.Padding;
        token.ArrowSize              = ShadcnOverlayShapes.ArrowSize;
        token.ArrowTipRadius         = ShadcnOverlayShapes.ArrowTipRadius;
        token.ArrowBaseRadius        = 0;
    }
}
