using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Rendering.Tests.Effect;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.DataGrid;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

/// <summary>Default typography links must compose with the actual Gallery table row surface.</summary>
public class HyperLinkDataGridCompositionTests
{
    [Fact]
    public void Gallery_DataGrid_Default_Links_Preserve_Tinted_Hovered_And_Selected_Row_Surfaces()
    {
        var previousDeferredLoading = GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled;
        using var viewModel = new DataGridViewModel(new TestScreen());
        var showCase = new DataGridShowCase { DataContext = viewModel };
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().WithToken("EnableMotion", "false")
                .WithControl(DataGridTokens.Identity, new ControlThemeConfigBuilder()
                    .WithToken("ColorBgContainer", "#ffe6e0ff")
                    .WithToken("RowHoverBg", "#ffe5f5ff")
                    .WithToken("RowSelectedBg", "#ffe0f0e0")
                    .WithToken("RowSelectedHoverBg", "#fff5e0e8").Build())
                // A link is typography: even an independent container colour must not replace its transparent default.
                .WithControl(HyperLinkTextBlockTokens.Identity, new ControlThemeConfigBuilder()
                    .WithToken("ColorBgContainer", "#ffff00ff").Build()).Build(),
            Child = showCase
        };
        var window = new AvaloniaWindow { Width = 1100, Height = 720, Content = provider };
        try
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = true;
            window.Show();
            FrameCapture.Refresh(window);
            var grid = showCase.GetVisualDescendants().OfType<DataGrid>().First(control => control.Name == "BasicCaseGrid");
            grid.Columns.Count.ShouldBe(5);
            var row = grid.GetVisualDescendants().OfType<DataGridRow>().First();
            row.BringIntoView();
            FrameCapture.Refresh(window);
            var links = row.GetVisualDescendants().OfType<HyperLinkTextBlock>().ToArray();
            links.Length.ShouldBe(4, "Keep the actual name link and three action links of the Gallery row.");
            links.Last().GetVisualAncestors().OfType<StackPanel>().First().Spacing.ShouldBe(15);
            SKRectI PixelBounds(HyperLinkTextBlock link)
            {
                var text = link.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.TextBlock>()
                    .Single(control => control.Name == "PART_Text");
                var origin = text.TranslatePoint(default, window)!.Value;
                var bounds = new SKRectI((int)Math.Ceiling(origin.X), (int)Math.Ceiling(origin.Y),
                    (int)Math.Floor(origin.X + text.Bounds.Width), (int)Math.Floor(origin.Y + text.Bounds.Height));
                bounds.Left.ShouldBeGreaterThanOrEqualTo(0);
                bounds.Top.ShouldBeGreaterThanOrEqualTo(0);
                bounds.Right.ShouldBeLessThanOrEqualTo((int)window.Bounds.Width);
                bounds.Bottom.ShouldBeLessThanOrEqualTo((int)window.Bounds.Height);
                bounds.Width.ShouldBeGreaterThan(0);
                bounds.Height.ShouldBeGreaterThan(0);
                return bounds;
            }

            var failures = new List<string>();
            foreach (var (selected, hovered, colour) in new[]
            {
                (false, false, "#ffe6e0ff"),
                (false, true, "#ffe5f5ff"),
                (true, false, "#ffe0f0e0"),
                (true, true, "#fff5e0e8")
            })
            {
                row.IsSelected = selected;
                ((IPseudoClasses)row.Classes).Set(":row-hover", hovered);
                Dispatcher.UIThread.RunJobs();
                FrameCapture.Refresh(window);
                var expected = Color.Parse(colour);
                ((ISolidColorBrush)row.Background!).Color.ShouldBe(expected);
                using var defaultFrame = FrameCapture.Capture(window);
                foreach (var link in links)
                {
                    link.Background = Brushes.Transparent;
                }
                FrameCapture.Refresh(window);
                using var transparentFrame = FrameCapture.Capture(window);
                foreach (var link in links)
                {
                    var changed = FrameCapture.CountDifferentPixels(defaultFrame, transparentFrame, PixelBounds(link), 0);
                    if (changed != 0)
                    {
                        failures.Add($"selected={selected}, hovered={hovered}, link={link.Text}, changed={changed}");
                    }
                    link.ClearValue(TemplatedControl.BackgroundProperty);
                }
            }
            // Explicit application Background still paints on the same real selected/hovered row.
            FrameCapture.Refresh(window);
            using var beforeExplicit = FrameCapture.Capture(window);
            links[0].Background = Brushes.Lime;
            FrameCapture.Refresh(window);
            using var explicitFrame = FrameCapture.Capture(window);
            var projectedText = links[0].GetVisualDescendants().OfType<AtomUI.Desktop.Controls.TextBlock>().Single(control => control.Name == "PART_Text");
            projectedText.Background.ShouldBe(Brushes.Lime);
            FrameCapture.CountDifferentPixels(beforeExplicit, explicitFrame, PixelBounds(links[0]), 0).ShouldBeGreaterThan(0);
            failures.ShouldBeEmpty("Default links must reveal their real DataGrid row background around the glyphs.");
            EffectTestData.For("AtomUI", "HyperLinkTextBlock").ShouldNotContain(row => (string)row[0] == "ColorBgContainer");
        }
        finally
        {
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            window.Close();
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = previousDeferredLoading;
        }
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
