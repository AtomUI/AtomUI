using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomToggleSwitch = AtomUI.Desktop.Controls.ToggleSwitch;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Switch;

public class ToggleSwitchBehaviorTests
{
    static ToggleSwitchBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void WaveSpirit_Brush_Follows_The_Groove_Color_On_Check_Change()
    {
        var groove = new SolidColorBrush(Color.Parse("#123456"));
        var toggleSwitch = new AtomToggleSwitch
        {
            Width               = 48,
            Height              = 24,
            GrooveBackground    = groove,
            IsMotionEnabled     = true,
            IsWaveSpiritEnabled = true
        };

        ShowInWindow(toggleSwitch, () =>
        {
            var wave = GetWaveSpiritDecorator(toggleSwitch);

            toggleSwitch.IsChecked = true;
            Dispatcher.UIThread.RunJobs();

            GetColor(GetWaveBrush(wave)).ShouldBe(Color.Parse("#123456"));
        });
    }

    [Fact]
    public void WaveSpirit_Brush_Follows_The_Groove_Color_When_Toggled_Back_Off()
    {
        var groove = new SolidColorBrush(Color.Parse("#654321"));
        var toggleSwitch = new AtomToggleSwitch
        {
            Width               = 48,
            Height              = 24,
            GrooveBackground    = groove,
            IsChecked           = true,
            IsMotionEnabled     = true,
            IsWaveSpiritEnabled = true
        };

        ShowInWindow(toggleSwitch, () =>
        {
            var wave = GetWaveSpiritDecorator(toggleSwitch);

            toggleSwitch.IsChecked = false;
            Dispatcher.UIThread.RunJobs();

            GetColor(GetWaveBrush(wave)).ShouldBe(Color.Parse("#654321"));
        });
    }

    [Fact]
    public void Custom_Geometry_Values_Place_The_Indicator_With_Antd_Style_Overhang()
    {
        var toggleSwitch = new AtomToggleSwitch
        {
            TrackHeight     = 14,
            TrackMinWidth   = 32,
            TrackPadding    = -3,
            KnobSize        = new Size(20, 20),
            IsMotionEnabled = false
        };

        ShowInWindow(toggleSwitch, () =>
        {
            toggleSwitch.Bounds.Size.ShouldBe(new Size(32, 14));

            var knob = FindIndicator(toggleSwitch);
            GetIndicatorBounds(knob, toggleSwitch).ShouldBe(new Rect(-3, -3, 20, 20));

            toggleSwitch.IsChecked = true;
            Dispatcher.UIThread.RunJobs();
            GetIndicatorBounds(knob, toggleSwitch).ShouldBe(new Rect(15, -3, 20, 20));

            // antd renders the switch handle outside the content container, so custom
            // geometry (the 20px MUI handle on a 14px track) overhangs instead of clipping.
            knob.GetVisualAncestors()
                .TakeWhile(visual => !ReferenceEquals(visual, toggleSwitch))
                .OfType<Canvas>()
                .ShouldBeEmpty();

            toggleSwitch.GetVisualDescendants()
                        .OfType<Canvas>()
                        .Single()
                        .ClipToBounds.ShouldBeTrue();
        });
    }

    [Fact]
    public void Explicit_Width_Drives_Groove_And_Knob_Geometry()
    {
        var toggleSwitch = new AtomToggleSwitch
        {
            SizeType        = CustomizableSizeType.Small,
            OnContent       = "on",
            OffContent      = "off",
            Width           = 40,
            IsChecked       = true,
            IsMotionEnabled = true
        };

        ShowInWindow(toggleSwitch, () =>
        {
            toggleSwitch.Bounds.Width.ShouldBe(40);

            // The internal knob geometry must follow the explicit width instead of the
            // content-measured width, mirroring antd's `width: 40px` root styling.
            var knob = FindIndicator(toggleSwitch);
            GetIndicatorBounds(knob, toggleSwitch).X.ShouldBe(toggleSwitch.Bounds.Width - toggleSwitch.TrackPadding - knob.Bounds.Width, 1e-6);
        });
    }

    private sealed class CheckedModel
    {
        public bool IsOn { get; set; } = true;
    }

    // A switch checked through a binding after it is shown animates its knob. The knob's final position must survive the
    // end of that transition: written into the animation layer, it was dropped with the transition and the knob fell
    // back to the start while the groove showed checked.
    [Fact]
    public void Knob_Stays_At_The_Checked_End_After_A_Bound_Check_Animates()
    {
        var toggleSwitch = new AtomToggleSwitch { IsMotionEnabled = true };
        toggleSwitch.Bind(ToggleButton.IsCheckedProperty, new Avalonia.Data.Binding(nameof(CheckedModel.IsOn)));
        var host = new StackPanel { Children = { toggleSwitch } };

        ShowInWindow(host, () =>
        {
            host.DataContext = new CheckedModel();
            Dispatcher.UIThread.RunJobs();
            for (var tick = 0; tick < 60; tick++)
            {
                Avalonia.Headless.AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(5);
            }

            toggleSwitch.IsChecked.ShouldBe(true);
            var knob = FindIndicator(toggleSwitch);
            GetIndicatorBounds(knob, toggleSwitch).X.ShouldBe(
                toggleSwitch.Bounds.Width - toggleSwitch.TrackPadding - knob.Bounds.Width, 1e-6);
        });
    }

    private static Rect GetIndicatorBounds(TemplatedControl knob, AtomToggleSwitch owner) =>
        new Rect(knob.Bounds.Size).TransformToAABB(knob.TransformToVisual(owner)!.Value);

    private static TemplatedControl FindIndicator(AtomToggleSwitch toggleSwitch)
    {
        return toggleSwitch.GetVisualDescendants()
                           .OfType<TemplatedControl>()
                           .Single(control => control.Classes.Contains("semantic-indicator"));
    }

    private static object GetWaveSpiritDecorator(AtomToggleSwitch toggleSwitch)
    {
        return toggleSwitch.GetVisualDescendants()
                           .Single(visual => visual.GetType().Name == "WaveSpiritDecorator");
    }

    private static IBrush? GetWaveBrush(object waveSpiritDecorator)
    {
        var property = waveSpiritDecorator.GetType().GetProperty(
            "WaveBrush",
            BindingFlags.Instance | BindingFlags.Public);
        property.ShouldNotBeNull();
        return (IBrush?)property.GetValue(waveSpiritDecorator);
    }

    private static Color GetColor(IBrush? brush)
    {
        brush.ShouldNotBeNull()
             .ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 160,
            Content = content
        };
        // Reproduce XAML-style logical initialization before visual attachment.
        ((ISupportInitialize)content).BeginInit();
        ((ISupportInitialize)content).EndInit();
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
}
