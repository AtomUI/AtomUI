using System.Collections;
using System.Reflection;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;
using AtomUIListView = AtomUI.Desktop.Controls.ListView;
using AtomUIListBox = AtomUI.Desktop.Controls.ListBox;
using AtomUIListViewItem = AtomUI.Desktop.Controls.ListViewItem;
using AtomUIListBoxItem = AtomUI.Desktop.Controls.ListBoxItem;
using ListShowCase = AtomUIGallery.ShowCases.List.ListShowCase;
using ListViewModel = AtomUIGallery.ShowCases.List.ListViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class ListShowCasePageTests
{

    [Fact]
    public void List_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/List/Views/ListShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/List/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "list-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"ListViewSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #ListViewSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:ListView}\"");
        source.ShouldContain("Name=\"ListBoxSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #ListBoxSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:ListBox}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(5);
        CountOccurrences(source, "Path=\"root\"").ShouldBe(2);
        CountOccurrences(source, "Path=\"item\"").ShouldBe(2);
        source.ShouldContain("Path=\"groupHeader\"");

        semanticSource.ShouldContain("SourceKey=\"list-semantic-part\"");
        semanticSource.ShouldContain("ListShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("ListShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|ListView.semantic-object\"");
        semanticSource.ShouldContain("Selector=\"atom|ListBox.semantic-listbox\"");
        semanticSource.ShouldContain("Value=\"#91caff\"");
        semanticSource.ShouldContain("Value=\"#1677ff\"");
        semanticSource.ShouldContain("Value=\"#e6f4ff\"");
        semanticSource.ShouldContain("Property=\"FontStyle\"");
        semanticSource.ShouldContain("Property=\"Foreground\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Property=\"BorderBrush\"");
        CountOccurrences(semanticSource, "<atom:ListViewItemStyle").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:ListViewGroupHeaderStyle").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:ListBoxItemStyle").ShouldBe(1);
        semanticSource.ShouldContain("x:SetterTargetType=\"atom:ListViewItem\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"atom:ListBoxItem\"");
        CountOccurrences(semanticSource, "Classes=\"semantic-object\"").ShouldBe(1);
        CountOccurrences(semanticSource, "Classes=\"semantic-listbox\"").ShouldBe(1);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Root element, the scroll container, with font and relative positioning");
        english.ShouldContain("Item element, with padding, split line and hover background");
        english.ShouldContain("Group header element, with sticky positioning and background color");
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Use generated style classes to customize the published Semantic Parts.");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void List_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ListShowCase
        {
            DataContext = new ListViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUIListView>()
                .ShouldNotContain(static listView => listView.Name == "ListViewSemanticOwner");
            page.GetVisualDescendants()
                .OfType<AtomUIListBox>()
                .ShouldNotContain(static listBox => listBox.Name == "ListBoxSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(2);

            var listViewPreview = page.GetVisualDescendants()
                                      .OfType<SemanticPartPreview>()
                                      .Single(static candidate => candidate.Name == "ListViewSemanticPreview");
            var semanticListView = listViewPreview.SemanticOwner.ShouldBeOfType<AtomUIListView>();
            semanticListView.Name.ShouldBe("ListViewSemanticOwner");
            var listViewItems = semanticListView.GetVisualDescendants()
                                                .OfType<AtomUIListViewItem>()
                                                .Where(static item => item.Classes.Contains("semantic-item"))
                                                .ToArray();
            listViewItems.ShouldNotBeEmpty();
            listViewItems.ShouldContain(static item => item.IsEffectivelyVisible);
            var groupHeaders = semanticListView.GetVisualDescendants()
                                               .OfType<AtomUIListViewItem>()
                                               .Where(static item => item.Classes.Contains("semantic-group-header"))
                                               .ToArray();
            groupHeaders.ShouldNotBeEmpty();
            groupHeaders.ShouldContain(static header => header.IsEffectivelyVisible);

            var listBoxPreview = page.GetVisualDescendants()
                                     .OfType<SemanticPartPreview>()
                                     .Single(static candidate => candidate.Name == "ListBoxSemanticPreview");
            var semanticListBox = listBoxPreview.SemanticOwner.ShouldBeOfType<AtomUIListBox>();
            semanticListBox.Name.ShouldBe("ListBoxSemanticOwner");
            var listBoxItems = semanticListBox.GetVisualDescendants()
                                              .OfType<AtomUIListBoxItem>()
                                              .Where(static item => item.Classes.Contains("semantic-item"))
                                              .ToArray();
            listBoxItems.ShouldNotBeEmpty();
            listBoxItems.ShouldContain(static item => item.IsEffectivelyVisible);

            var listViewPreviewItems = GetPreviewItems(listViewPreview);
            listViewPreviewItems.Length.ShouldBe(3);
            var listBoxPreviewItems = GetPreviewItems(listBoxPreview);
            listBoxPreviewItems.Length.ShouldBe(2);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void List_Semantic_Previews_Render_Every_Row_Without_Clipping()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ListShowCase
        {
            DataContext = new ListViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var listViewPreview = page.GetVisualDescendants()
                                      .OfType<SemanticPartPreview>()
                                      .Single(static candidate => candidate.Name == "ListViewSemanticPreview");
            var semanticListView = listViewPreview.SemanticOwner.ShouldBeOfType<AtomUIListView>();

            var listViewItems = semanticListView.GetVisualDescendants()
                                                .OfType<AtomUIListViewItem>()
                                                .Where(static item => item.Classes.Contains("semantic-item"))
                                                .ToArray();
            listViewItems.Length.ShouldBe(6);
            var groupHeaders = semanticListView.GetVisualDescendants()
                                               .OfType<AtomUIListViewItem>()
                                               .Where(static item => item.Classes.Contains("semantic-group-header"))
                                               .ToArray();
            groupHeaders.Length.ShouldBe(2);

            // The Ant Design Listy reference renders the complete list without a height
            // constraint; every row, including the last one, must sit inside the viewport.
            listViewItems.ShouldContain(static item => GetItemLabel(item) == "Ethan");
            foreach (var item in listViewItems.Concat(groupHeaders))
            {
                var bottom = item.TransformToVisual(semanticListView)!
                                 .Value
                                 .Transform(new Point(0, item.Bounds.Height))
                                 .Y;
                bottom.ShouldBeLessThanOrEqualTo(semanticListView.Bounds.Height);
            }

            var listViewScrollViewer = semanticListView.GetVisualDescendants()
                                                       .OfType<AvaloniaScrollViewer>()
                                                       .Single();
            listViewScrollViewer.Extent.Height.ShouldBeLessThanOrEqualTo(listViewScrollViewer.Viewport.Height);

            var listBoxPreview = page.GetVisualDescendants()
                                     .OfType<SemanticPartPreview>()
                                     .Single(static candidate => candidate.Name == "ListBoxSemanticPreview");
            var semanticListBox = listBoxPreview.SemanticOwner.ShouldBeOfType<AtomUIListBox>();

            // The preview must not clip the list to a fixed height: the internal
            // ScrollViewer has to be exactly as tall as its content.
            semanticListBox.Items.Count.ShouldBe(6);
            var listBoxScrollViewer = semanticListBox.GetVisualDescendants()
                                                     .OfType<AvaloniaScrollViewer>()
                                                     .Single();
            listBoxScrollViewer.Extent.Height.ShouldBeLessThanOrEqualTo(listBoxScrollViewer.Viewport.Height);
            listBoxScrollViewer.Extent.Height.ShouldBeGreaterThanOrEqualTo(6 * 32);
        });
    }

    [Fact]
    public void List_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ListShowCase
        {
            DataContext = new ListViewModel(new TestScreen())
        };

        // The style demo sits at the bottom of the scrollable ShowCasePanel; use a tall
        // window so the virtualized demo controls land inside the viewport and realize items.
        ShowInWindow(page, 1280, 5200, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "list-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var listViewDemo = page.GetVisualDescendants()
                                   .OfType<AtomUIListView>()
                                   .Single(static candidate => candidate.Classes.Contains("semantic-object"));
            var listViewFrame = FindRootFrame(listViewDemo);
            AssertSolidColor(listViewFrame.BorderBrush, "#91caff");
            listViewFrame.BorderThickness.ShouldBe(new Thickness(1));
            listViewFrame.CornerRadius.ShouldBe(new CornerRadius(8));
            var groupHeader = listViewDemo.GetVisualDescendants()
                                          .OfType<AtomUIListViewItem>()
                                          .First(static candidate => candidate.Classes.Contains("semantic-group-header"));
            AssertSolidColor(groupHeader.Foreground, "#1677ff");
            AssertSolidColor(groupHeader.Background, "#e6f4ff");
            var listViewItems = listViewDemo.GetVisualDescendants()
                                            .OfType<AtomUIListViewItem>()
                                            .Where(static candidate => candidate.Classes.Contains("semantic-item"))
                                            .ToArray();
            listViewItems.ShouldNotBeEmpty();
            listViewItems.ShouldAllBe(static candidate => candidate.FontStyle == FontStyle.Italic);

            var listBoxDemo = page.GetVisualDescendants()
                                  .OfType<AtomUIListBox>()
                                  .Single(static candidate => candidate.Classes.Contains("semantic-listbox"));
            var listBoxFrame = FindRootFrame(listBoxDemo);
            AssertSolidColor(listBoxFrame.BorderBrush, "#91caff");
            listBoxFrame.BorderThickness.ShouldBe(new Thickness(1));
            listBoxFrame.CornerRadius.ShouldBe(new CornerRadius(8));
            var listBoxItems = listBoxDemo.GetVisualDescendants()
                                          .OfType<AtomUIListBoxItem>()
                                          .Where(static candidate => candidate.Classes.Contains("semantic-item"))
                                          .ToArray();
            listBoxItems.ShouldNotBeEmpty();
            listBoxItems.ShouldAllBe(static candidate => candidate.FontStyle == FontStyle.Italic);
        });
    }

    private static object[] GetPreviewItems(SemanticPartPreview preview)
    {
        var items = typeof(SemanticPartPreview)
                    .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(preview);
        items.ShouldNotBeNull();
        return items.ShouldBeAssignableTo<IEnumerable>().Cast<object>().ToArray();
    }

    private static string? GetItemLabel(AtomUIListViewItem item)
    {
        return (item.Content as IListItemData)?.Content as string;
    }

    private static AtomUI.Controls.Primitives.PixelAlignedBorder FindRootFrame(Control owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<AtomUI.Controls.Primitives.PixelAlignedBorder>()
                    .First(static frame => frame.Name == "Frame");
    }

    private static string ExtractListExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
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

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
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
