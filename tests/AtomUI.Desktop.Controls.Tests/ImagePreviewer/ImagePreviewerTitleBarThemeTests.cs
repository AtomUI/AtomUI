using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.ImagePreviewer;

public class ImagePreviewerTitleBarThemeTests
{
    static ImagePreviewerTitleBarThemeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(OsType.Windows)]
    [InlineData(OsType.Linux)]
    [InlineData(OsType.macOS)]
    public void ImagePreviewer_TitleBar_Caption_Group_Projects_Host_Contract_And_Executes_Window_Commands(
        OsType osType)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var titleBar = new ImagePreviewerTitleBar
            {
                Width  = 640,
                Height = 40
            };
            titleBar.SetValue(WindowTitleBar.OsTypeProperty, osType);
            Application.Current!.TryFindResource(typeof(ImagePreviewerTitleBar), out var resource)
                       .ShouldBeTrue();
            titleBar.Theme = resource.ShouldBeAssignableTo<ControlTheme>();

            var window = new global::AtomUI.Desktop.Controls.Window
            {
                Width                            = 640,
                Height                           = 480,
                IsTitleBarVisible                = false,
                IsMinimizeCaptionButtonVisible   = false,
                IsMaximizeCaptionButtonVisible   = true,
                IsCloseCaptionButtonVisible      = false,
                IsFullScreenCaptionButtonVisible = true,
                IsPinCaptionButtonVisible        = true,
                CanMinimize                      = false,
                CanMaximize                      = false,
                Topmost                         = true,
                Content                         = titleBar
            };

            try
            {
                window.Show();
                titleBar.ApplyTemplate();
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();

                var captionButtonGroup = titleBar.GetVisualDescendants()
                                                 .OfType<CaptionButtonGroup>()
                                                 .Single();
                var command = captionButtonGroup.CaptionButtonCommand.ShouldNotBeNull();

                command.ShouldBeSameAs(window.CaptionButtonCommand);
                captionButtonGroup.IsWindowActive.ShouldBe(titleBar.IsWindowActive);
                captionButtonGroup.IsMotionEnabled.ShouldBe(titleBar.IsMotionEnabled);
                captionButtonGroup.HostWindowState.ShouldBe(titleBar.HostWindowState);
                captionButtonGroup.IsWindowTopmost.ShouldBe(titleBar.IsWindowTopmost);
                captionButtonGroup.IsMinimizeCaptionButtonVisible.ShouldBeFalse();
                captionButtonGroup.IsMaximizeCaptionButtonVisible.ShouldBeTrue();
                captionButtonGroup.IsCloseCaptionButtonVisible.ShouldBeFalse();
                captionButtonGroup.IsFullScreenCaptionButtonVisible.ShouldBeTrue();
                captionButtonGroup.IsPinCaptionButtonVisible.ShouldBeTrue();
                captionButtonGroup.CanMinimize.ShouldBeFalse();
                captionButtonGroup.CanMaximize.ShouldBeFalse();
                captionButtonGroup.IsPinCaptionButtonSupported.ShouldBe(titleBar.IsPinCaptionButtonSupported);

                window.CanMinimize = true;
                window.CanMaximize = true;
                Dispatcher.UIThread.RunJobs();

                captionButtonGroup.CanMinimize.ShouldBeTrue();
                captionButtonGroup.CanMaximize.ShouldBeTrue();

                command.Execute(CaptionButtonAction.ToggleMaximize);
                window.WindowState.ShouldBe(WindowState.Maximized);
                captionButtonGroup.HostWindowState.ShouldBe(WindowState.Maximized);
                captionButtonGroup.IsWindowMaximized.ShouldBeTrue();

                command.Execute(CaptionButtonAction.ToggleMaximize);
                window.WindowState.ShouldBe(WindowState.Normal);
                captionButtonGroup.HostWindowState.ShouldBe(WindowState.Normal);
                captionButtonGroup.IsWindowMaximized.ShouldBeFalse();

                command.Execute(CaptionButtonAction.Minimize);
                window.WindowState.ShouldBe(WindowState.Minimized);
                captionButtonGroup.HostWindowState.ShouldBe(WindowState.Minimized);

                window.WindowState = WindowState.Normal;
                command.Execute(CaptionButtonAction.Close);
                window.IsVisible.ShouldBeFalse();
            }
            finally
            {
                window.Content = null;
                window.Close();
                Dispatcher.UIThread.RunJobs();
            }
        });
    }

    [Fact]
    public void ImagePreviewer_TitleBar_Background_Uses_Dialog_Frame_Background_Token()
    {
        var document = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerTitleBarTheme.axaml"));

        var backgroundSetter = document.Root!.Elements()
                                       .SingleOrDefault(element =>
                                           element.Name.LocalName == "Setter" &&
                                           (string?)element.Attribute("Property") == "Background");

        backgroundSetter.ShouldNotBeNull();
        backgroundSetter.Attribute("Value")?.Value.ShouldBe(
            "{atom:ImagePreviewerTokenResource TitleBarBackgroundColor}");
    }

    [Fact]
    public void ImagePreviewer_Linux_TitleBar_Applies_Frame_Padding_Through_Shared_Layout()
    {
        var document = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerTitleBarTheme.axaml"));

        var linuxStyle = document.Descendants()
                                 .Single(element =>
                                     element.Name.LocalName == "Style" &&
                                     (string?)element.Attribute("Selector") == "^[OsType=Linux]");
        var template = linuxStyle.Descendants()
                                 .Single(element => element.Name.LocalName == "ControlTemplate");
        var frame = template.Elements().Single();
        var layoutPanel = frame.Elements().Single();
        var captionButtonGroup = FindTemplatePart(template, "PART_CaptionButtonGroup");

        frame.Name.LocalName.ShouldBe("Border");
        frame.Attribute("Name")?.Value.ShouldBe("Frame");
        frame.Attribute("Padding").ShouldBeNull();
        layoutPanel.Name.LocalName.ShouldBe("WindowTitleBarLayoutPanel");
        layoutPanel.Attribute("Padding")?.Value.ShouldBe("{TemplateBinding Padding}");
        captionButtonGroup.Ancestors().ShouldContain(layoutPanel);
    }

    [Fact]
    public void ImagePreviewer_Windows_TitleBar_Leaves_Close_Button_Flush_To_Right_Edge()
    {
        var document = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerTitleBarTheme.axaml"));

        var windowsStyle = document.Descendants()
                                   .Single(element =>
                                       element.Name.LocalName == "Style" &&
                                       (string?)element.Attribute("Selector") == "^[OsType=Windows]");
        var template = windowsStyle.Descendants()
                                   .Single(element => element.Name.LocalName == "ControlTemplate");
        var frame = template.Elements().Single();
        var captionButtonGroup = FindTemplatePart(template, "PART_CaptionButtonGroup");
        var layoutPanel = frame.Elements().Single();
        var leftPaddingConverter = FindResource(document.Root!, "WindowTitleBarLeftPaddingConverter");

        frame.Name.LocalName.ShouldBe("Border");
        frame.Attribute("Name")?.Value.ShouldBe("Frame");
        frame.Attribute("Background")?.Value.ShouldBe("{TemplateBinding Background}");
        frame.Attribute("Padding").ShouldBeNull();
        layoutPanel.Name.LocalName.ShouldBe("WindowTitleBarLayoutPanel");
        layoutPanel.Parent.ShouldBe(frame);
        layoutPanel.Attribute("Padding")?.Value.ShouldBe(
            "{TemplateBinding Padding, Converter={StaticResource WindowTitleBarLeftPaddingConverter}}");
        leftPaddingConverter.Name.LocalName.ShouldBe("BorderThicknessFilterConverter");
        leftPaddingConverter.Attribute("Left")?.Value.ShouldNotBe("False");
        leftPaddingConverter.Attribute("Right")?.Value.ShouldBe("False");
        leftPaddingConverter.Attribute("Top")?.Value.ShouldBe("False");
        leftPaddingConverter.Attribute("Bottom")?.Value.ShouldBe("False");
        captionButtonGroup.Ancestors().ShouldContain(layoutPanel);
    }

    [Fact]
    public void ImagePreviewer_TitleBar_Templates_Use_One_Shared_Layout_With_Three_Roles()
    {
        var document = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerTitleBarTheme.axaml"));

        var templates = document.Descendants()
                                .Where(element => element.Name.LocalName == "ControlTemplate")
                                .ToList();

        templates.Count.ShouldBeGreaterThanOrEqualTo(3);

        foreach (var template in templates)
        {
            var frame = template.Elements().Single();
            var layoutPanel = frame.Elements().Single();
            var directChildren = layoutPanel.Elements().ToList();

            frame.Name.LocalName.ShouldBe("Border");
            frame.Attribute("Padding").ShouldBeNull();
            layoutPanel.Name.LocalName.ShouldBe("WindowTitleBarLayoutPanel");
            layoutPanel.Attribute("TitleAlignment")?.Value.ShouldBe("{TemplateBinding TitleAlignment}");
            layoutPanel.Attribute("OsType")?.Value.ShouldBe("{TemplateBinding OsType}");
            layoutPanel.Attribute("NativeChromeInsets")?.Value.ShouldBe("{TemplateBinding NativeChromeInsets}");
            layoutPanel.Attribute("IsCsdEnabled")?.Value.ShouldBe("{TemplateBinding IsCsdEnabled}");
            layoutPanel.Attribute("WindowState")?.Value.ShouldBe("{TemplateBinding HostWindowState}");
            layoutPanel.Attribute("HorizontalSpacing")?.Value.ShouldBe(
                "{atom:WindowTitleBarTokenResource HeaderHorizontalSpacing}");

            directChildren.Count(child => GetLayoutRole(child) == "Leading").ShouldBe(1);
            directChildren.Count(child => GetLayoutRole(child) == "Title").ShouldBe(1);
            directChildren.Count(child => GetLayoutRole(child) == "Trailing").ShouldBe(1);

            FindTemplatePart(template, "PART_LeftAddOn").Parent.ShouldBe(layoutPanel);
            FindTemplatePart(template, "PART_TitleLayout").Parent.ShouldBe(layoutPanel);
            FindTemplatePart(template, "PART_RightAddOn")
                .Ancestors()
                .First(element => element.Parent == layoutPanel)
                .ShouldBe(directChildren.Single(child => GetLayoutRole(child) == "Trailing"));
            FindTemplatePart(template, "PART_CaptionButtonGroup")
                .Ancestors()
                .First(element => element.Parent == layoutPanel)
                .ShouldBe(directChildren.Single(child => GetLayoutRole(child) == "Trailing"));
        }
    }

    [Fact]
    public void ImagePreviewer_TitleBar_Template_Constrains_The_Title_After_The_Explicit_Icon()
    {
        var document = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewerTitleBarTheme.axaml"));

        var templates = document.Descendants()
                                .Where(element => element.Name.LocalName == "ControlTemplate")
                                .ToList();

        templates.Count.ShouldBeGreaterThanOrEqualTo(3);

        foreach (var template in templates)
        {
            var titleLayout    = FindTemplatePart(template, "PART_TitleLayout");
            var iconPresenter  = FindTemplatePart(template, "PART_IconPresenter");
            var titlePresenter = FindTemplatePart(template, "PART_ContentPresenter");

            titleLayout.Name.LocalName.ShouldBe("DockPanel");
            titleLayout.Attribute("LastChildFill")?.Value.ShouldBe("True");
            GetLayoutRole(titleLayout).ShouldBe("Title");
            titleLayout.Attribute("IsHitTestVisible")?.Value.ShouldBe("False");
            titleLayout.Attribute("ClipToBounds")?.Value.ShouldBe("True");
            titleLayout.Attribute("HorizontalSpacing")?.Value.ShouldBe(
                "{atom:WindowTitleBarTokenResource LogoAndTitleSpacing}");

            iconPresenter.Name.LocalName.ShouldBe("IconPresenter");
            iconPresenter.Attribute("DockPanel.Dock")?.Value.ShouldBe("Left");
            iconPresenter.Attribute("Icon")?.Value.ShouldBe("{TemplateBinding Icon}");
            iconPresenter.Attribute("IsVisible")?.Value.ShouldBe("{TemplateBinding Icon, Converter={x:Static ObjectConverters.IsNotNull}}");
            iconPresenter.Parent.ShouldBe(titleLayout);
            titlePresenter.Parent.ShouldBe(titleLayout);
            iconPresenter.ElementsAfterSelf().ShouldContain(titlePresenter);
            titlePresenter.Attribute("TextWrapping")?.Value.ShouldBe("NoWrap");
            titlePresenter.Attribute("TextTrimming")?.Value.ShouldBe("CharacterEllipsis");
        }

        document.ToString().ShouldNotContain("PART_Logo");
        document.ToString().ShouldNotContain("TemplateBinding Logo");
        document.ToString().ShouldNotContain("IsEffectiveLogoVisible");
    }

    [Fact]
    public void ImagePreviewer_TitleBar_Toolbar_Uses_Same_Navigation_Icons_As_Side_Nav()
    {
        var document = XDocument.Load(GetRepoFile(
            "src/AtomUI.Desktop.Controls/ImagePreviewer/Themes/ImagePreviewToolbarTheme.axaml"));

        var previousButton = FindTemplatePart(document.Root!, "PART_PreviousButton");
        var nextButton     = FindTemplatePart(document.Root!, "PART_NextButton");

        previousButton.Attribute("Icon")?.Value.ShouldBe("{antdicons:AntDesignIconProvider LeftOutlined}");
        nextButton.Attribute("Icon")?.Value.ShouldBe("{antdicons:AntDesignIconProvider RightOutlined}");
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

    private static string? GetLayoutRole(XElement element)
    {
        return element.Attributes()
                      .SingleOrDefault(attribute =>
                          attribute.Name.LocalName == "WindowTitleBarLayoutPanel.Role")
                      ?.Value;
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
}
