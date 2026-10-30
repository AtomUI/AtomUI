using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using ComboBoxShowCase = AtomUIGallery.ShowCases.ComboBox.ComboBoxShowCase;
using ComboBoxViewModel = AtomUIGallery.ShowCases.ComboBox.ComboBoxViewModel;
using ComboBoxItemData = AtomUIGallery.ShowCases.ComboBox.ComboBoxItemData;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class ComboBoxSemanticPartHighlightTests
{
    /// <summary>
    /// 每个已声明 Part 的 SelectorClass 必须在预览中真实存在至少一个目标节点。
    /// 断言按 marker 在场性判定，而非依赖卡片悬停：预览按 AtomUI 标准方式钉住弹层
    /// （`IsPopupPinnedOpen` + `IsDropDownOpen`），钉住的模板 Popup 会在页面上方放置
    /// light-dismiss 遮罩层，输入框以外的区域指针被遮罩接管，卡片悬停高亮无法在该状态下触发。
    /// 触发侧部件的悬停高亮由「弹层关闭」状态的控件级测试覆盖。
    /// </summary>
    private static readonly (string Part, string Marker)[] DeclaredParts =
    [
        ("root",           ""),
        ("prefix",         "semantic-prefix"),
        ("frame",          "semantic-frame"),
        ("content",        "semantic-content"),
        ("placeholder",    "semantic-placeholder"),
        ("input",          "semantic-input"),
        ("suffix",         "semantic-suffix"),
        ("indicator",      "semantic-indicator"),
        ("popup.root",     "semantic-popup-root"),
        ("popup.list",     "semantic-popup-list"),
        ("popup.listItem", "semantic-popup-list-item"),
        ("popup.empty",    "semantic-popup-empty")
    ];

    [Fact]
    public void ComboBox_Semantic_Preview_Pins_The_Popup_And_Exposes_Every_Part_Target()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ComboBoxShowCase
        {
            DataContext = new ComboBoxViewModel(new ComboBoxTestScreen())
        };

        var window = new AtomUIWindow
        {
            Width = 1280,
            Height = 900,
            Content = page
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "ComboBoxSemanticPreview");

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();

            // 12 个已声明部件（root + 11 个非 root）都必须有预览卡片。
            cards.Length.ShouldBe(12);

            var owner = preview.GetVisualDescendants()
                               .OfType<AtomUI.Desktop.Controls.ComboBox>()
                               .Single(static candidate => candidate.Name == "ComboBoxSemanticOwner");

            // 按 AtomUI 标准方式钉住弹层：IsPopupPinnedOpen 为 public。
            owner.IsPopupPinnedOpen.ShouldBeTrue();

            // headless 宿主不产生窗口激活事件，控件按契约在失活时关闭弹层；重新打开后 pin 会保持其开启。
            if (!owner.IsDropDownOpen)
            {
                owner.IsDropDownOpen = true;
            }

            Dispatcher.UIThread.RunJobs();
            var popup = owner.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeFalse();

            // 每个部件的 marker 必须在树中至少存在一个目标节点。
            var markedNodes = window.GetVisualDescendants()
                                    .OfType<Control>()
                                    .SelectMany(static control => control.Classes
                                        .Where(static name => name.StartsWith("semantic-", StringComparison.Ordinal))
                                        .Select(name => (Node: control, Marker: name)))
                                    .ToArray();

            foreach (var (part, marker) in DeclaredParts)
            {
                if (string.IsNullOrEmpty(marker))
                {
                    // root 就是 owner 本身，不携带 marker class。
                    part.ShouldBe("root");
                    continue;
                }

                markedNodes.Count(entry => entry.Marker == marker).ShouldBeGreaterThan(
                    0,
                    $"Semantic Part '{part}' must have a target node carrying '.{marker}' in the preview.");
            }

            // 弹层部件必须位于弹层宿主中（而不是内联在触发侧树内）。
            // 弹层内容经 OverlayPopupHost 承载，因此不与 Popup 控件保持视觉祖先关系。
            var popupRoot = markedNodes.Single(entry => entry.Marker == "semantic-popup-root").Node;
            owner.GetVisualDescendants().Contains(popupRoot).ShouldBeFalse(
                "The popup.* parts must resolve inside the popup host layer, not inline in the trigger tree.");

            // 离开 Semantic Parts 页签后不残留高亮装饰。
            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            window.GetVisualDescendants()
                  .Count(static visual => visual.GetType().Name == "SemanticPartAdorner")
                  .ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    /// <summary>
    /// 钉住弹层不得让 light-dismiss 遮罩接管页面输入，否则预览卡片无法悬停、语义高亮整体失效。
    /// 该遮罩曾使本测试退化为"只按 marker 在场性断言"；修复后恢复真实悬停断言，覆盖触发侧与弹层侧两类部件。
    /// </summary>
    [Fact]
    public void ComboBox_Semantic_Preview_Cards_Highlight_On_Hover_While_Pinned()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ComboBoxShowCase
        {
            DataContext = new ComboBoxViewModel(new ComboBoxTestScreen())
        };

        var window = new AtomUIWindow
        {
            Width = 1280,
            Height = 900,
            Content = page
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "ComboBoxSemanticPreview");

            var owner = preview.GetVisualDescendants()
                               .OfType<AtomUI.Desktop.Controls.ComboBox>()
                               .Single(static candidate => candidate.Name == "ComboBoxSemanticOwner");
            if (!owner.IsDropDownOpen)
            {
                owner.IsDropDownOpen = true;
                Dispatcher.UIThread.RunJobs();
            }

            // 钉住状态下遮罩必须已抑制，页面远端点位不得被遮罩吞掉。
            window.GetVisualDescendants()
                  .Where(static layer => layer.GetType().Name == "LightDismissOverlayLayer")
                  .ShouldNotContain(static layer => layer.IsVisible,
                      "钉住弹层时不得留下可见的 light-dismiss 遮罩，否则预览卡片无法悬停。");

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();

            // 触发侧部件：root 即 owner 本身。
            HoverCard(cards, window, "root");
            GetHighlightCount(window).ShouldBeGreaterThan(0, "悬停 root 卡片必须产生高亮。");

            // 触发侧单节点部件。
            HoverCard(cards, window, "frame");
            GetHighlightCount(window).ShouldBe(1);

            HoverCard(cards, window, "indicator");
            GetHighlightCount(window).ShouldBe(1);

            // 弹层侧部件：跨根解析仍须在钉住状态下可高亮。
            HoverCard(cards, window, "popup.root");
            GetHighlightCount(window).ShouldBe(1);

            HoverCard(cards, window, "popup.list");
            GetHighlightCount(window).ShouldBe(1);

            // popup.listItem 为 Multiple：预览提供 3 个候选项。
            HoverCard(cards, window, "popup.listItem");
            GetHighlightCount(window).ShouldBe(3);

            // 离开 Semantic Parts 页签释放高亮会话。
            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            GetHighlightCount(window).ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static int GetHighlightCount(AtomUIWindow window)
    {
        return window.GetVisualDescendants()
                     .Count(static visual => visual.GetType().Name == "SemanticPartAdorner");
    }

    private static void HoverCard(
        UserControl[] cards,
        AtomUIWindow window,
        string path)
    {
        var card = cards.Single(candidate =>
            (string?)candidate.DataContext?.GetType().GetProperty("Path")!.GetValue(candidate.DataContext) == path);
        card.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        var center = card.TransformToVisual(window)!.Value
                         .Transform(new Point(card.Bounds.Width / 2, card.Bounds.Height / 2));
        window.MouseMove(new Point(center.X, center.Y));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>
    /// 预览为让 <c>input</c> 部件可见而开启 <c>IsEditable</c>，此时输入框显示的是
    /// <c>ComboBox.Text</c>；该值由 Avalonia 经 <c>TextSearch.GetEffectiveText</c> 从候选项推导，
    /// 末级回落到 <c>object.ToString()</c>。预览数据模型必须提供可读文本，否则输入框会显示
    /// 类型全名（例如 <c>AtomUIGallery.ShowCases.ComboBox.ComboBoxItemData</c>）而不是候选项文本。
    /// </summary>
    [Fact]
    public void ComboBox_Semantic_Preview_Editable_Input_Shows_The_Item_Text()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ComboBoxShowCase
        {
            DataContext = new ComboBoxViewModel(new ComboBoxTestScreen())
        };

        var window = new AtomUIWindow
        {
            Width = 1280,
            Height = 900,
            Content = page
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "ComboBoxSemanticPreview");

            var owner = preview.GetVisualDescendants()
                               .OfType<AtomUI.Desktop.Controls.ComboBox>()
                               .Single(static candidate => candidate.Name == "ComboBoxSemanticOwner");

            if (!owner.IsDropDownOpen)
            {
                owner.IsDropDownOpen = true;
                Dispatcher.UIThread.RunJobs();
            }

            // 与真机走查一致：提交预览列表中的候选项（Gamma）。
            owner.SelectedIndex = 2;
            Dispatcher.UIThread.RunJobs();

            var editableTextBox = owner.GetVisualDescendants()
                                       .OfType<Avalonia.Controls.TextBox>()
                                       .Single(static textBox => textBox.Name == "PART_EditableTextBox");

            editableTextBox.IsVisible.ShouldBeTrue();
            owner.Text.ShouldBe("Gamma");
            editableTextBox.Text.ShouldBe("Gamma");
            editableTextBox.Text.ShouldNotBe(
                typeof(ComboBoxItemData).FullName,
                "可编辑输入框必须显示候选项文本，而不是数据模型的类型全名。");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}

internal sealed class ComboBoxTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
