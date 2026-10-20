using System.ComponentModel;
using System.Reflection;
using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISegmented = AtomUI.Desktop.Controls.Segmented;
using AtomUISegmentedItem = AtomUI.Desktop.Controls.SegmentedItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Segmented;

public class SegmentedThumbMotionTests
{
    // antd 滑块契约（@rc-component/segmented 1.4.0 + antd components/segmented/style）：
    // transition: transform + width，时长 motionDurationSlow(300ms)，缓动 motionEaseInOut cubic-bezier(0.645, 0.045, 0.355, 1)。
    private static readonly TimeSpan AntdThumbDuration = TimeSpan.FromMilliseconds(300);
    private const double AntdEasingX1 = 0.645;
    private const double AntdEasingY1 = 0.045;
    private const double AntdEasingX2 = 0.355;
    private const double AntdEasingY2 = 1.0;

    static SegmentedThumbMotionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Thumb_Transitions_Use_Antd_Motion_Timing()
    {
        var segmented = CreateMotionSegmented();
        segmented.SelectedIndex = 2;

        ShowInWindow(segmented, () =>
        {
            var selected = segmented.ContainerFromIndex(2).ShouldNotBeNull();
            ((Point)segmented.GetValue(GetThumbProperty("SelectedThumbPosProperty"))!).ShouldBe(selected.TranslatePoint(default, segmented)!.Value);
            ((Size)segmented.GetValue(GetThumbProperty("SelectedThumbSizeProperty"))!).ShouldBe(selected.Bounds.Size);
            var transitions = segmented.Transitions.ShouldNotBeNull();
            AssertThumbTransition(transitions, GetThumbProperty("SelectedThumbPosProperty"));
            AssertThumbTransition(transitions, GetThumbProperty("SelectedThumbSizeProperty"));
        });
    }

    [Fact]
    public void Selected_Item_Background_Stays_Transparent()
    {
        // 选中背景只由根控件 render 层的滑块承载；item 自身在任何时刻都不绘制选中背景。
        var segmented = new AtomUISegmented
        {
            IsMotionEnabled = false
        };
        segmented.Items.Add("Daily");
        segmented.Items.Add("Weekly");

        ShowInWindow(segmented, () =>
        {
            segmented.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();

            var items = GetItems(segmented);
            AssertTransparent(items[1].Background);
        });
    }

    [Fact]
    public void Thumb_Motion_Marks_Items_For_The_Flight_Duration()
    {
        var segmented = CreateMotionSegmented();

        ShowInWindow(segmented, () =>
        {
            segmented.SelectedIndex = 2;
            Dispatcher.UIThread.RunJobs();

            var items = GetItems(segmented);
            items.ShouldAllBe(item => GetThumbMotionActive(item));

            // headless 环境不推进 DispatcherTimer，直接验证计时器接线并驱动完成回调。
            var timer = GetInstanceField<DispatcherTimer>(segmented, "_thumbMotionTimer");
            timer.IsEnabled.ShouldBeTrue();
            timer.Interval.ShouldBe(AntdThumbDuration + TimeSpan.FromMilliseconds(32));

            InvokeThumbMotionTimerTick(segmented);

            GetItems(segmented).ShouldAllBe(item => !GetThumbMotionActive(item));
            timer.IsEnabled.ShouldBeFalse();
        });
    }

    [Fact]
    public void Thumb_Motion_Is_Not_Marked_When_Motion_Disabled()
    {
        var segmented = new AtomUISegmented
        {
            IsMotionEnabled = false
        };
        segmented.Items.Add("Daily");
        segmented.Items.Add("Weekly");
        segmented.Items.Add("Monthly");

        ShowInWindow(segmented, () =>
        {
            segmented.SelectedIndex = 2;
            Dispatcher.UIThread.RunJobs();

            GetItems(segmented).ShouldAllBe(item => !GetThumbMotionActive(item));
        });
    }

    [Fact]
    public void Clearing_Selection_Hides_The_Thumb()
    {
        var segmented = new AtomUISegmented
        {
            IsMotionEnabled = false
        };
        segmented.Items.Add("Daily");
        segmented.Items.Add("Weekly");

        ShowInWindow(segmented, () =>
        {
            GetThumbVisible(segmented).ShouldBeTrue();

            segmented.SelectedItem = null;
            Dispatcher.UIThread.RunJobs();

            GetThumbVisible(segmented).ShouldBeFalse();
        });
    }

    [Fact]
    public void Detaching_Clears_Thumb_Motion_State()
    {
        var segmented = CreateMotionSegmented();
        var window = new AvaloniaWindow
        {
            Width   = 320,
            Height  = 240,
            Content = segmented
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            segmented.SelectedIndex = 2;
            Dispatcher.UIThread.RunJobs();
            GetItems(segmented).ShouldAllBe(item => GetThumbMotionActive(item));
        }
        finally
        {
            window.Close();
        }
        Dispatcher.UIThread.RunJobs();

        GetItems(segmented).ShouldAllBe(item => !GetThumbMotionActive(item));
        var timer = GetInstanceField<DispatcherTimer>(segmented, "_thumbMotionTimer");
        (timer is null || !timer.IsEnabled).ShouldBeTrue();
    }

    private static AtomUISegmented CreateMotionSegmented()
    {
        var segmented = new AtomUISegmented
        {
            IsMotionEnabled = true
        };
        segmented.Items.Add("Daily");
        segmented.Items.Add("Weekly");
        segmented.Items.Add("Monthly");
        return segmented;
    }

    private static void AssertThumbTransition(Transitions transitions, AvaloniaProperty property)
    {
        var transition = transitions.OfType<TransitionBase>()
                                    .FirstOrDefault(candidate => Equals(candidate.Property, property))
                                    .ShouldNotBeNull($"missing transition for {property.Name}");
        transition.Duration.ShouldBe(AntdThumbDuration);
        var easing = transition.Easing.ShouldBeOfType<SplineEasing>();
        easing.X1.ShouldBe(AntdEasingX1, 1e-6);
        easing.Y1.ShouldBe(AntdEasingY1, 1e-6);
        easing.X2.ShouldBe(AntdEasingX2, 1e-6);
        easing.Y2.ShouldBe(AntdEasingY2, 1e-6);
    }

    private static AvaloniaProperty GetThumbProperty(string fieldName)
    {
        return typeof(AbstractSegmented)
               .GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic)
               .ShouldNotBeNull(fieldName)
               .GetValue(null)
               .ShouldNotBeNull()
               .ShouldBeAssignableTo<AvaloniaProperty>();
    }

    private static bool GetThumbMotionActive(AtomUISegmentedItem item)
    {
        return (bool)(typeof(AbstractSegmentedItem)
                      .GetProperty("IsThumbMotionActive", BindingFlags.Instance | BindingFlags.NonPublic)
                      .ShouldNotBeNull()
                      .GetValue(item) ?? false);
    }

    private static bool GetThumbVisible(AtomUISegmented segmented)
    {
        return GetInstanceField<bool>(segmented, "_isThumbVisible");
    }

    private static T GetInstanceField<T>(object owner, string fieldName)
    {
        var field = FindInstanceField(owner.GetType(), fieldName);
        field.ShouldNotBeNull(fieldName);
        return (T)field.GetValue(owner)!;
    }

    private static FieldInfo? FindInstanceField(Type? type, string fieldName)
    {
        while (type is not null)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field is not null)
            {
                return field;
            }
            type = type.BaseType;
        }
        return null;
    }

    private static void InvokeThumbMotionTimerTick(AtomUISegmented segmented)
    {
        typeof(AbstractSegmented)
            .GetMethod("HandleThumbMotionTimerTick", BindingFlags.Instance | BindingFlags.NonPublic)
            .ShouldNotBeNull()
            .Invoke(segmented, [null, EventArgs.Empty]);
    }

    private static void AssertTransparent(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        ((ISolidColorBrush)brush).Color.ShouldBe(Colors.Transparent);
    }

    private static AtomUISegmentedItem[] GetItems(AtomUISegmented segmented)
    {
        return segmented.GetVisualDescendants()
                        .OfType<AtomUISegmentedItem>()
                        .ToArray();
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 320,
            Height  = 240,
            Content = content
        };

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
        }
    }
}
