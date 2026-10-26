using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TransferListViewToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public Thickness PaginationMargin { get; set; }

    [ControlTokenDefault(nameof(PaginationMargin))]
    private static Thickness DefaultForPaginationMargin(in ControlTokenDefaultContext context) =>
        new(0, context.Global.UniformlyMarginXXS);
}
