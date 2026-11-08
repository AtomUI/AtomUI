using System.Diagnostics;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// The input surface transitions are declared on the identity-less frame with <c>atom:TokenTransitions</c>; their durations
/// resolve through the frame's inherited owner, so <c>Controls[X].MotionDurationMid</c> reaches X's frame only.
/// </summary>
public partial class InputFamilyEffectTests
{
    [Fact]
    public void Controls_LineEdit_MotionDurationMid_Changes_Its_Frame_Transitions_But_Not_A_TextBox_Frame()
    {
        var lineEdit = new LineEdit { Width = 160, IsMotionEnabled = true };
        var textBox  = new TextBox { Width = 160, IsMotionEnabled = true };
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().Build(),
            Child = new StackPanel
            {
                Spacing             = 24,
                Margin              = new Thickness(24),
                HorizontalAlignment = HorizontalAlignment.Left,
                Children            = { lineEdit, textBox }
            }
        };
        var window = new AvaloniaWindow { Width = 400, Height = 240, Content = provider };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var lineEditFrame = lineEdit.GetVisualDescendants().OfType<InputControlFrame>().First();
            var textBoxFrame  = textBox.GetVisualDescendants().OfType<InputControlFrame>().First();
            var baseline      = BorderBrushDuration(lineEditFrame).ShouldNotBeNull();
            baseline.ShouldBeGreaterThan(TimeSpan.Zero);
            BorderBrushDuration(textBoxFrame).ShouldBe(baseline);

            provider.Config = new ThemeConfigBuilder()
                              .WithControl(LineEditTokens.Identity,
                                  new ControlThemeConfigBuilder()
                                      .WithToken(nameof(SharedTokenKind.MotionDurationMid), "00:00:00.5000000").Build())
                              .Build();
            var stopwatch = Stopwatch.StartNew();
            while (BorderBrushDuration(lineEditFrame) == baseline && stopwatch.Elapsed < TimeSpan.FromSeconds(10))
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(5);
            }

            BorderBrushDuration(lineEditFrame).ShouldBe(TimeSpan.FromMilliseconds(500));
            BorderBrushDuration(textBoxFrame).ShouldBe(baseline);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static TimeSpan? BorderBrushDuration(InputControlFrame frame) =>
        frame.Transitions?.OfType<TransitionBase>()
             .FirstOrDefault(static transition => transition.Property == TemplatedControlBorderBrush)?.Duration;

    private static readonly AvaloniaProperty TemplatedControlBorderBrush =
        Avalonia.Controls.Primitives.TemplatedControl.BorderBrushProperty;
}
