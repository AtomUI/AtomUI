using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class SplitButtonToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public double GutterToFlyout { get; set; }

    [ControlTokenDefault(nameof(GutterToFlyout))]
    private static double DefaultForGutterToFlyout(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXXS;
}
