using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class AlertToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// 默认内间距
    /// </summary>
    public Thickness DefaultPadding { get; set; }

    /// <summary>
    /// 带有描述的内间距
    /// </summary>
    public Thickness WithDescriptionPadding { get; set; }

    /// <summary>
    /// 带有描述的标题外间距；仅明确 Own 配置时应用，未配置保持 0
    /// </summary>
    public Thickness MessageWithDescriptionMargin { get; set; }

    /// <summary>
    /// 图标默认外间距
    /// </summary>
    public Thickness IconDefaultMargin { get; set; }

    /// <summary>
    /// 图标带描述信息外间距
    /// </summary>
    public Thickness IconWithDescriptionMargin { get; set; }

    /// <summary>
    /// 没有描述时的图标尺寸
    /// </summary>
    public double DefaultIconSize { get; set; }

    /// <summary>
    /// 带有描述时的图标尺寸
    /// </summary>
    public double WithDescriptionIconSize { get; set; }

    /// <summary>
    /// 关闭图标大小；仅明确 Own 配置时应用，未配置保持有效 Global IconSizeSM
    /// </summary>
    public double CloseIconSize { get; set; }

    /// <summary>
    /// 额外元素的外间距
    /// </summary>
    public Thickness ExtraElementMargin { get; set; }
    
    /// <summary>
    /// 描述标签外间距
    /// </summary>
    public Thickness DescriptionLabelMargin { get; set; }

    [ControlTokenDefault(nameof(DefaultPadding))]
    private static Thickness DefaultForDefaultPadding(in ControlTokenDefaultContext context) =>
        new Thickness(12, context.Global.PaddingContentVerticalSM);

    [ControlTokenDefault(nameof(WithDescriptionPadding))]
    private static Thickness DefaultForWithDescriptionPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.PaddingContentHorizontalLG, context.Global.UniformlyPaddingMD);

    [ControlTokenDefault(nameof(MessageWithDescriptionMargin))]
    private static Thickness DefaultForMessageWithDescriptionMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, 0, context.Global.UniformlyMarginXS);

    [ControlTokenDefault(nameof(IconDefaultMargin))]
    private static Thickness DefaultForIconDefaultMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyMarginXS, 0);

    [ControlTokenDefault(nameof(IconWithDescriptionMargin))]
    private static Thickness DefaultForIconWithDescriptionMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyMarginSM, 0);

    [ControlTokenDefault(nameof(DefaultIconSize))]
    private static double DefaultForDefaultIconSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeLG;

    [ControlTokenDefault(nameof(WithDescriptionIconSize))]
    private static double DefaultForWithDescriptionIconSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeHeading3;

    [ControlTokenDefault(nameof(CloseIconSize))]
    private static double DefaultForCloseIconSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeIcon + 2;

    [ControlTokenDefault(nameof(ExtraElementMargin))]
    private static Thickness DefaultForExtraElementMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXS, 0, 0, 0);

    [ControlTokenDefault(nameof(DescriptionLabelMargin))]
    private static Thickness DefaultForDescriptionLabelMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXS, 0, 0);
}
