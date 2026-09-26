using System.Xml.Linq;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.VisualTree;
using AtomUI.Icons.AntDesign;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class WindowTitleBarTokenTests
{
    static WindowTitleBarTokenTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Caption_Button_Padding_Uses_Default_Two_Size_Units()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBarToken.cs"));

        source.ShouldContain("CaptionButtonPadding = new Thickness(EffectiveGlobalToken.SizeUnit * 2)");
    }

    [Fact]
    public void Windows_Caption_Button_Icons_Use_Native_Size()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBarToken.cs"));

        source.ShouldContain("CaptionButtonIconSize       = EffectiveGlobalToken.IconSize;");
        source.ShouldContain("WindowsCaptionIconSize      = 11;");
    }

    [Fact]
    public void Title_Bar_Content_Padding_Has_No_Vertical_Inset()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBarToken.cs"));

        source.ShouldContain("TitleBarPadding             = new Thickness(LogoAndTitleSpacing * 1.8, 0)");
    }

    [Fact]
    public void Windows_And_Linux_Leading_Use_The_Dedicated_Logo_Left_AddOn_Spacing()
    {
        var tokenSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBarToken.cs"));
        var document = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowTitleBarTheme.axaml"));

        tokenSource.ShouldContain("public double LogoAndLeftAddOnSpacing { get; set; }");
        tokenSource.ShouldContain(
            "LogoAndLeftAddOnSpacing     = EffectiveGlobalToken.SpacingXXS;");

        foreach (var selector in new[] { "^[OsType=Windows]", "^[OsType=Linux]" })
        {
            var platformStyle = document.Descendants()
                                        .Single(element =>
                                            element.Name.LocalName == "Style" &&
                                            (string?)element.Attribute("Selector") == selector);
            var layoutPanel = platformStyle.Descendants()
                                           .Single(element =>
                                               element.Name.LocalName == "WindowTitleBarLayoutPanel");
            var leading = layoutPanel.Elements()
                                     .Single(element =>
                                         element.Attributes().Any(attribute =>
                                             attribute.Name.LocalName == "WindowTitleBarLayoutPanel.Role" &&
                                             attribute.Value == "Leading"));

            leading.Name.LocalName.ShouldBe("DockPanel");
            leading.Attribute("HorizontalSpacing")?.Value.ShouldBe(
                "{atom:WindowTitleBarTokenResource LogoAndLeftAddOnSpacing}");
        }

        var macOsStyle = document.Descendants()
                                 .Single(element =>
                                     element.Name.LocalName == "Style" &&
                                     (string?)element.Attribute("Selector") == "^[OsType=macOS]");
        var macOsLeading = macOsStyle.Descendants()
                                       .Single(element =>
                                           element.Attributes().Any(attribute =>
                                               attribute.Name.LocalName == "WindowTitleBarLayoutPanel.Role" &&
                                               attribute.Value == "Leading"));

        macOsLeading.Name.LocalName.ShouldBe("ContentPresenter");
        macOsLeading.Attribute("HorizontalSpacing").ShouldBeNull();
    }

    [Fact]
    public void Windows_Title_Bar_Theme_Leaves_Close_Button_Flush_To_The_Right_Edge()
    {
        var document = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowTitleBarTheme.axaml"));

        var windowsStyle = document.Descendants()
                                   .Single(element =>
                                       element.Name.LocalName == "Style" &&
                                       (string?)element.Attribute("Selector") ==
                                       "^[OsType=Windows]");
        var template = windowsStyle.Descendants()
                                     .Single(element => element.Name.LocalName == "ControlTemplate");
        var frame = template.Elements().Single();
        var captionButtonGroup = FindTemplatePart(template, "PART_CaptionButtonGroup");
        var leftPaddingConverter = FindResource(document.Root!, "WindowTitleBarLeftPaddingConverter");
        var layoutPanel = frame.Elements()
                               .Single(element => element.Name.LocalName == "WindowTitleBarLayoutPanel");
        var trailing = layoutPanel.Elements()
                                  .Single(element =>
                                      element.Attributes().Any(attribute =>
                                          attribute.Name.LocalName == "WindowTitleBarLayoutPanel.Role" &&
                                          attribute.Value == "Trailing"));

        frame.Name.LocalName.ShouldBe("Border");
        frame.Attribute("Name")?.Value.ShouldBe("Frame");
        frame.Attribute("Background")?.Value.ShouldBe("{TemplateBinding Background}");
        frame.Attribute("Padding").ShouldBeNull();
        layoutPanel.Attribute("Padding")?.Value.ShouldBe(
            "{TemplateBinding Padding, Converter={StaticResource WindowTitleBarLeftPaddingConverter}}");
        leftPaddingConverter.Name.LocalName.ShouldBe("BorderThicknessFilterConverter");
        leftPaddingConverter.Attribute("Left")?.Value.ShouldNotBe("False");
        leftPaddingConverter.Attribute("Right")?.Value.ShouldBe("False");
        leftPaddingConverter.Attribute("Top")?.Value.ShouldBe("False");
        leftPaddingConverter.Attribute("Bottom")?.Value.ShouldBe("False");
        captionButtonGroup.Ancestors().ShouldContain(frame);
        captionButtonGroup.Parent.ShouldBe(trailing);
        captionButtonGroup.ElementsAfterSelf().ShouldBeEmpty();
    }

    [Fact]
    public void Shared_Window_Chrome_Metrics_Do_Not_Use_Speculative_Os_Branches()
    {
        var windowTokenSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/WindowToken.cs"));
        var titleBarTokenSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/WindowTitleBarToken.cs"));

        windowTokenSource.ShouldContain("TitleBarHeight               = 40;");
        windowTokenSource.ShouldContain("FullscreenHeaderFramePadding = new Thickness(24, 0);");
        windowTokenSource.ShouldNotContain("GetPlatformTitleBarHeight");
        windowTokenSource.ShouldNotContain("GetPlatformFullscreenHeaderPadding");
        windowTokenSource.ShouldNotContain("OperatingSystem.");

        titleBarTokenSource.ShouldContain("Height                      = 40;");
        titleBarTokenSource.ShouldContain("FullscreenCaptionButtonSize = 24;");
        titleBarTokenSource.ShouldContain("HeaderHorizontalSpacing     = 8;");
        titleBarTokenSource.ShouldNotContain("GetPlatformTitleBarHeight");
        titleBarTokenSource.ShouldNotContain("GetPlatformFullscreenCaptionButtonSize");
        titleBarTokenSource.ShouldNotContain("GetPlatformHeaderHorizontalSpacing");
        titleBarTokenSource.ShouldNotContain("OperatingSystem.");
    }

    [Fact]
    public void Linux_Caption_Button_Background_Is_Inset_Without_Changing_Button_Layout_Size()
    {
        var frameTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonFrameTheme.axaml"));
        var buttonTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonTheme.axaml"));
        var groupTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonGroupTheme.axaml"));

        // 共享几何外壳自己拥有两层：内缩的背景层 + 决定布局盒的内容层。
        var frame = frameTheme.Descendants()
                               .Single(element => (string?)element.Attribute("Name") == "PART_Frame");
        var contentFrame = frame.ElementsAfterSelf().Single();
        frame.Name.LocalName.ShouldBe("PixelAlignedBorder");
        frame.Parent.ShouldNotBeNull();
        frame.Parent.Name.LocalName.ShouldBe("Panel");
        frame.Attribute("Margin")?.Value.ShouldBe("{TemplateBinding BackgroundInset}");
        frame.Attribute("CornerRadius")?.Value.ShouldBe("{TemplateBinding CornerRadius}");
        frame.Attribute("Padding").ShouldBeNull();
        contentFrame.Name.LocalName.ShouldBe("Border");
        contentFrame.Attribute("Padding")?.Value.ShouldBe("{TemplateBinding Padding}");

        // caption button 只组合该外壳，不再自己声明背景层。
        var captionFrame = buttonTheme.Descendants()
                                      .Single(element => element.Name.LocalName == "CaptionButtonFrame");
        captionFrame.Attribute("BackgroundInset")?.Value.ShouldBe("{TemplateBinding BackgroundInset}");
        captionFrame.Attribute("CornerRadius")?.Value.ShouldBe("{TemplateBinding EffectiveCornerRadius}");
        captionFrame.Descendants()
                    .ShouldContain(element => (string?)element.Attribute("Name") == "PART_IconPresenter");
        buttonTheme.Descendants()
                   .ShouldNotContain(element => (string?)element.Attribute("Name") == "PART_Frame");

        var linuxStyle = groupTheme.Descendants()
                                   .Single(element =>
                                       element.Name.LocalName == "Style" &&
                                       (string?)element.Attribute("Selector") ==
                                       "^ /template/ atom|CaptionButton" &&
                                       element.Ancestors().Any(ancestor =>
                                           ancestor.Name.LocalName == "Style" &&
                                           (string?)ancestor.Attribute("Selector") == "^[OsType=Linux]"));
        var insetSetter = linuxStyle.Elements()
                                    .Single(element => element.Name.LocalName == "Setter");

        insetSetter.Attribute("Property")?.Value.ShouldBe("BackgroundInset");
        insetSetter.Attribute("Value")?.Value.ShouldBe(
            "{atom:WindowTitleBarTokenResource CaptionButtonBackgroundInset}");
        linuxStyle.Ancestors()
                  .ShouldContain(element =>
                      element.Name.LocalName == "Style" &&
                      (string?)element.Attribute("Selector") == "^[OsType=Linux]");
    }

    [Fact]
    public void Linux_Caption_Button_Background_Inset_Is_Applied_At_Runtime()
    {
        var group = new CaptionButtonGroup();
        group.SetValue(CaptionButtonGroup.OsTypeProperty, OsType.Linux);
        Application.Current!.TryFindResource(typeof(CaptionButtonGroup), out var resource).ShouldBeTrue();
        group.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width   = 400,
            Height  = 100,
            Content = group
        };

        try
        {
            host.Show();
            group.ApplyTemplate();
            host.UpdateLayout();

            var buttons = group.GetVisualDescendants().OfType<CaptionButton>().ToList();
            buttons.ShouldNotBeEmpty();

            foreach (var button in buttons)
            {
                button.ApplyTemplate();
                // headless harness 不保证实例化嵌套模板；只补齐尚未实例化的 CaptionButtonFrame。
                foreach (var captionFrame in button.GetVisualDescendants()
                                                   .OfType<CaptionButtonFrame>()
                                                   .Where(candidate => !candidate.GetVisualChildren().Any())
                                                   .ToList())
                {
                    captionFrame.ApplyTemplate();
                }
                var frame = button.GetVisualDescendants()
                                  .OfType<PixelAlignedBorder>()
                                  .Single(border => border.Name == "PART_Frame");

                button.BackgroundInset.ShouldBe(new Thickness(2));
                frame.Margin.ShouldBe(new Thickness(2));
            }
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void Windows_Caption_Buttons_Use_Dedicated_Token_Colored_Geometries()
    {
        var iconResources = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowsCaptionIconGeometries.axaml"));
        var groupTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonGroupTheme.axaml"));
        var buttonTheme = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonTheme.axaml"));
        var assetManifest = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/GeneratedFiles/AtomUI.Generator/AtomUI.Generator.ThemeAssetManifestGenerator/GeneratedControlThemeAssetManifest.g.cs"));

        var expectedResources = new[]
        {
            "WindowCaptionMinimizeIconGeometry",
            "WindowCaptionMaximizeIconGeometry",
            "WindowCaptionRestoreIconGeometry",
            "WindowCaptionCloseIconGeometry"
        };
        var geometryKeys = iconResources.Root!
                                            .Elements()
                                            .Where(element => element.Name.LocalName == "StreamGeometry")
                                            .Select(element => element.Attributes()
                                                                      .Single(attribute =>
                                                                          attribute.Name.LocalName == "Key")
                                                                      .Value)
                                            .ToList();

        geometryKeys.ShouldBe(expectedResources, ignoreOrder: true);
        iconResources.ToString().ShouldNotContain("#020202");

        var windowsStyle = groupTheme.Descendants()
                                     .Single(element =>
                                         element.Name.LocalName == "Style" &&
                                         (string?)element.Attribute("Selector") == "^[OsType=Windows]" &&
                                         element.Descendants().Any(descendant =>
                                             descendant.Name.LocalName == "PathIcon"));
        foreach (var resourceKey in expectedResources)
        {
            windowsStyle.Descendants()
                        .ShouldContain(element =>
                            element.Name.LocalName == "PathIcon" &&
                            (string?)element.Attribute("Data") == $"{{StaticResource {resourceKey}}}");
        }

        windowsStyle.ToString().ShouldNotContain("MinusOutlined");
        windowsStyle.ToString().ShouldNotContain("WindowMaximizedOutlined");
        windowsStyle.ToString().ShouldNotContain("WindowRestoreOutlined");
        windowsStyle.ToString().ShouldNotContain("WindowCloseOutlined");
        groupTheme.ToString().ShouldContain("WindowTitleBarTokenResource WindowsCaptionIconSize");
        var extendedActionButtons = windowsStyle.Descendants()
                                                .Where(element =>
                                                    element.Name.LocalName == "WindowsCaptionButton" &&
                                                    (string?)element.Attribute("Classes") == "extended-action")
                                                .Select(element => (string?)element.Attribute("Name"))
                                                .ToList();
        extendedActionButtons.ShouldBe(
            ["PART_FullScreenButton", "PART_PinButton"],
            ignoreOrder: true);

        groupTheme.Descendants()
                  .Single(element =>
                      element.Name.LocalName == "Style" &&
                      (string?)element.Attribute("Selector") ==
                      "^[OsType=Windows] /template/ atom|WindowsCaptionButton.extended-action")
                  .ToString()
                  .ShouldContain("WindowTitleBarTokenResource CaptionButtonIconSize");
        buttonTheme.ToString().ShouldContain("WindowTitleBarTokenResource ActiveColor");
        buttonTheme.ToString().ShouldContain("WindowTitleBarTokenResource InactiveColor");

        assetManifest.ShouldContain("WindowTitleBar/Themes/WindowsCaptionIconGeometries.axaml");
        assetManifest.ShouldContain("WindowTitleBar/Themes/CaptionButtonGroupTheme.axaml");
    }

    [Fact]
    public void Built_In_Window_Themes_Use_Concrete_Icons_For_NativeAot_Trimming()
    {
        var themePaths = new[]
        {
            "src/AtomUI.Desktop.Controls/Window/Themes/WindowDrawnDecorationsTheme.axaml",
            "src/AtomUI.Desktop.Controls/Window/Themes/FullscreenPopoverLayerTheme.axaml",
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonGroupTheme.axaml"
        };
        var themes = themePaths.Select(path => XDocument.Load(GetRepoFile(path))).ToList();
        var iconTypes = themes.SelectMany(theme => theme.Descendants())
                              .Select(element => element.Name.LocalName)
                              .ToHashSet(StringComparer.Ordinal);

        foreach (var theme in themes)
        {
            theme.ToString().ShouldNotContain("AntDesignIconProvider");
        }

        var expectedIconTypes = new[]
        {
            "FullscreenOutlined",
            "FullscreenExitOutlined",
            "MinusOutlined",
            "WindowCloseOutlined",
            "WindowMaximizedOutlined",
            "WindowPinOutlined",
            "WindowRestoreOutlined",
            "WindowUnpinOutlined"
        };

        foreach (var iconType in expectedIconTypes)
        {
            iconTypes.ShouldContain(iconType);
        }
    }

    [Fact]
    public void Linux_Caption_Buttons_Instantiate_Concrete_AntDesign_Icons()
    {
        var group = new CaptionButtonGroup();
        group.SetValue(CaptionButtonGroup.OsTypeProperty, OsType.Linux);
        Application.Current!.TryFindResource(typeof(CaptionButtonGroup), out var resource).ShouldBeTrue();
        group.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

        var host = new Avalonia.Controls.Window
        {
            Width   = 400,
            Height  = 100,
            Content = group
        };

        try
        {
            host.Show();
            group.ApplyTemplate();
            host.UpdateLayout();

            var buttons = group.GetVisualDescendants()
                               .OfType<CaptionButton>()
                               .ToDictionary(button => button.Name!, StringComparer.Ordinal);

            buttons["PART_FullScreenButton"].NormalIcon.ShouldBeOfType<FullscreenOutlined>();
            buttons["PART_FullScreenButton"].CheckedIcon.ShouldBeOfType<FullscreenExitOutlined>();
            buttons["PART_PinButton"].NormalIcon.ShouldBeOfType<WindowPinOutlined>();
            buttons["PART_PinButton"].CheckedIcon.ShouldBeOfType<WindowUnpinOutlined>();
            buttons["PART_MinimizeButton"].NormalIcon.ShouldBeOfType<MinusOutlined>();
            buttons["PART_MaximizeButton"].NormalIcon.ShouldBeOfType<WindowMaximizedOutlined>();
            buttons["PART_MaximizeButton"].CheckedIcon.ShouldBeOfType<WindowRestoreOutlined>();
            buttons["PART_CloseButton"].NormalIcon.ShouldBeOfType<WindowCloseOutlined>();
        }
        finally
        {
            host.Close();
        }
    }

    [Fact]
    public void Window_State_Changes_Invalidate_Stale_Maximize_Hover()
    {
        var buttonSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/WindowsCaptionButton.cs"));
        var groupThemeSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/CaptionButtonGroupTheme.axaml"));
        var themeSource = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/WindowTitleBar/Themes/WindowsCaptionButtonTheme.axaml"));

        buttonSource.ShouldContain("HostWindowStateProperty.Changed.AddClassHandler<WindowsCaptionButton>");
        buttonSource.ShouldContain("button.InvalidatePointerOverVisualState()");
        buttonSource.ShouldContain("IsPointerOverSuppressed = IsPointerOver;");
        buttonSource.ShouldContain("protected override void OnPointerMoved(PointerEventArgs e)");
        groupThemeSource.ShouldContain("HostWindowState=\"{TemplateBinding HostWindowState}\"");
        themeSource.ShouldContain("^[IsPointerOverSuppressed=False]:pointerover");
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

    private static XElement FindTemplatePart(XElement root, string partName)
    {
        var part = root.Descendants()
                       .SingleOrDefault(element => (string?)element.Attribute("Name") == partName);

        part.ShouldNotBeNull();
        return part;
    }

    private static XElement FindResource(XElement root, string resourceKey)
    {
        var resource = root.Descendants()
                           .SingleOrDefault(element =>
                               element.Attributes().Any(attribute =>
                                   attribute.Name.LocalName == "Key" &&
                                   attribute.Value == resourceKey));

        resource.ShouldNotBeNull();
        return resource;
    }
}
