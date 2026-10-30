using System.Security.Cryptography;
using System.Text;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using AtomUIRangeTimePicker = AtomUI.Desktop.Controls.RangeTimePicker;
using TimePickerShowCase = AtomUIGallery.ShowCases.TimePicker.TimePickerShowCase;
using TimePickerViewModel = AtomUIGallery.ShowCases.TimePicker.TimePickerViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class TimePickerShowCasePageTests
{

    [Fact]
    public void TimePicker_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/TimePicker/Views/TimePickerShowCase.axaml");
        var semanticSource = ExtractShowCaseItemBySourceKey(source, "timepicker-semantic-part");

        source.ShouldContain("Name=\"TimePickerSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #TimeRangeSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:RangeTimePicker}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(12);
        foreach (var path in new[]
                 {
                     "root", "prefix", "input", "secondaryInput", "suffix", "clear",
                     "popup.root", "popup.container", "popup.content", "popup.column", "popup.item", "popup.footer"
                 })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("TimePickerShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("TimePickerShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|TimePicker.semantic-styles-demo\"");
        semanticSource.ShouldContain("Selector=\"atom|RangeTimePicker.semantic-styles-demo\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"atom:ArrowDecoratedBox\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"ListBoxItem\"");
        semanticSource.ShouldContain("<atom:TimePickerPrefixStyle");
        semanticSource.ShouldContain("<atom:TimePickerPopupRootStyle");
        semanticSource.ShouldContain("<atom:TimePickerPopupItemStyle");
        semanticSource.ShouldContain("<atom:RangeTimePickerInputStyle");
        semanticSource.ShouldContain("<atom:RangeTimePickerPopupColumnStyle");
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);

        foreach (var locale in new[] { "en-US", "zh-CN", "zh-TW", "pt-BR" })
        {
            var localization = ReadRepoFile(
                $"controlgallery/AtomUIGallery/ShowCases/DataEntry/TimePicker/Localization/{locale}.xlf");
            localization.ShouldContain("<source>Custom Semantic Part styling</source>");
            foreach (var key in new[]
                     {
                         "SemanticRootDescription", "SemanticPrefixDescription", "SemanticInputDescription",
                         "SemanticSecondaryInputDescription", "SemanticSuffixDescription", "SemanticClearDescription",
                         "SemanticPopupRootDescription", "SemanticPopupContainerDescription",
                         "SemanticPopupContentDescription", "SemanticPopupColumnDescription",
                         "SemanticPopupItemDescription", "SemanticPopupFooterDescription",
                         "SemanticPartStyleTitle", "SemanticPartStyleDescription"
                     })
            {
                localization.ShouldContain($"<unit id=\"{key}\">");
            }

            localization.ShouldNotContain("semantic dom", Case.Insensitive);
            localization.ShouldNotContain("classNames", Case.Insensitive);
        }
    }

    [Fact]
    public void TimePicker_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new TimePickerShowCase
        {
            DataContext = new TimePickerViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, window =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUIRangeTimePicker>()
                .ShouldNotContain(static picker => picker.Name == "TimeRangeSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            page.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "TimePickerSemanticPreview");
            var semanticPicker = preview.SemanticOwner.ShouldBeOfType<AtomUIRangeTimePicker>();
            semanticPicker.Name.ShouldBe("TimeRangeSemanticOwner");
            window.GetVisualDescendants()
                  .OfType<Control>()
                  .Count(static control => control.Classes.Contains("semantic-popup-root"))
                  .ShouldBe(1);
            window.GetVisualDescendants()
                  .OfType<Control>()
                  .Count(static control => control.Classes.Contains("semantic-time-column"))
                  .ShouldBe(4);
            window.GetVisualDescendants()
                  .OfType<Control>()
                  .Count(static control => control.Classes.Contains("semantic-time-item"))
                  .ShouldBeGreaterThanOrEqualTo(21);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
        });
    }

    private static string ExtractTimePickerExampleItems(string source)
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

    private static void AssertResourceOrder(string source, params string[] resources)
    {
        var previousIndex = -1;
        foreach (var resource in resources)
        {
            var index = source.IndexOf(resource, StringComparison.Ordinal);
            index.ShouldBeGreaterThan(previousIndex, $"{resource} should appear after the previous resource.");
            previousIndex = index;
        }
    }

    private static string ExtractShowCaseItemByTitle(string source, string titleResource)
    {
        var titleIndex = source.IndexOf(titleResource, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        const string itemStartMarker = "<gallery:ShowCaseItem";
        const string itemEndMarker   = "</gallery:ShowCaseItem>";

        var itemStart = source.LastIndexOf(itemStartMarker, titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemEnd = source.IndexOf(itemEndMarker, titleIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(titleIndex);

        return source[itemStart..(itemEnd + itemEndMarker.Length)];
    }

    private static string ExtractShowCaseItemBySourceKey(string source, string sourceKey)
    {
        var keyIndex = source.IndexOf($"SourceKey=\"{sourceKey}\"", StringComparison.Ordinal);
        keyIndex.ShouldBeGreaterThanOrEqualTo(0);

        const string itemStartMarker = "<gallery:ShowCaseItem";
        const string itemEndMarker   = "</gallery:ShowCaseItem>";

        var itemStart = source.LastIndexOf(itemStartMarker, keyIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemEnd = source.IndexOf(itemEndMarker, keyIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(keyIndex);

        return source[itemStart..(itemEnd + itemEndMarker.Length)];
    }

    private static void ShowInWindow(Control content, double width, double height, Action<AvaloniaWindow> assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            EnableOverlayLayer = true,
            Child = content
        };
        EnablePopupOverlayLayer(visualLayerManager);
        var window = new AtomUIWindow
        {
            Content = visualLayerManager,
            Width = width,
            Height = height
        };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
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
