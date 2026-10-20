using Avalonia;
using Avalonia.Media;
using AtomUI.Controls;

namespace AtomUIGallery.ShowCases.Window;

// Window showcase 各演示窗口共用的 AtomUI Logo（path 数据来自官方 atom-ui.svg）
// 走 AtomUI Icon 渲染管线（与 AntDesign 图标一致），裸 PathIcon 不做 ViewBox 缩放不可用
internal class AtomUiLogoIcon : AtomUI.Controls.Icon
{
    public AtomUiLogoIcon()
    {
        ViewBox  = new Rect(0, 0, 1024, 1024);
        Width    = 16;
        Height   = 16;
        // Icon 渲染取色只认 FillBrush/StrokeBrush 属性（Foreground 不参与），Fill 模式必须设 FillBrush
        FillBrush = new SolidColorBrush(Color.Parse("#CF1322"));
    }

    public override AtomUI.Controls.Icon CreateInstance()
    {
        return new AtomUiLogoIcon();
    }

    private static readonly DrawingInstruction[] StaticInstructions =
    [
        new PathDrawingInstruction()
        {
            Data = StreamGeometry.Parse(
                "M224 800c0 9.6 3.2 44.8 6.4 54.4 6.4 48-48 76.8-48 76.8s80 41.6 147.2 0S464 796.8 368 736c-22.4-12.8-41.6-19.2-57.6-19.2-51.2 0-83.2 44.8-86.4 83.2z m336-124.8l-32 51.2c-51.2 51.2-83.2 32-83.2 32 25.6 67.2 0 112-12.8 128 25.6 6.4 51.2 9.6 80 9.6 54.4 0 102.4-9.6 150.4-32 3.2 0 3.2-3.2 3.2-3.2 22.4-16 12.8-35.2 6.4-44.8-9.6-12.8-12.8-25.6-12.8-41.6 0-54.4 60.8-99.2 137.6-99.2h22.4c12.8 0 38.4 9.6 48-25.6 0-3.2 0-3.2 3.2-6.4 0-3.2 3.2-6.4 3.2-6.4 6.4-16 6.4-16 6.4-19.2 9.6-35.2 16-73.6 16-115.2 0-105.6-41.6-198.4-108.8-268.8C704 396.8 560 675.2 560 675.2z m-336-256c0-28.8 22.4-51.2 51.2-51.2 28.8 0 51.2 22.4 51.2 51.2 0 28.8-22.4 51.2-51.2 51.2-28.8 0-51.2-22.4-51.2-51.2z m96-134.4c0-22.4 19.2-41.6 41.6-41.6 22.4 0 41.6 19.2 41.6 41.6 0 22.4-19.2 41.6-41.6 41.6-22.4 0-41.6-19.2-41.6-41.6zM457.6 208c0-12.8 12.8-25.6 25.6-25.6s25.6 12.8 25.6 25.6-12.8 25.6-25.6 25.6-25.6-12.8-25.6-25.6zM128 505.6C128 592 153.6 672 201.6 736c28.8-60.8 112-60.8 124.8-60.8-16-51.2 16-99.2 16-99.2l316.8-422.4c-48-19.2-99.2-32-150.4-32-211.2-3.2-380.8 169.6-380.8 384zM764.8 86.4c-22.4 19.2-390.4 518.4-390.4 518.4-22.4 28.8-12.8 76.8 22.4 99.2l9.6 6.4c35.2 22.4 80 12.8 99.2-25.6l9.6-19.2C569.6 560 790.4 140.8 803.2 112c6.4-19.2-3.2-32-19.2-32-6.4-3.2-12.8 0-19.2 6.4z"),
            FillBrush = IconBrushType.Fill,
        }
    ];

    protected override IList<DrawingInstruction> DrawingInstructions => StaticInstructions;
}
