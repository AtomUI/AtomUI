using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISteps = AtomUI.Desktop.Controls.Steps;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Steps;

public class StepsRootFrameTests
{
    static StepsRootFrameTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Steps_Root_Dash_Properties_Propagate_To_The_Template_Root_DashedBorder()
    {
        var steps = new AtomUISteps
        {
            BorderDashArray  = new[] { 4d, 2d },
            BorderDashOffset = 1.5
        };

        using var window = Show(steps);
        AssertDashPropagation(steps, new[] { 4d, 2d }, 1.5);
    }

    [Fact]
    public void Steps_Root_Dash_Properties_Default_To_Neutral_Values()
    {
        var steps = new AtomUISteps();

        using var window = Show(steps);
        AssertDashDefaults(steps);
    }

    private static void AssertDashPropagation(AtomUISteps steps, IReadOnlyList<double> dashArray, double dashOffset)
    {
        var rootBorder = steps.GetVisualChildren().OfType<DashedBorder>().Single();
        rootBorder.StrokeDashArray.ShouldBe(dashArray);
        rootBorder.StrokeDaskOffset.ShouldBe(dashOffset);
    }

    private static void AssertDashDefaults(AtomUISteps steps)
    {
        var rootBorder = steps.GetVisualChildren().OfType<DashedBorder>().Single();
        rootBorder.StrokeDashArray.ShouldBeNull();
        rootBorder.StrokeDaskOffset.ShouldBe(0d);
    }

    private static IDisposable Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 900,
            Height  = 400,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowLifetime(window);
    }

    private sealed class WindowLifetime(AvaloniaWindow window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
