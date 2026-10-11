using AtomUI.Media;
using AtomUI.Theme.Algorithms;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

public partial class DesignToken : AbstractDesignToken
{
    private const int PresetColorCount = 14;
    private static readonly IReadOnlyDictionary<PresetColorType, PresetPrimaryColor> s_defaultPresetColors =
        BuildDefaultPresetColors();
    private static readonly Color s_defaultColorPrimary = Color.Parse("#1677ff");
    private static readonly Color s_defaultColorSuccess = Color.Parse("#52c41a");
    private static readonly Color s_defaultColorWarning = Color.Parse("#faad14");
    private static readonly Color s_defaultColorError = Color.Parse("#ff4d4f");
    // FontFamily parsing creates an immutable description; font resolution remains with FontManager.
    private static readonly FontFamily DefaultFontFamily = FontFamily.Parse("fonts:AlibabaSans#Alibaba Sans, Segoe UI, Segoe UI Symbol, Helvetica Neue, Noto Sans, Noto Sans CJK SC, 文泉驿正黑, Microsoft YaHei, PingFang SC, $Default");

    /// <summary>
    /// 现在这里的实现是写死的主色，后面是不是可以读取配置
    /// </summary>

    [NotTokenDefinition] public IDictionary<PresetPrimaryColor, ColorMap> ColorPalettes { get; set; }

    public DesignToken() : this(initializeSeed: true)
    {
    }

    internal DesignToken(bool initializeSeed)
    {
        if (initializeSeed)
        {
            InitSeedTokenValues();
        }
        ColorPalettes = new Dictionary<PresetPrimaryColor, ColorMap>(PresetColorCount);
    }

    private void InitSeedTokenValues()
    {
        ColorPrimary     = s_defaultColorPrimary;
        ColorSuccess     = s_defaultColorSuccess;
        ColorWarning     = s_defaultColorWarning;
        ColorError       = s_defaultColorError;
        ColorInfo        = s_defaultColorPrimary;
        FontFamily       = DefaultFontFamily;
        BorderRadius     = new CornerRadius(6);
        ColorTransparent = Colors.Transparent;
    }

    public PresetPrimaryColor GetPresetPrimaryColor(PresetColorType colorType)
    {
        return s_defaultPresetColors[colorType];
    }

    public void SetColorPalette(PresetPrimaryColor primaryColor, ColorMap colorMap)
    {
        ColorPalettes[primaryColor] = colorMap;
    }

    public ColorMap? GetColorPalette(PresetPrimaryColor primaryColor)
    {
        ColorMap? value;
        if (ColorPalettes.TryGetValue(primaryColor, out value))
        {
            return value;
        }

        return null;
    }

    internal void CalculateAliasTokenValues()
    {
        var screenXS  = 480;
        var screenSM  = 576;
        var screenMD  = 768;
        var screenLG  = 992;
        var screenXL  = 1200;
        var screenXXL = 1600;
        var screenXXXL = 1920;

        // setup alias token
        // ============== Background ============== //
        ColorFillContent         = ColorFillSecondary;
        ColorFillContentHover    = ColorFill;
        ColorFillAlter           = ColorFillQuaternary;
        ColorBgContainerDisabled = ColorFillTertiary;

        // ============== Split ============== //
        ColorBorderBg = ColorBgContainer;
        ColorSplit    = ColorUtils.CalculateAlphaColor(ColorBorderSecondary, ColorBgContainer);

        // ============== Text ============== //
        ColorTextPlaceholder = ColorTextQuaternary;
        ColorTextDisabled    = ColorTextQuaternary;
        ColorTextHeading     = ColorText;
        ColorTextLabel       = ColorTextSecondary;
        ColorTextDescription = ColorTextTertiary;
        ColorTextLightSolid  = ColorWhite;
        ColorHighlight       = ColorError;
        ColorBgTextHover     = ColorFillSecondary;
        ColorBgTextActive    = ColorFill;

        ColorIcon      = ColorTextTertiary;
        ColorIconHover = ColorText;

        ColorErrorOutline   = ColorUtils.CalculateAlphaColor(ColorErrorBg, ColorBgContainer);
        ColorWarningOutline = ColorUtils.CalculateAlphaColor(ColorWarningBg, ColorBgContainer);

        // Font
        FontSizeIcon = FontSizeSM;

        // icon
        IconSizeXS = (int)FontSizeSM - 2;
        IconSizeSM = (int)FontSizeSM;
        IconSize   = (int)FontSize;
        IconSizeLG = (int)FontSizeLG;

        // Line
        LineWidthFocus             = LineWidth * 2;
        FocusVisualBorderThickness = new Thickness(LineWidthFocus);
        ColorFocusBorder           = ColorPrimaryBorder;
        WaveAnimationRange         = LineWidth * 6;
        WaveStartOpacity           = 0.4;

        // Control
        ControlOutlineWidth = LineWidth * 2;

        // Checkbox size and expand icon size
        ControlInteractiveSize = ControlHeight / 2;

        ControlItemBgHover          = ColorFillTertiary;
        ControlItemBgActive         = ColorPrimaryBg;
        ControlItemBgActiveHover    = ColorPrimaryBgHover;
        ControlItemBgActiveDisabled = ColorFill;
        ColorControlOutline         = ColorUtils.CalculateAlphaColor(ColorPrimaryBg, ColorBgContainer);

        FontWeightStrong    = FontWeight.SemiBold;
        OpacityLoading      = 0.65;
        LinkDecoration      = null;
        LinkHoverDecoration = null;
        LinkFocusDecoration = null;

        ControlPaddingHorizontal   = 12;
        ControlPaddingHorizontalSM = 8;

        UniformlyPaddingXXS = SizeXXS;
        UniformlyPaddingXS  = SizeXS;
        UniformlyPaddingSM  = SizeSM;
        UniformlyPadding    = Size;
        UniformlyPaddingMD  = SizeMD;
        UniformlyPaddingLG  = SizeLG;
        UniformlyPaddingXL  = SizeXL;
        
        PaddingXXS = new Thickness(SizeXXS);
        PaddingXS  = new Thickness(SizeXS);
        PaddingSM  = new Thickness(SizeSM);
        Padding    = new Thickness(Size);
        PaddingMD  = new Thickness(SizeMD);
        PaddingLG  = new Thickness(SizeLG);
        PaddingXL  = new Thickness(SizeXL);

        InputPadding = new Thickness(
            UniformlyPaddingSM - LineWidth,
            Math.Round((ControlHeight - FontSize * RelativeLineHeight) / 2 * 10) / 10 - LineWidth);
        InputPaddingSM = new Thickness(
            ControlPaddingHorizontalSM - LineWidth,
            Math.Round((ControlHeightSM - FontSize * RelativeLineHeight) / 2 * 10) / 10 - LineWidth * 2);
        InputPaddingLG = new Thickness(
            ControlPaddingHorizontal - LineWidth,
            Math.Ceiling((ControlHeightLG - FontSizeLG * RelativeLineHeightLG) / 2 * 10) / 10 - LineWidth);
        InputContentMargin = new Thickness(LineWidth);
        InputActiveShadow = new BoxShadows(new BoxShadow
        {
            Spread = ControlOutlineWidth,
            Color  = ColorControlOutline
        });
        InputErrorActiveShadow = new BoxShadows(new BoxShadow
        {
            Spread = ControlOutlineWidth,
            Color  = ColorErrorOutline
        });
        InputWarningActiveShadow = new BoxShadows(new BoxShadow
        {
            Spread = ControlOutlineWidth,
            Color  = ColorWarningOutline
        });

        PaddingContentHorizontalLG = SizeLG;
        PaddingContentVerticalLG   = SizeMS;
        PaddingContentHorizontal   = SizeMS;
        PaddingContentVertical     = SizeSM;
        PaddingContentHorizontalSM = SizeSM;
        PaddingContentHorizontalXS = SizeXS;
        PaddingContentVerticalSM   = SizeXS;

        UniformlyMarginXXS = SizeXXS;
        UniformlyMarginXS  = SizeXS;
        UniformlyMarginSM  = SizeSM;
        UniformlyMargin    = Size;
        UniformlyMarginMD  = SizeMD;
        UniformlyMarginLG  = SizeLG;
        UniformlyMarginXL  = SizeXL;
        UniformlyMarginXXL = SizeXXL;

        SpacingXXS = SizeXXS;
        SpacingXS  = SizeXS;
        SpacingSM  = SizeSM;
        Spacing    = Size;
        SpacingMD  = SizeMD;
        SpacingLG  = SizeLG;
        SpacingXL  = SizeXL;
        SpacingXXL = SizeXXL;
        
        MarginXXS          = new Thickness(SizeXXS);
        MarginXS           = new Thickness(SizeXS);
        MarginSM           = new Thickness(SizeSM);
        Margin             = new Thickness(Size);
        MarginMD           = new Thickness(SizeMD);
        MarginLG           = new Thickness(SizeLG);
        MarginXL           = new Thickness(SizeXL);
        MarginXXL          = new Thickness(SizeXXL);

        ScreenXS     = screenXS;
        ScreenXSMin  = screenXS;
        ScreenXSMax  = screenSM - 1;
        ScreenSM     = screenSM;
        ScreenSMMin  = screenSM;
        ScreenSMMax  = screenMD - 1;
        ScreenMD     = screenMD;
        ScreenMDMin  = screenMD;
        ScreenMDMax  = screenLG - 1;
        ScreenLG     = screenLG;
        ScreenLGMin  = screenLG;
        ScreenLGMax  = screenXL - 1;
        ScreenXL     = screenXL;
        ScreenXLMin  = screenXL;
        ScreenXLMax  = screenXXL - 1;
        ScreenXXL     = screenXXL;
        ScreenXXLMin  = screenXXL;
        ScreenXXLMax  = screenXXXL - 1;
        ScreenXXXL    = screenXXXL;
        ScreenXXXLMin = screenXXXL;

        BoxShadows = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 6,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        }, [new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 3,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 9,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.05, 0, 0, 0)
            }]);

        BoxShadowsSecondary = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 6,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.10, 0, 0, 0)
        }, [new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 3,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 9,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.07, 0, 0, 0)
            }]);

        BoxShadowsTertiary = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 1,
            Blur    = 2,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.03, 0, 0, 0)
        }, [new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 1,
                Blur    = 6,
                Spread  = -1,
                Color   = ColorUtils.FromRgbF(0.02, 0, 0, 0)
            },
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 2,
                Blur    = 4,
                Spread  = 0,
                Color   = ColorUtils.FromRgbF(0.02, 0, 0, 0)
            }]);
    }
    
    private static IReadOnlyDictionary<PresetColorType, PresetPrimaryColor> BuildDefaultPresetColors()
    {
        var colors = new Dictionary<PresetColorType, PresetPrimaryColor>(PresetColorCount);
        foreach (var color in PresetPrimaryColor.AllColorTypes())
        {
            colors[color.Type] = color;
        }

        return colors;
    }
}
