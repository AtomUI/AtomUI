using System.Text.RegularExpressions;
using AtomUI.Toolkits.GalleryBase.Localization;
using AtomUI.Toolkits.GalleryBase.Navigation;
using AtomUIGallery.ShowCases.BorderBeam;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class BorderBeamShowCasePageTests
{
    [Fact]
    public void BorderBeam_ShowCase_Is_Registered_Under_Other_Category()
    {
        var configuration = global::AtomUIGallery.AtomUIGalleryModule.CreateConfiguration();
        var registerSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/ShowCaseRegister.cs");
        var navigationEn = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/en-US.xlf");
        var navigationZhCn = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/zh-CN.xlf");
        var navigationZhTw = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/zh-TW.xlf");

        var otherNode = Walk(configuration.NavigationNodes)
            .First(node => node.Key == "Other");
        var borderBeamNode = Walk(configuration.NavigationNodes)
            .First(node => node.Key == BorderBeamViewModel.ID);

        otherNode.IsRoute.ShouldBeFalse();
        otherNode.Header.ShouldBeAssignableTo<IGalleryLocalizedText>();
        otherNode.Icon.ShouldNotBeNull();
        borderBeamNode.IsRoute.ShouldBeTrue();
        borderBeamNode.Header.ShouldBeAssignableTo<IGalleryLocalizedText>();
        configuration.Routes.ContainsRoute(BorderBeamViewModel.ID).ShouldBeTrue();
        registerSource.ShouldContain("AtomUIGalleryModule.RegisterViews(locator)");

        foreach (var localization in new[] { navigationEn, navigationZhCn, navigationZhTw })
        {
            localization.ContainsKey("Other").ShouldBeTrue();
            localization.ContainsKey("Other_BorderBeam").ShouldBeTrue();
        }
    }

    [Fact]
    public void BorderBeam_ShowCase_Header_Centers_Title_Tags_And_Uses_Blue_Introduced_Version_Tag()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/BorderBeam/Views/BorderBeamShowCase.axaml");
        var en = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/ShowCases/Other/BorderBeam/Localization/en-US.xlf");
        var zhCn = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/ShowCases/Other/BorderBeam/Localization/zh-CN.xlf");
        var zhTw = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/ShowCases/Other/BorderBeam/Localization/zh-TW.xlf");

        source.ShouldContain("<gallery:GalleryShowCaseHeader");

        foreach (var localization in new[] { en, zhCn, zhTw })
        {
            localization["ComponentIntroducedVersion"].ShouldBe("v6.0.5");
        }
    }

    [Fact]
    public void BorderBeam_ShowCase_Demos_Match_Ant_Design_Examples()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/BorderBeam/Views/BorderBeamShowCase.axaml");

        Regex.Matches(source, @"<gallery:ShowCaseItem\s").Count.ShouldBe(9);
        source.Split("<gallery:ShowCaseItem.DeferredContentTemplate>").Length.ShouldBe(10);

        var basicDemo = ExtractShowCaseItemMarkup(source, "BorderBeamShowCaseLangResource BasicTitle");
        basicDemo.ShouldContain("SourceKey=\"border-beam-basic\"");
        basicDemo.ShouldContain("<atom:BorderBeam");
        basicDemo.ShouldContain("<atom:Card Header=\"Workspace overview\"");
        basicDemo.ShouldContain("Users");
        basicDemo.ShouldContain("Projects");
        basicDemo.ShouldContain("Tasks");

        var hoverDemo = ExtractShowCaseItemMarkup(source, "BorderBeamShowCaseLangResource ShowOnHoverTitle");
        hoverDemo.ShouldContain("SourceKey=\"border-beam-show-on-hover\"");
        hoverDemo.ShouldContain("Classes=\"show-on-hover\"");
        hoverDemo.ShouldContain("BorderBeamShowCaseLangResource ShowOnHoverCardTitle");
        hoverDemo.ShouldContain("BorderBeamShowCaseLangResource ShowOnHoverCardDescription");

        source.ShouldContain("<Style Selector=\"atom|BorderBeam.show-on-hover\">");
        source.ShouldContain("<Style Selector=\"atom|BorderBeam.show-on-hover:pointerover\">");
        source.ShouldContain("<Setter Property=\"IsMotionEnabled\" Value=\"False\" />");
        source.ShouldContain("<Setter Property=\"IsMotionEnabled\" Value=\"True\" />");

        var multipleBeamsDemo = ExtractShowCaseItemMarkup(source, "BorderBeamShowCaseLangResource MultipleBeamsTitle");
        multipleBeamsDemo.ShouldContain("SourceKey=\"border-beam-multiple-beams\"");
        Regex.Matches(multipleBeamsDemo, @"<atom:BorderBeam\s").Count.ShouldBe(2);
        Regex.Matches(multipleBeamsDemo, "Count=\"3\"").Count.ShouldBe(1);
        Regex.Matches(multipleBeamsDemo, "Count=\"2\"").Count.ShouldBe(1);
        multipleBeamsDemo.ShouldContain("BorderBeamShowCaseLangResource MultipleBeamsCardTitle");
        multipleBeamsDemo.ShouldContain("BorderBeamShowCaseLangResource MultipleBeamsCardDescription");

        var customContainerDemo = ExtractShowCaseItemMarkup(source, "BorderBeamShowCaseLangResource CustomContainerTitle");
        customContainerDemo.ShouldContain("SourceKey=\"border-beam-custom-container\"");
        customContainerDemo.ShouldContain("<atom:BorderBeam Width=\"420\"");
        customContainerDemo.ShouldContain("BorderThickness=\"1\"");
        customContainerDemo.ShouldContain("CornerRadius=\"8\"");
        customContainerDemo.ShouldContain("<Border MinHeight=\"160\"");
        customContainerDemo.ShouldContain("BorderBeamShowCaseLangResource CustomContainerContent");

        var customizedColorDemo = ExtractShowCaseItemMarkup(source, "BorderBeamShowCaseLangResource CustomizedColorTitle");
        customizedColorDemo.ShouldContain("SourceKey=\"border-beam-customized-color\"");
        customizedColorDemo.ShouldContain("<atom:Segmented");
        customizedColorDemo.ShouldContain("MaxWidth=\"480\"");
        customizedColorDemo.ShouldContain("ItemsSource=\"{Binding ColorPresets}\"");
        customizedColorDemo.ShouldContain("SelectedItem=\"{Binding SelectedColorPreset}\"");
        customizedColorDemo.ShouldContain("HorizontalAlignment=\"Stretch\"");
        customizedColorDemo.ShouldContain("ColorStops=\"{Binding SelectedColorStops}\"");
        customizedColorDemo.ShouldContain("<atom:Card.Extra>");
        customizedColorDemo.ShouldContain("Text=\"{Binding SelectedColorPreset.Usage}\"");
        customizedColorDemo.ShouldContain("Text=\"{Binding SelectedColorPreset.Description}\"");
        customizedColorDemo.ShouldContain("ItemsSource=\"{Binding SelectedColorPreset.Stops}\"");
        customizedColorDemo.ShouldContain("TagColor=\"{Binding Color}\"");
        customizedColorDemo.ShouldContain("Text=\"{Binding Label}\"");
        customizedColorDemo.ShouldContain("Variant=\"Filled\"");
        customizedColorDemo.ShouldContain("BorderBeamShowCaseLangResource CustomizedColorCardDescription");
        customizedColorDemo.ShouldContain("TextWrapping=\"Wrap\"");

        var nonUniformRadiusDemo = ExtractShowCaseItemMarkup(source, "BorderBeamShowCaseLangResource NonUniformRadiusTitle");
        nonUniformRadiusDemo.ShouldContain("SourceKey=\"border-beam-non-uniform-radius\"");
        nonUniformRadiusDemo.ShouldContain("Outset=\"0\"");
        nonUniformRadiusDemo.ShouldContain("CornerRadius=\"20,20,0,0\"");
        nonUniformRadiusDemo.ShouldContain("ClipToBounds=\"True\"");
        nonUniformRadiusDemo.ShouldContain("Non-uniform radius");

        var durationDemo = ExtractShowCaseItemMarkup(source, "BorderBeamShowCaseLangResource DurationTitle");
        durationDemo.ShouldContain("SourceKey=\"border-beam-duration\"");
        durationDemo.ShouldContain("<WrapPanel");
        durationDemo.ShouldContain("MaxWidth=\"480\"");
        Regex.Matches(durationDemo, @"<atom:BorderBeam\s").Count.ShouldBe(3);
        Regex.Matches(durationDemo, "Width=\"220\"").Count.ShouldBe(3);
        durationDemo.ShouldContain("Duration=\"0:0:3\"");
        durationDemo.ShouldContain("Duration=\"0:0:6\"");
        durationDemo.ShouldContain("Duration=\"0:0:12\"");
        durationDemo.ShouldContain("Text=\"3s\"");
        durationDemo.ShouldContain("Text=\"6s\"");
        durationDemo.ShouldContain("Text=\"12s\"");
        durationDemo.ShouldContain("BorderBeamShowCaseLangResource DurationFastCardDescription");
        durationDemo.ShouldContain("BorderBeamShowCaseLangResource DurationDefaultCardDescription");
        durationDemo.ShouldContain("BorderBeamShowCaseLangResource DurationSlowCardDescription");

        var sizeDemo = ExtractShowCaseItemMarkup(source, "BorderBeamShowCaseLangResource SizeTitle");
        sizeDemo.ShouldContain("SourceKey=\"border-beam-size\"");
        sizeDemo.ShouldContain("MaxWidth=\"960\"");
        sizeDemo.ShouldContain("ColumnDefinitions=\"*,*\"");
        Regex.Matches(sizeDemo, @"<atom:BorderBeam\s").Count.ShouldBe(3);
        sizeDemo.ShouldContain("BeamSize=\"56\"");
        sizeDemo.ShouldContain("BeamSize=\"160\"");
        sizeDemo.ShouldContain("Grid.ColumnSpan=\"2\"");
        sizeDemo.ShouldContain("MinHeight=\"112\"");
        sizeDemo.ShouldContain("MinHeight=\"192\"");
        sizeDemo.ShouldContain("Text=\"100px\"");
        sizeDemo.ShouldContain("Text=\"56px\"");
        sizeDemo.ShouldContain("Text=\"160px\"");
        sizeDemo.ShouldContain("BorderBeamShowCaseLangResource SizeDefaultCardDescription");
        sizeDemo.ShouldContain("BorderBeamShowCaseLangResource SizeCompactCardDescription");
        sizeDemo.ShouldContain("BorderBeamShowCaseLangResource SizeExtendedCardDescription");

        var lineWidthDemo = ExtractShowCaseItemMarkup(source, "BorderBeamShowCaseLangResource LineWidthTitle");
        lineWidthDemo.ShouldContain("SourceKey=\"border-beam-line-width\"");
        lineWidthDemo.ShouldContain("<atom:BorderBeam Width=\"360\"");
        Regex.Matches(lineWidthDemo, "BorderThickness=\"2\"").Count.ShouldBe(2);
        lineWidthDemo.ShouldContain("BorderBeamShowCaseLangResource LineWidthCardTitle");
        lineWidthDemo.ShouldContain("BorderBeamShowCaseLangResource LineWidthCardDescription");
    }

    [Fact]
    public void BorderBeam_ShowCase_ViewModel_Provides_Ant_Design_Gradient_Presets()
    {
        var viewModel = new BorderBeamViewModel(null!);

        viewModel.ColorPresets.Select(preset => preset.Name).ShouldBe(new[]
        {
            "Ocean",
            "Sunset",
            "Aurora",
            "Forest",
            "Ember",
            "Nebula"
        });
        viewModel.SelectedColorPreset.ShouldBe(viewModel.ColorPresets[0]);

        viewModel.ColorPresets.Select(preset => preset.Usage).ShouldBe(new[]
        {
            "Dashboard", "Upgrade", "AI", "Recommendation", "Alert", "Labs"
        });
        viewModel.ColorPresets.Select(preset => preset.Description).ShouldBe(new[]
        {
            "A calm blue-green accent that works well for data views and cloud tooling.",
            "A warm highlight for upgrade prompts, featured cards, and marketing blocks.",
            "A vivid cool-toned beam suited for AI assistants, copilots, and automation panels.",
            "A bright natural palette that feels good on recommendation and growth-oriented cards.",
            "A high-energy warm gradient for important alerts, launch cards, and hot paths.",
            "A cool purple-pink mix that fits experimental modules and product lab surfaces."
        });

        var expectedStops = new[]
        {
            new[] { "#1677ff|0|#1677ff · 0%", "#36cfc9|52|#36cfc9 · 52%", "#95de64|100|#95de64 · 100%" },
            new[] { "#ff7a45|0|#ff7a45 · 0%", "#ff4d4f|49|#ff4d4f · 49%", "#ff85c0|100|#ff85c0 · 100%" },
            new[] { "#7c3aed|0|#7c3aed · 0%", "#06b6d4|57|#06b6d4 · 57%", "#67e8f9|100|#67e8f9 · 100%" },
            new[] { "#22c55e|0|#22c55e · 0%", "#a3e635|54|#a3e635 · 54%", "#facc15|100|#facc15 · 100%" },
            new[] { "#fa541c|0|#fa541c · 0%", "#ff7875|46|#ff7875 · 46%", "#ffd666|100|#ffd666 · 100%" },
            new[] { "#2f54eb|0|#2f54eb · 0%", "#722ed1|44|#722ed1 · 44%", "#ff85c0|100|#ff85c0 · 100%" }
        };

        for (var presetIndex = 0; presetIndex < viewModel.ColorPresets.Count; presetIndex++)
        {
            var stops = viewModel.ColorPresets[presetIndex].Stops
                .Select(stop => $"{stop.Color}|{stop.Percent}|{stop.Label}")
                .ToArray();
            stops.ShouldBe(expectedStops[presetIndex]);

            viewModel.SelectedColorPreset = viewModel.ColorPresets[presetIndex];
            viewModel.SelectedColorStops.Select(stop => (stop.Color, stop.Percent)).ShouldBe(
                expectedStops[presetIndex].Select(expected =>
                {
                    var parts = expected.Split('|');
                    return (Color.Parse(parts[0]), double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture));
                }).ToArray());
        }
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

    private static string ExtractHeaderTitleMarkup(string source)
    {
        const string headerStartMarker = "Text=\"BorderBeam\"";
        const string headerEndMarker = "BorderBeamShowCaseLangResource PageSubtitle";

        var titleIndex = source.IndexOf(headerStartMarker, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);
        var headerStart = source.LastIndexOf("<Grid", titleIndex, StringComparison.Ordinal);
        headerStart.ShouldBeGreaterThanOrEqualTo(0);
        var headerEnd = source.IndexOf(headerEndMarker, headerStart, StringComparison.Ordinal);
        headerEnd.ShouldBeGreaterThan(headerStart);

        return source[headerStart..headerEnd];
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
