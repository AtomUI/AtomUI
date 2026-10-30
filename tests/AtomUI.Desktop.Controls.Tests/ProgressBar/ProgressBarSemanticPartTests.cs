using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUICircleProgress = AtomUI.Desktop.Controls.CircleProgress;
using AtomUIDashboardProgress = AtomUI.Desktop.Controls.DashboardProgress;
using AtomUIProgressBar = AtomUI.Desktop.Controls.ProgressBar;
using AtomUIStepsProgressBar = AtomUI.Desktop.Controls.StepsProgressBar;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ProgressBar;

public class ProgressBarSemanticPartTests
{
    private const string BodyClass = "semantic-body";
    private const string RailClass = "semantic-rail";
    private const string TrackClass = "semantic-track";
    private const string IndicatorClass = "semantic-indicator";
    private const string ProgressThemePath =
        "src/AtomUI.Desktop.Controls/ProgressBar/Themes/ProgressBarTheme.axaml";
    private const string StepsThemePath =
        "src/AtomUI.Desktop.Controls/ProgressBar/Themes/StepsProgressBarTheme.axaml";
    private const string AbstractCircleThemePath =
        "src/AtomUI.Desktop.Controls/ProgressBar/Themes/AbstractCircleProgressTheme.axaml";
    private const string CircleThemePath =
        "src/AtomUI.Desktop.Controls/ProgressBar/Themes/CircleProgressTheme.axaml";
    private const string DashboardThemePath =
        "src/AtomUI.Desktop.Controls/ProgressBar/Themes/DashboardProgressTheme.axaml";

    static ProgressBarSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(typeof(AtomUIProgressBar), true, 5)]
    [InlineData(typeof(AtomUIStepsProgressBar), false, 4)]
    [InlineData(typeof(AtomUICircleProgress), true, 5)]
    [InlineData(typeof(AtomUIDashboardProgress), true, 5)]
    public void Registered_Descriptors_Expose_The_Approved_Progress_Parts(
        Type ownerType,
        bool hasRail,
        int expectedPartCount)
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(ownerType, out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Count.ShouldBe(expectedPartCount);

        AssertRoot(descriptor, ownerType);
        AssertPart(descriptor, ownerType, "body", BodyClass, typeof(Panel));
        AssertPart(descriptor, ownerType, "indicator", IndicatorClass, typeof(Panel),
            selectorRoute: "/template/ .semantic-body > .semantic-indicator");
        AssertPart(descriptor, ownerType, "track", TrackClass,
            ownerType == typeof(AtomUIProgressBar)
                ? typeof(Border)
                : ownerType == typeof(AtomUIStepsProgressBar)
                    ? typeof(Rectangle)
                : typeof(Shape),
            ownerType == typeof(AtomUIStepsProgressBar)
                ? SemanticPartCardinality.Multiple
                : SemanticPartCardinality.Single,
            runtimeCreated: ownerType == typeof(AtomUIStepsProgressBar),
            selectorRoute: "/template/ .semantic-body > .semantic-track");

        if (hasRail)
        {
            AssertPart(descriptor, ownerType, "rail", RailClass,
                ownerType == typeof(AtomUIProgressBar) ? typeof(Border) : typeof(Shape),
                selectorRoute: "/template/ .semantic-body > .semantic-rail");
        }
        else
        {
            descriptor.Parts.ShouldNotContain(static part => part.Name == "rail");
        }
    }

    [Theory]
    [InlineData(ProgressThemePath, 4)]
    [InlineData(StepsThemePath, 2)]
    [InlineData(CircleThemePath, 4)]
    [InlineData(DashboardThemePath, 4)]
    public void Built_In_Templates_Declare_Only_The_Approved_Static_Markers(
        string relativePath,
        int expectedMarkerCount)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
        var markers = document.Descendants()
                              .Attributes()
                              .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                  "Classes.semantic-",
                                  StringComparison.Ordinal))
                              .ToArray();

        markers.Length.ShouldBe(expectedMarkerCount);
        markers.ShouldAllBe(static marker =>
            string.Equals(marker.Value, "true", StringComparison.OrdinalIgnoreCase));
        markers.ShouldNotContain(static marker => marker.Name.LocalName == "Classes.semantic-root");

        var markerNames = markers.Select(static marker => marker.Name.LocalName).ToArray();
        markerNames.ShouldContain("Classes.semantic-body");
        markerNames.ShouldContain("Classes.semantic-indicator");

        if (relativePath == StepsThemePath)
        {
            markerNames.ShouldNotContain("Classes.semantic-rail");
            markerNames.ShouldNotContain("Classes.semantic-track");
        }
        else
        {
            markerNames.ShouldContain("Classes.semantic-rail");
            markerNames.ShouldContain("Classes.semantic-track");
        }
    }

    [Theory]
    [InlineData(ProgressThemePath)]
    [InlineData(StepsThemePath)]
    [InlineData(CircleThemePath)]
    [InlineData(DashboardThemePath)]
    public void Built_In_Themes_Do_Not_Consume_Semantic_Selectors(string relativePath)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);

        document.Descendants()
                .Where(static element => element.Name.LocalName == "Style")
                .Select(static element => (string?)element.Attribute("Selector"))
                .Where(static selector => selector?.Contains(".semantic-", StringComparison.Ordinal) == true)
                .ShouldBeEmpty();
    }

    [Fact]
    public void Circle_And_Dashboard_Own_Concrete_Leaf_Templates()
    {
        var abstractTheme = File.ReadAllText(GetRepoFile(AbstractCircleThemePath));
        abstractTheme.ShouldNotContain("<Setter Property=\"Template\">");

        var circleTheme = File.ReadAllText(GetRepoFile(CircleThemePath));
        circleTheme.ShouldContain("<ControlTheme x:Key=\"{x:Type atom:CircleProgress}\"");
        circleTheme.ShouldContain("<ControlTemplate TargetType=\"atom:CircleProgress\">");

        var dashboardTheme = File.ReadAllText(GetRepoFile(DashboardThemePath));
        dashboardTheme.ShouldContain("<ControlTheme x:Key=\"{x:Type atom:DashboardProgress}\"");
        dashboardTheme.ShouldContain("<ControlTemplate TargetType=\"atom:DashboardProgress\">");
    }

    [Fact]
    public void Static_Progress_Owners_Expose_One_Real_Target_Per_Declared_Part()
    {
        Control[] owners =
        [
            new AtomUIProgressBar { Value = 80 },
            new AtomUICircleProgress { Value = 80 },
            new AtomUIDashboardProgress { Value = 80 }
        ];

        foreach (var owner in owners)
        {
            var window = Show(owner);
            try
            {
                var body = FindSemanticControls<Panel>(owner, BodyClass).ShouldHaveSingleItem();
                var indicator = FindSemanticControls<Panel>(owner, IndicatorClass).ShouldHaveSingleItem();

                if (owner is AtomUIProgressBar)
                {
                    FindSemanticControls<Border>(owner, RailClass)
                        .ShouldHaveSingleItem()
                        .Parent.ShouldBeSameAs(body);
                    FindSemanticControls<Border>(owner, TrackClass)
                        .ShouldHaveSingleItem()
                        .Parent.ShouldBeSameAs(body);
                }
                else
                {
                    FindSemanticControls<Shape>(owner, RailClass)
                        .ShouldHaveSingleItem()
                        .Parent.ShouldBeSameAs(body);
                    FindSemanticControls<Shape>(owner, TrackClass)
                        .ShouldHaveSingleItem()
                        .Parent.ShouldBeSameAs(body);
                }

                indicator.Parent.ShouldBeSameAs(body);
            }
            finally
            {
                window.Close();
            }
        }
    }

    [Theory]
    [InlineData(typeof(AtomUIProgressBar))]
    [InlineData(typeof(AtomUIStepsProgressBar))]
    [InlineData(typeof(AtomUICircleProgress))]
    [InlineData(typeof(AtomUIDashboardProgress))]
    public void Indicator_Keeps_Real_Bounds_When_Text_Changes_To_A_Status_Icon(Type ownerType)
    {
        var owner = (Control)Activator.CreateInstance(ownerType).ShouldNotBeNull();
        if (owner is AtomUIStepsProgressBar steps)
        {
            steps.Steps = 5;
        }

        ((RangeBase)owner).Value = 80;
        var window = Show(owner);
        try
        {
            var body = FindSemanticControls<Panel>(owner, BodyClass).ShouldHaveSingleItem();
            var indicator = FindSemanticControls<Panel>(owner, IndicatorClass).ShouldHaveSingleItem();
            var statusIcon = owner.GetVisualDescendants()
                                  .OfType<IconPresenter>()
                                  .Single(static control => control.Name == "PART_ExceptionCompletedIconPresenter");

            statusIcon.IsEffectivelyVisible.ShouldBeFalse();
            AssertEligibleHighlightTarget(indicator);
            indicator.Bounds.Size.ShouldNotBe(body.Bounds.Size);

            owner.SetValue(AtomUI.Controls.Commons.AbstractProgressBar.StatusProperty, ProgressStatus.Exception);
            Dispatcher.UIThread.RunJobs();

            FindSemanticControls<Panel>(owner, IndicatorClass).ShouldHaveSingleItem().ShouldBeSameAs(indicator);
            statusIcon.IsEffectivelyVisible.ShouldBeTrue();
            AssertEligibleHighlightTarget(indicator);
            indicator.Bounds.Size.ShouldNotBe(body.Bounds.Size);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Steps_Expose_Only_Multiple_Runtime_Track_Targets()
    {
        var progress = new AtomUIStepsProgressBar
        {
            Steps = 5,
            Value = 40
        };
        var window = Show(progress);
        try
        {
            var body = FindSemanticControls<Panel>(progress, BodyClass).ShouldHaveSingleItem();
            FindSemanticControls<Shape>(progress, RailClass).ShouldBeEmpty();
            var tracks = FindSemanticControls<Rectangle>(progress, TrackClass);
            tracks.Count.ShouldBe(5);
            tracks.ShouldAllBe(track => ReferenceEquals(track.Parent, body));

            progress.Value = 80;
            Dispatcher.UIThread.RunJobs();
            FindSemanticControls<Rectangle>(progress, TrackClass).ShouldBe(tracks);

            progress.Steps = 3;
            Dispatcher.UIThread.RunJobs();
            FindSemanticControls<Rectangle>(progress, TrackClass).Count.ShouldBe(3);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Steps_Indicator_Uses_The_Correct_First_Layout_When_Added_To_An_Attached_Tree()
    {
        var horizontalHost = new StackPanel { Spacing = 12 };
        var verticalHost = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 12,
            Height = 240
        };
        var root = new StackPanel
        {
            Spacing = 24,
            Children = { horizontalHost, verticalHost }
        };
        var window = Show(root, 800, 600);
        try
        {
            foreach (var position in Enum.GetValues<LinePercentAlignment>())
            {
                horizontalHost.Children.Add(new AtomUIStepsProgressBar
                {
                    Steps = 8,
                    Value = 80,
                    PercentPosition = position
                });
                verticalHost.Children.Add(new AtomUIStepsProgressBar
                {
                    Steps = 8,
                    Value = 80,
                    Orientation = Avalonia.Layout.Orientation.Vertical,
                    PercentPosition = position
                });
            }

            Dispatcher.UIThread.RunJobs();

            foreach (var progress in horizontalHost.Children.Cast<AtomUIStepsProgressBar>())
            {
                AssertStepsIndicatorPlacement(progress);
                AssertStepsIndicatorContent(progress, "80%");
            }

            foreach (var progress in verticalHost.Children.Cast<AtomUIStepsProgressBar>())
            {
                AssertStepsIndicatorPlacement(progress);
                AssertStepsIndicatorContent(progress, "80%");
            }

            var implementation = File.ReadAllText(GetRepoFile(
                "src/AtomUI.Controls/ProgressBar/AbstractGeneralStepsProgressBar.cs"));
            implementation.ShouldNotContain("protected override Size ArrangeOverride");
            implementation.ShouldNotContain("ArrangeProgressIndicator(");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(typeof(AtomUIProgressBar))]
    [InlineData(typeof(AtomUIStepsProgressBar))]
    [InlineData(typeof(AtomUICircleProgress))]
    [InlineData(typeof(AtomUIDashboardProgress))]
    public void Generated_Semantic_Styles_Apply_To_All_Real_Targets(Type ownerType)
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(ownerType, out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var owner = (Control)Activator.CreateInstance(ownerType).ShouldNotBeNull();
        if (owner is AtomUIStepsProgressBar steps)
        {
            steps.Steps = 5;
        }
        owner.Classes.Add("semantic-style-test");
        var ownerStyle = new Style(selector => selector.OfType(ownerType).Class("semantic-style-test"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var style = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            style.Setters.Add(new Setter(Control.TagProperty, $"styled-{part.Name}"));
            ownerStyle.Children.Add(style);
        }
        owner.Styles.Add(ownerStyle);

        var window = Show(owner);
        try
        {
            foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
            {
                FindSemanticControls<Control>(owner, part.SelectorClass.ShouldNotBeNull())
                    .ShouldAllBe(control => Equals(control.Tag, $"styled-{part.Name}"));
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Line_Semantic_Rail_And_Track_Are_The_Visible_Rendering_Targets()
    {
        var railBrush = new SolidColorBrush(Color.Parse("#1A000000"));
        var trackBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.Parse("#46D8E8"), 0),
                new GradientStop(Color.Parse("#2F86F6"), 1)
            }
        };
        var progress = new AtomUIProgressBar
        {
            Width = 300,
            Height = 24,
            Value = 40
        };
        progress.Classes.Add("semantic-render-test");

        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUIProgressBar), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var ownerStyle = new Style(
            selector => selector.OfType<AtomUIProgressBar>().Class("semantic-render-test"));
        AddPartBackgroundStyle(ownerStyle, descriptor, "rail", railBrush);
        AddPartBackgroundStyle(ownerStyle, descriptor, "track", trackBrush);
        progress.Styles.Add(ownerStyle);

        var window = Show(progress, 360, 120);
        try
        {
            var rail = FindSemanticControls<Border>(progress, RailClass).ShouldHaveSingleItem();
            var track = FindSemanticControls<Border>(progress, TrackClass).ShouldHaveSingleItem();

            rail.Background.ShouldBeSameAs(railBrush);
            track.Background.ShouldBeSameAs(trackBrush);
            AssertEligibleHighlightTarget(rail);
            AssertEligibleHighlightTarget(track);
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(typeof(AtomUICircleProgress))]
    [InlineData(typeof(AtomUIDashboardProgress))]
    public void Circle_Semantic_Track_Stroke_Drives_The_Visible_Path(Type ownerType)
    {
        var trackBrush = new SolidColorBrush(Color.Parse("#2F86F6"));
        var owner = (Control)Activator.CreateInstance(ownerType).ShouldNotBeNull();
        owner.Width = 120;
        owner.Height = 120;
        ((RangeBase)owner).Value = 75;
        owner.Classes.Add("semantic-render-test");

        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(ownerType, out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var ownerStyle = new Style(selector => selector.OfType(ownerType).Class("semantic-render-test"));
        AddPartStrokeStyle(ownerStyle, descriptor, "track", trackBrush);
        owner.Styles.Add(ownerStyle);

        var window = Show(owner, 180, 180);
        try
        {
            var track = FindSemanticControls<Shape>(owner, TrackClass).ShouldHaveSingleItem();
            track.Stroke.ShouldBeSameAs(trackBrush);
            AssertEligibleHighlightTarget(track);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Circle_And_Dashboard_Semantic_Shapes_Keep_The_Visible_Stroke_Thickness()
    {
        Control[] owners =
        [
            new AtomUICircleProgress { Width = 120, Height = 120, Value = 75 },
            new AtomUIDashboardProgress { Width = 120, Height = 120, Value = 75 }
        ];

        foreach (var owner in owners)
        {
            var window = Show(owner, 180, 180);
            try
            {
                var rail = FindSemanticControls<Shape>(owner, RailClass).ShouldHaveSingleItem();
                var track = FindSemanticControls<Shape>(owner, TrackClass).ShouldHaveSingleItem();

                rail.StrokeThickness.ShouldBe(8, 0.001);
                track.StrokeThickness.ShouldBe(8, 0.001);
                AssertEligibleHighlightTarget(rail);
                AssertEligibleHighlightTarget(track);
            }
            finally
            {
                window.Close();
            }
        }
    }

    [Fact]
    public void Retemplate_Replaces_Steps_Runtime_Tracks_Without_Retaining_Old_Parents()
    {
        var progress = new AtomUIStepsProgressBar { Steps = 4, Value = 50 };
        var window = Show(progress);
        try
        {
            var oldTracks = FindSemanticControls<Rectangle>(progress, TrackClass);
            oldTracks.Count.ShouldBe(4);

            ReapplyDefaultTemplate(progress);

            var newTracks = FindSemanticControls<Rectangle>(progress, TrackClass);
            newTracks.Count.ShouldBe(4);
            newTracks.ShouldNotBe(oldTracks);
            oldTracks.ShouldAllBe(static track => track.Parent == null);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Line_Template_Visuals_Stay_Inside_Their_Gallery_Like_Layout_Bounds()
    {
        var horizontal = new AtomUIProgressBar
        {
            Value = 50,
            Width = 600
        };
        var horizontalInner = new AtomUIProgressBar
        {
            Value = 55,
            Width = 600,
            PercentPosition = new PercentPosition
            {
                IsInner = true,
                Alignment = LinePercentAlignment.End
            }
        };
        var horizontalSuccess = new AtomUIProgressBar
        {
            Value = 60,
            Width = 600,
            SuccessThreshold = 30
        };
        var horizontalException = new AtomUIProgressBar
        {
            Value = 70,
            Width = 600,
            Status = ProgressStatus.Exception,
            StrokeLineCap = PenLineCap.Flat
        };
        var horizontalWithoutIndicator = new AtomUIProgressBar
        {
            Value = 50,
            Width = 600,
            IsProgressInfoVisible = false
        };
        var vertical = new AtomUIProgressBar
        {
            Value = 55,
            Width = 48,
            Height = 240,
            Orientation = Avalonia.Layout.Orientation.Vertical
        };
        var verticalInner = new AtomUIProgressBar
        {
            Value = 55,
            Width = 48,
            Height = 240,
            Orientation = Avalonia.Layout.Orientation.Vertical,
            PercentPosition = new PercentPosition
            {
                IsInner = true,
                Alignment = LinePercentAlignment.Center
            }
        };
        var host = new StackPanel
        {
            Width = 640,
            Spacing = 10,
            Children =
            {
                horizontal,
                horizontalInner,
                horizontalSuccess,
                horizontalException,
                horizontalWithoutIndicator,
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    Spacing = 16,
                    Children = { vertical, verticalInner }
                }
            }
        };

        var window = Show(host, 720, 620);
        try
        {
            AssertLineVisualBounds(horizontal);
            AssertLineVisualBounds(horizontalInner);
            AssertLineVisualBounds(horizontalSuccess);
            AssertLineVisualBounds(horizontalException);
            AssertLineVisualBounds(horizontalWithoutIndicator);
            AssertLineVisualBounds(vertical);
            AssertLineVisualBounds(verticalInner);

            horizontal.Width = 360;
            horizontalInner.Width = 360;
            horizontalSuccess.Width = 360;
            horizontalException.Width = 360;
            horizontalWithoutIndicator.Width = 360;
            vertical.Height = 160;
            verticalInner.Height = 160;
            Dispatcher.UIThread.RunJobs();

            AssertLineVisualBounds(horizontal);
            AssertLineVisualBounds(horizontalInner);
            AssertLineVisualBounds(horizontalSuccess);
            AssertLineVisualBounds(horizontalException);
            AssertLineVisualBounds(horizontalWithoutIndicator);
            AssertLineVisualBounds(vertical);
            AssertLineVisualBounds(verticalInner);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Line_Template_Uses_Box_Visuals_With_Exact_Progress_Bounds()
    {
        var trackColor = Color.FromRgb(24, 144, 255);
        var railColor = Color.FromRgb(224, 224, 224);
        var progress = new AtomUIProgressBar
        {
            Width = 300,
            Height = 20,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Value = 50,
            IsProgressInfoVisible = false,
            IndicatorThickness = 8,
            StrokeLineCap = PenLineCap.Flat,
            StrokeBrush = new SolidColorBrush(trackColor),
            TrailColor = railColor
        };

        var window = Show(progress, 340, 80);
        try
        {
            var rail = FindSemanticControls<Border>(progress, RailClass).ShouldHaveSingleItem();
            var track = FindSemanticControls<Border>(progress, TrackClass).ShouldHaveSingleItem();
            rail.Bounds.ShouldBe(new Rect(0, 6, 300, 8));
            track.Bounds.ShouldBe(new Rect(0, 6, 150, 8));
            rail.Background.ShouldBeOfType<SolidColorBrush>().Color.ShouldBe(railColor);
            track.Background.ShouldBeOfType<SolidColorBrush>().Color.ShouldBe(trackColor);
            progress.GetVisualDescendants()
                    .OfType<Shape>()
                    .ShouldBeEmpty();

            var theme = File.ReadAllText(GetRepoFile(ProgressThemePath));
            theme.ShouldNotContain("<Rectangle");
            theme.ShouldNotContain("Stretch=");

            var implementation = File.ReadAllText(GetRepoFile(
                "src/AtomUI.Controls/ProgressBar/AbstractGeneralProgressBar.cs"));
            implementation.ShouldNotContain(".Measure(");
            implementation.ShouldNotContain(".Arrange(");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Circle_And_Dashboard_Template_Arcs_Have_Visible_Arranged_Bounds()
    {
        var circle = new AtomUICircleProgress { Value = 75 };
        var dashboard = new AtomUIDashboardProgress { Value = 70 };
        var host = new WrapPanel
        {
            Width = 640,
            Children = { circle, dashboard }
        };

        var window = Show(host, 720, 360);
        try
        {
            AssertCircleVisualBounds(circle);
            AssertCircleVisualBounds(dashboard);

            circle.Width = 60;
            circle.Height = 60;
            dashboard.Width = 90;
            dashboard.Height = 90;
            Dispatcher.UIThread.RunJobs();

            AssertCircleVisualBounds(circle);
            AssertCircleVisualBounds(dashboard);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Steps_Template_Tracks_And_Indicator_Stay_Inside_The_Body()
    {
        var horizontal = new AtomUIStepsProgressBar
        {
            Value = 55,
            Steps = 8
        };
        var vertical = new AtomUIStepsProgressBar
        {
            Value = 55,
            Steps = 6,
            Height = 240,
            Orientation = Avalonia.Layout.Orientation.Vertical
        };
        var host = new StackPanel
        {
            Width = 640,
            Spacing = 16,
            Children = { horizontal, vertical }
        };

        var window = Show(host, 720, 520);
        try
        {
            AssertStepsVisualBounds(horizontal);
            AssertStepsVisualBounds(vertical);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Legacy_Custom_Template_Without_Progress_Shapes_Uses_Direct_Rendering_Fallback()
    {
        var progress = new RecordingProgressBar
        {
            Width = 300,
            Height = 20,
            Value = 50,
            Template = new FuncControlTemplate<RecordingProgressBar>((_, _) => new Canvas())
        };

        var window = Show(progress, 360, 120);
        try
        {
            using var bitmap = new RenderTargetBitmap(new PixelSize(300, 20), new Vector(96, 96));
            bitmap.Render(progress);

            progress.GrooveRenderCount.ShouldBeGreaterThan(0);
            progress.TrackRenderCount.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Built_In_Template_Uses_Shapes_Without_Direct_Rendering_Fallback()
    {
        var progress = new RecordingProgressBar
        {
            Width = 300,
            Height = 20,
            Value = 50
        };

        var window = Show(progress, 360, 120);
        try
        {
            using var bitmap = new RenderTargetBitmap(new PixelSize(300, 20), new Vector(96, 96));
            bitmap.Render(progress);

            progress.GetVisualDescendants()
                    .OfType<Border>()
                    .Count(static border => border.Name is "PART_ProgressRail" or "PART_ProgressTrack")
                    .ShouldBe(2);
            progress.GrooveRenderCount.ShouldBe(0);
            progress.TrackRenderCount.ShouldBe(0);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor, Type ownerType)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(ownerType);
        root.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        root.Customization.ShouldBe(SemanticPartCustomization.Root);
        root.StyleType.ShouldBeNull();
        root.CrossVisualRoot.ShouldBeFalse();
        root.RuntimeCreated.ShouldBeFalse();
        root.Since.ShouldBe("6.2.0");
    }

    private static void AddPartBackgroundStyle(
        Style ownerStyle,
        ControlSemanticDescriptor descriptor,
        string partName,
        IBrush brush)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == partName);
        var style = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
        style.Setters.Add(new Setter(Border.BackgroundProperty, brush));
        ownerStyle.Children.Add(style);
    }

    private static void AddPartStrokeStyle(
        Style ownerStyle,
        ControlSemanticDescriptor descriptor,
        string partName,
        IBrush brush)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == partName);
        var style = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
        style.Setters.Add(new Setter(Shape.StrokeProperty, brush));
        ownerStyle.Children.Add(style);
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        Type ownerType,
        string name,
        string selectorClass,
        Type contractType,
        SemanticPartCardinality cardinality = SemanticPartCardinality.Single,
        bool runtimeCreated = false,
        string? selectorRoute = null)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute ?? $"/template/ .{selectorClass}");
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.StyleType.ShouldBe(ownerType.Assembly.GetType(
            $"AtomUI.Theme.Styling.{ownerType.Name}{char.ToUpperInvariant(name[0])}{name[1..]}Style"));
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBe(runtimeCreated);
        part.Since.ShouldBe("6.2.0");
    }

    private static AvaloniaWindow Show(Control control)
    {
        return Show(control, 560, 280);
    }

    private static AvaloniaWindow Show(Control control, double width, double height)
    {
        var window = new AvaloniaWindow
        {
            Width = width,
            Height = height,
            Content = control
        };
        window.Show();
        control.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void AssertLineVisualBounds(AtomUIProgressBar owner)
    {
        var body = FindSemanticControls<Panel>(owner, BodyClass).ShouldHaveSingleItem();
        var rail = FindSemanticControls<Border>(owner, RailClass).ShouldHaveSingleItem();
        var track = FindSemanticControls<Border>(owner, TrackClass).ShouldHaveSingleItem();
        var indicator = FindSemanticControls<Panel>(owner, IndicatorClass).ShouldHaveSingleItem();

        body.Bounds.Size.ShouldBe(owner.Bounds.Size);
        AssertEligibleHighlightTarget(body);
        AssertInside(body.Bounds.Size, rail.Bounds);
        AssertInside(body.Bounds.Size, track.Bounds);
        indicator.Name.ShouldBe("PART_ProgressIndicator");
        AssertEligibleHighlightTarget(rail);
        AssertEligibleHighlightTarget(track);

        var success = owner.GetVisualDescendants()
                           .OfType<Border>()
                           .Single(static border => border.Name == "PART_ProgressSuccess");

        if (owner.Orientation == Avalonia.Layout.Orientation.Horizontal)
        {
            rail.Bounds.Height.ShouldBeGreaterThan(0);
            track.Bounds.Height.ShouldBe(rail.Bounds.Height, 0.001);
            track.Bounds.Width.ShouldBe(rail.Bounds.Width * owner.Percentage / 100, 1);
        }
        else
        {
            rail.Bounds.Width.ShouldBeGreaterThan(0);
            track.Bounds.Width.ShouldBe(rail.Bounds.Width, 0.001);
            track.Bounds.Height.ShouldBe(rail.Bounds.Height * owner.Percentage / 100, 1);
        }

        if (!owner.IsProgressInfoVisible)
        {
            indicator.IsEffectivelyVisible.ShouldBeFalse();
            return;
        }

        AssertEligibleHighlightTarget(indicator);
        indicator.Bounds.Width.ShouldBeLessThan(body.Bounds.Width);
        indicator.Bounds.Height.ShouldBeLessThan(body.Bounds.Height);
        if (owner.Orientation == Avalonia.Layout.Orientation.Horizontal &&
            !owner.PercentPosition.IsInner &&
            owner.PercentPosition.Alignment == LinePercentAlignment.End)
        {
            indicator.Bounds.Right.ShouldBe(body.Bounds.Width, 0.001);
            rail.Bounds.Right.ShouldBeLessThan(indicator.Bounds.Left);
        }
    }

    private static void AssertCircleVisualBounds(Control owner)
    {
        var body = FindSemanticControls<Panel>(owner, BodyClass).ShouldHaveSingleItem();
        var rail = FindSemanticControls<Shape>(owner, RailClass).ShouldHaveSingleItem();
        var track = FindSemanticControls<Shape>(owner, TrackClass).ShouldHaveSingleItem();
        var indicator = FindSemanticControls<Panel>(owner, IndicatorClass).ShouldHaveSingleItem();

        body.Bounds.Size.ShouldBe(owner.Bounds.Size);
        AssertEligibleHighlightTarget(body);
        AssertInside(body.Bounds.Size, rail.Bounds);
        AssertInside(body.Bounds.Size, track.Bounds);
        rail.Stroke.ShouldNotBeNull();
        track.Stroke.ShouldNotBeNull();
        ((Avalonia.Controls.Shapes.Path)rail).Data.ShouldNotBeNull();
        ((Avalonia.Controls.Shapes.Path)track).Data.ShouldNotBeNull();
        rail.Bounds.Width.ShouldBeGreaterThan(0);
        rail.Bounds.Height.ShouldBeGreaterThan(0);
        track.Bounds.Width.ShouldBeGreaterThan(0);
        track.Bounds.Height.ShouldBeGreaterThan(0);
        var expectedPathSize = new Size(
            Math.Floor(Math.Max(0, body.Bounds.Width - rail.StrokeThickness)),
            Math.Floor(Math.Max(0, body.Bounds.Height - rail.StrokeThickness)));
        rail.Bounds.Size.ShouldBe(expectedPathSize);
        track.Bounds.Size.ShouldBe(expectedPathSize);
        rail.Bounds.Width.ShouldBeLessThan(body.Bounds.Width);
        rail.Bounds.Height.ShouldBeLessThan(body.Bounds.Height);
        rail.Bounds.Center.ShouldBe(body.Bounds.Center);
        track.Bounds.Center.ShouldBe(body.Bounds.Center);
        indicator.Name.ShouldBe("PART_ProgressIndicator");
        AssertEligibleHighlightTarget(rail);
        AssertEligibleHighlightTarget(track);
        AssertEligibleHighlightTarget(indicator);
        indicator.Bounds.Width.ShouldBe(body.Bounds.Width, 0.001);
        indicator.Bounds.Height.ShouldBeGreaterThan(0);
        indicator.Bounds.Height.ShouldBeLessThan(body.Bounds.Height);
        (indicator.Bounds.Center.Y - body.Bounds.Center.Y).ShouldBe(0, 1);
    }

    private static void AssertStepsVisualBounds(AtomUIStepsProgressBar owner)
    {
        var body = FindSemanticControls<Panel>(owner, BodyClass).ShouldHaveSingleItem();
        var tracks = FindSemanticControls<Rectangle>(owner, TrackClass);
        var indicator = FindSemanticControls<Panel>(owner, IndicatorClass).ShouldHaveSingleItem();

        body.Bounds.Size.ShouldBe(owner.Bounds.Size);
        AssertEligibleHighlightTarget(body);
        tracks.Count.ShouldBe(owner.Steps);
        tracks.ShouldAllBe(track => IsInside(body.Bounds.Size, track.Bounds));
        foreach (var track in tracks)
        {
            AssertEligibleHighlightTarget(track);
            AssertRenderedGeometryMatchesBounds(track);
        }
        AssertEligibleHighlightTarget(indicator);
        indicator.Bounds.Size.ShouldNotBe(body.Bounds.Size);
    }

    private static void AssertStepsIndicatorPlacement(AtomUIStepsProgressBar owner)
    {
        const double tolerance = 0.001;
        var tracks = FindSemanticControls<Rectangle>(owner, TrackClass);
        var indicator = FindSemanticControls<Panel>(owner, IndicatorClass).ShouldHaveSingleItem();

        AssertEligibleHighlightTarget(indicator);
        if (owner.Orientation == Avalonia.Layout.Orientation.Horizontal)
        {
            var trackLeft = tracks.Min(static track => track.Bounds.Left);
            var trackRight = tracks.Max(static track => track.Bounds.Right);
            var trackBottom = tracks.Max(static track => track.Bounds.Bottom);
            if (owner.PercentPosition == LinePercentAlignment.Start)
            {
                indicator.Bounds.Right.ShouldBeLessThanOrEqualTo(trackLeft + tolerance);
            }
            else if (owner.PercentPosition == LinePercentAlignment.Center)
            {
                indicator.Bounds.Top.ShouldBeGreaterThanOrEqualTo(trackBottom - tolerance);
            }
            else
            {
                indicator.Bounds.Left.ShouldBeGreaterThanOrEqualTo(trackRight - tolerance);
            }
        }
        else
        {
            var trackTop = tracks.Min(static track => track.Bounds.Top);
            var trackRight = tracks.Max(static track => track.Bounds.Right);
            var trackBottom = tracks.Max(static track => track.Bounds.Bottom);
            if (owner.PercentPosition == LinePercentAlignment.Start)
            {
                indicator.Bounds.Bottom.ShouldBeLessThanOrEqualTo(trackTop + tolerance);
            }
            else if (owner.PercentPosition == LinePercentAlignment.Center)
            {
                indicator.Bounds.Left.ShouldBeGreaterThanOrEqualTo(trackRight - tolerance);
            }
            else
            {
                indicator.Bounds.Top.ShouldBeGreaterThanOrEqualTo(trackBottom - tolerance);
            }
        }
    }

    private static void AssertStepsIndicatorContent(AtomUIStepsProgressBar owner, string expectedText)
    {
        var indicator = FindSemanticControls<Panel>(owner, IndicatorClass).ShouldHaveSingleItem();
        var label = indicator.GetVisualDescendants()
                             .OfType<Label>()
                             .Single(static candidate => candidate.Name == "PART_PercentageLabel");

        label.Content.ShouldBe(expectedText);
        label.IsVisible.ShouldBeTrue();
        label.Bounds.Width.ShouldBeGreaterThan(0);
        label.Bounds.Height.ShouldBeGreaterThan(0);

        var labelForeground = label.Foreground.ShouldNotBeNull().ShouldBeAssignableTo<ISolidColorBrush>();
        var ownerForeground = owner.Foreground.ShouldNotBeNull().ShouldBeAssignableTo<ISolidColorBrush>();
        labelForeground.Color.ShouldBe(ownerForeground.Color);
        labelForeground.Color.A.ShouldBeGreaterThan((byte)0);

        var labelOrigin = label.TranslatePoint(default, owner).ShouldNotBeNull();
        var labelRect = new Rect(labelOrigin, label.Bounds.Size);
        labelRect.Right.ShouldBeGreaterThan(0);
        labelRect.Bottom.ShouldBeGreaterThan(0);
        labelRect.Left.ShouldBeLessThan(owner.Bounds.Width);
        labelRect.Top.ShouldBeLessThan(owner.Bounds.Height);
    }

    private static void AssertInside(Size outerSize, Rect innerBounds)
    {
        IsInside(outerSize, innerBounds).ShouldBeTrue(
            $"Expected {innerBounds} to stay inside a {outerSize} body.");
    }

    private static bool IsInside(Size outerSize, Rect innerBounds)
    {
        const double tolerance = 0.001;
        return innerBounds.X >= -tolerance &&
               innerBounds.Y >= -tolerance &&
               innerBounds.Right <= outerSize.Width + tolerance &&
               innerBounds.Bottom <= outerSize.Height + tolerance;
    }

    private static void AssertEligibleHighlightTarget(Control target)
    {
        target.IsAttachedToVisualTree().ShouldBeTrue();
        target.IsEffectivelyVisible.ShouldBeTrue();
        target.Bounds.Width.ShouldBeGreaterThan(0);
        target.Bounds.Height.ShouldBeGreaterThan(0);
        target.GetSelfAndVisualAncestors()
              .ShouldAllBe(static ancestor => ancestor.Opacity > 0);
    }

    private static void ReapplyDefaultTemplate(TemplatedControl control)
    {
        control.SetValue(TemplatedControl.TemplateProperty, null);
        Dispatcher.UIThread.RunJobs();
        control.ClearValue(TemplatedControl.TemplateProperty);
        control.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
    }

    private static IReadOnlyList<T> FindSemanticControls<T>(Control owner, string marker)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Where(control => control.Classes.Contains(marker))
                    .ToArray();
    }

    private static void AssertRenderedGeometryMatchesBounds(Rectangle rectangle)
    {
        var geometryBounds = rectangle.RenderedGeometry.ShouldNotBeNull().Bounds;
        geometryBounds.Width.ShouldBe(rectangle.Bounds.Width, 0.001);
        geometryBounds.Height.ShouldBe(rectangle.Bounds.Height, 0.001);
    }


    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = System.IO.Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }
            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }

    private sealed class RecordingProgressBar : AtomUIProgressBar
    {
        protected override Type StyleKeyOverride => typeof(AtomUIProgressBar);

        public int GrooveRenderCount { get; private set; }
        public int TrackRenderCount { get; private set; }

        protected override void RenderGroove(DrawingContext context)
        {
            GrooveRenderCount++;
            base.RenderGroove(context);
        }

        protected override void RenderIndicatorBar(DrawingContext context)
        {
            TrackRenderCount++;
            base.RenderIndicatorBar(context);
        }
    }
}
