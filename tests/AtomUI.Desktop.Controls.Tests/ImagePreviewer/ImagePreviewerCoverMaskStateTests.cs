using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

public class ImagePreviewerCoverMaskStateTests
{
    public ImagePreviewerCoverMaskStateTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Cover_Mask_Has_Zero_Opacity_In_Normal_State()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var cover = new ImagePreviewerCover
            {
                Width = 100,
                Height = 100,
                IsShowCoverMask = true,
                Content = "preview",
            };
            var window = new AvaloniaWindow { Content = cover, Width = 200, Height = 200 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                cover.ApplyTemplate();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                var mask = cover.GetVisualDescendants()
                    .OfType<Border>()
                    .Single(b => b.Name == "Mask");
                mask.ShouldNotBeNull();
                cover.MaskOpacity.ShouldBe(0);
                mask.Opacity.ShouldBe(0);
                mask.Background.ShouldNotBeNull();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Cover_Mask_Indicator_Content_Is_Inside_Mask()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var cover = new ImagePreviewerCover
            {
                Width = 100,
                Height = 100,
                IsShowCoverMask = true,
                Content = "preview",
            };
            var window = new AvaloniaWindow { Content = cover, Width = 200, Height = 200 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                cover.ApplyTemplate();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                var mask = cover.GetVisualDescendants()
                    .OfType<Border>()
                    .Single(b => b.Name == "Mask");
                // 指示器必须挂在 Mask 内部：常态 Opacity=0 时随遮罩一起隐藏。
                // raw string 无默认 DataTemplate，断言 ContentPresenter 已挂在 Mask 下即可；
                // 真实指示器（眼睛 + Preview 文案）由 owner 的 CoverIndicatorContentTemplate 提供。
                mask.GetVisualDescendants()
                    .OfType<ContentPresenter>()
                    .ShouldContain(p => ReferenceEquals(p.Content, "preview"));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void FullPreviewer_Mask_Spans_Entire_Root_Including_Padding()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var previewer = new global::AtomUI.Desktop.Controls.ImagePreviewer
            {
                Width = 200,
                Height = 200,
                Padding = new Thickness(4),
                CornerRadius = new CornerRadius(8),
            };
            var window = new AvaloniaWindow { Content = previewer, Width = 400, Height = 400 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                previewer.ApplyTemplate();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                var mask = previewer.GetVisualDescendants()
                    .OfType<Border>()
                    .Single(b => b.Name == "Mask");
                // ant 对齐基线：cover 是 root 的 absolute inset:0 覆盖层，遮罩必须
                // 铺满整个 root（含 Padding 环），hover 时才能把白 padding 压成灰框。
                // Mask 挂在 cover（位于 root padding 内）之下，坐标须换算回 root 空间比较。
                var maskInRoot = mask.TranslatePoint(new Point(0, 0), previewer).ShouldNotBeNull();
                var maskRectInRoot = new Rect(maskInRoot, mask.Bounds.Size);
                maskRectInRoot.ShouldBe(new Rect(new Point(0, 0), previewer.Bounds.Size));
                // 上游 root 的 overflow:hidden + border-radius 把 inset:0 cover 裁成圆角；
                // 遮罩以负 Margin 越过 owner padding，无法被 owner 圆角裁剪覆盖，
                // 必须直接把 owner CornerRadius 涂到 Mask 上（经 OwnerCornerRadius 中继）。
                mask.CornerRadius.ShouldBe(new CornerRadius(8));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void PointerOver_Raises_Mask_Opacity_To_One()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var cover = new ImagePreviewerCover
            {
                Width = 100,
                Height = 100,
                IsShowCoverMask = true,
                IsMotionEnabled = false,
            };
            var window = new AvaloniaWindow { Content = cover, Width = 200, Height = 200 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                cover.ApplyTemplate();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                cover.MaskOpacity.ShouldBe(0);
                ((IPseudoClasses)cover.Classes).Set(":pointerover", true);
                Dispatcher.UIThread.RunJobs();
                cover.MaskOpacity.ShouldBe(1);
                ((IPseudoClasses)cover.Classes).Set(":pointerover", false);
                Dispatcher.UIThread.RunJobs();
                cover.MaskOpacity.ShouldBe(0);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void GroupPreviewer_Mask_Spans_Entire_Root_Including_Padding()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var group = new global::AtomUI.Desktop.Controls.ImageGroupPreviewer
            {
                Width = 200,
                Height = 200,
                Padding = new Thickness(4),
                CornerRadius = new CornerRadius(8),
                CoverWidth = 192,
                CoverHeight = 192,
                ItemsSource = new List<AtomUI.Desktop.Controls.ImagePreviewItem>
                {
                    new(AtomUI.Controls.ImageSource.Parse("avares://AtomUI.Tests/Assets/first.png")),
                },
            };
            var window = new AvaloniaWindow { Content = group, Width = 400, Height = 400 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                group.ApplyTemplate();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                var mask = group.GetVisualDescendants()
                    .OfType<Border>()
                    .Single(b => b.Name == "Mask");
                var maskInRoot = mask.TranslatePoint(new Point(0, 0), group).ShouldNotBeNull();
                var maskRectInRoot = new Rect(maskInRoot, mask.Bounds.Size);
                // 组封面 root 的 padding 环只在 root 边缘；单张封面 (200-4*2=192) 的
                // 遮罩铺满“封面 + root padding”，因为封面本身已铺满 root 内区。
                maskRectInRoot.ShouldBe(new Rect(new Point(0, 0), group.Bounds.Size));
                // 组封面 DataTemplate 经 RelativeSource AncestorType 中继，圆角同样跟随 owner。
                mask.CornerRadius.ShouldBe(new CornerRadius(8));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Mask_Extends_Beyond_Every_Clipping_Ancestor_When_Spanning_Padding_Ring()
    {
        // 回归锁：上一版缺陷中遮罩虽被负 Margin 铺到 root 全域，但 ImagePreviewerCover
        // ControlTheme 的 ClipToBounds=True Setter 在合成层把它裁回 cover 内区，padding
        // 环永远压不暗（用户录屏复现）。合成层裁剪不体现在 Bounds 上，headless 渲染探针
        // 也无法观察（Visual.Render 只画控件自身内容且绕过合成裁剪）；按合成器与
        // VisualExtensions 的裁剪传播规则做结构性锁定：遮罩矩形换算到每个 ClipToBounds
        // 祖先的坐标空间后必须仍被其边界完整包含，否则外环在该祖先处被裁掉。
        Dispatcher.UIThread.Invoke(() =>
        {
            var cover = new ImagePreviewerCover
            {
                Width = 80,
                Height = 80,
                IsShowCoverMask = true,
                IsMotionEnabled = false,
                OwnerPadding = new Thickness(10),
            };
            var window = new AvaloniaWindow { Content = cover, Width = 200, Height = 200 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                cover.ApplyTemplate();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                var mask = cover.GetVisualDescendants()
                    .OfType<Border>()
                    .Single(b => b.Name == "Mask");
                mask.Bounds.ShouldBe(new Rect(-10, -10, 100, 100));

                // 悬停态触发遮罩绘制，模拟用户录屏中的复现路径。
                ((IPseudoClasses)cover.Classes).Set(":pointerover", true);
                Dispatcher.UIThread.RunJobs();
                mask.Opacity.ShouldBe(1);

                foreach (var ancestor in mask.GetVisualAncestors())
                {
                    if (!ancestor.ClipToBounds)
                    {
                        continue;
                    }
                    var ancestorType = ancestor.GetType().Name;
                    var maskTopLeft = mask.TranslatePoint(new Point(0, 0), ancestor);
                    maskTopLeft.ShouldNotBeNull($"无法换算到裁剪祖先 {ancestorType} 的坐标空间");
                    var maskRectInAncestor = new Rect(maskTopLeft.Value, mask.Bounds.Size);
                    var ancestorBounds = new Rect(new Point(0, 0), ancestor.Bounds.Size);
                    ancestorBounds.Contains(maskRectInAncestor)
                        .ShouldBeTrue(
                            $"裁剪祖先 {ancestorType}（{ancestorBounds}）裁掉了遮罩外环（{maskRectInAncestor}）");
                }
            }
            finally
            {
                window.Close();
            }
        });
    }
}
