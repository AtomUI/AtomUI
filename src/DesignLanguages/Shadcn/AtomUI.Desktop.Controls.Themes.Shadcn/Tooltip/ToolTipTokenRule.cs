using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="ToolTipToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ToolTipTokenRule : IControlTokenRule<ShadcnToken, ToolTipToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ToolTipToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        // Ant Design formula kept as is: registry `tooltip` is merged only as the surface, radius, padding, shadow
        // and arrow, at the end of this method.
        token.ToolTipMaxWidth   = 250;
        token.ToolTipFontSize   = global.FontSize;
        token.ToolTipMinHeight  = global.ControlHeight;
        token.ToolTipColor      = global.ColorTextLightSolid;
        token.ToolTipBackground = global.ColorBgSpotlight;
        token.ToolTipCornerRadius = new CornerRadius(4);
        token.ContentPadding = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingSM / 2 + 2);
        token.MotionDuration = global.MotionDurationFast;
        token.ToolTipShadows = global.BoxShadowsSecondary;
        token.MarginToAnchor       = 4;
        token.ArrowSize            = global.SizePopupArrow;
        token.ArrowTipRadius       = global.BorderRadiusXS.TopLeft;
        token.ArrowBaseRadius      = global.BorderRadiusOuter.TopLeft;
        token.ArrowStrokeThickness = 1;
        // The inverted surface, the style's radius, the 12 x 6 padding; no shadow and no ring.
        token.ToolTipBackground   = context.Language.Foreground;
        token.ToolTipColor        = context.Language.Background;
        token.ToolTipCornerRadius = new CornerRadius(context.Language.TooltipRadius);
        token.ContentPadding      = new Thickness(12, 6, 12, 6);
        token.ToolTipShadows      = default;
        token.ArrowSize           = ShadcnOverlayShapes.ArrowSize;
        token.ArrowTipRadius      = ShadcnOverlayShapes.ArrowTipRadius;
        token.ArrowBaseRadius     = 0;
    }
}
