using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ImagePreviewer;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class ImagePreviewerShowCasePageTests
{

    [Fact]
    public void ImagePreviewer_ShowCase_Semantic_Example_Materializes_Without_Access_Violation()
    {
        // Semantic Style 经 x:SetterTargetType 在 internal ImagePreviewRenderer 上解析
        // CornerRadiusProperty；internal 属性字段会让 XAML 编译器生成的 ldsfld 在运行时抛
        // FieldAccessException（跨程序集访问器检查），必须实例化真实页面走一遍 deferred 物化。
        AvaloniaTestApp.EnsureInitialized();
        var page = new ImagePreviewerShowCase();

        var window = new AvaloniaWindow { Width = 900, Height = 800, Content = page };
        window.Show();
        page.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var showcaseItems = page.GetVisualDescendants().OfType<ShowCaseItem>().ToList();
            showcaseItems.ShouldNotBeEmpty();

            foreach (var item in showcaseItems.Where(i => i.IsDeferredContentEnabled))
            {
                item.MaterializeDeferredContent();
            }
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            // 物化本身不抛 FieldAccessException 即通过；再校验语义示例（两个 semantic-styles-* 预览器）的
            // 圆角确实落到渲染器。全页有多个示例的渲染器（默认无圆角），只断言语义示例两个 owner 内的。
            var semanticOwners = page.GetVisualDescendants()
                                     .OfType<AtomUI.Desktop.Controls.ImagePreviewer>()
                                     .Where(p => p.Classes.Contains("semantic-styles-demo") ||
                                                 p.Classes.Contains("semantic-styles-filled-demo"))
                                     .ToList();
            semanticOwners.Count.ShouldBe(2);
            var renderers = semanticOwners
                .SelectMany(o => o.GetVisualDescendants())
                .OfType<Control>()
                .Where(c => c.Classes.Contains("semantic-image"))
                .ToList();
            renderers.ShouldNotBeEmpty();
            renderers.ShouldAllBe(r => r.GetValue(Border.CornerRadiusProperty).Equals(new CornerRadius(4)));
        }
        finally
        {
            window.Close();
        }
    }

    private static string ExtractImagePreviewerExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
    }

    private static int CountShowCaseItemElements(string source)
    {
        return Regex.Matches(source, @"<gallery:ShowCaseItem(\s|>)").Count;
    }

    private static string ComputeSha256(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static int CountOccurrences(string source, string value)
    {
        var count      = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
