using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

public class WindowTitleBarCaptionFrameRenderingTests
{
    [Fact]
    public void AddOn_Buttons_Paint_The_Shared_Inset_Circular_Frame()
    {
        var button = new WindowTitleBarButton
        {
            Icon = new CameraOutlined()
        };
        var toggleButton = new WindowTitleBarToggleButton
        {
            CheckedIcon = new SearchOutlined(),
            UnCheckedIcon = new SettingOutlined()
        };
        var titleBar = new WindowTitleBar
        {
            RightAddOn = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { button, toggleButton }
            },
            IsWindowActive = true,
            Title = "Title"
        };
        // Linux managed caption band 默认显示 minimize/maximize/close，且背景带 2 逻辑像素 inset。
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Linux);

        var window = new AvaloniaWindow
        {
            Width = 400,
            Height = 100,
            Background = Brushes.White,
            Content = titleBar
        };

        window.Show();
        try
        {
            window.SetRenderScaling(1);
            titleBar.ApplyTemplate();
            button.ApplyTemplate();
            toggleButton.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            using var renderedFrame = window.CaptureRenderedFrame().ShouldNotBeNull();
            using var stream = new MemoryStream();
            renderedFrame.Save(stream, PngBitmapEncoderOptions.Default);
            stream.Position = 0;
            using var bitmap = SKBitmap.Decode(stream);

            var addOnOrigin = button.TranslatePoint(default, window).ShouldNotBeNull();
            var toggleOrigin = toggleButton.TranslatePoint(default, window).ShouldNotBeNull();
            var background = bitmap.GetPixel(0, 0);

            foreach (var (origin, width, height) in new[]
                     {
                         (addOnOrigin, button.Bounds.Width, button.Bounds.Height),
                         (toggleOrigin, toggleButton.Bounds.Width, toggleButton.Bounds.Height)
                     })
            {
                var centerX = (int)Math.Round(origin.X + width / 2);
                var centerY = (int)Math.Round(origin.Y + height / 2);
                var boxTopX = (int)Math.Round(origin.X + width / 2);
                var boxTopY = (int)Math.Round(origin.Y);

                // 命中盒中心与顶部 1 逻辑像素处：前者落在正圆背景内，后者在圆外。
                bitmap.GetPixel(centerX, centerY).ShouldNotBe(background);
                bitmap.GetPixel(boxTopX, boxTopY).ShouldBe(background);
                // 命中盒左上角在圆外，证明背景相对 30 逻辑像素命中盒是内缩的正圆。
                bitmap.GetPixel((int)Math.Round(origin.X), (int)Math.Round(origin.Y)).ShouldBe(background);
            }

            var addOnCenter = new SKPointI(
                (int)Math.Round(addOnOrigin.X + button.Bounds.Width / 2),
                (int)Math.Round(addOnOrigin.Y + button.Bounds.Height / 2));
            var toggleCenter = new SKPointI(
                (int)Math.Round(toggleOrigin.X + toggleButton.Bounds.Width / 2),
                (int)Math.Round(toggleOrigin.Y + toggleButton.Bounds.Height / 2));
            bitmap.GetPixel(addOnCenter.X, addOnCenter.Y)
                  .ShouldBe(bitmap.GetPixel(toggleCenter.X, toggleCenter.Y));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
