using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Data;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Notifications;

public class NotificationCardThemeTests
{
    static NotificationCardThemeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Notification_Default_Expiration_Matches_Ant_Default_Duration()
    {
        var notification = new Notification("Notification Title", "Notification body");

        notification.Expiration.ShouldBe(TimeSpan.FromSeconds(4.5));
    }

    [Fact]
    public void Notification_Card_Motion_Does_Not_Invalidate_Layout()
    {
        using var manager = new WindowNotificationManager();
        var card = new NotificationCard(manager)
        {
            Title = "Notification Title",
            Content = "Notification body"
        };

        using var clock = new AtomUI.Desktop.Controls.Tests.Motion.ManualMotionClock();
        ShowInWindow(card, window =>
        {
            var frame = card.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "Frame");
            window.UpdateLayout();
            clock.Step(0);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            frame.IsMeasureValid.ShouldBeTrue();
            card.IsMeasureValid.ShouldBeTrue();
            clock.Step(50);
            frame.IsMeasureValid.ShouldBeTrue();
            card.IsMeasureValid.ShouldBeTrue();
        });
    }

    [Fact]
    public void Notification_Template_Wraps_Only_The_Content_Layout_In_A_Stack_Transition_Snapshot_Host()
    {
        using var manager = new WindowNotificationManager();
        var card = new NotificationCard(manager)
        {
            Title = "Notification Title",
            Content = "Notification body",
            IsMotionEnabled = false
        };

        ShowInWindow(card, () =>
        {
            var frame = card.GetVisualDescendants().OfType<Border>().Single(item => item.Name == "Frame");
            var host = frame.GetVisualDescendants()
                            .OfType<FeedbackStackTransitionSnapshotHost>()
                            .Single();
            var layout = card.GetVisualDescendants()
                             .OfType<Avalonia.Controls.Grid>()
                             .Single(item => item.Name == "PART_Layout");

            host.Child.ShouldBeSameAs(layout);
            frame.Child.ShouldBeSameAs(host);
        });
    }

    [Fact]
    public void Stack_Transition_Snapshot_Host_Restores_Content_State_And_Releases_Its_Bitmap()
    {
        var content = new Border
        {
            Width = 120,
            Height = 60,
            Opacity = 0.75,
            IsHitTestVisible = true,
            Background = Brushes.Red
        };
        var host = new FeedbackStackTransitionSnapshotHost
        {
            Child = content
        };

        ShowInWindow(host, () =>
        {
            host.TryBeginSnapshot().ShouldBeTrue();
            host.IsSnapshotActive.ShouldBeTrue();
            host.SnapshotBitmap.ShouldNotBeNull();
            content.Opacity.ShouldBe(0);
            content.IsHitTestVisible.ShouldBeFalse();

            host.ReleaseSnapshot();

            host.IsSnapshotActive.ShouldBeFalse();
            host.SnapshotBitmap.ShouldBeNull();
            content.Opacity.ShouldBe(0.75);
            content.IsHitTestVisible.ShouldBeTrue();
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Stack_Transition_Snapshot_Uses_Arranged_Size_For_Stretched_Content(double scaling)
    {
        var content = new Avalonia.Controls.Grid
        {
            Children = { new Avalonia.Controls.TextBlock { Text = "Short notification" } }
        };
        var host = new FeedbackStackTransitionSnapshotHost { Child = content };

        ShowInWindow(host, window =>
        {
            window.SetRenderScaling(scaling);
            window.UpdateLayout();
            content.Bounds.Width.ShouldBeGreaterThan(content.DesiredSize.Width);
            host.TryBeginSnapshot().ShouldBeTrue();

            host.SnapshotBitmap!.PixelSize.ShouldBe(PixelSize.FromSize(content.Bounds.Size, scaling),
                "A snapshot of stretched content must cover its arranged bounds without magnifying its intrinsic desired size.");
            host.ReleaseSnapshot();
        });
    }

    [Fact]
    public void Notification_Card_Releases_The_Content_Snapshot_When_Animated_Transform_Reaches_Its_Target()
    {
        using var manager = new WindowNotificationManager();
        var card = new NotificationCard(manager)
        {
            Title = "Notification Title",
            Content = "Notification body",
            IsMotionEnabled = true
        };

        ShowInWindow(card, () =>
        {
            var host = card.GetVisualDescendants()
                           .OfType<FeedbackStackTransitionSnapshotHost>()
                           .Single();
            var snapshotItem = (IFeedbackStackTransitionSnapshotItem)card;
            var target = BuildTransform(0.94, 8);
            var intermediate = BuildTransform(0.97, 4);

            snapshotItem.TryBeginStackCollapseSnapshot().ShouldBeTrue();
            card.SetCurrentValue(Visual.RenderTransformProperty, target);
            snapshotItem.ArmStackCollapseSnapshot(target);
            host.IsSnapshotActive.ShouldBeTrue();

            card.SetValue(Visual.RenderTransformProperty, intermediate, BindingPriority.Animation);
            host.IsSnapshotActive.ShouldBeTrue();

            card.SetValue(Visual.RenderTransformProperty, target, BindingPriority.Animation);
            host.IsSnapshotActive.ShouldBeFalse();
            host.SnapshotBitmap.ShouldBeNull();
        });
    }

    [Fact]
    public void Open_Notification_Without_Type_Does_Not_Create_Default_Icon()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = false
        };

        ShowInWindow(manager, window =>
        {
            manager.Show(new Notification(
                title: "Notification Title",
                content: "Notification body",
                expiration: TimeSpan.Zero));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var card = manager.GetVisualDescendants().OfType<NotificationCard>().Single();
            card.Icon.ShouldBeNull();

            var iconPresenter = GetSemanticIconPresenter(card);
            iconPresenter.IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Typed_Notification_Keeps_Ant_Type_Icon()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = false
        };

        ShowInWindow(manager, window =>
        {
            manager.Show(new Notification(
                title: "Notification Title",
                content: "Notification body",
                type: NotificationType.Information,
                expiration: TimeSpan.Zero));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var card = manager.GetVisualDescendants().OfType<NotificationCard>().Single();
            card.Icon.ShouldBeOfType<InfoCircleFilled>();

            var iconPresenter = GetSemanticIconPresenter(card);
            iconPresenter.IsVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void NotificationType_Change_From_Default_To_Information_Creates_Type_Icon()
    {
        using var manager = new WindowNotificationManager();
        var card = new NotificationCard(manager)
        {
            Title           = "Notification Title",
            Content         = "Notification body",
            IsMotionEnabled = false
        };

        ShowInWindow(card, () =>
        {
            card.Icon.ShouldBeNull();

            card.NotificationType = NotificationType.Information;
            Dispatcher.UIThread.RunJobs();

            card.Icon.ShouldBeOfType<InfoCircleFilled>();

            var iconPresenter = GetSemanticIconPresenter(card);
            iconPresenter.IsVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void NotificationType_Change_From_Information_To_Default_Removes_Template_Type_Icon()
    {
        using var manager = new WindowNotificationManager();
        var card = new NotificationCard(manager)
        {
            Title            = "Notification Title",
            Content          = "Notification body",
            NotificationType = NotificationType.Information,
            IsMotionEnabled  = false
        };

        ShowInWindow(card, () =>
        {
            card.Icon.ShouldBeOfType<InfoCircleFilled>();

            card.NotificationType = NotificationType.Default;
            Dispatcher.UIThread.RunJobs();

            card.Icon.ShouldBeNull();

            var iconPresenter = GetSemanticIconPresenter(card);
            iconPresenter.IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Custom_Icon_Is_Not_Replaced_By_NotificationType_Change()
    {
        using var manager = new WindowNotificationManager();
        var customIcon = new CloseOutlined();
        var card = new NotificationCard(manager)
        {
            Title           = "Notification Title",
            Content         = "Notification body",
            Icon            = customIcon,
            IsMotionEnabled = false
        };

        ShowInWindow(card, () =>
        {
            card.NotificationType = NotificationType.Success;
            Dispatcher.UIThread.RunJobs();

            card.Icon.ShouldBeSameAs(customIcon);

            var iconPresenter = GetSemanticIconPresenter(card);
            iconPresenter.IsVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void Close_Button_Uses_Ant_Notification_Hover_And_Pressed_Visuals()
    {
        using var manager = new WindowNotificationManager();
        var card = new NotificationCard(manager)
        {
            Title           = "Notification Title",
            Content         = "Notification body",
            IsMotionEnabled = false
        };

        ShowInWindow(card, () =>
        {
            var expectedSize = GetThemeResource<double>(NotificationCardTokenKind.NotificationCloseButtonSize);
            var closeButton  = card.GetVisualDescendants()
                                   .OfType<IconButton>()
                                   .Single(item => item.Name == "PART_CloseButton");

            closeButton.IsMotionEnabled = false;
            closeButton.Width.ShouldBe(expectedSize, 0.5);
            closeButton.Height.ShouldBe(expectedSize, 0.5);

            BrushShouldHaveSameColor(closeButton.IconBrush, GetThemeColor(SharedTokenKind.ColorIcon));

            ((IPseudoClasses)closeButton.Classes).Set(":pointerover", true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(closeButton.Background, GetThemeColor(SharedTokenKind.ColorBgTextHover));
            BrushShouldHaveSameColor(closeButton.IconBrush, GetThemeColor(SharedTokenKind.ColorIconHover));

            ((IPseudoClasses)closeButton.Classes).Set(":pressed", true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(closeButton.Background, GetThemeColor(SharedTokenKind.ColorBgTextActive));
            BrushShouldHaveSameColor(closeButton.IconBrush, GetThemeColor(SharedTokenKind.ColorIconHover));
        });
    }

    [Fact]
    public void Progress_Bar_Gradient_Uses_Primary_Border_Hover_To_Primary()
    {
        var progressBrush = GetThemeResource<IBrush>(NotificationCardTokenKind.NotificationProgressBg);
        var gradient      = progressBrush.ShouldBeAssignableTo<IGradientBrush>();

        gradient.GradientStops.Count.ShouldBe(2);
        gradient.GradientStops[0].Color.ShouldBe(GetThemeColor(SharedTokenKind.ColorPrimaryBorderHover));
        gradient.GradientStops[0].Offset.ShouldBe(0);
        gradient.GradientStops[1].Color.ShouldBe(GetThemeColor(SharedTokenKind.ColorPrimary));
        gradient.GradientStops[1].Offset.ShouldBe(1);
    }

    [Fact]
    public void Notice_Geometry_Tokens_Align_With_Upstream_Notice_Structure()
    {
        // 间距全部直接取全局 Token，不做任何比例缩放：
        //   卡片内边距     = paddingMD(纵向) / paddingLG(横向)
        //   图标间距       = marginSM；标题与描述间距 = marginXS；操作组上边距 = marginSM
        //   标题右侧留白   = paddingLG；关闭按钮偏移 = paddingMD(纵向) / paddingLG(横向)
        var horizontalPadding = GetThemeResource<double>(SharedTokenKind.UniformlyPaddingLG);
        var verticalPadding   = GetThemeResource<double>(SharedTokenKind.UniformlyPaddingMD);
        var sectionGap        = GetThemeResource<double>(SharedTokenKind.UniformlyMarginXS);
        var iconGap           = GetThemeResource<double>(SharedTokenKind.UniformlyMarginSM);

        // 卡片内边距上下对称：纵向 paddingMD、横向 paddingLG。
        var notificationPadding = GetThemeResource<Thickness>(NotificationCardTokenKind.NotificationPadding);
        ThicknessShouldBe(
            notificationPadding,
            new Thickness(horizontalPadding, verticalPadding, horizontalPadding, verticalPadding));

        // 标题与描述之间的纵向间距。
        GetThemeResource<double>(NotificationCardTokenKind.NotificationSectionSpacing).ShouldBe(sectionGap, 0.001);

        // 操作组与上方内容之间的间距。
        var actionsMargin = GetThemeResource<Thickness>(NotificationCardTokenKind.NotificationActionsMargin);
        ThicknessShouldBe(actionsMargin, new Thickness(0, iconGap, 0, 0));

        // 关闭按钮是贴卡片右上角的覆盖层：纵向离顶 paddingMD、横向离右 paddingLG。
        var closeMargin = GetThemeResource<Thickness>(NotificationCardTokenKind.NotificationCloseButtonMargin);
        ThicknessShouldBe(closeMargin, new Thickness(0, verticalPadding, horizontalPadding, 0));

        // 标题右侧留白，避免文本压到关闭按钮。
        var titlePadding = GetThemeResource<Thickness>(NotificationCardTokenKind.NotificationTitlePadding);
        ThicknessShouldBe(titlePadding, new Thickness(0, 0, horizontalPadding, 0));

        // 图标与右侧内容之间的横向间距。
        var iconMargin = GetThemeResource<Thickness>(NotificationCardTokenKind.NotificationIconMargin);
        ThicknessShouldBe(iconMargin, new Thickness(0, 0, iconGap, 0));

        // 进度条贴卡片底边，左右各内缩一个圆角半径。
        var radius = GetThemeResource<CornerRadius>(SharedTokenKind.BorderRadiusLG).TopLeft;
        var progressMargin = GetThemeResource<Thickness>(NotificationCardTokenKind.NotificationProgressMargin);
        ThicknessShouldBe(progressMargin, new Thickness(radius, 0, radius, 0));
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        ShowInWindow(content, _ => assertion());
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 500,
            Height  = 300,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            window.UpdateLayout();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static ITransform BuildTransform(double scale, double translateY)
    {
        var builder = new TransformOperations.Builder(2);
        builder.AppendScale(scale, scale);
        builder.AppendTranslate(0, translateY);
        return builder.Build();
    }

    private static IconPresenter GetSemanticIconPresenter(NotificationCard card)
    {
        return card.GetVisualDescendants()
                   .OfType<IconPresenter>()
                   .Single(item => item.Name == "IconPresenter" && ReferenceEquals(item.Icon, card.Icon));
    }

    private static Color GetThemeColor(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        return ToColor(value);
    }

    private static void BrushShouldHaveSameColor(IBrush? actual, Color expected)
    {
        actual.ShouldNotBeNull();
        ToColor(actual).ShouldBe(expected);
    }

    private static void ThicknessShouldBe(Thickness actual, Thickness expected)
    {
        actual.Left.ShouldBe(expected.Left, 0.001);
        actual.Top.ShouldBe(expected.Top, 0.001);
        actual.Right.ShouldBe(expected.Right, 0.001);
        actual.Bottom.ShouldBe(expected.Bottom, 0.001);
    }

    private static Color ToColor(object? value)
    {
        return value switch
        {
            Color color => color,
            ISolidColorBrush brush => brush.Color,
            _ => throw new ShouldAssertException($"Expected color or solid brush, got {value?.GetType().FullName ?? "<null>"}")
        };
    }
}
