using System.Collections;
using System.Reflection;
using AtomUIGallery.ShowCases.Calendar;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUICalendar = AtomUI.Desktop.Controls.Calendar;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class CalendarShowCasePageTests
{

    [Fact]
    public void Calendar_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Calendar/Views/CalendarShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Calendar/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "calendar-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"CalendarSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #CalendarSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Calendar}\"");
        source.ShouldContain("<atom:Calendar Name=\"CalendarSemanticOwner\" />");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(6);
        foreach (var path in new[] { "root", "header", "body", "content", "item", "itemContent" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"calendar-semantic-part\"");
        semanticSource.ShouldContain("CalendarShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("CalendarShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|Calendar.semantic-object\"");
        semanticSource.ShouldContain("Selector=\"atom|Calendar.semantic-function\"");
        semanticSource.ShouldContain("<Setter Property=\"Fullscreen\" Value=\"False\" />");
        semanticSource.ShouldContain("<Setter Property=\"Width\" Value=\"600\" />");
        semanticSource.ShouldContain("<Setter Property=\"CornerRadius\" Value=\"8\" />");
        CountOccurrences(semanticSource, "<Setter Property=\"Padding\" Value=\"10\" />").ShouldBe(2);
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"{atom:SharedTokenResource ColorPrimaryBg}\" />");
        semanticSource.ShouldContain("<Setter Property=\"BorderThickness\" Value=\"2\" />");
        semanticSource.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#BDE3C3\" />");
        semanticSource.ShouldContain("<Setter Property=\"CornerRadius\" Value=\"10\" />");
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"#4DBDE3C3\" />");
        CountOccurrences(semanticSource, "<atom:Calendar ").ShouldBe(2);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);

        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain(
            "Root element containing background, border, border-radius and overall layout structure of the calendar component");
        english.ShouldContain(
            "Header element with layout and style control for year selector, month selector and mode switcher");
        english.ShouldContain(
            "Body element with padding and layout control for the calendar table that contains the calendar grid");
        english.ShouldContain(
            "Content element with width, height and table styling control for the calendar table");
        english.ShouldContain(
            "Item element with background, border, hover state, selected state and other interactive styles for calendar cells");
        english.ShouldContain(
            "Item content element with height, overflow and other style control for custom content area inside calendar cells");
        english.ShouldNotContain("classNames", Case.Insensitive);
        english.ShouldNotContain("semantic dom", Case.Insensitive);
    }

    [Fact]
    public void Calendar_Semantic_Preview_Is_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new CalendarShowCase
        {
            ViewModel = new CalendarViewModel(null!)
        };

        ShowInWindow(page, 1200, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUICalendar>()
                .ShouldNotContain(static calendar => calendar.Name == "CalendarSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "CalendarSemanticPreview");
            var calendar = preview.SemanticOwner.ShouldBeOfType<AtomUICalendar>();
            calendar.Name.ShouldBe("CalendarSemanticOwner");
            calendar.GetVisualDescendants()
                    .OfType<TemplatedControl>()
                    .Count(static cell => cell.Classes.Contains("semantic-cell"))
                    .ShouldBe(42);
            calendar.GetVisualDescendants()
                    .OfType<ContentControl>()
                    .Count(static content => content.Classes.Contains("semantic-cell-content"))
                    .ShouldBe(42);

            var previewItemsValue = typeof(SemanticPartPreview)
                                    .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)
                                    ?.GetValue(preview);
            previewItemsValue.ShouldNotBeNull();
            var previewItems = previewItemsValue.ShouldBeAssignableTo<IEnumerable>()
                                                .Cast<object>()
                                                .ToArray();
            previewItems.Length.ShouldBe(6);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Calendar_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new CalendarShowCase
        {
            ViewModel = new CalendarViewModel(null!)
        };

        ShowInWindow(page, 1280, 1200, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "calendar-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomUICalendar>()
                            .Where(static calendar => calendar.Classes.Contains("semantic-object") ||
                                                      calendar.Classes.Contains("semantic-function"))
                            .ToArray();
            demos.Length.ShouldBe(2);

            var objectDemo = demos.Single(static calendar => calendar.Classes.Contains("semantic-object"));
            objectDemo.Fullscreen.ShouldBeFalse();
            objectDemo.Bounds.Width.ShouldBe(600);
            objectDemo.CornerRadius.ShouldBe(new CornerRadius(8));
            objectDemo.Padding.ShouldBe(new Thickness(10));
            AssertSolidColor(objectDemo.Background, "#E6F4FF");
            var objectContent = objectDemo.GetVisualDescendants()
                                           .OfType<AtomUI.Desktop.Controls.DateViewer>()
                                           .Single(static control => control.Classes.Contains("semantic-content"));
            AssertSolidColor(objectContent.Background, "#FFFFFF");

            var functionDemo = demos.Single(static calendar => calendar.Classes.Contains("semantic-function"));
            functionDemo.Fullscreen.ShouldBeTrue();
            functionDemo.BorderThickness.ShouldBe(new Thickness(2));
            AssertSolidColor(functionDemo.BorderBrush, "#BDE3C3");
            functionDemo.CornerRadius.ShouldBe(new CornerRadius(10));
            functionDemo.Padding.ShouldBe(new Thickness(10));
            AssertSolidColor(functionDemo.Background, "#4DBDE3C3");
            var functionContent = functionDemo.GetVisualDescendants()
                                              .OfType<AtomUI.Desktop.Controls.DateViewer>()
                                              .Single(static control => control.Classes.Contains("semantic-content"));
            AssertSolidColor(functionContent.Background, "#FFFFFF");
        });
    }

    [Fact]
    public void Calendar_CustomHeader_ShowCase_Materializes_Without_Resource_Type_Errors()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new CalendarShowCase
        {
            ViewModel = new CalendarViewModel(null!)
        };
        var window = new AvaloniaWindow
        {
            Width = 1200,
            Height = 900,
            Content = page
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            page.ApplyTemplate();
            page.Measure(new Size(1200, 900));
            page.Arrange(new Rect(0, 0, 1200, 900));
            Dispatcher.UIThread.RunJobs();

            var items = page.GetVisualDescendants().OfType<ShowCaseItem>().ToArray();
            items.Length.ShouldBeGreaterThan(4);
            items[^1].SourceKey.ShouldBe("calendar-semantic-part");
            var customHeaderItem = items[^2];

            customHeaderItem.MaterializeDeferredContent();
            customHeaderItem.Measure(new Size(1200, 900));
            customHeaderItem.Arrange(new Rect(customHeaderItem.DesiredSize));
            Dispatcher.UIThread.RunJobs();

            customHeaderItem.GetVisualDescendants()
                .OfType<AtomUICalendar>()
                .Single()
                .HeaderTemplate.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Calendar_Selectable_ViewModel_Separates_Panel_Value_From_Selected_Value()
    {
        var viewModel = new CalendarViewModel(null!);

        var initialDate = new DateTime(2017, 1, 25);
        viewModel.SelectableCalendarValue.ShouldBe(initialDate);
        viewModel.SelectableCalendarSelectedValue.ShouldBe(initialDate);
        viewModel.SelectableCalendarSelectedText.ShouldBe("You selected date: 2017-01-25");

        viewModel.SelectableCalendarValue = new DateTime(2017, 2, 25);
        viewModel.SelectableCalendarSelectedValue.ShouldBe(initialDate);
        viewModel.SelectableCalendarSelectedText.ShouldBe("You selected date: 2017-01-25");

        var selectedDate = new DateTime(2017, 3, 12);
        viewModel.SelectSelectableCalendarDate(selectedDate);

        viewModel.SelectableCalendarValue.ShouldBe(selectedDate);
        viewModel.SelectableCalendarSelectedValue.ShouldBe(selectedDate);
        viewModel.SelectableCalendarSelectedText.ShouldBe("You selected date: 2017-03-12");
    }

    private static string ExtractCalendarExampleItems(string source)
    {
        const string firstItemMarker = "<gallery:ShowCaseItem";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"calendar-semantic-part\"";
        var semanticItemStart = source.IndexOf(semanticItemMarker, firstItemStart, StringComparison.Ordinal);
        semanticItemStart.ShouldBeGreaterThan(firstItemStart);
        var semanticItemStartTag = source.LastIndexOf(firstItemMarker, semanticItemStart, StringComparison.Ordinal);
        semanticItemStartTag.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..semanticItemStartTag];
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

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
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

    private static IReadOnlyDictionary<string, string> ReadCalendarLocalization(string languageTag)
    {
        return XliffTestDocument.Read(
            $"controlgallery/AtomUIGallery/ShowCases/DataDisplay/Calendar/Localization/{languageTag}.xlf");
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

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
    }
}
