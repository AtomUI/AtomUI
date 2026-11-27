using Avalonia;
using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// The brand primary color. It is the input the primary color palette is generated from, and that palette
    /// supplies the primary color gradient.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color ColorPrimary { get; set; }

    /// <summary>
    /// The success color. It is the input of the success color gradient, which components use to report a
    /// successful operation, for example Result and Progress.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color ColorSuccess { get; set; }

    /// <summary>
    /// The warning color. It is the input of the warning color gradient, which components use to report a
    /// warning, such as Notification and Alert, and which input components use in the warning state.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color ColorWarning { get; set; }

    /// <summary>
    /// The error color. It is the input of the error color gradient, which components use to report a failed
    /// operation, for example the button of a failed action or the error state of Result.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color ColorError { get; set; }

    /// <summary>
    /// The info color. It is the input of the info color gradient, which components use to present information,
    /// for example Alert, Tag and Progress.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color ColorInfo { get; set; }

    /// <summary>
    /// The base color the text color gradient is derived from. Do not use this seed directly in control code;
    /// read a text color token derived from it instead.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color? ColorTextBase { get; set; }

    /// <summary>
    /// The base color the background color gradient is derived from. Do not use this seed directly in control
    /// code; read a background color token derived from it instead.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color? ColorBgBase { get; set; }

    /// <summary>
    /// The hyperlink color. It is the input of the link color gradient.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color? ColorLink { get; set; }

    /// <summary>
    /// A fully transparent color. Controls use it where they paint no background, for example the item
    /// background of ListBox and ListView.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial Color ColorTransparent { get; set; }

    //  ----------   Font   ---------- //
    /// <summary>
    /// The base font family of every text. The default starts with the bundled Alibaba Sans and then falls back
    /// to the interface fonts of each platform, so that text stays legible on every platform and display density.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial FontFamily? FontFamily { get; set; }

    /// <summary>
    /// The default font size, used by most text. The font size gradient, from <see cref="FontSizeSM" /> to the
    /// heading sizes, is derived from it.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial double FontSize { get; set; } = 14;

    //  ----------   Line   ---------- //
    /// <summary>
    /// The base line width, used for component borders and dividers.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial double LineWidth { get; set; } = 1;

    /// <summary>
    /// The style of the lines of component borders and dividers, such as solid or dashed. The default is solid.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial LineStyle LineType { get; set; } = LineStyle.Solid;

    //  ----------   BorderRadius   ---------- //
    /// <summary>
    /// The base corner radius of controls such as buttons, inputs and cards. The other corner radius tokens are
    /// derived from its top-left corner.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial CornerRadius BorderRadius { get; set; }

    /// <summary>
    /// The unit of the size gradient: every size token is a multiple of it. It is also used directly for
    /// spacing, such as the paddings and the logo size of the window title bar.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial double SizeUnit { get; set; } = 4;

    /// <summary>
    /// The base step of the size gradient: <see cref="Size" /> equals <see cref="SizeUnit" /> times this value,
    /// and the other size tokens are <see cref="SizeUnit" /> times this value plus or minus an offset. A compact
    /// layout derives the gradient from a smaller step.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial double SizeStep { get; set; } = 4;

    /// <summary>
    /// The size of the arrow of popups such as tooltips.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial double SizePopupArrow { get; set; } = 16;

    /// <summary>
    /// The base height of controls such as buttons and inputs. <see cref="ControlHeightXS" />,
    /// <see cref="ControlHeightSM" /> and <see cref="ControlHeightLG" /> are derived from it as fractions of it.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial double ControlHeight { get; set; } = 32;

    //  ----------   zIndex   ---------- //
    /// <summary>
    /// The base Z order of components on a page. Elements that float over the page, such as BackTop and Affix,
    /// derive their Z order from it.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial int ZIndexBase { get; set; } = 0;

    /// <summary>
    /// The base Z order of popups and other elements that open a layer above the page, such as Modal, Affix and
    /// FloatButton.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial int ZIndexPopupBase { get; set; } = 1000;

    //  ----------   Opacity   ---------- //
    /// <summary>
    /// The opacity of images, from 0 for fully transparent to 1 for fully opaque.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial double OpacityImage { get; set; } = 1.0;

    //  ----------   motion   ---------- //
    /// <summary>
    /// The unit of the motion durations, in milliseconds. The durations are <see cref="MotionBase" /> plus 1, 2,
    /// 3 and 8 times this value.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial int MotionUnit { get; set; } = 100;

    /// <summary>
    /// The base of the motion durations, in milliseconds: every duration is this value plus a multiple of
    /// <see cref="MotionUnit" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial int MotionBase { get; set; } = 0;

    //  ----------   Style   ---------- //

    /// <summary>
    /// Enables motion. When <see langword="false" />, the theme compiler sets <see cref="MotionDurationFast" />,
    /// <see cref="MotionDurationMid" />, <see cref="MotionDurationSlow" /> and
    /// <see cref="MotionDurationVerySlow" /> to a zero duration.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial bool EnableMotion { get; set; } = true;
    
    /// <summary>
    /// Enables the wave animation that buttons and radio buttons play when clicked. When
    /// <see langword="false" />, the theme compiler sets <c>WaveAnimationRange</c> and
    /// <c>WaveStartOpacity</c> to 0.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public partial bool EnableInteractionEffect { get; set; } = true;
}