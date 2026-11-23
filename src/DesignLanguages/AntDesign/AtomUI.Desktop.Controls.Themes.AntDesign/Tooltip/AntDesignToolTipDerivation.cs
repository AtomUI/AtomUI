using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ToolTipToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignToolTipDerivation : IControlDerivation<AntDesignToken, ToolTipToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<ToolTipToken> view)
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
    }
}
