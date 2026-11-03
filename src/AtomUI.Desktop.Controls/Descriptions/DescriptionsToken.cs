using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class DescriptionsToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 标签背景色
    /// Background color of label
    /// </summary>
    public Color LabelBg { get; set; }

    /// <summary>
    /// 标签文字颜色
    /// Text color of label
    /// </summary>
    public Color LabelColor { get; set; }

    /// <summary>
    /// 标题文字颜色
    /// Text color of title
    /// </summary>
    public Color TitleColor { get; set; }

    /// <summary>
    /// 标题下间距
    /// Bottom margin of Header
    /// </summary>
    public Thickness HeaderMargin { get; set; }

    /// <summary>
    /// 子项下间距，大号
    /// padding of item
    /// </summary>
    public Thickness ItemPaddingLG { get; set; }
    
    /// <summary>
    /// 子项下间距，默认
    /// padding of item
    /// </summary>
    public Thickness ItemPadding { get; set; }
    
    /// <summary>
    /// 子项下间距，小号
    /// padding of item
    /// </summary>
    public Thickness ItemPaddingSM { get; set; }

    /// <summary>
    /// 冒号间距
    /// margin of colon
    /// </summary>
    public Thickness ColonMargin { get; set; }

    /// <summary>
    /// 内容区域文字颜色
    /// Text color of content
    /// </summary>
    public Color ContentColor { get; set; }

    /// <summary>
    /// 额外区域文字颜色
    /// Text color of extra area
    /// </summary>
    public Color ExtraColor { get; set; }



    [ControlTokenDefault(nameof(LabelBg))]
    private static Color DefaultForLabelBg(in ControlTokenDefaultContext context) => context.Global.ColorFillAlter;

    [ControlTokenDefault(nameof(LabelColor))]
    private static Color DefaultForLabelColor(in ControlTokenDefaultContext context) => context.Global.ColorTextTertiary;

    [ControlTokenDefault(nameof(TitleColor))]
    private static Color DefaultForTitleColor(in ControlTokenDefaultContext context) => context.Global.ColorText;

    [ControlTokenDefault(nameof(HeaderMargin))]
    private static Thickness DefaultForHeaderMargin(in ControlTokenDefaultContext context) => new Thickness(0, 0, 0, context.Global.FontSizeSM * context.Global.RelativeLineHeightSM);

    [ControlTokenDefault(nameof(ItemPaddingLG))]
    private static Thickness DefaultForItemPaddingLG(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyPaddingLG, context.Global.UniformlyPadding);

    [ControlTokenDefault(nameof(ItemPadding))]
    private static Thickness DefaultForItemPadding(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyPaddingLG, context.Global.UniformlyPaddingSM);

    [ControlTokenDefault(nameof(ItemPaddingSM))]
    private static Thickness DefaultForItemPaddingSM(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyPadding, context.Global.UniformlyPaddingXS);

    [ControlTokenDefault(nameof(ColonMargin))]
    private static Thickness DefaultForColonMargin(in ControlTokenDefaultContext context) => new Thickness(context.Global.UniformlyMarginXXS / 2, 0, context.Global.UniformlyMarginXS, 0);

    [ControlTokenDefault(nameof(ContentColor))]
    private static Color DefaultForContentColor(in ControlTokenDefaultContext context) => context.Global.ColorText;

    [ControlTokenDefault(nameof(ExtraColor))]
    private static Color DefaultForExtraColor(in ControlTokenDefaultContext context) => context.Global.ColorText;
}
