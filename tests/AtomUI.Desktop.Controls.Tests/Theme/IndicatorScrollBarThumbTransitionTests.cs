using System.Diagnostics;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Theme;

/// <summary>
/// The indicator scroll bar thumb animates its frame inside a ControlTemplate; its duration is declared with
/// <c>atom:TokenTransitions</c>, so a scoped <c>MotionDurationMid</c> reaches it.
/// </summary>
[Collection(ControlEffectiveGlobalBindingTestCollection.Name)]
public class IndicatorScrollBarThumbTransitionTests
{
    static IndicatorScrollBarThumbTransitionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Scoped_MotionDurationMid_Reaches_The_Thumb_Frame_Transition()
    {
        var viewer = new IndicatorScrollViewer
        {
            Width                         = 120,
            Height                        = 80,
            VerticalScrollBarVisibility   = ScrollBarVisibility.Visible,
            Content                       = new AvaloniaBorder { Height = 400 }
        };
        var provider = new ThemeConfigProvider { Config = new ThemeConfigBuilder().Build(), Child = viewer };
        provider.ThemeChangeFailed += static (_, args) =>
            throw new InvalidOperationException(args.Exception?.Message ?? "Theme compilation failed.");
        var window = new AvaloniaWindow { Width = 240, Height = 160, Content = provider };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            FrameDuration(viewer).ShouldBe(TimeSpan.FromMilliseconds(200));

            provider.Config = new ThemeConfigBuilder()
                              .WithToken(nameof(SharedTokenKind.MotionDurationMid), "00:00:00.5000000")
                              .Build();
            WaitUntil(() => FrameDuration(viewer) == TimeSpan.FromMilliseconds(500));
            FrameDuration(viewer).ShouldBe(TimeSpan.FromMilliseconds(500));

            provider.Config = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.EnableMotion), "false").Build();
            WaitUntil(() => FrameDuration(viewer) == TimeSpan.Zero);
            FrameDuration(viewer).ShouldBe(TimeSpan.Zero);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static TimeSpan FrameDuration(Control viewer)
    {
        var thumb = viewer.GetVisualDescendants().OfType<IndicatorScrollBarThumb>().First();
        var frame = thumb.GetVisualDescendants().OfType<AvaloniaBorder>().Single(border => border.Name == "Frame");
        return frame.Transitions.ShouldNotBeNull().OfType<TransitionBase>().ShouldHaveSingleItem().Duration;
    }

    private static void WaitUntil(Func<bool> condition)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }
            Thread.Sleep(1);
        }
    }
}
