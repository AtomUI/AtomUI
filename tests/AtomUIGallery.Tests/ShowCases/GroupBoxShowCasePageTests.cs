using System.Collections;
using System.Reflection;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIGroupBox = AtomUI.Desktop.Controls.GroupBox;
using GroupBoxShowCase = AtomUIGallery.ShowCases.GroupBox.GroupBoxShowCase;
using GroupBoxViewModel = AtomUIGallery.ShowCases.GroupBox.GroupBoxViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class GroupBoxShowCasePageTests
{

    [Fact]
    public void GroupBox_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/GroupBox/Views/GroupBoxShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/GroupBox/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "group-box-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"GroupBoxSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #GroupBoxSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:GroupBox}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(5);
        foreach (var path in new[] { "root", "header", "icon", "title", "content" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"group-box-semantic-part\"");
        semanticSource.ShouldContain("GroupBoxShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("GroupBoxShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("GroupBoxShowCaseLangResource SemanticBorderDescription");
        semanticSource.ShouldContain("Selector=\"atom|GroupBox.semantic-object\"");
        semanticSource.ShouldContain("Selector=\"atom|GroupBox.semantic-function\"");
        semanticSource.ShouldContain("Selector=\"atom|GroupBox.semantic-border\"");
        semanticSource.ShouldContain("Selector=\"atom|GroupBox.semantic-border-plain\"");
        semanticSource.ShouldContain("Value=\"#f0f0f0\"");
        semanticSource.ShouldContain("Value=\"#141414\"");
        semanticSource.ShouldContain("Value=\"#f5efff\"");
        semanticSource.ShouldContain("Value=\"#722ED1\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Property=\"Padding\"");
        semanticSource.ShouldContain("Property=\"Foreground\"");
        // 边框定制走 owner 作用域普通 Setter：分组边框由控件自绘，模板中没有承载节点。
        semanticSource.ShouldContain("Property=\"BorderBrush\"");
        semanticSource.ShouldContain("Property=\"BorderThickness\"");
        semanticSource.ShouldContain("Property=\"CornerRadius\"");
        semanticSource.ShouldContain("Value=\"#13c2c2\"");
        CountOccurrences(semanticSource, "<atom:GroupBoxHeaderStyle").ShouldBe(3);
        CountOccurrences(semanticSource, "<atom:GroupBoxTitleStyle").ShouldBe(3);
        CountOccurrences(semanticSource, "<atom:GroupBoxContentStyle").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:GroupBoxIconStyle").ShouldBe(1);
        semanticSource.ShouldContain("x:SetterTargetType=\"Border\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"TextBlock\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"atom:IconPresenter\"");
        CountOccurrences(semanticSource, "<atom:GroupBox ").ShouldBe(4);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Root element with border, background, corner radius and header position that control the overall appearance of the group");
        english.ShouldContain("Header element with padding, background color and alignment for the header content area");
        english.ShouldContain("Icon element with icon size, margin and icon color for the header icon");
        english.ShouldContain("Title element with layout, foreground color and typography styles for the header text");
        english.ShouldContain("Content element with padding and background color for the grouped content area");
        english.ShouldContain("The group border, background and corner radius belong to the root part; customize them with an owner-scoped style instead of a part style.");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void GroupBox_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new GroupBoxShowCase
        {
            DataContext = new GroupBoxViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUIGroupBox>()
                .ShouldNotContain(static groupBox => groupBox.Name == "GroupBoxSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "GroupBoxSemanticPreview");
            var semanticGroupBox = preview.SemanticOwner.ShouldBeOfType<AtomUIGroupBox>();
            semanticGroupBox.Name.ShouldBe("GroupBoxSemanticOwner");
            foreach (var marker in new[] { "semantic-header", "semantic-icon", "semantic-title", "semantic-content" })
            {
                semanticGroupBox.GetVisualDescendants()
                                .OfType<Control>()
                                .Single(control => control.Classes.Contains(marker))
                                .IsEffectivelyVisible.ShouldBeTrue();
            }

            var previewItemsValue = typeof(SemanticPartPreview)
                                    .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)
                                    ?.GetValue(preview);
            previewItemsValue.ShouldNotBeNull();
            var previewItems = previewItemsValue.ShouldBeAssignableTo<IEnumerable>()
                                                .Cast<object>()
                                                .ToArray();
            previewItems.Length.ShouldBe(5);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(),
                       candidate => Assert.False(candidate.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void GroupBox_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new GroupBoxShowCase
        {
            DataContext = new GroupBoxViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "group-box-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomUIGroupBox>()
                            .Where(static groupBox => groupBox.Classes.Contains("semantic-object") ||
                                                      groupBox.Classes.Contains("semantic-function") ||
                                                      groupBox.Classes.Contains("semantic-border") ||
                                                      groupBox.Classes.Contains("semantic-border-plain"))
                            .ToArray();
            demos.Length.ShouldBe(4);

            var headerStyleDemo = demos.Single(static groupBox => groupBox.Classes.Contains("semantic-object"));
            var header = FindHeader(headerStyleDemo);
            AssertSolidColor(header.Background, "#f0f0f0");
            header.Padding.ShouldBe(new Thickness(16, 2));
            AssertSolidColor(FindTitle(headerStyleDemo).Foreground, "#141414");
            FindContent(headerStyleDemo).Padding.ShouldBe(new Thickness(16, 12));

            var iconStyleDemo = demos.Single(static groupBox => groupBox.Classes.Contains("semantic-function"));
            var iconHeader = FindHeader(iconStyleDemo);
            AssertSolidColor(iconHeader.Background, "#f5efff");
            iconHeader.Padding.ShouldBe(new Thickness(16, 2));
            AssertSolidColor(FindTitle(iconStyleDemo).Foreground, "#141414");
            // 主题为 PART_HeaderIconPresenter 设置了 IconSizeLG；此处必须由示例的
            // GroupBoxIconStyle 覆盖尺寸与图标颜色。
            var icon = FindIcon(iconStyleDemo);
            icon.Width.ShouldBe(20);
            icon.Height.ShouldBe(20);
            AssertSolidColor(icon.IconBrush, "#722ED1");

            // 边框定制走 owner 作用域普通 Setter：分组边框与背景由 GroupBox 自绘，
            // 因此必须同时断言属性值真正进入自绘画笔，而不是只改了属性表面。
            var borderDemo = demos.Single(static groupBox => groupBox.Classes.Contains("semantic-border"));
            AssertSolidColor(borderDemo.BorderBrush, "#722ED1");
            borderDemo.BorderThickness.ShouldBe(new Thickness(2));
            borderDemo.CornerRadius.ShouldBe(new CornerRadius(12));
            AssertSolidColor(borderDemo.Background, "#f9f0ff");
            AssertSolidColor(FindHeader(borderDemo).Background, "#ffffff");
            AssertDrawnColors(borderDemo, "#722ED1", "#f9f0ff");

            var plainBorderDemo = demos.Single(static groupBox => groupBox.Classes.Contains("semantic-border-plain"));
            AssertSolidColor(plainBorderDemo.BorderBrush, "#13c2c2");
            plainBorderDemo.BorderThickness.ShouldBe(new Thickness(2));
            plainBorderDemo.CornerRadius.ShouldBe(new CornerRadius(2));
            AssertSolidColor(FindTitle(plainBorderDemo).Foreground, "#08979c");
            AssertDrawnColors(plainBorderDemo, "#13c2c2");
        });
    }

    /// <summary>
    /// 边框演示的说明文字必须自动换行。Avalonia 的 <c>TextWrapping</c> 默认为 <c>NoWrap</c>，
    /// 而该说明是一整句自然语言；漏写 <c>TextWrapping="Wrap"</c> 会让文本横向溢出卡片。
    ///
    /// 断言只落在 <c>TextWrapping</c> 本身：headless 布局会把 <c>Bounds.Width</c> 与
    /// <c>DesiredSize.Width</c> 都夹到容器可用宽度内，二者在 <c>NoWrap</c> 下**不会**超过容器，
    /// 因此无法充当溢出检测器（已实测：去掉 <c>TextWrapping</c> 后这两个量仍满足“不超宽”）。
    /// 真正可判别的是该属性值——去掉 <c>TextWrapping="Wrap"</c> 时本用例会以
    /// <c>NoWrap</c> 失败，这正是用户看到的溢出缺陷的根因。
    /// </summary>
    [Fact]
    public void Semantic_Border_Caption_Declares_Wrapping_So_It_Does_Not_Overflow_The_Card()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new GroupBoxShowCase
        {
            DataContext = new GroupBoxViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "group-box-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var caption = page.GetVisualDescendants()
                              .OfType<AtomUI.Desktop.Controls.TextBlock>()
                              .Single(static block => block.Name == "SemanticBorderCaption");

            // 只有自然语言说明需要换行；该断言同时防止文案被清空导致用例失去意义。
            caption.Text.ShouldNotBeNullOrWhiteSpace();
            caption.Text!.Length.ShouldBeGreaterThan(40);
            caption.TextWrapping.ShouldBe(TextWrapping.Wrap);
        });
    }

    /// <summary>
    /// <c>GroupBox.Render</c> 把背景与边框都作为填充几何绘制（<c>DrawGeometry</c> 的 pen 为 null）：
    /// 背景用 <c>Background</c> 填充，边框用 <c>BorderBrush</c> 填充 outer-minus-inner 几何。
    /// </summary>
    private static void AssertDrawnColors(AtomUIGroupBox groupBox, params string[] expectedColors)
    {
        var drawingGroup = new DrawingGroup();
        using (var context = drawingGroup.Open())
        {
            groupBox.Render(context);
        }

        var drawnColors = EnumerateGeometryDrawings(drawingGroup)
                          .Select(static drawing => drawing.Brush)
                          .OfType<ISolidColorBrush>()
                          .Select(static brush => brush.Color)
                          .ToArray();

        foreach (var expected in expectedColors)
        {
            drawnColors.ShouldContain(Color.Parse(expected));
        }
    }

    private static IEnumerable<GeometryDrawing> EnumerateGeometryDrawings(Drawing drawing)
    {
        if (drawing is GeometryDrawing geometryDrawing)
        {
            yield return geometryDrawing;
        }
        else if (drawing is DrawingGroup drawingGroup)
        {
            foreach (var child in drawingGroup.Children.SelectMany(EnumerateGeometryDrawings))
            {
                yield return child;
            }
        }
    }

    private static Border FindHeader(AtomUIGroupBox owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<Border>()
                    .Single(static header => header.Classes.Contains("semantic-header"));
    }

    private static AtomUI.Desktop.Controls.TextBlock FindTitle(AtomUIGroupBox owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<AtomUI.Desktop.Controls.TextBlock>()
                    .Single(static title => title.Classes.Contains("semantic-title"));
    }

    private static ContentPresenter FindContent(AtomUIGroupBox owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<ContentPresenter>()
                    .Single(static content => content.Classes.Contains("semantic-content"));
    }

    private static AtomUI.Controls.IconPresenter FindIcon(AtomUIGroupBox owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<AtomUI.Controls.IconPresenter>()
                    .Single(static icon => icon.Classes.Contains("semantic-icon"));
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
    }

    private static string ExtractShowCaseItem(string source, string sourceKey)
    {
        var sourceKeyIndex = source.IndexOf($"SourceKey=\"{sourceKey}\"", StringComparison.Ordinal);
        sourceKeyIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", sourceKeyIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string itemEndMarker = "</gallery:ShowCaseItem>";
        var itemEnd = source.IndexOf(itemEndMarker, sourceKeyIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(sourceKeyIndex);

        return source[itemStart..(itemEnd + itemEndMarker.Length)];
    }

    private static string ExtractGroupBoxExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        // 语义结构示例（SourceKey="group-box-semantic-part"）不属于 Examples 快照，
        // 与 Expander 页面一致：快照只覆盖示例面板中的控件演示内容。
        const string semanticItemMarker = "SourceKey=\"group-box-semantic-part\"";
        var semanticItemStart = source.IndexOf(semanticItemMarker, firstItemStart, StringComparison.Ordinal);
        semanticItemStart.ShouldBeGreaterThan(firstItemStart);
        var semanticItemStartTag = source.LastIndexOf(firstItemMarker, semanticItemStart, StringComparison.Ordinal);
        semanticItemStartTag.ShouldBeGreaterThan(firstItemStart);

        source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal)
              .ShouldBeGreaterThan(semanticItemStart);

        return source[firstItemStart..semanticItemStartTag];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
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

    private static void ShowInWindow(Control content, double width, double height, Action assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child = content
        };
        var window = new AvaloniaWindow
        {
            Content = visualLayerManager,
            Width = width,
            Height = height
        };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
