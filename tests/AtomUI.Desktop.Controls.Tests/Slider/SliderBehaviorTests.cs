using Avalonia.Input.Raw;
using System.Reactive.Linq;
using Avalonia.VisualTree;
using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomUISlider = AtomUI.Desktop.Controls.Slider;
using AtomUISliderMark = AtomUI.Desktop.Controls.SliderMark;
using AtomUISliderTrack = AtomUI.Desktop.Controls.SliderTrack;
using AtomUIToolTip = AtomUI.Desktop.Controls.ToolTip;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Slider;

public class SliderBehaviorTests
{
    static SliderBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Snap_To_Decimal_Tick_Normalizes_Value_Written_By_Step_Move()
    {
        var slider = new AtomUISlider
        {
            Minimum             = 0,
            Maximum             = 1,
            SmallChange         = 0.1,
            TickFrequency       = 0.1,
            IsSnapToTickEnabled = true,
            Value               = 0.2
        };

        InvokePrivate(slider, "MoveToNextTick", 0.1);

        slider.Value.ToString("R", CultureInfo.InvariantCulture).ShouldBe("0.3");
    }

    [Fact]
    public void Mark_Label_Brush_Change_Rebuilds_Cached_Formatted_Text()
    {
        var mark = new AtomUISliderMark("30", 30);
        var track = new AtomUISliderTrack
        {
            Marks             = [mark],
            MarkLabelFontSize = 12,
            MarkLabelBrush    = Brushes.Red
        };

        InvokePrivate(track, "CalculateMaxMarkSize", false);
        var formattedText = GetInternalProperty<object>(mark, "FormattedText");

        track.MarkLabelBrush = Brushes.Blue;

        GetInternalProperty<object>(mark, "FormattedText")
            .ShouldNotBeSameAs(formattedText);
    }

    [Fact]
    public void Detaching_Slider_Releases_Template_Pointer_Handlers()
    {
        var slider = new AtomUISlider();
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 160,
            Content = slider
        };

        var inputManager = (IInputManager)AvaloniaLocator.CurrentMutable.GetService(typeof(IInputManager))!;
        IInputRoot? foreignRoot = null;
        IInputElement? focusAfterRouting = null;
        // Observe after device routing but before the track's Process subscription. Normal
        // window activation may move focus; this track must not alter that foreign-root result.
        using var observation = inputManager.Process.Subscribe(args =>
        {
            if (args is RawPointerEventArgs { Type: RawPointerEventType.LeftButtonDown } pointer &&
                pointer.Root == foreignRoot)
            {
                focusAfterRouting = window.FocusManager?.GetFocusedElement();
            }
        });
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetPrivateField<IDisposable?>(slider, "_pointerMovedDispose").ShouldNotBeNull();
            GetPrivateField<IDisposable?>(slider, "_pointerPressDispose").ShouldNotBeNull();
            GetPrivateField<IDisposable?>(slider, "_pointerReleaseDispose").ShouldNotBeNull();
            var thumb = slider.GetVisualDescendants().OfType<SliderThumb>().First();
            var other = new AvaloniaWindow { Width = 240, Height = 160, Content = new Border { Background = Brushes.Blue } };
            other.Show();
            foreignRoot = other.GetPresentationSource().ShouldNotBeNull().ShouldBeAssignableTo<IInputRoot>();
            try
            {
                thumb.Focus();
                thumb.IsFocused.ShouldBeTrue();
                other.MouseDown(new Point(230, 150), MouseButton.Left);
                window.FocusManager?.GetFocusedElement().ShouldBeSameAs(focusAfterRouting,
                    "the track must preserve the result of foreign-window input routing");
                other.MouseUp(new Point(230, 150), MouseButton.Left);
            }
            finally
            {
                other.Close();
            }

            var track = new AtomUISliderTrack { Width = 200, Height = 40, Value = 0 };
            var canvas = new Canvas { Children = { track } };
            Canvas.SetLeft(track, 25);
            Canvas.SetTop(track, 35);
            window.Content = canvas;
            Dispatcher.UIThread.RunJobs();
            var trackThumb = track.Thumbs.First();
            var point = trackThumb.TranslatePoint(new Point(trackThumb.Bounds.Width / 2, trackThumb.Bounds.Height / 2), window).ShouldNotBeNull();
            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            trackThumb.IsFocused.ShouldBeTrue("offset layout must not duplicate the track origin");
            window.MouseUp(point, MouseButton.Left);

            window.Close();
            Dispatcher.UIThread.RunJobs();

            GetPrivateField<IDisposable?>(slider, "_pointerMovedDispose").ShouldBeNull();
            GetPrivateField<IDisposable?>(slider, "_pointerPressDispose").ShouldBeNull();
            GetPrivateField<IDisposable?>(slider, "_pointerReleaseDispose").ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Enabling_Range_Mode_After_Template_Configures_Range_Thumb_ToolTips()
    {
        var slider = new AtomUISlider
        {
            Minimum             = 0,
            Maximum             = 100,
            Orientation         = Orientation.Vertical,
            RangeValues         = [20, 80],
            ValueFormatTemplate = "{0:0}%"
        };
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 240,
            Content = slider
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();
            track!.Thumbs.Count.ShouldBe(1);

            slider.IsRangeMode = true;
            Dispatcher.UIThread.RunJobs();

            track.Thumbs.Count.ShouldBe(2);
            AtomUIToolTip.GetPlacement(track.Thumbs[0]).ShouldBe(PlacementMode.Right);
            AtomUIToolTip.GetPlacement(track.Thumbs[1]).ShouldBe(PlacementMode.Right);
            AtomUIToolTip.GetTip(track.Thumbs[0]).ShouldBe("20%");
            AtomUIToolTip.GetTip(track.Thumbs[1]).ShouldBe("80%");
            AtomUIToolTip.GetTipHostWidth(track.Thumbs[1]).ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Range_Mode_Template_Creates_Thumb_Per_Range_Value()
    {
        var slider = new AtomUISlider
        {
            Minimum             = 0,
            Maximum             = 100,
            IsRangeMode         = true,
            RangeValues         = [10, 40, 80],
            ValueFormatTemplate = "{0:0}%"
        };

        ShowInWindow(slider, () =>
        {
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();
            track!.Thumbs.Count.ShouldBe(3);
            AtomUIToolTip.GetTip(track.Thumbs[0]).ShouldBe("10%");
            AtomUIToolTip.GetTip(track.Thumbs[1]).ShouldBe("40%");
            AtomUIToolTip.GetTip(track.Thumbs[2]).ShouldBe("80%");
        });
    }

    [Fact]
    public void Dynamic_Thumbs_Keep_Slider_As_Templated_Parent()
    {
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = [20, 50, 80]
        };

        ShowInWindow(slider, () =>
        {
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();
            track!.Thumbs.ShouldAllBe(thumb => ReferenceEquals(thumb.TemplatedParent, slider));
        });
    }

    [Fact]
    public void Dynamic_Thumbs_Follow_Runtime_Motion_Changes()
    {
        var slider = new AtomUISlider
        {
            Minimum         = 0,
            Maximum         = 100,
            IsRangeMode     = true,
            RangeValues     = [20, 50, 80],
            IsMotionEnabled = false
        };

        ShowInWindow(slider, () =>
        {
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();
            track!.Thumbs.ShouldAllBe(thumb => !thumb.IsMotionEnabled);

            slider.IsMotionEnabled = true;
            Dispatcher.UIThread.RunJobs();

            track.Thumbs.ShouldAllBe(thumb => thumb.IsMotionEnabled);
        });
    }

    [Fact]
    public void Range_Mode_With_Marks_Keeps_Thumbs_Centered_On_Rail()
    {
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = [20, 80],
            Marks       =
            [
                new AtomUISliderMark("0", 0),
                new AtomUISliderMark("20", 20),
                new AtomUISliderMark("80", 80),
                new AtomUISliderMark("100", 100)
            ]
        };

        ShowInWindow(slider, () =>
        {
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();
            var railRect = InvokePrivate<Rect>(track!, "GetRailRect", track.Bounds.Size);

            foreach (var thumb in track!.Thumbs)
            {
                thumb.Bounds.Center.Y.ShouldBe(railRect.Center.Y, 0.5);
            }
        });
    }

    [Fact]
    public void Template_Thumb_Drag_Is_Handled_Only_By_Slider_Pointer_Pipeline()
    {
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = [20, 80]
        };

        ShowInWindow(slider, () =>
        {
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();

            track!.Thumbs[0].RaiseEvent(new VectorEventArgs
            {
                RoutedEvent = AtomUI.Desktop.Controls.SliderThumb.DragDeltaEvent,
                Vector      = new Vector(40, 0)
            });

            slider.RangeValues.ShouldBe([20, 80]);
        });
    }

    [Fact]
    public void Draggable_Range_Track_Does_Not_Claim_Thumb_Press()
    {
        var slider = new AtomUISlider
        {
            Minimum          = 0,
            Maximum          = 100,
            IsRangeMode      = true,
            IsDraggableTrack = true,
            RangeValues      = [20, 50, 80]
        };

        ShowInWindow(slider, () =>
        {
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();

            foreach (var thumb in track!.Thumbs)
            {
                track.CanDragRangeTrackAt(thumb.Bounds.Center).ShouldBeFalse();
            }

            var firstSegmentCenter = new Point(
                (track.Thumbs[0].Bounds.Center.X + track.Thumbs[1].Bounds.Center.X) / 2,
                track.Thumbs[0].Bounds.Center.Y);
            track.CanDragRangeTrackAt(firstSegmentCenter).ShouldBeTrue();
        });
    }

    [Fact]
    public void Range_Thumb_Drag_Preserves_Grab_Offset()
    {
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = [20, 60]
        };

        ShowInWindow(slider, () =>
        {
            var window = TopLevel.GetTopLevel(slider).ShouldBeOfType<AvaloniaWindow>();
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();
            var thumb = track!.Thumbs[1];
            var pressPoint = thumb.TranslatePoint(
                new Point(thumb.Bounds.Width - 2, thumb.Bounds.Height / 2),
                window).ShouldNotBeNull();
            var initialCenterX = thumb.Bounds.Center.X;

            window.MouseMove(pressPoint);
            window.MouseDown(pressPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            slider.RangeValues.ShouldBe([20, 60]);

            window.MouseMove(pressPoint + new Vector(30, 0));
            Dispatcher.UIThread.RunJobs();

            thumb.Bounds.Center.X.ShouldBe(initialCenterX + 30, 0.5);
            window.MouseUp(pressPoint + new Vector(30, 0), MouseButton.Left);
        });
    }

    [Fact]
    public void Single_Thumb_Drag_By_Subpixel_Distance_Preserves_Fractional_Position()
    {
        var slider = new AtomUISlider
        {
            Minimum = 0,
            Maximum = 100,
            Value   = 50
        };

        ShowInWindow(slider, () =>
        {
            var window = TopLevel.GetTopLevel(slider).ShouldBeOfType<AvaloniaWindow>();
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();
            var thumb = track!.Thumbs[0];
            var initialTrackCenter = InvokePrivate<Point>(track,
                "ValueToCenterPoint",
                track.Bounds.Size,
                track.Value);
            var pressPoint = thumb.TranslatePoint(
                new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2),
                window).ShouldNotBeNull();
            const double dragDistance = 0.25;

            window.MouseMove(pressPoint);
            window.MouseDown(pressPoint, MouseButton.Left);
            window.MouseMove(pressPoint + new Vector(dragDistance, 0));
            Dispatcher.UIThread.RunJobs();
            track.UpdateLayout();

            var railRect = InvokePrivate<Rect>(track, "GetRailRect", track.Bounds.Size);
            slider.Value.ShouldBe(50 + 100 * dragDistance / railRect.Width, 0.0001);
            track.Value.ShouldBe(slider.Value);
            var expectedCenter = InvokePrivate<Point>(track,
                "ValueToCenterPoint",
                track.Bounds.Size,
                track.Value);
            expectedCenter.X.ShouldBe(initialTrackCenter.X + dragDistance, 0.0001);
            thumb.Bounds.Center.X.ShouldBe(expectedCenter.X, 0.0001);
            window.MouseUp(pressPoint + new Vector(dragDistance, 0), MouseButton.Left);
        });
    }

    [Fact]
    public void Range_Thumb_Repeated_One_Pixel_Moves_Remain_Continuous()
    {
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = [20, 60]
        };

        ShowInWindow(slider, () =>
        {
            var window = TopLevel.GetTopLevel(slider).ShouldBeOfType<AvaloniaWindow>();
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();
            var thumb = track!.Thumbs[1];
            var pressPoint = thumb.TranslatePoint(
                new Point(thumb.Bounds.Width - 2, thumb.Bounds.Height / 2),
                window).ShouldNotBeNull();
            var initialCenterX = thumb.Bounds.Center.X;
            var previousValue = slider.RangeValues![1];
            window.MouseMove(pressPoint);
            window.MouseDown(pressPoint, MouseButton.Left);

            for (var offset = 1; offset <= 20; offset++)
            {
                window.MouseMove(pressPoint + new Vector(offset, 0));
                Dispatcher.UIThread.RunJobs();

                var currentValue = slider.RangeValues![1];
                currentValue.ShouldBeGreaterThan(previousValue);
                (currentValue - previousValue).ShouldBeLessThan(1);
                thumb.Bounds.Center.X.ShouldBe(initialCenterX + offset, 0.5);
                previousValue = currentValue;
            }

            previousValue.ShouldNotBe(Math.Round(previousValue));
            window.MouseUp(pressPoint + new Vector(20, 0), MouseButton.Left);
        });
    }

    [Fact]
    public void Range_Thumb_Drag_Snaps_Only_When_Enabled()
    {
        var slider = new AtomUISlider
        {
            Minimum             = 0,
            Maximum             = 100,
            IsRangeMode         = true,
            IsSnapToTickEnabled = true,
            TickFrequency       = 5,
            RangeValues         = [20, 60]
        };

        ShowInWindow(slider, () =>
        {
            var window = TopLevel.GetTopLevel(slider).ShouldBeOfType<AvaloniaWindow>();
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();
            var thumb = track!.Thumbs[1];
            var pressPoint = thumb.TranslatePoint(
                new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2),
                window).ShouldNotBeNull();
            window.MouseMove(pressPoint);
            window.MouseDown(pressPoint, MouseButton.Left);
            window.MouseMove(pressPoint + new Vector(12, 0));
            Dispatcher.UIThread.RunJobs();

            slider.RangeValues.ShouldBe([20, 65]);
            window.MouseUp(pressPoint + new Vector(12, 0), MouseButton.Left);
        });
    }

    [Fact]
    public void Range_Thumb_Captures_Pointer_Until_Release()
    {
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = [20, 60]
        };

        ShowInWindow(slider, () =>
        {
            var window = TopLevel.GetTopLevel(slider).ShouldBeOfType<AvaloniaWindow>();
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();
            var thumb = track!.Thumbs[1];
            var pressPoint = thumb.TranslatePoint(
                new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2),
                window).ShouldNotBeNull();
            var outsideSlider = new Point(window.Bounds.Width - 2, window.Bounds.Height - 2);
            window.MouseMove(pressPoint);
            window.MouseDown(pressPoint, MouseButton.Left);
            window.MouseMove(outsideSlider);
            Dispatcher.UIThread.RunJobs();

            slider.RangeValues![1].ShouldBeGreaterThan(60);

            window.MouseUp(outsideSlider, MouseButton.Left);
            var releasedValues = slider.RangeValues;
            window.MouseMove(new Point(2, window.Bounds.Height - 2));
            Dispatcher.UIThread.RunJobs();
            slider.RangeValues.ShouldBeSameAs(releasedValues);
        });
    }

    [Fact]
    public void DisabledHandles_Disable_Corresponding_Template_Thumbs()
    {
        var slider = new AtomUISlider
        {
            Minimum         = 0,
            Maximum         = 100,
            IsRangeMode     = true,
            RangeValues     = [10, 40, 80],
            DisabledHandles = [false, true, false]
        };

        ShowInWindow(slider, () =>
        {
            var track = GetInternalProperty<AtomUISliderTrack?>(slider, "SliderTrack");
            track.ShouldNotBeNull();
            track!.Thumbs[0].IsEnabled.ShouldBeTrue();
            track.Thumbs[1].IsEnabled.ShouldBeFalse();
            track.Thumbs[2].IsEnabled.ShouldBeTrue();
        });
    }

    [Fact]
    public void MoveRangeHandle_Ignores_Disabled_Handle()
    {
        var slider = new AtomUISlider
        {
            Minimum         = 0,
            Maximum         = 100,
            IsRangeMode     = true,
            RangeValues     = [10, 40, 80],
            DisabledHandles = [false, true, false]
        };

        InvokePrivate(slider, "MoveRangeHandle", 1, 60d);

        slider.RangeValues.ShouldBe([10, 40, 80]);
    }

    [Fact]
    public void MoveRangeHandle_Stops_At_Disabled_Neighbor()
    {
        var slider = new AtomUISlider
        {
            Minimum         = 0,
            Maximum         = 100,
            IsRangeMode     = true,
            RangeValues     = [20, 50, 80],
            DisabledHandles = [false, true, false]
        };

        InvokePrivate(slider, "MoveRangeHandle", 0, 75d);
        slider.RangeValues.ShouldBe([50, 50, 80]);

        slider.RangeValues = [20, 50, 80];
        InvokePrivate(slider, "MoveRangeHandle", 2, 25d);
        slider.RangeValues.ShouldBe([20, 50, 50]);
    }

    [Fact]
    public void MoveRangeTrack_Shifts_All_Handles_When_Draggable()
    {
        var slider = new AtomUISlider
        {
            Minimum          = 0,
            Maximum          = 100,
            IsRangeMode      = true,
            IsDraggableTrack = true,
            RangeValues      = [20, 40, 80]
        };

        InvokePrivate(slider, "MoveRangeTrack", 10d);

        slider.RangeValues.ShouldBe([30, 50, 90]);
    }

    [Fact]
    public void MoveRangeTrack_Ignores_Disabled_Handles()
    {
        var slider = new AtomUISlider
        {
            Minimum          = 0,
            Maximum          = 100,
            IsRangeMode      = true,
            IsDraggableTrack = true,
            RangeValues      = [20, 40, 80],
            DisabledHandles  = [false, true, false]
        };

        InvokePrivate(slider, "MoveRangeTrack", 10d);

        slider.RangeValues.ShouldBe([20, 40, 80]);
    }

    [Fact]
    public void RangeValues_Property_Normalizes_Snapshot_Values()
    {
        var source = new List<double> { 80, 20, -10, 120, 20 };
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = source
        };

        slider.RangeValues.ShouldBe([0, 20, 20, 80, 100]);
        slider.RangeValues.ShouldNotBeSameAs(source);

        source[0] = 5;
        slider.RangeValues.ShouldBe([0, 20, 20, 80, 100]);
    }

    [Fact]
    public void RangeValues_Property_Rejects_Non_Finite_Values()
    {
        var slider = new AtomUISlider
        {
            Minimum     = 0,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = [20, 80]
        };

        slider.RangeValues = [double.NaN, 40];
        slider.RangeValues.ShouldBe([20, 80]);

        slider.RangeValues = [double.NegativeInfinity, 40];
        slider.RangeValues.ShouldBe([20, 80]);
    }

    [Fact]
    public void RangeValues_Property_Uses_Minimum_Pair_When_Too_Few_Values_Are_Provided()
    {
        var slider = new AtomUISlider
        {
            Minimum     = 10,
            Maximum     = 100,
            IsRangeMode = true,
            RangeValues = [70]
        };

        slider.RangeValues.ShouldBe([10, 10]);
    }

    [Fact]
    public void SliderRangeMath_NormalizeRangeValues_Preserves_Valid_Snapshots()
    {
        IReadOnlyList<double> values = [10, 40, 80];

        var normalized = SliderRangeMath.NormalizeRangeValues(values, 0, 100);

        normalized.ShouldBeSameAs(values);
    }

    [Fact]
    public void SliderRangeMath_NormalizeRangeValues_Stabilizes_Default_Range_Pair()
    {
        var normalized = SliderRangeMath.NormalizeRangeValues(null, 0, 100);

        SliderRangeMath.NormalizeRangeValues(normalized, 0, 100).ShouldBeSameAs(normalized);
    }

    [Fact]
    public void SliderRangeMath_Finds_Nearest_Enabled_Handle()
    {
        var values = new[] { 10d, 40d, 80d };

        SliderRangeMath.FindNearestEnabledHandleIndex(values, [false, true, false], 38)
                       .ShouldBe(0);
        SliderRangeMath.FindNearestEnabledHandleIndex(values, [false, true, false], 70)
                       .ShouldBe(2);
    }

    [Fact]
    public void SliderRangeMath_Applies_Track_Offset_Only_When_All_Handles_Can_Move()
    {
        SliderRangeMath.ApplyTrackOffset([20, 40, 80], [], 30, 0, 100)
                       .ShouldBe([40, 60, 100]);
        SliderRangeMath.ApplyTrackOffset([20, 40, 80], [false, true, false], 10, 0, 100)
                       .ShouldBe([20, 40, 80]);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
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

    private static void InvokePrivate(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        method.Invoke(target, args);
    }

    private static T InvokePrivate<T>(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return (T)method.Invoke(target, args)!;
    }

    private static T GetInternalProperty<T>(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        return (T)property.GetValue(target)!;
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (T)field.GetValue(target)!;
    }

}
