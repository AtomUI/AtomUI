using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class RibbonBadgeToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public Point Offset { get; set; }

    public ImmutableTransform? CornerTransform { get; set; }

    public int CornerDarkenAmount { get; set; }

    public Thickness TextPadding { get; set; }

    [ControlTokenDefault(nameof(Offset))]
    private static Point DefaultForOffset(in ControlTokenDefaultContext context) => new(context.Global.UniformlyMarginXS, context.Global.UniformlyMarginXS);

    [ControlTokenDefault(nameof(CornerTransform))]
    private static ImmutableTransform? DefaultForCornerTransform(in ControlTokenDefaultContext context) => new ScaleTransform(1, 0.75).ToImmutable();

    [ControlTokenDefault(nameof(CornerDarkenAmount))]
    private static int DefaultForCornerDarkenAmount(in ControlTokenDefaultContext context) => 15;

    [ControlTokenDefault(nameof(TextPadding))]
    private static Thickness DefaultForTextPadding(in ControlTokenDefaultContext context) => new(context.Global.UniformlyPaddingXS, 0);
}
