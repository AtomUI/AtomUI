using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISizeType = AtomUI.SizeType;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Steps;

public class StepsProgressTests
{
    static StepsProgressTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(-1d, 0d)]
    [InlineData(0d, 0d)]
    [InlineData(50d, 50d)]
    [InlineData(100d, 100d)]
    [InlineData(101d, 100d)]
    public void Percent_Is_Clamped(double input, double expected)
    {
        new Desktop.Controls.Steps { Percent = input }.Percent.ShouldBe(expected);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonFinite_Percent_Becomes_Null(double input)
    {
        new Desktop.Controls.Steps { Percent = input }.Percent.ShouldBeNull();
    }

    [Theory]
    [InlineData(true, Desktop.Controls.StepsStatus.Process, Desktop.Controls.StepsType.Default, false, 50d, true)]
    [InlineData(true, Desktop.Controls.StepsStatus.Process, Desktop.Controls.StepsType.Navigation, false, 50d, true)]
    [InlineData(false, Desktop.Controls.StepsStatus.Process, Desktop.Controls.StepsType.Default, false, 50d, false)]
    [InlineData(true, Desktop.Controls.StepsStatus.Wait, Desktop.Controls.StepsType.Default, false, 50d, false)]
    [InlineData(true, Desktop.Controls.StepsStatus.Finish, Desktop.Controls.StepsType.Default, false, 50d, false)]
    [InlineData(true, Desktop.Controls.StepsStatus.Error, Desktop.Controls.StepsType.Default, false, 50d, false)]
    [InlineData(true, Desktop.Controls.StepsStatus.Process, Desktop.Controls.StepsType.Dot, false, 50d, false)]
    [InlineData(true, Desktop.Controls.StepsStatus.Process, Desktop.Controls.StepsType.OutlineDot, false, 50d, false)]
    [InlineData(true, Desktop.Controls.StepsStatus.Process, Desktop.Controls.StepsType.Inline, false, 50d, false)]
    [InlineData(true, Desktop.Controls.StepsStatus.Process, Desktop.Controls.StepsType.Default, true, 50d, false)]
    [InlineData(true, Desktop.Controls.StepsStatus.Process, Desktop.Controls.StepsType.Default, false, null, false)]
    public void Progress_Visibility_Uses_The_Complete_State_Matrix(
        bool isCurrent,
        Desktop.Controls.StepsStatus effectiveStatus,
        Desktop.Controls.StepsType type,
        bool hasIcon,
        double? percent,
        bool expected)
    {
        var item = new Desktop.Controls.StepsItem
        {
            Type    = type,
            Percent = percent,
            Icon    = hasIcon ? new PathIcon() : null
        };

        item.ApplyOwnerState(
            stepNumber: 0,
            isCurrent,
            effectiveStatus,
            isFirst: true,
            isLast: true,
            connectorStatus: Desktop.Controls.StepsStatus.Wait);

        item.IsProgressVisible.ShouldBe(expected);
    }

    [Theory]
    [InlineData(RenderInput.Percent)]
    [InlineData(RenderInput.ProgressLineThickness)]
    [InlineData(RenderInput.ProgressGrooveColor)]
    [InlineData(RenderInput.ProgressColor)]
    [InlineData(RenderInput.IsCurrent)]
    [InlineData(RenderInput.IsProgressVisible)]
    [InlineData(RenderInput.UseLayoutRounding)]
    public void Progress_Render_Input_Changes_Invalidate_Indicator(RenderInput input)
    {
        var indicator = new RenderCountingStepsItemIndicator
        {
            Width                 = 32,
            Height                = 32,
            Percent               = 25,
            ProgressLineThickness = 2,
            UseProgressRingTokenResources = true,
            Resources = new ResourceDictionary
            {
                [StepsTokenKind.ProgressGrooveColor] = Brushes.Gray,
                [StepsTokenKind.ProgressColor] = Brushes.Blue
            },
            IsCurrent             = false,
            IsProgressVisible     = true
        };

        ShowInWindow(indicator, () =>
        {
            var initialRenderCount = indicator.RenderCount;
            initialRenderCount.ShouldBeGreaterThan(0);

            ChangeRenderInput(indicator, input);
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Dispatcher.UIThread.RunJobs();

            indicator.RenderCount.ShouldBeGreaterThan(initialRenderCount);
        });
    }

    [Theory]
    [InlineData(AtomUISizeType.Middle)]
    [InlineData(AtomUISizeType.Small)]
    public void Visible_Progress_Ring_Expands_Outside_The_Indicator_Frame(AtomUISizeType sizeType)
    {
        var steps = new Desktop.Controls.Steps
        {
            Width     = 400,
            Current   = 0,
            Percent   = 50,
            SizeType  = sizeType,
            Type      = Desktop.Controls.StepsType.Default
        };
        var item = new Desktop.Controls.StepsItem { Header = "Current" };
        steps.Items.Add(item);

        ShowInWindow(steps, () =>
        {
            var indicator = item.GetVisualDescendants()
                                .OfType<Desktop.Controls.StepsItemIndicator>()
                                .Single();
            var frame = indicator.GetVisualDescendants()
                                 .OfType<Control>()
                                 .Single(control => control.Name == "Frame");

            indicator.IsProgressVisible.ShouldBeTrue();
            indicator.Bounds.Width.ShouldBeGreaterThan(frame.Bounds.Width);
            indicator.Bounds.Height.ShouldBeGreaterThan(frame.Bounds.Height);
            indicator.ClipToBounds.ShouldBeFalse();
        });
    }

    private static void ChangeRenderInput(RenderCountingStepsItemIndicator indicator, RenderInput input)
    {
        switch (input)
        {
            case RenderInput.Percent:
                indicator.Percent = 75;
                break;
            case RenderInput.ProgressLineThickness:
                indicator.ProgressLineThickness = 3;
                break;
            case RenderInput.ProgressGrooveColor:
                indicator.Resources[StepsTokenKind.ProgressGrooveColor] = Brushes.Red;
                break;
            case RenderInput.ProgressColor:
                indicator.Resources[StepsTokenKind.ProgressColor] = Brushes.Green;
                break;
            case RenderInput.IsCurrent:
                indicator.IsCurrent = true;
                break;
            case RenderInput.IsProgressVisible:
                indicator.IsProgressVisible = false;
                break;
            case RenderInput.UseLayoutRounding:
                indicator.UseLayoutRounding = !indicator.UseLayoutRounding;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(input), input, null);
        }
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 200,
            Height  = 100,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    public enum RenderInput
    {
        Percent,
        ProgressLineThickness,
        ProgressGrooveColor,
        ProgressColor,
        IsCurrent,
        IsProgressVisible,
        UseLayoutRounding
    }

    private sealed class RenderCountingStepsItemIndicator : Desktop.Controls.StepsItemIndicator
    {
        public int RenderCount { get; private set; }

        public override void Render(DrawingContext context)
        {
            RenderCount++;
            base.Render(context);
        }
    }
}
