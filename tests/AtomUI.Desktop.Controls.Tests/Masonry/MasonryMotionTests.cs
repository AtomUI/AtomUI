using System.Reflection;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIMasonry = AtomUI.Desktop.Controls.Masonry;

namespace AtomUI.Desktop.Controls.Tests.Masonry;

public class MasonryMotionTests
{
    static MasonryMotionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Theme_Provides_Motion_Durations_From_Tokens()
    {
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2,
            ColumnGap   = 0,
            RowGap      = 0,
            ItemsSource = new[] { "Alpha", "Beta" }
        };

        ShowInWindow(masonry, () =>
        {
            // 默认种子：MotionUnit=100ms, MotionBase=0 → Slow=300ms, Fast=100ms
            masonry.MotionDuration.ShouldBe(TimeSpan.FromMilliseconds(300));
            masonry.LeaveMotionDuration.ShouldBe(TimeSpan.FromMilliseconds(100));

            var panel = masonry.GetVisualDescendants()
                               .OfType<MasonryPanel>().Single();
            panel.MotionDuration.ShouldBe(TimeSpan.FromMilliseconds(300));
            panel.LeaveMotionDuration.ShouldBe(TimeSpan.FromMilliseconds(100));
        });
    }

    [Fact]
    public void RightToLeft_Mirrors_Item_Columns()
    {
        var first  = new Border { Height = 40 };
        var second = new Border { Height = 40 };
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2,
            ColumnGap   = 0,
            RowGap      = 0,
            Width       = 200
        };
        masonry.Items.Add(first);
        masonry.Items.Add(second);
        masonry.FlowDirection = Avalonia.Media.FlowDirection.RightToLeft;

        ShowInWindow(masonry, () =>
        {
            // RTL：第一项（列 0）应贴右侧，第二项（列 1）贴左侧
            var panel = masonry.GetVisualDescendants()
                               .OfType<MasonryPanel>().Single();
            var panelWidth = panel.Bounds.Width;
            Math.Round(panelWidth - first.Bounds.Right, 2).ShouldBe(0);
            Math.Round(second.Bounds.X, 2).ShouldBe(0);
            Math.Round(first.Bounds.Width - second.Bounds.Width, 2).ShouldBe(0);
        });
    }

    [Fact]
    public void LeftToRight_Keeps_First_Item_On_The_Left()
    {
        var first  = new Border { Height = 40 };
        var second = new Border { Height = 40 };
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2,
            ColumnGap   = 0,
            RowGap      = 0,
            Width       = 200
        };
        masonry.Items.Add(first);
        masonry.Items.Add(second);

        ShowInWindow(masonry, () =>
        {
            Math.Round(first.Bounds.X, 2).ShouldBe(0);
            second.Bounds.X.ShouldBeGreaterThan(first.Bounds.Right - 0.01);
        });
    }

    [Fact]
    public void New_Item_Starts_At_Zero_Opacity_With_Motion_Enabled()
    {
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2, ColumnGap = 0, RowGap = 0,
            MotionDuration = TimeSpan.FromSeconds(2) // 长时长冻结观察起点
        };
        var item = new Border { Height = 40 };
        var clock = new ManualClock();
        ShowInWindow(masonry, () =>
        {
            InjectClock(item, clock); // 冻结时钟，动画停在起点
            masonry.Items.Add(item);
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            item.Opacity.ShouldBe(0d); // 同步预置 + 时钟未推进
        });
    }

    [Fact]
    public void Zero_Duration_Skips_Appear_Motion()
    {
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2, ColumnGap = 0, RowGap = 0,
            MotionDuration = TimeSpan.Zero
        };
        var item = new Border { Height = 40 };
        ShowInWindow(masonry, () =>
        {
            masonry.Items.Add(item);
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            item.Opacity.ShouldBe(1d); // 未预置、未动画，直接基值
        });
    }

    [Fact]
    public void Style_Opacity_Is_Respected_After_Appear_Motion()
    {
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2, ColumnGap = 0, RowGap = 0,
            MotionDuration = TimeSpan.FromMilliseconds(40)
        };
        var item = new Border { Height = 40 };
        masonry.Styles.Add(new Style(s => s.OfType<Border>())
        {
            Setters = { new Setter(Border.OpacityProperty, 0.42) }
        });
        var clock = new ManualClock();
        ShowInWindow(masonry, () =>
        {
            InjectClock(item, clock);
            masonry.Items.Add(item);
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            item.Opacity.ShouldBe(0d);
            // 推进时钟越过动画时长：完成后释放动画值，回落到样式基值 0.42
            // （对齐 antd：motion 类移除后样式接管）。首次脉冲建立基线，第二次才推进。
            clock.Step(0);
            clock.Step(60);
            Dispatcher.UIThread.RunJobs();
            WaitForCondition(() => Math.Abs(item.Opacity - 0.42) < 0.01);
        });
    }

    [Fact]
    public void Moved_Item_Glides_From_Previous_Visual_Position()
    {
        var first  = new Border { Height = 40 };
        var second = new Border { Height = 40 };
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2, ColumnGap = 0, RowGap = 0, Width = 200,
            MotionDuration = TimeSpan.FromSeconds(2) // 长时长冻结观察起点
        };
        var clock = new ManualClock();
        ShowInWindow(masonry, () =>
        {
            InjectClock(first, clock);
            InjectClock(second, clock);
            masonry.Items.Add(first);
            masonry.Items.Add(second);
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            // 初始：first 左列 (0,0)，second 右列 (100,0)。
            // 先完成入场淡入（越过 2s），入场中的项不做位置过渡（antd 语义）
            clock.Step(0);
            clock.Step(2000);
            Dispatcher.UIThread.RunJobs();
            WaitForCondition(() => Math.Abs(second.Opacity - 1d) < 0.01);

            masonry.ColumnCount = 1; // second → (0,40)，触发滑动
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();

            // first 原地不动，无滑动变换
            first.RenderTransform.ShouldBeNull();
            // second 预置了旧位置偏移：旧 (100,0) - 新 (0,40) = translate(100,-40)，动画优先级持有
            var matrix = second.RenderTransform.ShouldNotBeNull().Value;
            Math.Round(matrix.M31, 1).ShouldBe(100);
            Math.Round(matrix.M32, 1).ShouldBe(-40);
            // 布局矩形已是最终位置（Bounds 不受 RenderTransform 影响）
            Math.Round(second.Bounds.X, 2).ShouldBe(0);
        });
    }

    [Fact]
    public void Glide_Releases_Transform_After_Completion()
    {
        var second = new Border { Height = 40 };
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2, ColumnGap = 0, RowGap = 0, Width = 200,
            MotionDuration = TimeSpan.FromMilliseconds(40)
        };
        var clock = new ManualClock();
        ShowInWindow(masonry, () =>
        {
            InjectClock(second, clock);
            masonry.Items.Add(new Border { Height = 40 });
            masonry.Items.Add(second);
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            // 先完成入场淡入
            clock.Step(0);
            clock.Step(60);
            Dispatcher.UIThread.RunJobs();
            WaitForCondition(() => Math.Abs(second.Opacity - 1d) < 0.01);

            masonry.ColumnCount = 1;
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            second.RenderTransform.ShouldNotBeNull();
            // 每个动画实例内部包裹独立时钟：首次脉冲只建基线，第二次才推进（80ms > 40ms 完成）
            clock.Step(120);
            clock.Step(200);
            Dispatcher.UIThread.RunJobs();
            WaitForCondition(() => second.RenderTransform is null);
            second.RenderTransform.ShouldBeNull();
        });
    }

    [Fact]
    public void Zero_Duration_Skips_Glide()
    {
        var second = new Border { Height = 40 };
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2, ColumnGap = 0, RowGap = 0, Width = 200,
            MotionDuration = TimeSpan.Zero
        };
        masonry.Items.Add(new Border { Height = 40 });
        masonry.Items.Add(second);
        ShowInWindow(masonry, () =>
        {
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            masonry.ColumnCount = 1;
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            second.RenderTransform.ShouldBeNull();
        });
    }

    [Fact]
    public void Removed_Item_Fades_Out_In_Ghost_Layer_At_Old_Position()
    {
        var first  = new Border { Height = 40 };
        var second = new Border { Height = 40 };
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 1, ColumnGap = 0, RowGap = 0, Width = 200,
            LeaveMotionDuration = TimeSpan.FromSeconds(2) // 冻结观察
        };
        masonry.Items.Add(first);
        masonry.Items.Add(second);

        ShowInWindow(masonry, () =>
        {
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            var ghostLayer = masonry.GetVisualDescendants()
                .Single(v => v.Name == "PART_MotionGhostLayer");
            var panel = masonry.GetVisualDescendants()
                .OfType<MasonryPanel>().Single();
            var secondVisualY = second.Bounds.Y; // 移除前视觉位置

            masonry.Items.Remove(second);
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();

            // 容器被 ghost 层托管在原位置，保留语义 marker（antd leave 期间节点仍在 DOM）
            second.GetVisualAncestors().Any().ShouldBeTrue("container should stay in visual tree");
            second.Classes.Contains("semantic-item").ShouldBeTrue();
            var host = second.GetVisualAncestors()
                .OfType<Border>()
                .Single(b => b.Parent == ghostLayer);
            Math.Round(Avalonia.Controls.Canvas.GetLeft(host), 2).ShouldBe(0);
            Math.Round(Avalonia.Controls.Canvas.GetTop(host) - secondVisualY - panel.Bounds.Y, 2).ShouldBe(0);
        });
    }

    [Fact]
    public void Ghost_Is_Released_After_FadeOut()
    {
        var second = new Border { Height = 40 };
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 1, ColumnGap = 0, RowGap = 0, Width = 200,
            LeaveMotionDuration = TimeSpan.FromMilliseconds(40)
        };
        masonry.Items.Add(new Border { Height = 40 });
        masonry.Items.Add(second);
        var clock = new ManualClock();
        ShowInWindow(masonry, () =>
        {
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            // ghost host 由控件创建，其动画时钟继承自 ghost 层：在移除前注入冻结时钟
            var ghostLayer = masonry.GetVisualDescendants()
                .Single(v => v.Name == "PART_MotionGhostLayer");
            InjectClock(ghostLayer, clock);

            masonry.Items.Remove(second);
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            second.GetVisualAncestors().Any().ShouldBeTrue("container should stay in visual tree");

            // 双脉冲：首次建基线，第二次推进越过 40ms 完成 → ghost 释放
            clock.Step(0);
            clock.Step(200);
            Dispatcher.UIThread.RunJobs();
            WaitForCondition(() => !second.GetVisualAncestors().Any());
            second.GetVisualAncestors().Any().ShouldBeFalse("container should be released");
        });
    }

    [Fact]
    public void Zero_Leave_Duration_Removes_Container_Immediately()
    {
        var second = new Border { Height = 40 };
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 1, ColumnGap = 0, RowGap = 0, Width = 200,
            LeaveMotionDuration = TimeSpan.Zero
        };
        masonry.Items.Add(new Border { Height = 40 });
        masonry.Items.Add(second);

        ShowInWindow(masonry, () =>
        {
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            masonry.Items.Remove(second);
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            second.GetVisualAncestors().Any().ShouldBeFalse("container should be released");
            var ghostLayer = masonry.GetVisualDescendants()
                .SingleOrDefault(v => v.Name == "PART_MotionGhostLayer");
            if (ghostLayer is Canvas canvas)
            {
                canvas.Children.Count.ShouldBe(0);
            }
        });
    }

    [Fact]
    public void Readding_A_Ghosted_Item_Releases_The_Ghost_First()
    {
        var second = new Border { Height = 40 };
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 1, ColumnGap = 0, RowGap = 0, Width = 200,
            LeaveMotionDuration = TimeSpan.FromSeconds(2)
        };
        masonry.Items.Add(new Border { Height = 40 });
        masonry.Items.Add(second);

        ShowInWindow(masonry, () =>
        {
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            masonry.Items.Remove(second);
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            second.GetVisualAncestors().Any().ShouldBeTrue("container should stay in visual tree");

            // ghost 期内重加入：容器回到面板并参与布局，ghost 先释放（不抛"已有视觉父级"）
            masonry.Items.Add(second);
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            second.Bounds.Width.ShouldBeGreaterThan(0);
            var ghostLayer = (Canvas)masonry.GetVisualDescendants()
                .Single(v => v.Name == "PART_MotionGhostLayer");
            ghostLayer.Children.Count.ShouldBe(0);
        });
    }

    [Fact]
    public void Clear_Items_Ghosts_All_Removed_Containers()
    {
        var first  = new Border { Height = 40 };
        var second = new Border { Height = 40 };
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 1, ColumnGap = 0, RowGap = 0, Width = 200,
            LeaveMotionDuration = TimeSpan.FromSeconds(2)
        };
        masonry.Items.Add(first);
        masonry.Items.Add(second);

        ShowInWindow(masonry, () =>
        {
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            masonry.Items.Clear();
            Dispatcher.UIThread.RunJobs();
            masonry.UpdateLayout();
            var ghostLayer = (Canvas)masonry.GetVisualDescendants()
                .Single(v => v.Name == "PART_MotionGhostLayer");
            ghostLayer.Children.Count.ShouldBe(2);
            first.GetVisualAncestors().Any().ShouldBeTrue("first should stay in ghost layer");
            second.GetVisualAncestors().Any().ShouldBeTrue("container should stay in visual tree");
        });
    }

    private static void WaitForCondition(Func<bool> condition, int timeoutMs = 3000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }
            Thread.Sleep(20);
        }
        condition().ShouldBeTrue("condition not met within timeout");
    }

    private static void InjectClock(Animatable target, ManualClock clock)
    {
        // Avalonia 的动画时钟为 internal；反射仅限测试程序集（先例：ContentExpansionMotionTests）。
        var clockProperty = typeof(Animatable).GetProperty("Clock",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        clockProperty.SetValue(target, clock.Instance);
    }

    private sealed class ManualClock
    {
        private static readonly Type ClockType =
            typeof(Animation).Assembly.GetType("Avalonia.Animation.ClockBase", true)!;
        private static readonly MethodInfo Pulse =
            ClockType.GetMethod("Pulse", BindingFlags.Instance | BindingFlags.NonPublic)!;

        public object Instance { get; } = Activator.CreateInstance(ClockType, nonPublic: true)!;

        public void Step(int milliseconds) =>
            Pulse.Invoke(Instance, [TimeSpan.FromMilliseconds(milliseconds)]);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow { Width = 640, Height = 480, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        assertion();
        window.Close();
    }
}
