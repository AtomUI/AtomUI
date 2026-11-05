using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class FormToken : AbstractControlDesignToken, IControlTokenDefaults
{
    /// <summary>
    /// 必填项标记颜色
    /// Required mark color
    /// </summary>
    public Color LabelRequiredMarkColor { get; set; }

    /// <summary>
    /// 标签颜色
    /// Label color
    /// </summary>
    public Color LabelColor { get; set; }

    /// <summary>
    /// 标签字体大小
    /// Label font size
    /// </summary>
    public double LabelFontSize { get; set; }

    /// <summary>
    /// 标签冒号间距
    /// Label colon margin-inline
    /// </summary>
    public Thickness LabelColonMargin { get; set; }

    /// <summary>
    /// 表单项间距
    /// Form item spacing
    /// </summary>
    public double FormItemSpacing { get; set; }

    /// <summary>
    /// 行内布局表单项间距
    /// Inline layout form item spacing
    /// </summary>
    public double InlineItemSpacing { get; set; }

    /// <summary>
    /// 垂直布局标签内边距
    /// Vertical layout label padding
    /// </summary>
    // Compatibility resource retained; the built-in FormItem template does not consume it.
    public Thickness VerticalLabelPadding { get; set; }

    /// <summary>
    /// 垂直布局标签外边距
    /// Vertical layout label margin
    /// </summary>
    public Thickness VerticalLabelMargin { get; set; }

    [ControlTokenDefault(nameof(LabelRequiredMarkColor))]
    private static Color DefaultForLabelRequiredMarkColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(LabelColor))]
    private static Color DefaultForLabelColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextHeading;

    [ControlTokenDefault(nameof(LabelFontSize))]
    private static double DefaultForLabelFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(LabelColonMargin))]
    private static Thickness DefaultForLabelColonMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXXS / 2, 0, context.Global.UniformlyMarginXS, 0);

    [ControlTokenDefault(nameof(FormItemSpacing))]
    private static double DefaultForFormItemSpacing(in ControlTokenDefaultContext context) =>
        context.Global.SpacingLG;

    [ControlTokenDefault(nameof(VerticalLabelPadding))]
    private static Thickness DefaultForVerticalLabelPadding(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, 0, context.Global.UniformlyPaddingXS);

    [ControlTokenDefault(nameof(InlineItemSpacing))]
    private static double DefaultForInlineItemSpacing(in ControlTokenDefaultContext context) =>
        context.Global.Spacing;

    [ControlTokenDefault(nameof(VerticalLabelMargin))]
    private static Thickness DefaultForVerticalLabelMargin(in ControlTokenDefaultContext context) =>
        default;

}
