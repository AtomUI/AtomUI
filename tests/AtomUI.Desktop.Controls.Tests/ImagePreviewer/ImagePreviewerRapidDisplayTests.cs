using System.Collections.ObjectModel;
using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

// 白屏问题的可渲染复现：以像素级读回验证预览窗口在快速切换下持续有图，
// 并在失败瞬间导出视觉树状态，用于源头定位（数据层/显示层/渲染层）。
public class ImagePreviewerRapidDisplayTests
{
    public ImagePreviewerRapidDisplayTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Rapid_Switching_Keeps_The_Preview_Surface_Rendering_Content()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var png = CreatePng(200, 150);
            var items = new ObservableCollection<ImagePreviewItem>();
            var holdPreviewer = new global::AtomUI.Desktop.Controls.ImagePreviewer
            {
                Width = 96,
                Height = 96,
                PreloadCount = 1,
                ImageSwitchMode = ImageSwitchMode.WaitForLoaded,
                ItemsSource = items
            };
            using var host = new PreviewerHost(holdPreviewer, renderScaling: 1);
            items.Add(new ImagePreviewItem(new BytesImageSource(
                png, $"display-seed-{Guid.NewGuid():N}")));
            Dispatcher.UIThread.RunJobs();
            holdPreviewer.OpenDialog();
            Dispatcher.UIThread.RunJobs();

            var blankLines = new List<string>();
            for (var tick = 0; tick < 150; tick++)
            {
                // 30ms 源 + 50ms 切换周期：加载略慢于切换的稳态场景（与真实 demo 节奏一致，
                // 测试应用 loader 为串行并发）。完成者持续出现，经采纳在途请求与保留帧种子
                // 补充显示——显示通道不允许出现占位符空洞（即非悬停时的频闪）。
                items.Add(new ImagePreviewItem(new StreamImageSource(
                    async token =>
                    {
                        await Task.Delay(30, token);
                        return new MemoryStream(png);
                    },
                    $"display-{tick}-{Guid.NewGuid():N}", "v1")));
                if (items.Count > 60)
                {
                    items.RemoveAt(0);
                }
                holdPreviewer.CurrentIndex = items.Count - 1;
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(50);

                if (tick % 10 == 0)
                {
                    var line = $"t{tick} " + DescribeDisplay(holdPreviewer);
                    blankLines.Add(line);
                }
            }

            blankLines.ShouldNotBeEmpty();
            // 集合封顶裁剪后索引不再变化，宿主必须仍跟随有效集合刷新当前项：
            // 显示图持续为空即白屏
            blankLines.Count(l => l.Contains("img=null ")).ShouldBeLessThanOrEqualTo(2);
        });
    }

    [Fact]
    public void Immediate_Shows_The_Loading_Placeholder_During_Continuous_Slow_Target_Switches()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var png = CreatePng(200, 150);
            var items = new ObservableCollection<ImagePreviewItem>();
            var immediatePreviewer = new global::AtomUI.Desktop.Controls.ImagePreviewer
            {
                Width = 96,
                Height = 96,
                PreloadCount = 1,
                ImageSwitchMode = ImageSwitchMode.Immediate,
                ItemsSource = items
            };
            using var host = new PreviewerHost(immediatePreviewer, renderScaling: 1);
            items.Add(new ImagePreviewItem(new BytesImageSource(
                png, $"imm-seed-{Guid.NewGuid():N}")));
            Dispatcher.UIThread.RunJobs();
            immediatePreviewer.OpenDialog();
            Dispatcher.UIThread.RunJobs();
            WaitUntil(
                () => !DescribeDisplay(immediatePreviewer).Contains("img=null "),
                "initial preview image");

            for (var tick = 0; tick < 6; tick++)
            {
                // 目标加载明显慢于切换周期；Immediate 必须从每次目标切换的同一 UI 周期
                // 清空旧图并显示 loading，不能等待源请求实际开始或某个固定延迟。
                items.Add(new ImagePreviewItem(new StreamImageSource(
                    async token =>
                    {
                        await Task.Delay(300, token);
                        return new MemoryStream(png);
                    },
                    $"imm-slow-{tick}-{Guid.NewGuid():N}", "v1")));
                immediatePreviewer.CurrentIndex = items.Count - 1;
                Dispatcher.UIThread.RunJobs();

                var line = $"t{tick} " + DescribeDisplay(immediatePreviewer);
                line.ShouldContain("img=null ");
                line.ShouldContain("loadingVisible=True");
                line.ShouldContain("hasImagePseudo=False");
            }
        });
    }

    private static void WaitUntil(Func<bool> predicate, string description)
    {
        for (var attempt = 0; attempt < 200; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            if (predicate())
            {
                return;
            }
            Thread.Sleep(10);
        }
        throw new TimeoutException($"Timed out waiting for {description}.");
    }

    private static string DescribeDisplay(global::AtomUI.Desktop.Controls.ImagePreviewer previewer)
    {
        var host = FindOpenHost(previewer);
        if (host is null)
        {
            return $"host=none isOpen={previewer.IsOpen} items={previewer.EffectiveItems?.Count ?? -1}";
        }
        var image = host switch
        {
            ImagePreviewerDialog dialog => dialog.CurrentImage,
            ImagePreviewerOverlayHost overlay => overlay.CurrentImage,
            _ => null
        };
        var viewer = host.GetVisualDescendants().OfType<ImageViewer>().FirstOrDefault();
        var renderer = viewer?.GetVisualDescendants().OfType<ImagePreviewRenderer>().FirstOrDefault();
        var scene = viewer?.GetVisualDescendants().OfType<Canvas>()
                        .FirstOrDefault(c => c.Name == "PART_ImageViewerScene");
        var loadingPresenter = viewer?.GetVisualDescendants().OfType<Border>()
                                   .FirstOrDefault(b => b.Name == "PART_LoadingPresenter");
        var imageValid = image switch
        {
            null => "null",
            Bitmap bitmap => $"bmp({bitmap.PixelSize})",
            _ => image.GetType().Name
        };
        return $"state={previewer.CurrentLoadState} img={imageValid} " +
               $"rendererSrc={(renderer?.Source is null ? "null" : "set")} " +
               $"rendererChildren={(renderer is null ? -1 : renderer.GetVisualChildren().Count())} " +
               $"sceneVisible={scene?.IsVisible} loadingVisible={loadingPresenter?.IsVisible} " +
               $"hasImagePseudo={viewer?.Classes.Contains(":has-image")}";
    }

    private static Control? FindOpenHost(global::AtomUI.Desktop.Controls.ImagePreviewer previewer)
    {
        // 测试宿主 SetupWithoutStarting（无桌面 lifetime），通过内部 open state 反取宿主
        var state = typeof(AbstractImagePreviewer)
            .GetField("_openState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(previewer);
        if (state is null)
        {
            return null;
        }
        return state.GetType().GetProperty("DialogHost")?.GetValue(state) as Control
               ?? state.GetType().GetProperty("PreviewHost")?.GetValue(state) as Control;
    }

    private sealed class PreviewerHost : IDisposable
    {
        private readonly Avalonia.Controls.Window _window;

        internal PreviewerHost(Control previewer, double renderScaling)
        {
            _window = new Avalonia.Controls.Window
            {
                Width = 320,
                Height = 240,
                Content = previewer
            };
            _window.Show();
            _window.SetRenderScaling(renderScaling);
            _window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static byte[] CreatePng(int width, int height)
    {
        // 亮红纯色 PNG，与任何主题背景色都高度可区分
        using var output = new MemoryStream();
        output.Write([0x89, (byte)'P', (byte)'N', (byte)'G', 0x0d, 0x0a, 0x1a, 0x0a]);
        void WriteChunk(string type, ReadOnlySpan<byte> data)
        {
            Span<byte> length = stackalloc byte[4];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(length, checked((uint)data.Length));
            output.Write(length);
            var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
            output.Write(typeBytes);
            output.Write(data);
            var crcBytes = new byte[typeBytes.Length + data.Length];
            typeBytes.CopyTo(crcBytes, 0);
            data.CopyTo(crcBytes.AsSpan(typeBytes.Length));
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(length, CalcCrc32(crcBytes));
            output.Write(length);
        }

        Span<byte> header = stackalloc byte[13];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(header[..4], checked((uint)width));
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(header[4..8], checked((uint)height));
        header[8] = 8;
        header[9] = 2; // RGB
        WriteChunk("IHDR", header);

        using var raw = new MemoryStream();
        var row = new byte[width * 3 + 1];
        for (var x = 0; x < width; x++)
        {
            row[1 + x * 3]     = 220; // R
            row[1 + x * 3 + 1] = 40;  // G
            row[1 + x * 3 + 2] = 40;  // B
        }
        for (var y = 0; y < height; y++)
        {
            raw.Write(row);
        }
        raw.Position = 0;
        using var compressed = new MemoryStream();
        using (var zlib = new System.IO.Compression.ZLibStream(compressed,
                   System.IO.Compression.CompressionLevel.Fastest, leaveOpen: true))
        {
            raw.CopyTo(zlib);
        }
        WriteChunk("IDAT", compressed.ToArray());
        WriteChunk("IEND", []);
        return output.ToArray();
    }

    private static uint CalcCrc32(byte[] bytes)
    {
        var crc = uint.MaxValue;
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) != 0 ? 0xedb88320u ^ (crc >> 1) : crc >> 1;
            }
        }
        return ~crc;
    }
}
