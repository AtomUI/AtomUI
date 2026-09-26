using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AtomUI.Icons.AntDesign;
using Shouldly;
using Xunit;

using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class WindowTitleBarButtonTests
{
    static WindowTitleBarButtonTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Public_AddOn_Controls_Reuse_The_Icon_Button_Family()
    {
        var button = new WindowTitleBarButton();
        var toggleButton = new WindowTitleBarToggleButton();

        button.ShouldBeAssignableTo<IconButton>();
        toggleButton.ShouldBeAssignableTo<ToggleIconButton>();
        button.IsWindowActive.ShouldBeTrue();
        button.HostMotionEnabled.ShouldBeTrue();
        button.HostOsType.ShouldBe(OsType.Unknown);
        toggleButton.IsWindowActive.ShouldBeTrue();
        toggleButton.HostMotionEnabled.ShouldBeTrue();
        toggleButton.HostOsType.ShouldBe(OsType.Unknown);
    }

    [Fact]
    public void Windows_AddOn_Button_Uses_Full_Height_Square_Caption_Geometry()
    {
        var button = new WindowTitleBarButton
        {
            Icon = new SearchOutlined()
        };
        var titleBar = new WindowTitleBar
        {
            RightAddOn = button,
            IsWindowActive = true,
            IsMotionEnabled = false,
            Title = "Title",
            Height = 44
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Windows);
        titleBar.Theme = Application.Current!
            .FindResource(typeof(WindowTitleBar))
            .ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width = 400,
            Height = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            button.ApplyTemplate();
            host.UpdateLayout();

            button.DesiredSize.ShouldBe(new Size(44, 44));
            button.Bounds.Size.ShouldBe(new Size(44, 44));
            button.CornerRadius.ShouldBe(new CornerRadius(0));
            button.VerticalAlignment.ShouldBe(VerticalAlignment.Stretch);
            button.Cursor.ShouldNotBeNull();
            button.Cursor!.ToString().ShouldContain("Arrow");
            button.Background!
                  .ShouldBeAssignableTo<ISolidColorBrush>()
                  .Color.ShouldBe(Colors.Transparent);

            var point = button.TranslatePoint(
                new Point(button.Bounds.Width / 2, button.Bounds.Height / 2),
                host).ShouldNotBeNull();
            host.MouseMove(point);
            Dispatcher.UIThread.RunJobs();

            button.IsPointerOver.ShouldBeTrue();
            var hoverColor = button.Background!
                                   .ShouldBeAssignableTo<ISolidColorBrush>()
                                   .Color;
            hoverColor.ShouldNotBe(Colors.Transparent);

            host.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            button.Background!
                  .ShouldBeAssignableTo<ISolidColorBrush>()
                  .Color.ShouldNotBe(hoverColor);

            host.MouseUp(point, MouseButton.Left);
            host.MouseMove(new Point(10, 90));
            Dispatcher.UIThread.RunJobs();

            button.IsPointerOver.ShouldBeFalse();
            button.Background!
                  .ShouldBeAssignableTo<ISolidColorBrush>()
                  .Color.ShouldBe(Colors.Transparent);

            titleBar.IsWindowActive = false;
            host.MouseMove(point);
            Dispatcher.UIThread.RunJobs();

            var inactiveHoverColor = button.Background!
                                           .ShouldBeAssignableTo<ISolidColorBrush>()
                                           .Color;
            inactiveHoverColor.ShouldNotBe(Colors.Transparent);

            host.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            button.Background!
                  .ShouldBeAssignableTo<ISolidColorBrush>()
                  .Color.ShouldNotBe(inactiveHoverColor);

            host.MouseUp(point, MouseButton.Left);
            button.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();

            button.Background!
                  .ShouldBeAssignableTo<ISolidColorBrush>()
                  .Color.ShouldBe(Colors.Transparent);

            button.IsEnabled = true;
            titleBar.IsWindowActive = true;
            host.MouseMove(new Point(10, 90));
            Dispatcher.UIThread.RunJobs();

            titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Linux);
            host.UpdateLayout();

            button.HostOsType.ShouldBe(OsType.Linux);
            button.DesiredSize.ShouldBe(new Size(30, 30));

            titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Windows);
            host.UpdateLayout();

            button.HostOsType.ShouldBe(OsType.Windows);
            button.DesiredSize.ShouldBe(new Size(44, 44));
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void Windows_AddOn_Toggle_Button_Uses_Full_Height_Square_Caption_Geometry()
    {
        var button = new WindowTitleBarToggleButton
        {
            CheckedIcon = new SearchOutlined(),
            UnCheckedIcon = new SettingOutlined()
        };
        var titleBar = new WindowTitleBar
        {
            LeftAddOn = button,
            IsWindowActive = true,
            Title = "Title"
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Windows);
        titleBar.Theme = Application.Current!
            .FindResource(typeof(WindowTitleBar))
            .ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width = 400,
            Height = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            button.ApplyTemplate();
            host.UpdateLayout();

            button.DesiredSize.ShouldBe(new Size(40, 40));
            button.Bounds.Size.ShouldBe(new Size(40, 40));
            button.CornerRadius.ShouldBe(new CornerRadius(0));
            button.VerticalAlignment.ShouldBe(VerticalAlignment.Stretch);
            button.Cursor.ShouldNotBeNull();
            button.Cursor!.ToString().ShouldContain("Arrow");
            button.Background!
                  .ShouldBeAssignableTo<ISolidColorBrush>()
                  .Color.ShouldBe(Colors.Transparent);
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void MacOS_AddOn_Buttons_Match_The_Managed_Caption_Button_Circle()
    {
        var button = new WindowTitleBarButton
        {
            Icon = new CameraOutlined()
        };
        var toggleButton = new WindowTitleBarToggleButton
        {
            CheckedIcon = new SearchOutlined(),
            UnCheckedIcon = new SettingOutlined()
        };
        var titleBar = new WindowTitleBar
        {
            RightAddOn = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { button, toggleButton }
            },
            IsWindowActive = true,
            Title = "Title",
            IsPinCaptionButtonVisible = true,
            IsPinCaptionButtonSupported = true
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.macOS);
        titleBar.Theme = Application.Current!
            .FindResource(typeof(WindowTitleBar))
            .ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width = 400,
            Height = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            button.ApplyTemplate();
            toggleButton.ApplyTemplate();
            host.UpdateLayout();

            var pinButton = titleBar.GetVisualDescendants()
                                    .OfType<CaptionButton>()
                                    .Single();
            pinButton.IsVisible.ShouldBeTrue();
            pinButton.ApplyTemplate();
            host.UpdateLayout();

            button.Bounds.Size.ShouldBe(new Size(30, 30));
            toggleButton.Bounds.Size.ShouldBe(new Size(30, 30));
            pinButton.Bounds.Size.ShouldBe(new Size(30, 30));

            var buttonFrame = FindCaptionFrame(button);
            var toggleFrame = FindCaptionFrame(toggleButton);
            var pinFrame = FindCaptionFrame(pinButton);
            buttonFrame.CornerRadius.ShouldBe(new CornerRadius(15));
            toggleFrame.CornerRadius.ShouldBe(new CornerRadius(15));
            buttonFrame.CornerRadius.ShouldBe(pinFrame.CornerRadius);
            toggleFrame.CornerRadius.ShouldBe(pinFrame.CornerRadius);
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void MacOS_AddOn_Button_Is_Spaced_From_The_Caption_Button_Group()
    {
        var button = new WindowTitleBarButton
        {
            Icon = new CameraOutlined()
        };
        var titleBar = new WindowTitleBar
        {
            RightAddOn = button,
            IsWindowActive = true,
            Title = "Title",
            IsPinCaptionButtonVisible = true,
            IsPinCaptionButtonSupported = true
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.macOS);
        titleBar.Theme = Application.Current!
            .FindResource(typeof(WindowTitleBar))
            .ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width = 400,
            Height = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            button.ApplyTemplate();
            host.UpdateLayout();

            var pinButton = titleBar.GetVisualDescendants()
                                    .OfType<CaptionButton>()
                                    .Single();
            pinButton.ApplyTemplate();
            host.UpdateLayout();

            var addOnRight = button.TranslatePoint(new Point(button.Bounds.Width, 0), titleBar)!.Value.X;
            var pinLeft = pinButton.TranslatePoint(new Point(0, 0), titleBar)!.Value.X;

            (pinLeft - addOnRight).ShouldBe(8);

            titleBar.RightAddOn = null;
            host.UpdateLayout();

            var pinRight = pinButton.TranslatePoint(new Point(pinButton.Bounds.Width, 0), titleBar)!.Value.X;
            // 没有 AddOn 内容时，Trailing spacing 不产生幽灵间距，caption group 仍贴住尾随内容边界。
            Math.Abs(titleBar.Bounds.Width - titleBar.Padding.Right - pinRight)
                .ShouldBeLessThanOrEqualTo(1d);
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void Linux_AddOn_Buttons_Match_The_Managed_Caption_Band_Geometry()
    {
        var button = new WindowTitleBarButton
        {
            Icon = new CameraOutlined()
        };
        var toggleButton = new WindowTitleBarToggleButton
        {
            CheckedIcon = new SearchOutlined(),
            UnCheckedIcon = new SettingOutlined()
        };
        var titleBar = new WindowTitleBar
        {
            RightAddOn = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { button, toggleButton }
            },
            IsWindowActive = true,
            Title = "Title",
            IsPinCaptionButtonVisible = true,
            IsPinCaptionButtonSupported = true
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Linux);
        titleBar.Theme = Application.Current!
            .FindResource(typeof(WindowTitleBar))
            .ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width = 400,
            Height = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            button.ApplyTemplate();
            toggleButton.ApplyTemplate();
            host.UpdateLayout();

            // CaptionButtonGroup 在构造函数里用 LocalValue 写入真实平台 OsType，优先级高于主题的
            // OsType TemplateBinding，因此在测试机上必须显式驱动 Linux 模板，否则会静默落到宿主平台模板。
            var group = titleBar.GetVisualDescendants()
                                .OfType<CaptionButtonGroup>()
                                .Single();
            group.SetValue(CaptionButtonGroup.OsTypeProperty, OsType.Linux);
            host.UpdateLayout();

            titleBar.GetVisualDescendants()
                   .OfType<CaptionButton>()
                   .ShouldContain(candidate => candidate.Name == "PART_MinimizeButton");

            var pinButton = titleBar.GetVisualDescendants()
                                    .OfType<CaptionButton>()
                                    .Single(candidate => candidate.Name == "PART_PinButton");
            var minimizeButton = titleBar.GetVisualDescendants()
                                         .OfType<CaptionButton>()
                                         .Single(candidate => candidate.Name == "PART_MinimizeButton");
            pinButton.ApplyTemplate();
            minimizeButton.ApplyTemplate();
            host.UpdateLayout();

            var buttonFrame = FindCaptionFrame(button);
            var toggleFrame = FindCaptionFrame(toggleButton);
            var pinFrame = FindCaptionFrame(pinButton);
            var minimizeFrame = FindCaptionFrame(minimizeButton);

            // 圆角、可见背景尺寸和背景 inset 必须与相邻 managed caption button 完全一致。
            buttonFrame.CornerRadius.ShouldBe(new CornerRadius(15));
            buttonFrame.CornerRadius.ShouldBe(pinFrame.CornerRadius);
            toggleFrame.CornerRadius.ShouldBe(pinFrame.CornerRadius);
            buttonFrame.Bounds.Size.ShouldBe(new Size(26, 26));
            buttonFrame.Bounds.Size.ShouldBe(pinFrame.Bounds.Size);
            toggleFrame.Bounds.Size.ShouldBe(pinFrame.Bounds.Size);
            buttonFrame.Margin.ShouldBe(new Thickness(2));
            buttonFrame.Margin.ShouldBe(pinButton.BackgroundInset);
            toggleFrame.Margin.ShouldBe(pinButton.BackgroundInset);

            // 命中面仍是 30 逻辑像素，与 caption button 的布局盒一致。
            button.Bounds.Size.ShouldBe(new Size(30, 30));
            toggleButton.Bounds.Size.ShouldBe(new Size(30, 30));
            pinButton.Bounds.Size.ShouldBe(new Size(30, 30));

            // 可见背景之间的间距也要等于 caption band 内部的可见间距。
            var addOnVisibleRight = toggleButton.TranslatePoint(
                new Point(toggleFrame.Bounds.Right, 0), titleBar)!.Value.X;
            var pinVisibleLeft = pinButton.TranslatePoint(
                new Point(pinFrame.Bounds.X, 0), titleBar)!.Value.X;
            var pinVisibleRight = pinButton.TranslatePoint(
                new Point(pinFrame.Bounds.Right, 0), titleBar)!.Value.X;
            var minimizeVisibleLeft = minimizeButton.TranslatePoint(
                new Point(minimizeFrame.Bounds.X, 0), titleBar)!.Value.X;

            (pinVisibleLeft - addOnVisibleRight).ShouldBe(12);
            (pinVisibleLeft - addOnVisibleRight).ShouldBe(minimizeVisibleLeft - pinVisibleRight);
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void Non_Windows_AddOn_Buttons_Preserve_Managed_Button_Geometry()
    {
        var button = new WindowTitleBarButton
        {
            Icon = new SearchOutlined()
        };
        var toggleButton = new WindowTitleBarToggleButton
        {
            CheckedIcon = new SearchOutlined(),
            UnCheckedIcon = new SettingOutlined()
        };
        var titleBar = new WindowTitleBar
        {
            LeftAddOn = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { button, toggleButton }
            },
            IsWindowActive = true,
            Title = "Title"
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Linux);
        titleBar.Theme = Application.Current!
            .FindResource(typeof(WindowTitleBar))
            .ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width = 400,
            Height = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            button.ApplyTemplate();
            toggleButton.ApplyTemplate();
            host.UpdateLayout();

            button.DesiredSize.ShouldBe(new Size(30, 30));
            toggleButton.DesiredSize.ShouldBe(new Size(30, 30));
            button.CornerRadius.ShouldNotBe(new CornerRadius(0));
            toggleButton.CornerRadius.ShouldNotBe(new CornerRadius(0));
            button.VerticalAlignment.ShouldBe(VerticalAlignment.Center);
            toggleButton.VerticalAlignment.ShouldBe(VerticalAlignment.Center);
            button.Cursor!.ToString().ShouldContain("Hand");
            toggleButton.Cursor!.ToString().ShouldContain("Hand");
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void AddOn_Content_Receives_TitleBar_Host_State()
    {
        var button = new WindowTitleBarButton();
        var titleBar = new WindowTitleBar
        {
            LeftAddOn = button,
            IsWindowActive = false,
            IsMotionEnabled = false,
            Title = "Title"
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Linux);
        titleBar.Theme = Application.Current!
            .FindResource(typeof(WindowTitleBar))
            .ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width = 400,
            Height = 100,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            host.UpdateLayout();

            var projectedButton = titleBar.GetVisualDescendants()
                                          .OfType<WindowTitleBarButton>()
                                          .Single();

            projectedButton.ShouldBeSameAs(button);
            projectedButton.IsWindowActive.ShouldBeFalse();
            projectedButton.HostMotionEnabled.ShouldBeFalse();
            projectedButton.HostOsType.ShouldBe(OsType.Linux);

            titleBar.IsWindowActive = true;
            titleBar.IsMotionEnabled = true;
            host.UpdateLayout();

            projectedButton.IsWindowActive.ShouldBeTrue();
            projectedButton.HostMotionEnabled.ShouldBeTrue();
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void Toggle_AddOn_Preserves_Checked_State_And_Icon_Pair()
    {
        var checkedIcon   = new SearchOutlined();
        var uncheckedIcon = new SettingOutlined();
        var toggleButton = new WindowTitleBarToggleButton
        {
            CheckedIcon   = checkedIcon,
            UnCheckedIcon = uncheckedIcon,
            IsChecked     = false
        };
        toggleButton.Theme = Application.Current!
            .FindResource(typeof(WindowTitleBarToggleButton))
            .ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width   = 180,
            Height  = 80,
            Content = toggleButton
        };

        try
        {
            host.Show();
            toggleButton.ApplyTemplate();
            host.UpdateLayout();

            var presenters = toggleButton.GetVisualDescendants()
                                          .OfType<IconPresenter>()
                                          .Where(presenter => presenter.Name is not null)
                                          .ToDictionary(presenter => presenter.Name!);
            presenters["PART_CheckedIconPresenter"].Icon.ShouldBeSameAs(checkedIcon);
            presenters["PART_UnCheckedIconPresenter"].Icon.ShouldBeSameAs(uncheckedIcon);
            presenters["PART_CheckedIconPresenter"].IsVisible.ShouldBeFalse();
            presenters["PART_UnCheckedIconPresenter"].IsVisible.ShouldBeTrue();

            toggleButton.IsChecked = true;
            host.UpdateLayout();

            presenters["PART_CheckedIconPresenter"].IsVisible.ShouldBeTrue();
            presenters["PART_UnCheckedIconPresenter"].IsVisible.ShouldBeFalse();
            toggleButton.IsChecked.ShouldBe(true);
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void AddOn_Content_Returns_To_Standalone_Defaults_When_Removed()
    {
        var button = new WindowTitleBarButton();
        var titleBar = new WindowTitleBar
        {
            LeftAddOn      = button,
            IsWindowActive = false,
            IsMotionEnabled = false,
            Title          = "Title"
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Linux);
        titleBar.Theme = Application.Current!
            .FindResource(typeof(WindowTitleBar))
            .ShouldBeAssignableTo<ControlTheme>();

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

            button.IsWindowActive.ShouldBeFalse();
            button.HostMotionEnabled.ShouldBeFalse();
            button.HostOsType.ShouldBe(OsType.Linux);

            titleBar.LeftAddOn = null;
            host.UpdateLayout();

            button.IsWindowActive.ShouldBeTrue();
            button.HostMotionEnabled.ShouldBeTrue();
            button.HostOsType.ShouldBe(OsType.Unknown);
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void AddOn_Pointer_Input_Does_Not_Bubble_As_TitleBar_Double_Click()
    {
        var button = new WindowTitleBarButton
        {
            Icon = new SearchOutlined()
        };
        var toggleButton = new WindowTitleBarToggleButton
        {
            CheckedIcon = new SearchOutlined(),
            UnCheckedIcon = new SettingOutlined()
        };
        var titleBar = new WindowTitleBar
        {
            LeftAddOn = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { button, toggleButton }
            },
            Title = "Title"
        };
        titleBar.SetValue(WindowTitleBar.OsTypeProperty, OsType.Linux);
        titleBar.Theme = Application.Current!
            .FindResource(typeof(WindowTitleBar))
            .ShouldBeAssignableTo<ControlTheme>();

        var maximizeRequests = 0;
        titleBar.MaximizeWindowRequested += (_, _) => maximizeRequests++;

        var host = new AtomUIWindow
        {
            IsTitleBarVisible = false,
            Content = titleBar
        };

        try
        {
            host.Show();
            titleBar.ApplyTemplate();
            host.UpdateLayout();

            RaiseDoubleClick(button);
            RaiseDoubleClick(toggleButton);

            maximizeRequests.ShouldBe(0);
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void AddOn_Themes_Reuse_The_Shared_Managed_Caption_Frame()
    {
        var frameTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonFrameTheme.axaml"));
        var buttonTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowTitleBarButtonTheme.axaml"));
        var toggleTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowTitleBarToggleButtonTheme.axaml"));
        var manifest = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/GeneratedFiles/AtomUI.Generator/AtomUI.Generator.ThemeAssetManifestGenerator/GeneratedControlThemeAssetManifest.g.cs"));

        manifest.ShouldContain("WindowTitleBar/Themes/CaptionButtonFrameTheme.axaml");

        // 背景层 + 内容层的几何结构只在共享外壳里定义一次。
        frameTheme.Descendants()
                  .Count(element => (string?)element.Attribute("Name") == "PART_Frame")
                  .ShouldBe(1);

        foreach (var theme in new[] { buttonTheme, toggleTheme })
        {
            var frames = theme.Descendants()
                              .Where(element => element.Name.LocalName == "CaptionButtonFrame")
                              .ToList();
            frames.Count.ShouldBe(1);
            frames[0].Descendants()
                     .ShouldNotContain(element => (string?)element.Attribute("Name") == "PART_Frame");
            theme.Descendants()
                 .ShouldNotContain(element => element.Name.LocalName == "PixelAlignedBorder");
        }

        // Linux 通过共享外壳的属性表达背景内缩，而不是自己搭背景层。
        foreach (var theme in new[] { buttonTheme, toggleTheme })
        {
            theme.Descendants()
                 .ShouldContain(element =>
                     element.Name.LocalName == "Setter" &&
                     (string?)element.Attribute("Property") == "BackgroundInset" &&
                     (string?)element.Attribute("Value") ==
                     "{atom:WindowTitleBarTokenResource CaptionButtonBackgroundInset}");
        }
    }

    [Fact]
    public void AddOn_Themes_Are_Registered_As_Independent_Assets_And_Stay_Out_Of_System_Caption_Contract()
    {
        Application.Current!
            .FindResource(typeof(WindowTitleBarButton))
            .ShouldBeAssignableTo<ControlTheme>();
        Application.Current!
            .FindResource(typeof(WindowTitleBarToggleButton))
            .ShouldBeAssignableTo<ControlTheme>();

        var manifest = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/GeneratedFiles/AtomUI.Generator/AtomUI.Generator.ThemeAssetManifestGenerator/GeneratedControlThemeAssetManifest.g.cs"));
        manifest.ShouldContain("WindowTitleBarButtonTheme.axaml");
        manifest.ShouldContain("WindowTitleBarToggleButtonTheme.axaml");

        var buttonTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowTitleBarButtonTheme.axaml"));
        var toggleTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowTitleBarToggleButtonTheme.axaml"));
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        foreach (var theme in new[] { buttonTheme, toggleTheme })
        {
            theme.Root.ShouldNotBeNull();
            theme.Root!.Name.LocalName.ShouldBe("ControlTheme");
            theme.Root.Attribute(xaml + "Class").ShouldNotBeNull();
            theme.Root.Descendants()
                 .ShouldAllBe(element => element.Attribute(xaml + "Class") == null);
            theme.ToString().ShouldNotContain("CaptionButtonAction");
            theme.ToString().ShouldNotContain("ElementRole");
        }
    }

    // managed caption band 与 AddOn 家族共用 CaptionButtonFrame，背景层节点名同为 PART_Frame。
    // PixelAlignedBorder 派生自 Decorator（不是 Avalonia Border），必须按真实类型查找。
    private static PixelAlignedBorder FindCaptionFrame(Control control)
    {
        return control.GetVisualDescendants()
                      .OfType<PixelAlignedBorder>()
                      .Single(border => border.Name == "PART_Frame");
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }

    private static void RaiseDoubleClick(Control source)
    {
        var pointer = new Pointer(
            Pointer.GetNextFreeId(),
            PointerType.Mouse,
            true);
        source.RaiseEvent(new PointerPressedEventArgs(
            source,
            pointer,
            source,
            default,
            0,
            new PointerPointProperties(
                RawInputModifiers.LeftMouseButton,
                PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None,
            2));
        source.RaiseEvent(new PointerReleasedEventArgs(
            source,
            pointer,
            source,
            default,
            1,
            new PointerPointProperties(
                RawInputModifiers.None,
                PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None,
            MouseButton.Left));
    }
}
