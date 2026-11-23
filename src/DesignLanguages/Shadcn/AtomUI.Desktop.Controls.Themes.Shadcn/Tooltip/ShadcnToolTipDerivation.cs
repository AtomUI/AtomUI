using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ToolTipToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnToolTipDerivation : IControlDerivation<ShadcnToken, ToolTipToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ToolTipToken> view)
    {
        var token  = view.Token;
        var global = view.Global;

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
