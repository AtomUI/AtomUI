using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Controls.Tests.Watermark;

public class WatermarkTileRendererTests
{
    static WatermarkTileRendererTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    // Draws a filled rectangle covering its desired size, so each tile becomes one recorded leaf.
    private sealed class ProbeGlyph : WatermarkGlyph
    {
        public Size Size { get; set; }

        public override void Render(DrawingContext context)
        {
            context.DrawRectangle(Brushes.Black, null, new Rect(Size));
        }

        public override Size GetDesiredSize()
        {
            return Size;
        }
    }

    private sealed class ProbeImage : IImage
    {
        public Size Size { get; init; }

        public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
        {
            context.DrawRectangle(Brushes.Black, null, destRect);
        }
    }

    private sealed class ProbeTextGlyph : TextGlyph
    {
        public FormattedText? Formatted => FormattedText;
    }

    private readonly record struct Tile(Point Center, double AngleDegrees);

    // Records the rendered tiles: the center and rotation of every rectangle in its final (world) space.
    private static List<Tile> RecordTiles(WatermarkTileRenderer renderer, Size area, Size cell, WatermarkGlyph glyph,
                                          Size glyphSize)
    {
        var group = new DrawingGroup();
        using (var context = group.Open())
        {
            renderer.Render(context, area, cell, glyph);
        }

        var tiles = new List<Tile>();
        Collect(group, Matrix.Identity, glyphSize, tiles);
        return tiles;
    }

    private static void Collect(Drawing drawing, Matrix parent, Size glyphSize, List<Tile> tiles)
    {
        switch (drawing)
        {
            case DrawingGroup group:
                var matrix = group.Transform is { } transform ? transform.Value * parent : parent;
                foreach (var child in group.Children)
                {
                    Collect(child, matrix, glyphSize, tiles);
                }
                break;
            case GeometryDrawing geometry:
                var bounds = geometry.Geometry!.Bounds;
                var center = bounds.Center.Transform(parent);
                var xAxis = new Point(1, 0).Transform(parent) - new Point(0, 0).Transform(parent);
                tiles.Add(new Tile(center, Math.Atan2(xAxis.Y, xAxis.X) * 180 / Math.PI));
                break;
        }
    }

    private static ProbeGlyph CreateGlyph(Size size, double rotate = 0)
    {
        return new ProbeGlyph
        {
            Size            = size,
            HorizontalSpace = 10,
            VerticalSpace   = 10,
            Rotate          = rotate,
            IsCrossUsed     = false,
            IsMirrorUsed    = false
        };
    }

    [Fact]
    public void Renderer_covers_area_with_rotated_cells()
    {
        var glyph = CreateGlyph(new Size(40, 20), 30);
        var renderer = new WatermarkTileRenderer();

        // Cell equals the glyph size, as the desktop control passes it: columns 0,50,100,150 and rows 0,30,60,90.
        var tiles = RecordTiles(renderer, new Size(200, 100), new Size(40, 20), glyph, glyph.Size);
        tiles.Count.ShouldBe(16);
        foreach (var column in new[] { 0, 50, 100, 150 })
        {
            foreach (var row in new[] { 0, 30, 60, 90 })
            {
                var tile = tiles.SingleOrDefault(t => Math.Abs(t.Center.X - (column + 20)) < 1e-6 &&
                                                      Math.Abs(t.Center.Y - (row + 10)) < 1e-6);
                tile.ShouldNotBe(default);
                tile.AngleDegrees.ShouldBe(30, 1e-6);
            }
        }

        // A larger cell: the glyph is centered in it, so its center sits at the cell center and steps grow.
        var wide = RecordTiles(renderer, new Size(200, 100), new Size(80, 40), glyph, glyph.Size);
        wide.Count.ShouldBe(6); // columns 0,90,180 and rows 0,50
    }

    [Fact]
    public void Renderer_centers_glyph_in_larger_cell()
    {
        var glyph = CreateGlyph(new Size(20, 10), 90);
        var renderer = new WatermarkTileRenderer();
        glyph.HorizontalSpace = 0;
        glyph.VerticalSpace   = 0;

        var tiles = RecordTiles(renderer, new Size(120, 80), new Size(60, 40), glyph, glyph.Size);
        tiles.Count.ShouldBe(4);
        foreach (var (column, row) in new[] { (0, 0), (60, 0), (0, 40), (60, 40) })
        {
            var tile = tiles.SingleOrDefault(t => Math.Abs(t.Center.X - (column + 30)) < 1e-6 &&
                                                  Math.Abs(t.Center.Y - (row + 20)) < 1e-6);
            tile.ShouldNotBe(default);
            tile.AngleDegrees.ShouldBe(90, 1e-6);
        }
    }

    [Fact]
    public void Renderer_mirrors_odd_columns_and_offsets_odd_rows()
    {
        var glyph = CreateGlyph(new Size(40, 20), 30);
        glyph.IsMirrorUsed = true;
        glyph.IsCrossUsed  = true;
        var renderer = new WatermarkTileRenderer();

        var tiles = RecordTiles(renderer, new Size(200, 100), new Size(40, 20), glyph, glyph.Size);
        // Row 0 (t = 0): columns 0,50,100,150 alternate +30 / -30 degrees.
        var row0 = tiles.Where(t => Math.Abs(t.Center.Y - 10) < 1e-6).OrderBy(t => t.Center.X).ToList();
        row0.Select(t => Math.Round(t.AngleDegrees)).ShouldBe(new double[] { 30, -30, 30, -30 });
        // Row 1 (t = 30) is shifted by half a column step.
        var row1 = tiles.Where(t => Math.Abs(t.Center.Y - 40) < 1e-6).OrderBy(t => t.Center.X).ToList();
        row1.Select(t => t.Center.X).First().ShouldBe(25 + 20, 1e-6);
    }

    [Fact]
    public void Renderer_skips_invisible_tiles_but_keeps_grid_phase()
    {
        // Built-in image glyphs skip tiles that cannot be seen; the surviving tiles must stay on the same
        // lattice, including the half-step shift of odd rows.
        var glyph = new ImageGlyph
        {
            Source           = new ProbeImage { Size = new Size(40, 20) },
            Height           = 20,
            HorizontalSpace  = 10,
            VerticalSpace    = 10,
            HorizontalOffset = -1000,
            VerticalOffset   = -1000,
            Rotate           = 0,
            IsCrossUsed      = true,
            IsMirrorUsed     = true
        };
        var renderer = new WatermarkTileRenderer();

        var tiles = RecordTiles(renderer, new Size(200, 100), glyph.GetDesiredSize(), glyph, glyph.GetDesiredSize());
        tiles.Count.ShouldBe(18);
        foreach (var (y, xs) in new (double, double[])[]
                 {
                     (0, [-5, 45, 95, 145, 195]),
                     (30, [20, 70, 120, 170]),
                     (60, [-5, 45, 95, 145, 195]),
                     (90, [20, 70, 120, 170])
                 })
        {
            tiles.Where(t => Math.Abs(t.Center.Y - y) < 1e-6).Select(t => t.Center.X).OrderBy(x => x)
                 .ShouldBe(xs);
        }
    }

    [Fact]
    public void Matrix_cache_rebuilds_only_on_cell_or_angle_change()
    {
        var glyph = CreateGlyph(new Size(40, 20), 30);
        var renderer = new WatermarkTileRenderer();
        var cell = new Size(40, 20);

        RecordTiles(renderer, new Size(200, 100), cell, glyph, glyph.Size);
        renderer.MatrixRebuildCount.ShouldBe(1);

        // Area, spacing, offset, opacity and mirroring do not take part in the key.
        RecordTiles(renderer, new Size(300, 150), cell, glyph, glyph.Size);
        glyph.HorizontalSpace  = 25;
        glyph.HorizontalOffset = 7;
        glyph.Opacity          = 0.5;
        glyph.IsMirrorUsed     = true;
        RecordTiles(renderer, new Size(300, 150), cell, glyph, glyph.Size);
        renderer.MatrixRebuildCount.ShouldBe(1);

        RecordTiles(renderer, new Size(300, 150), new Size(41, 20), glyph, glyph.Size);
        renderer.MatrixRebuildCount.ShouldBe(2);

        glyph.Rotate = 45;
        RecordTiles(renderer, new Size(300, 150), new Size(41, 20), glyph, glyph.Size);
        renderer.MatrixRebuildCount.ShouldBe(3);

        RecordTiles(renderer, new Size(300, 150), new Size(41, 20), glyph, glyph.Size);
        renderer.MatrixRebuildCount.ShouldBe(3);
    }

    [Fact]
    public void Text_glyph_font_properties_affect_layout()
    {
        var glyph = new ProbeTextGlyph { Text = "Watermark" };
        var initial = glyph.Formatted.ShouldNotBeNull();
        var baseline = glyph.GetDesiredSize();

        glyph.FontWeight = FontWeight.Bold;
        var afterWeight = glyph.Formatted.ShouldNotBeNull();
        afterWeight.ShouldNotBeSameAs(initial);

        glyph.FontStyle = FontStyle.Italic;
        var afterStyle = glyph.Formatted.ShouldNotBeNull();
        afterStyle.ShouldNotBeSameAs(afterWeight);

        glyph.FontFamily = new FontFamily("Courier New");
        var afterFamily = glyph.Formatted.ShouldNotBeNull();
        afterFamily.ShouldNotBeSameAs(afterStyle);

        // A rebuilt text keeps the size and brush the glyph currently has.
        glyph.FontSize = 32;
        glyph.GetDesiredSize().Height.ShouldBeGreaterThan(baseline.Height);
        glyph.FontSize = 16;
        glyph.FontFamily = FontFamily.Default;
        glyph.FontWeight = FontWeight.Normal;
        glyph.FontStyle  = FontStyle.Normal;
        glyph.GetDesiredSize().ShouldBe(baseline);
    }

    [Fact]
    public void Text_direction_is_left_to_right()
    {
        var glyph = new ProbeTextGlyph { Text = "abc 123" };
        glyph.Formatted.ShouldNotBeNull().FlowDirection.ShouldBe(FlowDirection.LeftToRight);

        glyph.FontWeight = FontWeight.Bold;
        glyph.Formatted.ShouldNotBeNull().FlowDirection.ShouldBe(FlowDirection.LeftToRight);
    }

    private sealed class WindowContext : IDisposable
    {
        private readonly AvaloniaWindow _window;

        internal WindowContext(Control content)
        {
            _window = new AvaloniaWindow { Width = 400, Height = 200, Content = content };
            _window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static Color ThemeText(Control owner)
    {
        var snapshot = ThemeScope.ResolveContext(owner).ShouldNotBeNull().Snapshot;
        return new ThemeTokenResolver().GetGlobal<Color>(snapshot, (int)SharedTokenKind.ColorText);
    }

    private static Color BrushColor(IBrush? brush)
    {
        return brush.ShouldBeAssignableTo<ISolidColorBrush>().ShouldNotBeNull().Color;
    }

    private static async Task ApplyAppearanceAsync(ThemeAppearance appearance)
    {
        var manager = ((IThemeManager?)AvaloniaLocator.Current.GetService(typeof(ThemeManager))).ShouldNotBeNull();
        var algorithms = appearance == ThemeAppearance.Dark
            ? new[] { ThemeAlgorithm.Default, ThemeAlgorithm.Dark }
            : new[] { ThemeAlgorithm.Default };
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(
                IThemeManager.DEFAULT_THEME_ID,
                new ThemeConfigBuilder().WithAlgorithms(algorithms).Build(),
                ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public async Task Foreground_defaults_to_color_text_and_follows_theme()
    {
        await ApplyAppearanceAsync(ThemeAppearance.Light);
        var owner = new Border();
        var glyph = new TextGlyph { Text = "mark" };
        using var context = new WindowContext(owner);

        // Without an owner there is no theme to read.
        BrushColor(glyph.Foreground).ShouldBe(Colors.Black);

        try
        {
            var attachment = glyph.AttachToResourceOwner(owner);
            var light = ThemeText(owner);
            BrushColor(glyph.Foreground).ShouldBe(light);

            // A switch while attached is followed.
            await ApplyAppearanceAsync(ThemeAppearance.Dark);
            var dark = ThemeText(owner);
            dark.ShouldNotBe(light);
            BrushColor(glyph.Foreground).ShouldBe(dark);

            // A switch while detached is picked up when the glyph is attached again.
            attachment.Dispose();
            await ApplyAppearanceAsync(ThemeAppearance.Light);
            using var again = glyph.AttachToResourceOwner(owner);
            BrushColor(glyph.Foreground).ShouldBe(light);

            // An explicit value wins over the theme and clearing it falls back to the theme again.
            glyph.Foreground = Brushes.Red;
            await ApplyAppearanceAsync(ThemeAppearance.Dark);
            glyph.Foreground.ShouldBeSameAs(Brushes.Red);
            glyph.ClearValue(TextGlyph.ForegroundProperty);
            BrushColor(glyph.Foreground).ShouldBe(dark);
        }
        finally
        {
            await ApplyAppearanceAsync(ThemeAppearance.Light);
        }
    }

    [Fact]
    public async Task Detached_glyph_stops_following_theme()
    {
        await ApplyAppearanceAsync(ThemeAppearance.Light);
        var owner = new Border();
        var glyph = new TextGlyph { Text = "mark" };
        using var context = new WindowContext(owner);

        try
        {
            var attachment = glyph.AttachToResourceOwner(owner);
            var light = BrushColor(glyph.Foreground);
            attachment.Dispose();

            await ApplyAppearanceAsync(ThemeAppearance.Dark);
            BrushColor(glyph.Foreground).ShouldBe(light);
        }
        finally
        {
            await ApplyAppearanceAsync(ThemeAppearance.Light);
        }
    }
}
