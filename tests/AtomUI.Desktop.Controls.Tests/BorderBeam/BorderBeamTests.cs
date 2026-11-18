using System.Reflection;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.BorderBeam;

public class BorderBeamTests
{
    static BorderBeamTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Default_Property_Values_Are_Stable()
    {
        var borderBeam = new AtomUI.Desktop.Controls.BorderBeam();

        borderBeam.Color.ShouldBeNull();
        borderBeam.ColorStops.ShouldBeEmpty();
        borderBeam.Outset.ShouldBeNull();
        borderBeam.BorderThickness.ShouldBe(default);
        borderBeam.CornerRadius.ShouldBe(default);
        borderBeam.IsMotionEnabled.ShouldBeTrue();
        borderBeam.Duration.ShouldBe(TimeSpan.FromSeconds(6));
        borderBeam.BeamSize.ShouldBe(100d);
        borderBeam.Count.ShouldBe(1);
    }

    [Fact]
    public void Color_Stop_Percent_Maps_To_Ant_Design_Visible_Segment()
    {
        var stops = new[]
        {
            new BorderBeamColorStop { Color = Colors.Blue, Percent = 0 },
            new BorderBeamColorStop { Color = Colors.Cyan, Percent = 50 },
            new BorderBeamColorStop { Color = Colors.Lime, Percent = 100 }
        };

        var normalized = BorderBeamColorStops.Normalize(stops, Colors.Transparent, Colors.Transparent, 70).ToArray();

        normalized.Select(stop => stop.Offset).ShouldBe(new[] { 0d, 0.35d, 0.70d, 1d });
        normalized.Select(stop => stop.Color).ShouldBe(new[]
        {
            Colors.Blue,
            Colors.Cyan,
            Colors.Lime,
            Colors.Transparent
        });
    }

    [Fact]
    public void Color_Stop_Normalization_Clamps_Input_Percent_And_Fills_Final_Stop()
    {
        var stops = new[]
        {
            new BorderBeamColorStop { Color = Colors.Red, Percent = -10 },
            new BorderBeamColorStop { Color = Colors.Orange, Percent = 40 },
            new BorderBeamColorStop { Color = Colors.Yellow, Percent = 120 }
        };

        var normalized = BorderBeamColorStops.Normalize(stops, Colors.Transparent, Colors.Transparent, 70).ToArray();

        normalized.Select(stop => stop.Offset).ShouldBe(new[] { 0d, 0.28d, 0.70d, 1d });
        normalized[^1].Color.ShouldBe(Colors.Transparent);
    }

    [Fact]
    public void Aware_Content_Geometry_Takes_Precedence_And_Refreshes_On_Notification()
    {
        var content = new TestBorderBeamAwareControl(
            new Thickness(3, 4, 5, 6),
            new CornerRadius(8, 9, 10, 11));
        var borderBeam = new AtomUI.Desktop.Controls.BorderBeam
        {
            Content         = content,
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(2)
        };

        borderBeam.EffectiveBorderBeamGeometry.ShouldBe(
            new BorderBeamGeometry(new Thickness(3, 4, 5, 6), new CornerRadius(8, 9, 10, 11)));

        content.UpdateGeometry(new Thickness(7), new CornerRadius(12));

        borderBeam.EffectiveBorderBeamGeometry.ShouldBe(
            new BorderBeamGeometry(new Thickness(7), new CornerRadius(12)));
    }

    [Fact]
    public void Own_Geometry_Is_Used_When_Content_Is_Not_Aware()
    {
        var borderBeam = new AtomUI.Desktop.Controls.BorderBeam
        {
            Content         = new Border(),
            BorderThickness = new Thickness(2),
            CornerRadius    = new CornerRadius(6)
        };

        borderBeam.EffectiveBorderBeamGeometry.ShouldBe(
            new BorderBeamGeometry(new Thickness(2), new CornerRadius(6)));
    }

    [Fact]
    public void Default_Theme_Applies_Token_Backed_Colors()
    {
        var borderBeam = new AtomUI.Desktop.Controls.BorderBeam
        {
            Width  = 120,
            Height = 60,
            Content = new Border
            {
                Width  = 120,
                Height = 60
            }
        };

        ShowInWindow(borderBeam, () =>
        {
            borderBeam.ApplyTemplate();
            borderBeam.DefaultStartColor.ShouldBeAssignableTo<ISolidColorBrush>();
            borderBeam.DefaultEndColor.ShouldBeAssignableTo<ISolidColorBrush>();
        });
    }

    [Fact]
    public void Default_Theme_Keeps_Beam_Enabled_When_Global_Motion_Is_Disabled()
    {
        var borderBeam = new AtomUI.Desktop.Controls.BorderBeam
        {
            Width  = 120,
            Height = 60,
            Content = new Border
            {
                Width  = 120,
                Height = 60
            }
        };
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder()
                     .WithToken(nameof(SharedTokenKind.EnableMotion), "false")
                     .Build(),
            Child = borderBeam
        };

        ShowInWindow(provider, () =>
        {
            var presenter = borderBeam.GetVisualDescendants()
                                      .OfType<BorderBeamPresenter>()
                                      .Single(item => item.Name == "PART_BeamPresenter");

            borderBeam.IsMotionEnabled.ShouldBeTrue();
            presenter.IsMotionEnabled.ShouldBeTrue();
            presenter.IsVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void Count_Is_Forwarded_To_The_Single_Beam_Presenter()
    {
        var borderBeam = new AtomUI.Desktop.Controls.BorderBeam
        {
            Width = 120,
            Height = 60,
            Count = 3,
            Content = new Border()
        };

        ShowInWindow(borderBeam, () =>
        {
            borderBeam.ApplyTemplate();
            var presenters = borderBeam.GetVisualDescendants()
                                       .OfType<BorderBeamPresenter>()
                                       .ToArray();

            presenters.Length.ShouldBe(1);
            presenters[0].Count.ShouldBe(3);
        });
    }

    private sealed class TestBorderBeamAwareControl : Control, IBorderBeamAwareControl
    {
        private BorderBeamGeometry _geometry;

        public TestBorderBeamAwareControl(Thickness borderThickness, CornerRadius cornerRadius)
        {
            _geometry = new BorderBeamGeometry(borderThickness, cornerRadius);
        }

        public event EventHandler? BorderBeamGeometryChanged;

        public BorderBeamGeometry GetBorderBeamGeometry() => _geometry;

        public void UpdateGeometry(Thickness borderThickness, CornerRadius cornerRadius)
        {
            _geometry = new BorderBeamGeometry(borderThickness, cornerRadius);
            BorderBeamGeometryChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 160,
            Content = content
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

    [Fact]
    public void Path_Sampler_Uses_Beam_Size_Rounded_Motion_Path()
    {
        var point = BorderBeamPathSampler.GetPointAtProgress(
            new Rect(0, 0, 420, 180),
            100d,
            0d);

        point.Point.ShouldBe(new Point(100, 0));
        point.Tangent.ShouldBe(new Vector(1, 0));
    }

    [Theory]
    [InlineData(0.90, 0, 3, 0.90)]
    [InlineData(0.90, 1, 3, 0.23333333333333334)]
    [InlineData(0.90, 2, 3, 0.5666666666666667)]
    [InlineData(0.25, 0, 0, 0.25)]
    [InlineData(0.25, 0, -2, 0.25)]
    public void Beam_Progress_Is_Evenly_Distributed_And_Normalized(
        double progress,
        int index,
        int count,
        double expected)
    {
        BorderBeamPathSampler.GetBeamProgress(progress, index, count)
                             .ShouldBe(expected, 0.000000000001d);
    }

    [Fact]
    public void Beam_Transform_Uses_Ant_Design_Offset_Anchor()
    {
        var pathPoint = new BorderBeamPathPoint(new Point(100, 0), new Vector(1, 0));
        var transform = BorderBeamPathSampler.CreateBeamTransform(pathPoint, 100d);

        transform.Transform(new Point(90, 50)).ShouldBe(pathPoint.Point);
        transform.Transform(new Point(100, 50)).ShouldBe(new Point(110, 0));
    }

    [Fact]
    public void Default_Outset_Keeps_Rendering_On_Content_Bounds_Without_Changing_Layout()
    {
        var presenter = new BorderBeamPresenter
        {
            BeamSize            = 40,
            IsMotionEnabled     = true,
            BorderBeamGeometry  = new BorderBeamGeometry(new Thickness(2), new CornerRadius(8))
        };
        presenter.Measure(new Size(160, 80));
        presenter.Arrange(new Rect(0, 0, 160, 80));

        var desiredSize = presenter.DesiredSize;
        var bounds      = presenter.Bounds;
        var renderBounds = InvokeRenderBounds(presenter);

        renderBounds.ShouldBe(new Rect(0, 0, 160, 80));
        presenter.DesiredSize.ShouldBe(desiredSize);
        presenter.Bounds.ShouldBe(bounds);
    }

    [Fact]
    public void Explicit_Outset_Only_Offsets_Rendering_Bounds()
    {
        var presenter = new BorderBeamPresenter
        {
            Outset             = new Thickness(4),
            BeamSize           = 40,
            IsMotionEnabled    = true,
            BorderBeamGeometry = new BorderBeamGeometry(new Thickness(2), new CornerRadius(8))
        };
        presenter.Measure(new Size(160, 80));
        presenter.Arrange(new Rect(0, 0, 160, 80));

        InvokeRenderBounds(presenter)
            .ShouldBe(new Rect(-4, -4, 168, 88));
    }

    [Theory]
    [InlineData(3, 3)]
    [InlineData(0, 1)]
    public void Presenter_Draws_One_Beam_Per_Effective_Count(int count, int expectedDrawingCount)
    {
        var presenter = new BorderBeamPresenter
        {
            Count = count,
            BeamSize = 40,
            IsMotionEnabled = true,
            BorderBeamGeometry = new BorderBeamGeometry(new Thickness(1), new CornerRadius(8))
        };
        presenter.Measure(new Size(160, 80));
        presenter.Arrange(new Rect(0, 0, 160, 80));

        var drawing = RenderToDrawingGroup(presenter);

        EnumerateGeometryDrawings(drawing).Count().ShouldBe(expectedDrawingCount);
    }

    [Fact]
    public void Path_Sampler_Keeps_Elliptical_Corner_Speed_Uniform()
    {
        var samples = new[]
        {
            0d,
            1d / 16d,
            2d / 16d,
            3d / 16d,
            4d / 16d
        }.Select(progress => BorderBeamPathSampler.GetPointAtProgress(
            new Rect(0, 0, 200, 80),
            100d,
            progress).Point).ToArray();
        var distances = samples
            .Zip(samples.Skip(1), (start, end) => GetDistance(start, end))
            .ToArray();

        (distances.Max() / distances.Min()).ShouldBeLessThan(1.5d);
    }

    [Theory]
    [InlineData(8d)]
    [InlineData(26d)]
    [InlineData(0d)]
    public void Path_Sampler_Advances_Continuously_Through_Corners(double cornerRadius)
    {
        // Uniform progress steps must map to a uniformly advancing position on every ring, including through the
        // corner arcs: a positional jump there reads as the beam teleporting around the corner.
        var metrics = BorderBeamPathSampler.CreateMetrics(new Rect(0, 0, 300, 140), cornerRadius);
        var previous = BorderBeamPathSampler.GetPointAtProgress(metrics, 0d).Point;
        var steps = new List<double>();
        const int count = 720;
        for (var i = 1; i <= count; i++)
        {
            var current = BorderBeamPathSampler.GetPointAtProgress(metrics, i / (double)count).Point;
            steps.Add(GetDistance(previous, current));
            previous = current;
        }

        steps.ShouldNotContain(double.NaN);
        steps.Min().ShouldBeGreaterThan(0d);
        // Arc-length parameterization: no step may cover MORE path than the uniform share - an oversized step is a
        // positional jump (the beam teleporting around a corner). A square corner (radius 0) legitimately shortens
        // the Euclidean chord of the one straddling step, so only the upper bound is asserted.
        var uniformStep = metrics.Perimeter / count;
        steps.Max().ShouldBeLessThanOrEqualTo(uniformStep * 1.01d, $"corner radius {cornerRadius}");
    }

    private static double GetDistance(Point start, Point end)
    {
        return new Vector(end.X - start.X, end.Y - start.Y).Length;
    }

    private static DrawingGroup RenderToDrawingGroup(Control control)
    {
        var drawingGroup = new DrawingGroup();
        using (var context = drawingGroup.Open())
        {
            control.Render(context);
        }

        return drawingGroup;
    }

    private static Rect InvokeRenderBounds(BorderBeamPresenter presenter)
    {
        var method = typeof(BorderBeamPresenter).GetMethod(
            "GetRenderBounds",
            BindingFlags.Instance | BindingFlags.NonPublic);

        method.ShouldNotBeNull();
        return (Rect)method!.Invoke(presenter, Array.Empty<object>())!;
    }

    private static IEnumerable<GeometryDrawing> EnumerateGeometryDrawings(Drawing drawing)
    {
        if (drawing is GeometryDrawing geometryDrawing)
        {
            yield return geometryDrawing;
        }
        else if (drawing is DrawingGroup drawingGroup)
        {
            foreach (var child in drawingGroup.Children.SelectMany(EnumerateGeometryDrawings))
            {
                yield return child;
            }
        }
    }
}
