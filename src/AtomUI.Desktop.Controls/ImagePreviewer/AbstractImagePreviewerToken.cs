using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractImagePreviewerToken : AbstractControlDesignToken
{

    /// <summary>
    /// 预览操作图标大小
    /// Size of preview operation icon
    /// </summary>
    public partial double PreviewOperationSize { get; set; }

    /// <summary>
    /// 预览操作图标颜色
    /// Color of preview operation icon
    /// </summary>
    public partial Color PreviewOperationColor { get; set; }

    /// <summary>
    /// 预览操作图标悬浮颜色
    /// Color of hovered preview operation icon
    /// </summary>
    public partial Color PreviewOperationHoverColor { get; set; }

    /// <summary>
    /// 预览操作图标禁用颜色
    /// Disabled color of preview operation icon
    /// </summary>
    public partial Color PreviewOperationColorDisabled { get; set; }

    /// <summary>
    /// 预览切换按钮尺寸
    /// Size of preview switch button
    /// </summary>

    /// <summary>
    /// 加载失败占位图标尺寸（AtomUI 扩展，antd 加载失败时显示 fallback 图片）
    /// Size of the load-failure placeholder icon (an AtomUI extension; antd shows a fallback image)
    /// </summary>
    public partial double ErrorIconSize { get; set; }

    /// <summary>Background color of the mask over the preview cover.</summary>
    public partial Color MaskBgColor { get; set; }

    /// <summary>Minimum width of the image preview dialog.</summary>
    public partial double DialogMinWidth { get; set; }

    /// <summary>Minimum height of the image preview dialog.</summary>
    public partial double DialogMinHeight { get; set; }

    /// <summary>Background color of the preview window's title bar.</summary>
    public partial Color TitleBarBackgroundColor { get; set; }

    /// <summary>
    /// Default background color of the floating preview navigation buttons, paired with
    /// <see cref="PreviewOperationColor"/> .
    /// </summary>
    public partial Color NavButtonBgColor { get; set; }

    /// <summary>
    /// Background color of the floating preview navigation buttons on hover or press, paired with
    /// <see cref="PreviewOperationHoverColor"/> .
    /// </summary>
    public partial Color NavButtonBgHoverColor { get; set; }

    /// <summary>Padding inside the floating toolbar.</summary>
    public partial Thickness FloatToolbarPadding { get; set; }

    /// <summary>Padding around the image index indicator in the floating toolbar.</summary>
    public partial Thickness FloatToolbarIndicatorPadding { get; set; }

    /// <summary>Default width of the preview cover image.</summary>
    public partial double CoverImageWidth { get; set; }

}
