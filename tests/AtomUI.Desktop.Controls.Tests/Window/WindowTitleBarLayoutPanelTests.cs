using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class WindowTitleBarLayoutPanelTests
{
    static WindowTitleBarLayoutPanelTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void WindowCenter_Stays_On_The_Full_Frame_Center_With_Asymmetric_Operations()
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            Padding           = new Thickness(10, 0),
            HorizontalSpacing = 8,
            TitleAlignment    = Desktop.Controls.WindowTitleBarTitleAlignment.WindowCenter
        };
        var leading  = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Leading, 80, 24);
        var title    = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);
        var trailing = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Trailing, 20, 24);

        Layout(panel, 400, 40);

        leading.Bounds.ShouldBe(new Rect(10, 0, 80, 40));
        title.Bounds.ShouldBe(new Rect(150, 0, 100, 40));
        trailing.Bounds.ShouldBe(new Rect(370, 0, 20, 40));
        title.Bounds.Center.X.ShouldBe(200);
    }

    [Fact]
    public void MacOS_Auto_Uses_WindowCenter_With_Asymmetric_Operations()
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            OsType            = OsType.macOS,
            HorizontalSpacing = 8,
            TitleAlignment    = Desktop.Controls.WindowTitleBarTitleAlignment.Auto
        };
        AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Leading, 80, 24);
        var title = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);
        AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Trailing, 20, 24);

        Layout(panel, 400, 40);

        title.Bounds.ShouldBe(new Rect(150, 0, 100, 40));
    }

    [Fact]
    public void Native_Inset_Preserves_Padding_And_Empty_Leading_AddOn_Adds_No_Spacing()
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            OsType              = OsType.macOS,
            Padding            = new Thickness(20, 0),
            NativeChromeInsets = new Thickness(80, 0, 30, 0),
            HorizontalSpacing  = 8,
            TitleAlignment     = Desktop.Controls.WindowTitleBarTitleAlignment.Left
        };
        var emptyLeading = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Leading, 0, 24);
        var title        = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);
        var trailing     = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Trailing, 40, 24);

        Layout(panel, 400, 40);

        emptyLeading.Bounds.Width.ShouldBe(0);
        title.Bounds.ShouldBe(new Rect(100, 0, 100, 40));
        trailing.Bounds.ShouldBe(new Rect(310, 0, 40, 40));
    }

    [Fact]
    public void MacOS_Native_Inset_Preserves_Content_Padding_Before_NonEmpty_Leading_Operation()
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            OsType              = OsType.macOS,
            Padding             = new Thickness(20, 0),
            NativeChromeInsets = new Thickness(80, 0, 0, 0),
            HorizontalSpacing  = 8,
            TitleAlignment     = Desktop.Controls.WindowTitleBarTitleAlignment.WindowCenter
        };
        var leading = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Leading, 60, 24);
        var title   = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);

        Layout(panel, 600, 40);

        leading.Bounds.ShouldBe(new Rect(100, 0, 60, 40));
        title.Bounds.Center.X.ShouldBe(300);
    }

    [Fact]
    public void Infinite_Measure_Includes_Effective_Native_Edge_Insets_In_The_Natural_Width()
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            OsType              = OsType.macOS,
            Padding             = new Thickness(20, 0, 10, 0),
            NativeChromeInsets = new Thickness(80, 0, 30, 0),
            TitleAlignment      = Desktop.Controls.WindowTitleBarTitleAlignment.Left
        };
        AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);

        panel.Measure(Size.Infinity);

        panel.DesiredSize.ShouldBe(new Size(240, 24));
    }

    [Fact]
    public void FullScreen_Drops_Reported_Native_Insets_From_The_Panel_Safe_Region()
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            OsType              = OsType.macOS,
            IsCsdEnabled        = false,
            WindowState         = WindowState.FullScreen,
            NativeChromeInsets = new Thickness(80, 0, 0, 0),
            TitleAlignment     = Desktop.Controls.WindowTitleBarTitleAlignment.Left
        };
        var title = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);

        Layout(panel, 400, 40);

        title.Bounds.ShouldBe(new Rect(0, 0, 100, 40));
    }

    [Theory]
    [InlineData(Desktop.Controls.WindowTitleBarTitleAlignment.Left, 98)]
    [InlineData(Desktop.Controls.WindowTitleBarTitleAlignment.Center, 180)]
    [InlineData(Desktop.Controls.WindowTitleBarTitleAlignment.WindowCenter, 150)]
    [InlineData(Desktop.Controls.WindowTitleBarTitleAlignment.Right, 262)]
    public void Explicit_Alignment_Uses_The_Shared_Safe_Region_Formula(
        Desktop.Controls.WindowTitleBarTitleAlignment alignment,
        double expectedX)
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            Padding           = new Thickness(10, 0),
            HorizontalSpacing = 8,
            TitleAlignment    = alignment
        };
        AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Leading, 80, 24);
        var title = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);
        AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Trailing, 20, 24);

        Layout(panel, 400, 40);

        title.Bounds.ShouldBe(new Rect(expectedX, 0, 100, 40));
    }

    [Fact]
    public void Empty_AddOn_Content_Does_Not_Leave_Ghost_Spacing_Across_Content_Changes()
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            HorizontalSpacing = 8,
            TitleAlignment    = Desktop.Controls.WindowTitleBarTitleAlignment.Left
        };
        var leading = new Border();
        Desktop.Controls.WindowTitleBarLayoutPanel.SetRole(
            leading,
            Desktop.Controls.WindowTitleBarLayoutRole.Leading);
        panel.Children.Add(leading);
        var title = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);

        Layout(panel, 400, 40);
        title.Bounds.X.ShouldBe(0);

        leading.Child = new FixedSizeControl(30, 24);
        panel.InvalidateMeasure();
        Layout(panel, 400, 40);
        title.Bounds.X.ShouldBe(38);

        leading.Child = null;
        panel.InvalidateMeasure();
        Layout(panel, 400, 40);
        title.Bounds.X.ShouldBe(0);
    }

    [Theory]
    [InlineData(OsType.Windows)]
    [InlineData(OsType.Linux)]
    public void Windows_And_Linux_Templated_AddOn_Content_Changes_Recalculate_The_Shared_Title_Formula(
        OsType osType)
    {
        var leftAddOn = new Border();
        var rightAddOn = new Border();
        var titleBar = new Desktop.Controls.WindowTitleBar
        {
            Width          = 400,
            Height         = 40,
            LeftAddOn      = leftAddOn,
            RightAddOn     = rightAddOn,
            Title          = "Dynamic add-on",
            TitleAlignment = Desktop.Controls.WindowTitleBarTitleAlignment.Left
        };
        titleBar.SetValue(Desktop.Controls.WindowTitleBar.OsTypeProperty, osType);
        Application.Current!.TryFindResource(typeof(Desktop.Controls.WindowTitleBar), out var resource)
                   .ShouldBeTrue();
        titleBar.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width   = 400,
            Height  = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            host.UpdateLayout();

            var panel = titleBar.GetVisualDescendants()
                                .OfType<Desktop.Controls.WindowTitleBarLayoutPanel>()
                                .Single();
            var title = panel.Children.Single(child =>
                Desktop.Controls.WindowTitleBarLayoutPanel.GetRole(child) ==
                Desktop.Controls.WindowTitleBarLayoutRole.Title);
            AssertTitleMatchesCurrentLayout(titleBar, panel, title);
            var emptyX = title.Bounds.X;

            leftAddOn.Child = new Border { Width = 30, Height = 24 };
            Dispatcher.UIThread.RunJobs();
            host.UpdateLayout();
            AssertTitleMatchesCurrentLayout(titleBar, panel, title);
            title.Bounds.X.ShouldBeGreaterThan(emptyX);

            leftAddOn.Child = null;
            Dispatcher.UIThread.RunJobs();
            host.UpdateLayout();
            AssertTitleMatchesCurrentLayout(titleBar, panel, title);
            title.Bounds.X.ShouldBe(emptyX);

            titleBar.TitleAlignment = Desktop.Controls.WindowTitleBarTitleAlignment.Right;
            Dispatcher.UIThread.RunJobs();
            host.UpdateLayout();
            AssertTitleMatchesCurrentLayout(titleBar, panel, title);
            var emptyRightX = title.Bounds.X;

            rightAddOn.Child = new Border { Width = 30, Height = 24 };
            Dispatcher.UIThread.RunJobs();
            host.UpdateLayout();
            AssertTitleMatchesCurrentLayout(titleBar, panel, title);
            title.Bounds.X.ShouldBeLessThan(emptyRightX);

            rightAddOn.Child = null;
            Dispatcher.UIThread.RunJobs();
            host.UpdateLayout();
            AssertTitleMatchesCurrentLayout(titleBar, panel, title);
            title.Bounds.X.ShouldBe(emptyRightX);
        }
        finally
        {
            host.Close();
        }
    }

    [Theory]
    [InlineData(OsType.Linux)]
    [InlineData(OsType.Windows)]
    [InlineData(OsType.macOS)]
    public void String_Title_Is_Constrained_To_The_Safe_Region_With_Character_Ellipsis(OsType osType)
    {
        var titleBar = new Desktop.Controls.WindowTitleBar
        {
            Width          = 400,
            Height         = 40,
            LeftAddOn      = new Border { Width = 160, Height = 24 },
            RightAddOn     = new Border { Width = 48, Height = 24 },
            Title          = "TitleAlignment: WindowCenter",
            TitleAlignment = Desktop.Controls.WindowTitleBarTitleAlignment.WindowCenter,
            LogoVisibility = Desktop.Controls.WindowTitleBarLogoVisibility.Never
        };
        titleBar.SetValue(Desktop.Controls.WindowTitleBar.OsTypeProperty, osType);
        Application.Current!.TryFindResource(typeof(Desktop.Controls.WindowTitleBar), out var resource)
                   .ShouldBeTrue();
        titleBar.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width   = 400,
            Height  = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            host.UpdateLayout();

            var titleLayout = titleBar.GetVisualDescendants()
                                          .OfType<Desktop.Controls.WindowTitleBarLayoutPanel>()
                                          .Single()
                                          .Children
                                          .Single(child =>
                                              Desktop.Controls.WindowTitleBarLayoutPanel.GetRole(child) ==
                                              Desktop.Controls.WindowTitleBarLayoutRole.Title);
            // CaptionButtonFrame 等内嵌部件模板同样定义 PART_ContentPresenter，
            // 标题部件必须在 Title 角色子树内查找，不能假设整棵树名字唯一。
            var titlePresenter = titleLayout.GetVisualDescendants()
                                            .OfType<ContentPresenter>()
                                            .Single(presenter => presenter.Name == "PART_ContentPresenter");

            titlePresenter.Bounds.Width.ShouldBeLessThanOrEqualTo(titleLayout.Bounds.Width);
            titlePresenter.TextTrimming.ShouldBe(TextTrimming.CharacterEllipsis);
        }
        finally
        {
            host.Close();
        }
    }

    [Theory]
    [InlineData(OsType.Windows)]
    [InlineData(OsType.Linux)]
    public void Windows_And_Linux_Templates_Place_Logo_At_The_Physical_Left_Edge_Before_Left_AddOn(
        OsType osType)
    {
        var titleBar = new Desktop.Controls.WindowTitleBar
        {
            Width          = 400,
            Height         = 40,
            Logo           = new Border { Width = 16, Height = 16 },
            LeftAddOn      = new Border { Width = 120, Height = 24 },
            TitleAlignment = Desktop.Controls.WindowTitleBarTitleAlignment.Left,
            LogoVisibility = Desktop.Controls.WindowTitleBarLogoVisibility.Always
        };
        titleBar.SetValue(Desktop.Controls.WindowTitleBar.OsTypeProperty, osType);
        Application.Current!.TryFindResource(typeof(Desktop.Controls.WindowTitleBar), out var resource)
                   .ShouldBeTrue();
        titleBar.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width   = 400,
            Height  = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            host.UpdateLayout();

            var logoPresenter = titleBar.GetVisualDescendants()
                                        .OfType<ContentPresenter>()
                                        .Single(presenter => presenter.Name == "PART_Logo");
            var leftAddOnPresenter = titleBar.GetVisualDescendants()
                                             .OfType<ContentPresenter>()
                                             .Single(presenter => presenter.Name == "PART_LeftAddOn");
            var leadingHost = logoPresenter.GetVisualParent().ShouldBeAssignableTo<Control>()!;

            leftAddOnPresenter.GetVisualParent().ShouldBe(leadingHost);
            Desktop.Controls.WindowTitleBarLayoutPanel.GetRole(leadingHost)
                   .ShouldBe(Desktop.Controls.WindowTitleBarLayoutRole.Leading);
            logoPresenter.Bounds.X.ShouldBeLessThan(leftAddOnPresenter.Bounds.X);
            logoPresenter.Bounds.X.ShouldBe(0, 0.5);
            (leftAddOnPresenter.Bounds.X - logoPresenter.Bounds.Right).ShouldBe(4, 0.5);
        }
        finally
        {
            host.Close();
        }
    }

    [Theory]
    [InlineData(OsType.Windows)]
    [InlineData(OsType.Linux)]
    public void Windows_And_Linux_Logo_Left_AddOn_Spacing_Tracks_Presenter_Visibility(
        OsType osType)
    {
        var titleBar = new Desktop.Controls.WindowTitleBar
        {
            Width          = 400,
            Height         = 40,
            Logo           = new Border { Width = 16, Height = 16 },
            LeftAddOn      = new Border { Width = 120, Height = 24 },
            TitleAlignment = Desktop.Controls.WindowTitleBarTitleAlignment.Left,
            LogoVisibility = Desktop.Controls.WindowTitleBarLogoVisibility.Always
        };
        titleBar.SetValue(Desktop.Controls.WindowTitleBar.OsTypeProperty, osType);
        Application.Current!.TryFindResource(typeof(Desktop.Controls.WindowTitleBar), out var resource)
                   .ShouldBeTrue();
        titleBar.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width   = 400,
            Height  = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            host.UpdateLayout();

            var logoPresenter = titleBar.GetVisualDescendants()
                                        .OfType<ContentPresenter>()
                                        .Single(presenter => presenter.Name == "PART_Logo");
            var leftAddOnPresenter = titleBar.GetVisualDescendants()
                                             .OfType<ContentPresenter>()
                                             .Single(presenter => presenter.Name == "PART_LeftAddOn");
            var leadingHost = logoPresenter.GetVisualParent().ShouldBeAssignableTo<Control>()!;

            (leftAddOnPresenter.Bounds.X - logoPresenter.Bounds.Right).ShouldBe(4, 0.5);

            titleBar.LogoVisibility = Desktop.Controls.WindowTitleBarLogoVisibility.Never;
            Dispatcher.UIThread.RunJobs();
            host.UpdateLayout();

            logoPresenter.IsVisible.ShouldBeFalse();
            leftAddOnPresenter.Bounds.X.ShouldBe(0, 0.5);

            titleBar.LogoVisibility = Desktop.Controls.WindowTitleBarLogoVisibility.Always;
            titleBar.LeftAddOn = null;
            Dispatcher.UIThread.RunJobs();
            host.UpdateLayout();

            logoPresenter.IsVisible.ShouldBeTrue();
            leftAddOnPresenter.IsVisible.ShouldBeFalse();
            leadingHost.Bounds.Width.ShouldBe(logoPresenter.Bounds.Width, 0.5);
        }
        finally
        {
            host.Close();
        }
    }

    [Theory]
    [InlineData(OsType.Windows, Desktop.Controls.WindowTitleBarTitleAlignment.Left)]
    [InlineData(OsType.Windows, Desktop.Controls.WindowTitleBarTitleAlignment.Center)]
    [InlineData(OsType.Windows, Desktop.Controls.WindowTitleBarTitleAlignment.WindowCenter)]
    [InlineData(OsType.Windows, Desktop.Controls.WindowTitleBarTitleAlignment.Right)]
    [InlineData(OsType.Linux, Desktop.Controls.WindowTitleBarTitleAlignment.Left)]
    [InlineData(OsType.Linux, Desktop.Controls.WindowTitleBarTitleAlignment.Center)]
    [InlineData(OsType.Linux, Desktop.Controls.WindowTitleBarTitleAlignment.WindowCenter)]
    [InlineData(OsType.Linux, Desktop.Controls.WindowTitleBarTitleAlignment.Right)]
    public void Windows_And_Linux_Template_Title_Alignment_Still_Uses_The_Shared_Safe_Region_Formula(
        OsType osType,
        Desktop.Controls.WindowTitleBarTitleAlignment alignment)
    {
        var titleBar = new Desktop.Controls.WindowTitleBar
        {
            Width          = 800,
            Height         = 40,
            Logo           = new Border { Width = 16, Height = 16 },
            LeftAddOn      = new Border { Width = 64, Height = 24 },
            RightAddOn     = new Border { Width = 48, Height = 24 },
            Title          = new Border { Width = 100, Height = 24 },
            TitleAlignment = alignment,
            LogoVisibility = Desktop.Controls.WindowTitleBarLogoVisibility.Always
        };
        titleBar.SetValue(Desktop.Controls.WindowTitleBar.OsTypeProperty, osType);
        Application.Current!.TryFindResource(typeof(Desktop.Controls.WindowTitleBar), out var resource)
                   .ShouldBeTrue();
        titleBar.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width   = 800,
            Height  = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            host.UpdateLayout();

            var panel = titleBar.GetVisualDescendants()
                                .OfType<Desktop.Controls.WindowTitleBarLayoutPanel>()
                                .Single();
            var leading = FindRoleChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Leading);
            var title   = FindRoleChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title);
            var trailing = FindRoleChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Trailing);

            var expectedX = CalculateExpectedTitleX(
                titleBar.Width,
                panel,
                leading.Bounds.Width,
                title.Bounds.Width,
                trailing.Bounds.Width,
                alignment);

            title.Bounds.X.ShouldBe(expectedX, 0.5);
            if (alignment == Desktop.Controls.WindowTitleBarTitleAlignment.WindowCenter)
            {
                title.Bounds.Center.X.ShouldBe(titleBar.Width / 2, 0.5);
            }
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void Hidden_AddOn_Does_Not_Occupy_The_Title_Safe_Region()
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            HorizontalSpacing = 8,
            TitleAlignment    = Desktop.Controls.WindowTitleBarTitleAlignment.Left
        };
        var leading = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Leading, 40, 24);
        leading.IsVisible = false;
        var title = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);

        Layout(panel, 400, 40);

        title.Bounds.X.ShouldBe(0);
    }

    [Fact]
    public void AddOn_Margin_Is_Measured_Once_Before_The_Conditional_Spacing()
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            HorizontalSpacing = 8,
            TitleAlignment    = Desktop.Controls.WindowTitleBarTitleAlignment.Left
        };
        var leading = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Leading, 20, 24);
        leading.Margin = new Thickness(5, 0);
        var title = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);

        Layout(panel, 400, 40);

        leading.DesiredSize.Width.ShouldBe(30);
        title.Bounds.X.ShouldBe(38);
    }

    [Theory]
    [InlineData(Desktop.Controls.WindowTitleBarTitleAlignment.Left)]
    [InlineData(Desktop.Controls.WindowTitleBarTitleAlignment.Center)]
    [InlineData(Desktop.Controls.WindowTitleBarTitleAlignment.WindowCenter)]
    [InlineData(Desktop.Controls.WindowTitleBarTitleAlignment.Right)]
    public void Intersecting_Operation_Regions_Collapse_The_Title_First(
        Desktop.Controls.WindowTitleBarTitleAlignment alignment)
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            HorizontalSpacing = 8,
            TitleAlignment    = alignment
        };
        AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Leading, 60, 24);
        var title = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);
        AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Trailing, 60, 24);

        Layout(panel, 100, 40);

        title.Bounds.Width.ShouldBe(0);
    }

    [Fact]
    public void Invalid_And_Negative_Metrics_Normalize_To_Zero()
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            Padding            = new Thickness(double.NaN, 0, -10, 0),
            NativeChromeInsets = new Thickness(double.PositiveInfinity, 0, double.NegativeInfinity, 0),
            HorizontalSpacing  = double.NaN,
            TitleAlignment     = Desktop.Controls.WindowTitleBarTitleAlignment.Left
        };
        var title = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);

        Layout(panel, 400, 40);

        title.Bounds.ShouldBe(new Rect(0, 0, 100, 40));
    }

    [Fact]
    public void RightToLeft_Does_Not_Swap_The_Physical_Left_And_Right_Contracts()
    {
        var panel = new Desktop.Controls.WindowTitleBarLayoutPanel
        {
            FlowDirection     = FlowDirection.RightToLeft,
            HorizontalSpacing = 8,
            TitleAlignment    = Desktop.Controls.WindowTitleBarTitleAlignment.Left
        };
        AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Leading, 40, 24);
        var title = AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Title, 100, 24);
        AddChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Trailing, 20, 24);

        Layout(panel, 400, 40);

        title.Bounds.X.ShouldBe(48);
    }

    private static FixedSizeControl AddChild(
        Desktop.Controls.WindowTitleBarLayoutPanel panel,
        Desktop.Controls.WindowTitleBarLayoutRole role,
        double width,
        double height)
    {
        var child = new FixedSizeControl(width, height);
        Desktop.Controls.WindowTitleBarLayoutPanel.SetRole(child, role);
        panel.Children.Add(child);
        return child;
    }

    private static Control FindRoleChild(
        Desktop.Controls.WindowTitleBarLayoutPanel panel,
        Desktop.Controls.WindowTitleBarLayoutRole role)
    {
        return panel.Children.Single(child =>
            Desktop.Controls.WindowTitleBarLayoutPanel.GetRole(child) == role);
    }

    private static void AssertTitleMatchesCurrentLayout(
        Desktop.Controls.WindowTitleBar titleBar,
        Desktop.Controls.WindowTitleBarLayoutPanel panel,
        Control title)
    {
        var leading = FindRoleChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Leading);
        var trailing = FindRoleChild(panel, Desktop.Controls.WindowTitleBarLayoutRole.Trailing);
        var expectedX = CalculateExpectedTitleX(
            titleBar.Width,
            panel,
            leading.Bounds.Width,
            title.Bounds.Width,
            trailing.Bounds.Width,
            titleBar.TitleAlignment);

        title.Bounds.X.ShouldBe(expectedX, 0.5);
    }

    private static double CalculateExpectedTitleX(
        double width,
        Desktop.Controls.WindowTitleBarLayoutPanel panel,
        double leadingWidth,
        double titleWidth,
        double trailingWidth,
        Desktop.Controls.WindowTitleBarTitleAlignment alignment)
    {
        var effectiveAlignment = alignment == Desktop.Controls.WindowTitleBarTitleAlignment.Auto
            ? Desktop.Controls.WindowTitleBarLayoutStrategies.Get(panel.OsType).AutoAlignment
            : alignment;
        var nativeChromeInsets = Desktop.Controls.WindowTitleBarLayoutStrategies.Get(panel.OsType)
                                     .ResolveNativeChromeInsets(
                                         width,
                                         panel.NativeChromeInsets,
                                         panel.IsCsdEnabled,
                                         panel.WindowState);
        var leftBoundary = Math.Clamp(
            Normalize(nativeChromeInsets.Left) + Normalize(panel.Padding.Left),
            0,
            width);
        var rightBoundary = Math.Clamp(
            width - Normalize(nativeChromeInsets.Right) - Normalize(panel.Padding.Right),
            0,
            width);
        var horizontalSpacing  = Normalize(panel.HorizontalSpacing);
        var leadingOccupation  = leadingWidth > 0 ? leadingWidth + horizontalSpacing : 0;
        var trailingOccupation = trailingWidth > 0 ? trailingWidth + horizontalSpacing : 0;
        var left               = Math.Clamp(leftBoundary + leadingOccupation, 0, width);
        var right              = Math.Clamp(rightBoundary - trailingOccupation, 0, width);
        var availableWidth     = Math.Max(0, right - left);
        var effectiveWidth     = Math.Min(titleWidth, availableWidth);

        return effectiveAlignment switch
        {
            Desktop.Controls.WindowTitleBarTitleAlignment.Center =>
                left + (availableWidth - effectiveWidth) / 2,
            Desktop.Controls.WindowTitleBarTitleAlignment.WindowCenter =>
                width / 2 - Math.Min(
                    titleWidth,
                    2 * Math.Max(0, Math.Min(width / 2 - left, right - width / 2))) / 2,
            Desktop.Controls.WindowTitleBarTitleAlignment.Right => right - effectiveWidth,
            _ => left
        };
    }

    private static double Normalize(double value)
    {
        return double.IsFinite(value) && value > 0 ? value : 0;
    }

    private static void Layout(Control control, double width, double height)
    {
        control.Measure(new Size(width, height));
        control.Arrange(new Rect(0, 0, width, height));
    }

    private sealed class FixedSizeControl(double width, double height) : Control
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(Math.Min(width, availableSize.Width), Math.Min(height, availableSize.Height));
        }
    }
}
