using AtomUI.Toolkits.GalleryBase.Localization;
using AtomUI.Toolkits.GalleryBase.Navigation;
using AtomUIGallery.ShowCases.Window;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class WindowShowCasePageTests
{
    [Fact]
    public void Window_ShowCase_Is_Registered_Under_General_Category()
    {
        var configuration      = global::AtomUIGallery.AtomUIGalleryModule.CreateConfiguration();
        var assemblyInfoSource = ReadRepoFile("controlgallery/AtomUIGallery/Properties/AssemblyInfo.cs");
        var navigationEn = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/en-US.xlf");
        var navigationZhCn = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/zh-CN.xlf");
        var navigationZhTw = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/zh-TW.xlf");

        var generalNode = Walk(configuration.NavigationNodes)
            .First(node => node.Key == "General");
        var windowNode = Walk(configuration.NavigationNodes)
            .First(node => node.Key == WindowViewModel.ID);

        generalNode.IsRoute.ShouldBeFalse();
        generalNode.Header.ShouldBeAssignableTo<IGalleryLocalizedText>();
        windowNode.IsRoute.ShouldBeTrue();
        windowNode.Header.ShouldBeAssignableTo<IGalleryLocalizedText>();
        configuration.Routes.ContainsRoute(WindowViewModel.ID).ShouldBeTrue();
        assemblyInfoSource.ShouldContain("AtomUIGallery.ShowCases.Window");

        foreach (var localization in new[] { navigationEn, navigationZhCn, navigationZhTw })
        {
            localization.ContainsKey("General_Window").ShouldBeTrue();
        }
    }

    [Fact]
    public void Window_ShowCase_First_Four_Items_Open_Configured_Demo_Windows()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/Views/WindowShowCase.axaml");

        var basicDemo = ExtractShowCaseItemMarkup(source, "WindowShowCaseLangResource BasicTitle");
        basicDemo.ShouldContain("WindowShowCaseLangResource BasicDescription");
        basicDemo.ShouldContain("Click=\"HandleBasicWindowButtonClick\"");

        var alignmentDemo = ExtractShowCaseItemMarkup(source, "WindowShowCaseLangResource TitleAlignmentTitle");
        alignmentDemo.ShouldContain("WindowShowCaseLangResource TitleAlignmentDescription");
        alignmentDemo.ShouldContain("Click=\"HandleTitleAlignmentWindowButtonClick\"");

        var titleHiddenDemo = ExtractShowCaseItemMarkup(source, "WindowShowCaseLangResource IsTitleVisibleTitle");
        titleHiddenDemo.ShouldContain("WindowShowCaseLangResource IsTitleVisibleDescription");
        titleHiddenDemo.ShouldContain("SourceKey=\"window-title-visibility\"");
        titleHiddenDemo.ShouldContain("Click=\"HandleIsTitleVisibleWindowButtonClick\"");

        var logoDemo = ExtractShowCaseItemMarkup(source, "WindowShowCaseLangResource LogoVisibilityTitle");
        logoDemo.ShouldContain("WindowShowCaseLangResource LogoVisibilityDescription");
        logoDemo.ShouldContain("Click=\"HandleLogoVisibilityWindowButtonClick\"");

        var codeBehind = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/Views/WindowShowCase.axaml.cs");
        codeBehind.ShouldContain("ShowDemoWindow(() => new BasicDemoWindow())");
        codeBehind.ShouldContain("ShowDemoWindow(() => new TitleAlignmentDemoWindow())");
        codeBehind.ShouldContain("ShowDemoWindow(() => new IsTitleVisibleDemoWindow())");
        codeBehind.ShouldContain("ShowDemoWindow(() => new LogoVisibilityDemoWindow())");
    }

    [Fact]
    public void Window_First_Four_Demo_Windows_Apply_The_Documented_Configuration()
    {
        var basicSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/BasicDemoWindow.axaml");
        basicSource.ShouldContain("<atom:Window");
        basicSource.ShouldContain("Title=\"AtomUI Window\"");

        var alignmentSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/TitleAlignmentDemoWindow.axaml");
        alignmentSource.ShouldContain("<atom:Segmented");
        alignmentSource.ShouldContain("SelectionChanged=\"HandleAlignmentSelectionChanged\"");
        var alignmentCode = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/TitleAlignmentDemoWindow.axaml.cs");
        alignmentCode.ShouldContain("TitleAlignment = ");

        var titleHiddenSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/IsTitleVisibleDemoWindow.axaml");
        titleHiddenSource.ShouldContain("Title=\"AtomUI Demo\"");
        titleHiddenSource.ShouldContain("IsTitleVisible=\"False\"");

        var logoSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/LogoVisibilityDemoWindow.axaml");
        logoSource.ShouldContain("<atom:Segmented");
        logoSource.ShouldContain("SelectionChanged=\"HandleLogoVisibilitySelectionChanged\"");
        var logoCode = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/LogoVisibilityDemoWindow.axaml.cs");
        logoCode.ShouldContain("LogoVisibility = ");
    }

    [Fact]
    public void Window_ShowCase_Items_Five_To_Eight_Open_Configured_Demo_Windows()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/Views/WindowShowCase.axaml");

        var addOnDemo = ExtractShowCaseItemMarkup(source, "WindowShowCaseLangResource AddOnTitle");
        addOnDemo.ShouldContain("WindowShowCaseLangResource AddOnDescription");
        addOnDemo.ShouldContain("Click=\"HandleAddOnWindowButtonClick\"");

        var captionDemo = ExtractShowCaseItemMarkup(source, "WindowShowCaseLangResource CaptionButtonsTitle");
        captionDemo.ShouldContain("WindowShowCaseLangResource CaptionButtonsDescription");
        captionDemo.ShouldContain("Click=\"HandleCaptionButtonsWindowButtonClick\"");

        var titleBarlessDemo = ExtractShowCaseItemMarkup(source, "WindowShowCaseLangResource TitleBarlessTitle");
        titleBarlessDemo.ShouldContain("WindowShowCaseLangResource TitleBarlessDescription");
        titleBarlessDemo.ShouldContain("Click=\"HandleTitleBarlessWindowButtonClick\"");

        var frameLayerDemo = ExtractShowCaseItemMarkup(source, "WindowShowCaseLangResource FrameLayerTitle");
        frameLayerDemo.ShouldContain("WindowShowCaseLangResource FrameLayerDescription");
        frameLayerDemo.ShouldContain("Click=\"HandleFrameLayerWindowButtonClick\"");

        var codeBehind = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/Views/WindowShowCase.axaml.cs");
        codeBehind.ShouldContain("ShowDemoWindow(() => new AddOnDemoWindow())");
        codeBehind.ShouldContain("ShowDemoWindow(() => new CaptionButtonsDemoWindow())");
        codeBehind.ShouldContain("ShowDemoWindow(() => new TitleBarlessDemoWindow())");
        codeBehind.ShouldContain("ShowDemoWindow(() => new FrameLayerDemoWindow())");
    }

    [Fact]
    public void Window_Demo_Windows_Five_To_Eight_Apply_The_Documented_Configuration()
    {
        var addOnSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/AddOnDemoWindow.axaml");
        addOnSource.ShouldContain("<atom:Window.LeftAddOn>");
        addOnSource.ShouldContain("<atom:WindowTitleBarButton");
        addOnSource.ShouldContain("<atom:WindowTitleBarToggleButton");
        addOnSource.ShouldContain("<atom:Window.RightAddOn>");

        var captionSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/CaptionButtonsDemoWindow.axaml");
        captionSource.ShouldContain("IsMinimizeCaptionButtonVisible=\"False\"");
        captionSource.ShouldContain("IsFullScreenCaptionButtonVisible=\"True\"");
        captionSource.ShouldContain("IsPinCaptionButtonVisible=\"True\"");

        var titleBarlessSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/TitleBarlessDemoWindow.axaml");
        titleBarlessSource.ShouldContain("IsTitleBarVisible=\"False\"");
        titleBarlessSource.ShouldContain("<atom:WindowTitleBar");

        var frameLayerSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/FrameLayerDemoWindow.axaml");
        frameLayerSource.ShouldContain("TitleBarFrameBackground");
        frameLayerSource.ShouldContain("ContentFrameBackground");
        frameLayerSource.ShouldContain("<atom:Window.WindowFrameLayer>");
        frameLayerSource.ShouldContain("WindowFrameLayerOpacity");
    }

    [Fact]
    public void Window_ShowCase_Playground_Item_Opens_The_Configured_Window()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/Views/WindowShowCase.axaml");

        var playgroundItem = ExtractShowCaseItemMarkup(source, "WindowShowCaseLangResource PlaygroundTitle");
        playgroundItem.ShouldContain("WindowShowCaseLangResource PlaygroundDescription");
        playgroundItem.ShouldContain("Click=\"HandlePlaygroundButtonClick\"");

        var codeBehind = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/Views/WindowShowCase.axaml.cs");
        codeBehind.ShouldContain("ShowDemoWindow(() => new WindowPlayground())");
    }

    [Fact]
    public void Window_Playground_Expose_All_Documented_TitleBar_And_Frame_Options()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/WindowPlayground.axaml");
        var codeBehind = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Window/ShowCaseControls/WindowPlayground.axaml.cs");

        source.ShouldContain("IsTitleVisible");
        source.ShouldContain("IsTitleBarVisible");
        source.ShouldContain("TitleAlignment");
        source.ShouldContain("LogoVisibility");
        source.ShouldContain("IsMinimizeCaptionButtonVisible");
        source.ShouldContain("IsMaximizeCaptionButtonVisible");
        source.ShouldContain("IsFullScreenCaptionButtonVisible");
        source.ShouldContain("IsPinCaptionButtonVisible");

        codeBehind.ShouldContain("Title = ");
        codeBehind.ShouldContain("TitleAlignment = ");
        codeBehind.ShouldContain("LogoVisibility = ");
        codeBehind.ShouldContain("TitleBarFrameBackground = ");
        codeBehind.ShouldContain("ContentFrameBackground = ");
    }

    private static string ExtractWindowExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string ComputeSha256(string source)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static int CountShowCaseItemElements(string source)
    {
        return System.Text.RegularExpressions.Regex
            .Matches(source, @"<gallery:ShowCaseItem(\s|>)").Count;
    }

    private static string ExtractShowCaseItemMarkup(string source, string titleMarker)
    {
        var titleIndex = source.IndexOf(titleMarker, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        const string itemStartMarker = "<gallery:ShowCaseItem";
        const string itemEndMarker = "</gallery:ShowCaseItem>";

        var itemStart = source.LastIndexOf(itemStartMarker, titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemEnd = source.IndexOf(itemEndMarker, titleIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(itemStart);

        return source[itemStart..(itemEnd + itemEndMarker.Length)];
    }

    private static IEnumerable<GalleryNavigationNode> Walk(IEnumerable<GalleryNavigationNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            foreach (var child in Walk(node.Children))
            {
                yield return child;
            }
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
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

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
