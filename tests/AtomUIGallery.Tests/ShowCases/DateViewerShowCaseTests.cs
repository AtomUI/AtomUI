// The standalone DateViewer Gallery page is a public consumption and Semantic Style contract.
using System.Xml.Linq;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUI.Toolkits.GalleryBase.SourceCode;
using AtomUIGallery.SourceCode;
using AtomUIGallery.ShowCases.DateViewer;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class DateViewerShowCaseTests
{
    [Fact]
    public void Generated_Snippets_Describe_The_Public_Embedded_Examples()
    {
        var provider = new AtomUIGalleryShowCaseCodeSnippetProvider();
        var keys = new[] { "basic", "units", "browsing", "content", "cell", "fullcell", "header", "semantic" };
        for (var index = 0; index < keys.Length; index++)
        {
            provider.TryGetSnippetGroup(new ShowCaseCodeSnippetKey(
                typeof(DateViewerShowCase).FullName!, "ExamplesContent", index, $"dateviewer-{keys[index]}"),
                out var group).ShouldBeTrue();
            group.Snippets.ShouldNotBeEmpty();
            group.Snippets.ShouldContain(snippet => snippet.Language == "axaml" && snippet.Text.Contains("atom:DateViewer"));
            group.Snippets.ShouldAllBe(snippet => !snippet.Text.Contains("Internal.DateViewer"));
        }
    }

    [Fact]
    public void Compiled_Page_Materializes_All_Examples_And_Semantic_Cell_Setters()
    {
        AvaloniaTestApp.EnsureInitialized();
        var model = new DateViewerViewModel(new DateViewerTestScreen());
        var page = new DateViewerShowCase { DataContext = model };
        var window = new AtomUI.Desktop.Controls.Window { Width = 1400, Height = 1000, Content = page };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var items = panel.Children.OfType<ShowCaseItem>().ToArray();
            items.Length.ShouldBe(8);
            foreach (var item in items)
                item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            items.ShouldAllBe(item => item.IsDeferredContentMaterialized);

            var semantic = items.Single(item => item.SourceKey == "dateviewer-semantic");
            foreach (var owner in semantic.GetVisualDescendants().Where(control =>
                         control is AtomUI.Desktop.Controls.DateViewer or RangeDateViewer))
            {
                var cells = owner.GetVisualDescendants().OfType<TemplatedControl>()
                    .Where(control => control.Classes.Contains("semantic-cell")).ToArray();
                cells.Length.ShouldBeGreaterThan(0);
                cells.ShouldAllBe(cell => Equals(cell.Tag, "dateviewer-cell-style-hit") &&
                    cell.FontWeight == FontWeight.Bold, owner.GetType().Name);
            }

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            var preview = page.GetVisualDescendants().OfType<SemanticPartPreview>().Single();
            preview.Title.ShouldBe("DateViewer");
            preview.SemanticOwnerType.ShouldBe(typeof(AtomUI.Desktop.Controls.DateViewer));
            preview.SemanticOwner.ShouldBeOfType<AtomUI.Desktop.Controls.DateViewer>();
            ((AtomUI.Desktop.Controls.DateViewer)preview.SemanticOwner).Bounds.Width.ShouldBeGreaterThan(0);
            preview.GetVisualDescendants().OfType<RangeDateViewer>().ShouldBeEmpty();
            preview.GetVisualDescendants().OfType<TextBlock>().ShouldContain(text => text.Text == "footer");
            var footer = ((AtomUI.Desktop.Controls.DateViewer)preview.SemanticOwner)
                .GetVisualDescendants().OfType<DashedBorder>()
                .Single(border => border.Classes.Contains("semantic-footer"));
            footer.IsVisible.ShouldBeTrue();
            footer.Tag.ShouldBe("dateviewer-footer-style-hit");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Standalone_DateViewer_Page_Preserves_Public_Examples_And_Semantic_Styles()
    {
        var root = FindRoot();
        var page = Path.Combine(root, "controlgallery/AtomUIGallery/ShowCases/DataDisplay/DateViewer/Views/DateViewerShowCase.axaml");
        File.Exists(page).ShouldBeTrue("DateViewer requires its own standalone Gallery page.");
        var source = File.ReadAllText(page);
        source.ShouldContain("GalleryShowCaseHost");
        source.ShouldContain("SemanticPartsContentTemplate");
        source.ShouldContain("IsDeferredLoadingEnabled=\"True\"");
        source.ShouldContain("IsScrollEnabled=\"False\"");
        source.ShouldContain("atom:DateViewerCellStyle");
        source.ShouldContain("atom:DateViewerFooterStyle");
        source.ShouldContain("x:SetterTargetType=\"atom:DashedBorder\"");
        source.ShouldContain("Property=\"Tag\" Value=\"dateviewer-footer-style-hit\"");
        source.ShouldContain("Path=\"footer\"");
        source.ShouldContain("atom:RangeDateViewerCellStyle");
        source.ShouldContain("x:SetterTargetType=\"TemplatedControl\"");
        source.ShouldContain("Property=\"FontWeight\" Value=\"Bold\"");
        source.ShouldContain("Property=\"Tag\" Value=\"dateviewer-cell-style-hit\"");
        source.ShouldContain("x:DataType=\"atom:DateViewerCellContext\"");
        source.ShouldContain("x:DataType=\"atom:DateViewerHeaderContext\"");
        source.ShouldContain("Presentation=\"Content\"");
        source.ShouldContain("FullCellTemplate");
        source.ShouldContain("DisabledDate=\"{Binding DisableWeekends}\"");
        source.ShouldNotContain("PART_");
        source.ShouldNotContain("ScenarioApi");
        source.ShouldNotContain("ScenarioDesignToken");
        var code = File.ReadAllText(page + ".cs");
        code.ShouldNotContain("FindControl");
        code.ShouldNotContain("Loaded=");
        code.ShouldNotContain("Unloaded=");

        var module = File.ReadAllText(Path.Combine(root, "controlgallery/AtomUIGallery/AtomUIGalleryModule.cs"));
        module.ShouldContain(".AddPage(DateViewerViewModel.ID");
        module.ShouldContain("routes.Map(DateViewerViewModel.ID");

        var document = XDocument.Load(page);
        XNamespace gallery = "https://atomui.net/oss-controls/gallery";
        var items = document.Descendants(gallery + "ShowCaseItem").ToArray();
        items.Length.ShouldBe(8);
        items.ShouldAllBe(item => (string?)item.Attribute("IsDeferredContentEnabled") == "True");
        document.Descendants(gallery + "ShowCaseItem.DeferredContentTemplate").Count().ShouldBe(items.Length);
    }

    [Fact]
    public void Range_Value_Is_Atomic_And_All_Five_Units_Are_Demonstrated()
    {
        var root = FindRoot();
        var model = File.ReadAllText(Path.Combine(root, "controlgallery/AtomUIGallery/ShowCases/DataDisplay/DateViewer/ViewModels/DateViewerViewModel.cs"));
        model.ShouldContain("DateViewerRange? RangeValue");
        model.ShouldContain("DateViewerSelectionUnit SelectionUnit");
        model.ShouldNotContain("RangeStartSelectedDate");
        model.ShouldNotContain("RangeEndSelectedDate");
        var page = File.ReadAllText(Path.Combine(root, "controlgallery/AtomUIGallery/ShowCases/DataDisplay/DateViewer/Views/DateViewerShowCase.axaml"));
        foreach (var unit in new[] { "Date", "Week", "Month", "Quarter", "Year" })
            page.ShouldContain($"DateViewerShowCaseLangResource Unit{unit}");
        page.ShouldContain("<atom:RangeDateViewer");
        page.ShouldContain("Value=\"{Binding RangeValue}\"");
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
                return directory.FullName;
            if (Directory.Exists(Path.Combine(directory.FullName, "controlgallery/AtomUIGallery")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Repository root not found.");
    }
}

internal sealed class DateViewerTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
