using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.InfoFlyout;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomFlyoutPresenter = AtomUI.Desktop.Controls.FlyoutPresenter;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class InfoFlyoutShowCasePageTests
{

    [Fact]
    public void InfoFlyout_ShowCase_Declares_Antd_Aligned_Semantic_Preview()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataDisplay/InfoFlyout/Views/InfoFlyoutShowCase.axaml");

        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:FlyoutHost}\"");
        source.ShouldContain("Name=\"InfoFlyoutSemanticOwner\"");
        source.ShouldContain("IsPopupPinnedOpen=\"True\"");
        source.ShouldContain("IsArrowVisible=\"True\"");
        // antd Popover 语义槽位对齐：root / container / content / arrow（无 title 节点故省略）。
        source.ShouldContain("Path=\"root\"");
        source.ShouldContain("Path=\"popup.root\"");
        source.ShouldContain("Path=\"popup.container\"");
        source.ShouldContain("Path=\"popup.content\"");
        source.ShouldContain("Path=\"popup.arrow\"");
        source.ShouldNotContain("Path=\"title\"");
        source.ShouldContain("Loaded=\"HandleSemanticPreviewLoaded\"");
        source.ShouldContain("Unloaded=\"HandleSemanticPreviewUnloaded\"");
    }

    [Fact]
    public void InfoFlyout_ShowCase_StyleClass_Example_Is_Deferred_Scoped_And_Versioned()
    {
        var source       = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataDisplay/InfoFlyout/Views/InfoFlyoutShowCase.axaml");
        var localization = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataDisplay/InfoFlyout/Localization/en-US.xlf");

        source.ShouldContain("SourceKey=\"infoflyout-semantic-part\"");
        source.ShouldContain("InfoFlyoutShowCaseLangResource StyleClassTitle");
        source.ShouldContain("InfoFlyoutShowCaseLangResource StyleClassDescription");
        source.ShouldContain("Classes=\"semantic-styles-object-demo\"");
        source.ShouldContain("Classes=\"semantic-styles-function-demo\"");
        source.ShouldContain("Selector=\"atom|FlyoutHost.semantic-styles-object-demo\"");
        source.ShouldContain("Selector=\"atom|FlyoutHost.semantic-styles-function-demo\"");
        source.ShouldContain("<atom:FlyoutHostPopupRootStyle x:SetterTargetType=\"atom:FlyoutPresenter\">");
        source.ShouldContain("InfoFlyoutShowCaseLangResource SemanticStyleObjectContent");
        source.ShouldContain("InfoFlyoutShowCaseLangResource SemanticStyleObjectTrigger");
        source.ShouldContain("InfoFlyoutShowCaseLangResource SemanticStyleFunctionContent");
        source.ShouldContain("InfoFlyoutShowCaseLangResource SemanticStyleFunctionTrigger");
        source.ShouldNotContain("Loaded=\"HandleSemanticStyleDemoLoaded\"");
        source.ShouldNotContain("Unloaded=\"HandleSemanticStyleDemoUnloaded\"");
        CountOccurrences(source, "IsArrowVisible=\"False\"").ShouldBe(2);
        localization.ShouldContain("<source>Customize the semantic structure style of the InfoFlyout popup with semantic part styles.</source>");
        localization.ShouldContain("<source>The InfoFlyout host itself.</source>");
        localization.ShouldContain("<source>Root container of the popup layer.</source>");
        localization.ShouldContain("<source>Inner container that carries the popup background, padding and border.</source>");
        localization.ShouldContain("<source>Content region of the popup.</source>");
        localization.ShouldContain("<source>Arrow indicator pointing to the anchor.</source>");
        localization.ShouldContain("<source>Object text</source>");
        localization.ShouldContain("<source>Object Style</source>");
        localization.ShouldContain("<source>Function text</source>");
        localization.ShouldContain("<source>Function Style</source>");
    }

    [Fact]
    public void InfoFlyout_Semantic_Preview_Registers_The_CodeCreated_Popup_Root()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new InfoFlyoutShowCase
        {
            DataContext = new InfoFlyoutViewModel(new TestScreen())
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
                              .Single(candidate => candidate.Name == "InfoFlyoutSemanticPreview");
            var owner = preview.PreviewContent.ShouldBeOfType<FlyoutHost>();
            owner.Name.ShouldBe("InfoFlyoutSemanticOwner");
            owner.Flyout.ShouldNotBeNull().IsOpen.ShouldBeTrue();

            Dispatcher.UIThread.RunJobs();

            preview.AdditionalRoots.Count.ShouldBe(
                1,
                "the code-created cross-visual-root popup root must be registered so popup.container/content/arrow resolve");
            preview.AdditionalRoots[0].ShouldBeOfType<AtomFlyoutPresenter>();
        });
    }

    private static string ExtractInfoFlyoutExampleItems(string source)
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
