using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISplitButton = AtomUI.Desktop.Controls.SplitButton;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Buttons;

// 回归测试：锁定「hover 次按钮时控件几何与分隔线状态稳定」。用真实 headless MouseMove
// 驱动（而非手动 RaiseEvent），让 Avalonia 的 hover 状态机与布局后重评估参与运行：
// 1. 次按钮被指向时 Bounds 必须完全不变（曾因左移 1px 覆盖接缝导致可见位移与边缘快速闪动）；
// 2. 穿越接缝一次，分隔线可见性最多翻转 2 次，驻留时不得继续翻转（事件风暴/振荡环防护）。
public class SplitButtonSeparatorStabilityTests
{
    static SplitButtonSeparatorStabilityTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Diagnose_Separator_Visibility_Transitions_While_Pointer_Crosses_Seam()
    {
        var splitButton = new AtomUISplitButton
        {
            Width = 160,
            Height = 36,
            Content = "Split Action",
            IsPrimaryButtonType = true,
            IsMotionEnabled = false,
            IsWaveSpiritEnabled = false
        };

        ShowInWindow(splitButton, window =>
        {
            var secondaryButton = window.GetVisualDescendants()
                                        .OfType<AtomUI.Desktop.Controls.Button>()
                                        .Single(button => button.Name == "PART_SecondaryButton");
            var primaryButton = window.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.Button>()
                                      .Single(button => button.Name == "PART_PrimaryButton");

            var seamStart = primaryButton.Bounds.Right;                  // 接缝条带左缘
            var secondaryLeftBeforeHover = secondaryButton.Bounds.Left;  // 应为 seamStart + strip
            var centerY = secondaryButton.Bounds.Center.Y;

            var transitions = 0;
            var lastVisible = splitButton.IsSeparatorVisible;

            void MoveTo(double x)
            {
                window.MouseMove(new Point(x, centerY));
                for (var i = 0; i < 4; i++)
                {
                    Dispatcher.UIThread.RunJobs();
                }

                var visible = splitButton.IsSeparatorVisible;
                if (visible != lastVisible)
                {
                    transitions++;
                    lastVisible = visible;
                }
            }

            // 从主按钮内部逐步穿越接缝进入次按钮，再原路返回。
            var steps = new List<double>();
            for (var x = seamStart - 6; x <= secondaryLeftBeforeHover + 6; x += 0.5)
            {
                steps.Add(x);
            }

            for (var i = steps.Count - 1; i >= 0; i--)
            {
                steps.Add(steps[i]);
            }

            foreach (var x in steps)
            {
                MoveTo(x);
            }

            // 一次进入 + 一次离开：分隔线可见性最多翻转 2 次。
            // 若存在振荡环（布局改变 hover，hover 再改布局），翻转次数会远超 2。
            transitions.ShouldBeLessThanOrEqualTo(2,
                $"separator visibility flipped {transitions} times while crossing the seam once; " +
                $"hover storm suspected. secondary left before hover: {secondaryLeftBeforeHover}, " +
                $"after: {secondaryButton.Bounds.Left}");

            // 驻留在次按钮边缘内侧：布局稳定后可见性不得继续翻转。
            MoveTo(secondaryLeftBeforeHover + 0.25);
            var stableTransitions = transitions;
            for (var i = 0; i < 10; i++)
            {
                Dispatcher.UIThread.RunJobs();
                var visible = splitButton.IsSeparatorVisible;
                if (visible != lastVisible)
                {
                    transitions++;
                    lastVisible = visible;
                }
            }

            transitions.ShouldBe(stableTransitions,
                "separator visibility kept flipping without pointer movement; layout/hover feedback loop confirmed");
        });
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    public void Diagnose_Separator_Stability_While_Pointer_Rests_On_Secondary_Edge(
        bool enableMotion, bool enableWave, bool useLayoutRounding)
    {
        var splitButton = new AtomUISplitButton
        {
            Width = 160,
            Height = 36,
            Content = "Split Action",
            IsPrimaryButtonType = true,
            IsMotionEnabled = enableMotion,
            IsWaveSpiritEnabled = enableWave,
            UseLayoutRounding = useLayoutRounding
        };

        ShowInWindow(splitButton, window =>
        {
            var secondaryButton = window.GetVisualDescendants()
                                        .OfType<AtomUI.Desktop.Controls.Button>()
                                        .Single(button => button.Name == "PART_SecondaryButton");
            var primaryButton = window.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.Button>()
                                      .Single(button => button.Name == "PART_PrimaryButton");

            var secondaryLeftBeforeHover = secondaryButton.Bounds.Left;
            var centerY = secondaryButton.Bounds.Center.Y;

            // 指针驻留在次按钮左缘内侧 0.25 DIP（最贴近视频里"移到按钮边缘"的场景）。
            window.MouseMove(new Point(secondaryLeftBeforeHover + 0.25, centerY));
            PumpFrames(window);

            var lastVisible = splitButton.IsSeparatorVisible;
            var transitions = 0;
            // 无指针移动，仅推进帧（motion transition / wave 动画持续运行）。
            for (var i = 0; i < 30; i++)
            {
                PumpFrames(window);
                var visible = splitButton.IsSeparatorVisible;
                if (visible != lastVisible)
                {
                    transitions++;
                    lastVisible = visible;
                }
            }

            transitions.ShouldBe(0,
                $"motion={enableMotion} wave={enableWave} rounding={useLayoutRounding}: " +
                $"separator flipped {transitions} times while the pointer rests on the secondary edge");
        });
    }

    [Fact]
    public void Secondary_Button_Keeps_Identical_Geometry_While_Pointed_Over()
    {
        // 回归：hover 次按钮不得改变次按钮的位置或尺寸。曾用「次按钮左移 1px 覆盖接缝」
        // 实现分隔线隐藏，导致 hover 时按钮可见位移、边缘处 Enter/Exit 抖动放大为快速闪动。
        // 分隔线的 hover 隐藏必须由颜色涌现（线色 = 次按钮 hover 背景 token），不允许动布局。
        var splitButton = new AtomUISplitButton
        {
            Width = 160,
            Height = 36,
            Content = "Split Action",
            IsPrimaryButtonType = true,
            IsMotionEnabled = false,
            IsWaveSpiritEnabled = false
        };

        ShowInWindow(splitButton, window =>
        {
            var secondaryButton = window.GetVisualDescendants()
                                        .OfType<AtomUI.Desktop.Controls.Button>()
                                        .Single(button => button.Name == "PART_SecondaryButton");
            var boundsBeforeHover = secondaryButton.Bounds;
            var center = boundsBeforeHover.Center;

            window.MouseMove(center);
            PumpFrames(window);

            splitButton.IsSeparatorVisible.ShouldBeFalse();
            secondaryButton.Bounds.ShouldBe(boundsBeforeHover,
                "hover must not shift or resize the secondary button");

            window.MouseMove(center.WithX(center.X - 1));
            PumpFrames(window);

            secondaryButton.Bounds.ShouldBe(boundsBeforeHover);
        });
    }

    private static void PumpFrames(AvaloniaWindow window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width = 240,
            Height = 160,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
