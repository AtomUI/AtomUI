using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class DatePickerToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public Thickness PanelContentPadding { get; set; }
    public Thickness ButtonsPanelMargin { get; set; }
    public double RangePanelSpacing { get; set; }

    [ControlTokenDefault(nameof(PanelContentPadding))]
    private static Thickness DefaultForPanelContentPadding(in ControlTokenDefaultContext context) =>
        context.Global.PaddingSM;

    [ControlTokenDefault(nameof(ButtonsPanelMargin))]
    private static Thickness DefaultForButtonsPanelMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXS, 0, 0);

    [ControlTokenDefault(nameof(RangePanelSpacing))]
    private static double DefaultForRangePanelSpacing(in ControlTokenDefaultContext context) =>
        20;
}
