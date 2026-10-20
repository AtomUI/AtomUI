using System.Reflection;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
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

public class ProgressBarBehaviorTests
{
    static ProgressBarBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void IsIndeterminate_Toggle_After_Attach_Updates_PseudoClass()
    {
        var progress = new AtomUIProgressBar();

        ShowInWindow(progress, () =>
        {
            progress.Classes.ShouldNotContain(ProgressBarPseudoClass.Indeterminate);

            progress.IsIndeterminate = true;
            Dispatcher.UIThread.RunJobs();

            progress.Classes.ShouldContain(ProgressBarPseudoClass.Indeterminate);

            progress.IsIndeterminate = false;
            Dispatcher.UIThread.RunJobs();

            progress.Classes.ShouldNotContain(ProgressBarPseudoClass.Indeterminate);
        });
    }

    [Fact]
    public void Percentage_Uses_Value_Relative_To_Minimum()
    {
        var progress = new AtomUIProgressBar
        {
            Minimum = 50,
            Maximum = 150,
            Value   = 100
        };

        progress.Percentage.ShouldBe(50d);
    }

    [Fact]
    public void SuccessThreshold_Render_Uses_Value_Relative_To_Minimum()
    {
        var progress = new AtomUIProgressBar
        {
            Width                 = 100,
            Height                = 10,
            Minimum               = 50,
            Maximum               = 150,
            Value                 = 100,
            SuccessThreshold      = 75,
            IndicatorThickness    = 10,
            IsProgressInfoVisible = false,
            StrokeLineCap         = PenLineCap.Flat,
            StrokeBrush           = Brushes.Blue,
            SuccessStrokeBrush    = Brushes.Green,
            TrailColor            = Colors.Gray
        };

        ShowInWindow(progress, () =>
        {
            var successTrack = progress.GetVisualDescendants()
                                       .OfType<Border>()
                                       .Single(static border => border.Name == "PART_ProgressSuccess");

            successTrack.Background.ShouldBeSameAs(progress.SuccessStrokeBrush);
            successTrack.Bounds.Width.ShouldBe(25, 0.001);
        });
    }

    [Fact]
    public void Inner_Percent_Position_Uses_Value_Relative_To_Minimum()
    {
        var progress = new TestProgressBar
        {
            Minimum               = 50,
            Maximum               = 150,
            Value                 = 100,
            Orientation           = Orientation.Horizontal,
            IsProgressInfoVisible = true,
            PercentPosition       = new PercentPosition
            {
                IsInner   = true,
                Alignment = LinePercentAlignment.Center
            }
        };

        progress.SetStrokeThickness(10);
        progress.SetExtraInfoSize(new Size(20, 8));

        var extraInfoRect = progress.GetExtraInfoRectForTest(new Size(200, 20));

        extraInfoRect.X.ShouldBe(40, 0.001);
    }

    [Fact]
    public void Circle_IndicatorAngle_Uses_Value_Relative_To_Minimum()
    {
        var progress = new AtomUICircleProgress
        {
            Minimum = 50,
            Maximum = 150,
            Value   = 100
        };

        GetNonPublicProperty<double>(progress, "IndicatorAngle").ShouldBe(180d);
    }

    [Fact]
    public void Dashboard_IndicatorAngle_Uses_Value_Relative_To_Minimum()
    {
        var progress = new AtomUIDashboardProgress
        {
            Minimum   = 50,
            Maximum   = 150,
            Value     = 100,
            GapDegree = 75
        };

        GetNonPublicProperty<double>(progress, "IndicatorAngle").ShouldBe(142.5d);
    }

    [Fact]
    public void Steps_Automatic_ChunkWidth_Remains_Automatic_Across_SizeType_Changes()
    {
        var progress = new AtomUIStepsProgressBar
        {
            Steps = 3,
            IsProgressInfoVisible = false
        };

        ShowInWindow(progress, () =>
        {
            progress.ChunkWidth.ShouldBe(double.NaN);

            progress.Measure(Size.Infinity);
            progress.DesiredSize.Width.ShouldBe(46d);

            progress.SizeType = global::AtomUI.SizeType.Middle;
            Dispatcher.UIThread.RunJobs();
            progress.Measure(Size.Infinity);
            progress.ChunkWidth.ShouldBe(double.NaN);
            progress.DesiredSize.Width.ShouldBe(22d);

            progress.SizeType = global::AtomUI.SizeType.Small;
            Dispatcher.UIThread.RunJobs();
            progress.Measure(Size.Infinity);
            progress.ChunkWidth.ShouldBe(double.NaN);
            progress.DesiredSize.Width.ShouldBe(10d);
        });
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 320,
            Height  = 220,
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
        }
    }

    private static T GetNonPublicProperty<T>(object target, string propertyName)
    {
        var property = target.GetType()
                             .GetProperty(propertyName,
                                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        return (T)property.GetValue(target)!;
    }

    private sealed class TestProgressBar : AtomUIProgressBar
    {
        public void SetStrokeThickness(double value)
        {
            StrokeThickness = value;
        }

        public void SetExtraInfoSize(Size value)
        {
            _extraInfoSize = value;
        }

        public Rect GetExtraInfoRectForTest(Size controlSize)
        {
            return GetExtraInfoRect(new Rect(default, controlSize));
        }
    }
}
