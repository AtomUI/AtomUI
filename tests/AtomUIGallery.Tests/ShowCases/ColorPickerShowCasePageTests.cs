using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.ColorPicker;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomUIColorPicker = AtomUI.Desktop.Controls.ColorPicker;
using AtomUIPopup = AtomUI.Desktop.Controls.Popup;
using AtomUITabStrip = AtomUI.Desktop.Controls.TabStrip;
using AtomUITabStripItem = AtomUI.Desktop.Controls.TabStripItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class ColorPickerShowCasePageTests
{

    [Fact]
    public void ColorPicker_Semantic_Popup_Is_Pinned_Open_On_First_Tab_Selection()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ColorPickerShowCase
        {
            DataContext = new ColorPickerViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, window =>
        {
            page.GetVisualDescendants()
                .OfType<AtomUIColorPicker>()
                .ShouldNotContain(static picker => picker.Name == "ColorPickerSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            var tabStrip = page.GetVisualDescendants().OfType<AtomUITabStrip>().Single();
            Click(tabStrip.Items.OfType<AtomUITabStripItem>().ElementAt(1), window);
            Dispatcher.UIThread.RunJobs();

            var semanticPicker = page.GetVisualDescendants()
                                     .OfType<AtomUIColorPicker>()
                                     .Single(static picker => picker.Name == "ColorPickerSemanticOwner");
            var popup = semanticPicker.GetVisualDescendants()
                                      .OfType<AtomUIPopup>()
                                      .Single(static candidate => candidate.Name == "PART_Popup");

            semanticPicker.IsEffectivelyVisible.ShouldBeTrue();
            semanticPicker.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();

            semanticPicker.IsEffectivelyVisible.ShouldBeFalse();
            popup.IsOpen.ShouldBeFalse();

            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants()
                .OfType<AtomUIColorPicker>()
                .Single(static picker => picker.Name == "ColorPickerSemanticOwner")
                .ShouldBeSameAs(semanticPicker);
            semanticPicker.IsEffectivelyVisible.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
        });
    }

    [Fact]
    public void ColorPicker_Semantic_Preview_Keeps_Trigger_Fully_Visible_Above_Popup()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ColorPickerShowCase
        {
            DataContext = new ColorPickerViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, window =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(control => control.Name == "ColorPickerSemanticPreview");
            var stage = preview.GetVisualDescendants()
                               .OfType<Border>()
                               .Single(control => control.Name == "PART_PreviewStage");
            var picker = page.GetVisualDescendants()
                             .OfType<AtomUIColorPicker>()
                             .Single(control => control.Name == "ColorPickerSemanticOwner");
            var triggerOrigin = picker.TranslatePoint(default, stage).ShouldNotBeNull();

            stage.Bounds.Height.ShouldBeGreaterThanOrEqualTo(600,
                "the stage must reserve the trigger plus the pinned popup's downward expansion space");
            triggerOrigin.Y.ShouldBeGreaterThanOrEqualTo(0,
                "the trigger top must not be centered above and clipped by the preview stage");
            (triggerOrigin.Y + picker.Bounds.Height).ShouldBeLessThanOrEqualTo(stage.Bounds.Height,
                "the complete trigger must remain visible inside the preview stage");
        });
    }

    private static string ExtractColorPickerExampleItems(string source)
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

    private static void ShowInWindow(
        Control content,
        double width,
        double height,
        Action<AvaloniaWindow> assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            EnableOverlayLayer = true,
            Child              = content
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Content = visualLayerManager,
            Width   = width,
            Height  = height
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

    private static void Click(Control control, AvaloniaWindow window)
    {
        var point = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window);
        point.ShouldNotBeNull();

        window.MouseDown(point.Value, MouseButton.Left);
        window.MouseUp(point.Value, MouseButton.Left);
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
