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
            knob.Bounds.ShouldBe(new Rect(-3, -3, 20, 20));

            toggleSwitch.IsChecked = true;
            Dispatcher.UIThread.RunJobs();
            knob.Bounds.ShouldBe(new Rect(15, -3, 20, 20));

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
            knob.Bounds.X.ShouldBe(toggleSwitch.Bounds.Width - toggleSwitch.TrackPadding - knob.Bounds.Width, 1e-6);
        });
    }

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
