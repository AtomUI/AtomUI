using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.PopupConfirm;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomFlyoutPresenter = AtomUI.Desktop.Controls.FlyoutPresenter;
using AtomPopupConfirm = AtomUI.Desktop.Controls.PopupConfirm;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class PopupConfirmShowCasePageTests
{

    [Fact]
    public void PopupConfirm_ShowCase_Declares_Antd_Aligned_Semantic_Preview()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/PopupConfirm/Views/PopupConfirmShowCase.axaml");

        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:PopupConfirm}\"");
        source.ShouldContain("Name=\"PopupConfirmSemanticOwner\"");
        source.ShouldContain("IsPopupPinnedOpen=\"True\"");
        source.ShouldContain("IsArrowVisible=\"True\"");
        // antd Popconfirm 语义槽位对齐：root / container / icon / title / content / arrow；
        // 上游 content（描述）在 AtomUI 映射为 popup.description，按钮区以 popup.actions 发布。
        source.ShouldContain("Path=\"root\"");
        source.ShouldContain("Path=\"popup.root\"");
        source.ShouldContain("Path=\"popup.container\"");
        source.ShouldContain("Path=\"popup.content\"");
        source.ShouldContain("Path=\"popup.arrow\"");
        source.ShouldContain("Path=\"popup.icon\"");
        source.ShouldContain("Path=\"popup.title\"");
        source.ShouldContain("Path=\"popup.description\"");
        source.ShouldContain("Path=\"popup.actions\"");
        source.ShouldContain("Loaded=\"HandleSemanticPreviewLoaded\"");
        source.ShouldContain("Unloaded=\"HandleSemanticPreviewUnloaded\"");
    }

    [Fact]
    public void PopupConfirm_ShowCase_StyleClass_Example_Is_Deferred_Scoped_And_Versioned()
    {
        var source       = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/PopupConfirm/Views/PopupConfirmShowCase.axaml");
        var localization = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/PopupConfirm/Localization/en-US.xlf");

        source.ShouldContain("SourceKey=\"popupconfirm-semantic-part\"");
        source.ShouldContain("PopupConfirmShowCaseLangResource StyleClassTitle");
        source.ShouldContain("PopupConfirmShowCaseLangResource StyleClassDescription");
        source.ShouldContain("Classes=\"semantic-styles-object-demo\"");
        source.ShouldContain("Classes=\"semantic-styles-function-demo\"");
        source.ShouldContain("Selector=\"atom|PopupConfirm.semantic-styles-object-demo\"");
        source.ShouldContain("Selector=\"atom|PopupConfirm.semantic-styles-function-demo\"");
        source.ShouldContain("<atom:PopupConfirmPopupContainerStyle x:SetterTargetType=\"Border\">");
        source.ShouldContain("<atom:PopupConfirmPopupRootStyle x:SetterTargetType=\"atom:FlyoutPresenter\">");
        source.ShouldContain("<atom:PopupConfirmPopupTitleStyle x:SetterTargetType=\"TextBlock\">");
        source.ShouldContain("<atom:PopupConfirmPopupActionsStyle x:SetterTargetType=\"StackPanel\">");
        // 上游 PopupConfirm 在 title/content 上显式设置前景色；默认 ControlTheme 在 PART_Title
        // 静态设置 ColorTextHeading，会压过 popup.root 的 Foreground 继承，因此标题颜色必须由
        // PopupConfirmPopupTitleStyle 显式给出，否则 Function 分支标题仍是深色。
        CountOccurrences(source, "<atom:PopupConfirmPopupTitleStyle x:SetterTargetType=\"TextBlock\">").ShouldBe(2);
        CountOccurrences(source, "<Setter Property=\"Foreground\" Value=\"#262626\" />").ShouldBe(1);
        // Function 分支的标题与弹层根都要显式设为 White：根负责内容区继承，标题因主题静态
        // 前景色需要单独覆盖。
        CountOccurrences(source, "<Setter Property=\"Foreground\" Value=\"White\" />").ShouldBe(2);
        source.ShouldContain("PopupConfirmShowCaseLangResource SemanticStyleObjectTrigger");
        source.ShouldContain("PopupConfirmShowCaseLangResource SemanticStyleFunctionTrigger");
        source.ShouldNotContain("Loaded=\"HandleSemanticStyleDemoLoaded\"");
        source.ShouldNotContain("Unloaded=\"HandleSemanticStyleDemoUnloaded\"");
        CountOccurrences(source, "IsArrowVisible=\"False\"").ShouldBe(2);
        localization.ShouldContain("<source>The PopupConfirm host itself.</source>");
        localization.ShouldContain("<source>Root surface of the confirmation popup.</source>");
        localization.ShouldContain("<source>Inner container carrying the popup background, border and padding.</source>");
        localization.ShouldContain("<source>Content surface of the popup frame that hosts the confirmation body.</source>");
        localization.ShouldContain("<source>Arrow indicator pointing to the anchor.</source>");
        localization.ShouldContain("<source>Confirmation status icon, colored by ConfirmStatus.</source>");
        localization.ShouldContain("<source>Confirmation title.</source>");
        localization.ShouldContain("<source>Confirmation description text.</source>");
        localization.ShouldContain("<source>Confirm and cancel action row.</source>");
        localization.ShouldContain("<source>Object text</source>");
        localization.ShouldContain("<source>Object Style</source>");
        localization.ShouldContain("<source>Function text</source>");
        localization.ShouldContain("<source>Function Style</source>");
    }

    [Fact]
    public void PopupConfirm_Semantic_Preview_Registers_The_CodeCreated_Popup_Root()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new PopupConfirmShowCase
        {
            DataContext = new PopupConfirmViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            page.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(candidate => candidate.Name == "PopupConfirmSemanticPreview");
            var owner = preview.PreviewContent.ShouldBeOfType<AtomPopupConfirm>();
            owner.Name.ShouldBe("PopupConfirmSemanticOwner");
            owner.Flyout.ShouldNotBeNull().IsOpen.ShouldBeTrue();

            Dispatcher.UIThread.RunJobs();

            preview.AdditionalRoots.Count.ShouldBe(
                1,
                "the code-created cross-visual-root popup root must be registered so popup.frame and confirmation parts resolve");
            preview.AdditionalRoots[0].ShouldBeOfType<AtomFlyoutPresenter>();
        });
    }

    private static string ExtractPopupConfirmExampleItems(string source)
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

    private static int CountShowCaseItemElements(string source)
    {
        return Regex.Matches(source, @"<gallery:ShowCaseItem(\s|>)", RegexOptions.CultureInvariant).Count;
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
            EnableOverlayLayer = true,
            Child              = content
        };
        EnablePopupOverlayLayer(visualLayerManager);
        var window = new AtomUIWindow
        {
            Content = visualLayerManager,
            Width   = width,
            Height  = height
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

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
