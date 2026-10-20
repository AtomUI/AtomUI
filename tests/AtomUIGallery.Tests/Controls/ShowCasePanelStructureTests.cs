using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomRibbonBadge = AtomUI.Desktop.Controls.RibbonBadge;
using AtomSeparator = AtomUI.Desktop.Controls.Separator;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.Controls;

public class ShowCasePanelStructureTests
{
    [Fact]
    public void ShowCasePanel_Uses_Masonry_Panel_Instead_Of_Manual_Grid_Placement()
    {
        var source = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/ShowCasePanel.axaml.cs");
        var theme  = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/Themes/ShowCasePanelTheme.axaml");

        theme.ShouldContain("gallery:ShowCaseMasonryPanel");
        theme.ShouldNotContain("<Grid Margin=\"5\" Name=\"PART_MainPanel\"");
        source.ShouldNotContain("new ShowCaseItem()");
        source.ShouldNotContain("IsFake = true");
        source.ShouldNotContain("Grid.SetRow");
        source.ShouldNotContain("Grid.SetColumn");
        source.ShouldNotContain("Grid.SetColumnSpan");
        source.ShouldNotContain("LogicalChildren.Add");
    }

    [Fact]
    public void ShowCasePanel_And_Item_Use_Gallery_Tokens_For_Layout_And_Cards()
    {
        var panelTheme = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/Themes/ShowCasePanelTheme.axaml");
        var itemTheme  = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/Themes/ShowCaseItemTheme.axaml");
        var panelToken = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/ShowCasePanelToken.cs");
        var itemToken  = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/ShowCaseItemToken.cs");

        panelToken.ShouldContain("[ControlDesignToken]");
        itemToken.ShouldContain("[ControlDesignToken]");
        panelToken.ShouldNotContain("ScopeProvider");
        itemToken.ShouldNotContain("ScopeProvider");
        panelTheme.ShouldContain("ShowCasePanelTokenResource");
        panelTheme.ShouldContain("ContentMargin\" Value=\"{gallery:ShowCasePanelTokenResource ContentMargin}");
        panelTheme.ShouldContain("Margin=\"{TemplateBinding ContentMargin}\"");
        panelTheme.ShouldContain("VerticalScrollBarVisibility=\"Auto\"");
        panelTheme.ShouldContain("Selector=\"^[IsScrollEnabled=False]\"");
        itemTheme.ShouldContain("ShowCaseItemTokenResource");
        itemTheme.ShouldContain("{atom:SharedTokenResource ");
        itemTheme.ShouldNotContain("ControlTokenScope.Identity");
        itemTheme.ShouldNotContain("TokenSharedTokenResource");
        itemTheme.ShouldContain("ShowCaseItemTokenResource BadgePreviewMargin");
        var panelSource = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/ShowCasePanel.axaml.cs");
        panelSource.ShouldContain("ContentMarginProperty");
        panelSource.ShouldContain("IsScrollEnabledProperty");
        panelSource.ShouldContain("public bool IsScrollEnabled");
        panelTheme.ShouldNotContain("Margin=\"5\"");
        itemTheme.ShouldNotContain("Padding=\"20\"");
        itemTheme.ShouldNotContain("CornerRadius=\"8\"");
        itemTheme.ShouldNotContain("Margin=\"0, 0, 0, 40\"");
        itemToken.ShouldContain("BadgePreviewMargin");
    }

    [Fact]
    public void GalleryShowCaseHeader_Uses_Gallery_Token_Theme_And_Localization_Conventions()
    {
        var headerSource      = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/GalleryShowCaseHeader.cs");
        var headerTheme       = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/Themes/GalleryShowCaseHeaderTheme.axaml");
        var headerToken       = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/GalleryShowCaseHeaderToken.cs");
        var assetManifest     = ReadRepoFile(
            "src/AtomUI.Toolkits.GalleryBase/GeneratedFiles/AtomUI.Generator/AtomUI.Generator.TokenResourceKeyGenerator/GeneratedTypeMapRegistration.g.cs");
        var assemblyInfo      = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Properties/AssemblyInfo.cs");
        var tokenResources    = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/GeneratedFiles/AtomUI.Generator/AtomUI.Generator.TokenResourceKeyGenerator/TokenResourceConst.g.cs");
        var languageResources = ReadRepoFile(
            "src/AtomUI.Toolkits.GalleryBase/Localization/GalleryShowCaseHeaderLang/GalleryShowCaseHeaderLangResourceKind.cs");
        var languageModuleRegistration = ReadRepoFile(
            "src/AtomUI.Toolkits.GalleryBase/GeneratedFiles/AtomUI.Generator/AtomUI.Generator.LocalizationGenerator/GeneratedLanguageModuleRegistration.g.cs");

        headerSource.ShouldContain("public const string LanguageId = \"GalleryShowCaseHeader\"");
        headerSource.ShouldNotContain("RegisterTokenResourceScope");
        headerSource.ShouldContain("CategoryTagColorProperty");
        headerSource.ShouldContain("StatusTagColorProperty");
        headerSource.ShouldContain("IntroducedVersionProperty");
        headerSource.ShouldNotContain("IsIntroducedVersionTagBorderedProperty");
        headerSource.ShouldContain("MetadataLabelWidthProperty");
        headerSource.ShouldContain("MetadataValueWidthProperty");
        headerSource.ShouldContain("MetadataLabelWidthProperty,\n            MetadataValueWidthProperty");
        headerSource.ShouldContain("IsMetadataVisibleProperty");

        headerToken.ShouldContain("[ControlDesignToken]");
        headerToken.ShouldNotContain("public const string ID");
        headerToken.ShouldNotContain("ScopeProvider");
        headerToken.ShouldContain("MetadataLabelWidth");
        headerToken.ShouldContain("MetadataValueWidth");

        headerTheme.ShouldContain("GalleryShowCaseHeaderTokenResource");
        headerTheme.ShouldContain("{atom:SharedTokenResource ");
        headerTheme.ShouldNotContain("ControlTokenScope.Identity");
        headerTheme.ShouldNotContain("TokenSharedTokenResource");
        headerTheme.ShouldContain("GalleryShowCaseHeaderLangResource");
        headerTheme.ShouldContain("PART_IntroducedVersionTag");
        headerTheme.ShouldContain("VerticalAlignment=\"Center\"");
        headerTheme.ShouldContain("Text=\"{gallery:GalleryShowCaseHeaderLangResource NamespaceLabel}\"");
        headerTheme.ShouldContain("Text=\"{gallery:GalleryShowCaseHeaderLangResource PackageLabel}\"");
        headerTheme.ShouldContain("Text=\"{gallery:GalleryShowCaseHeaderLangResource BaseClassLabel}\"");

        assetManifest.ShouldContain("Controls/Themes/GalleryShowCaseHeaderTheme.axaml");
        assemblyInfo.ShouldContain("AtomUI.Toolkits.GalleryBase.Localization");
        tokenResources.ShouldContain("enum GalleryShowCaseHeaderTokenKind");
        tokenResources.ShouldContain("ControlTokenIdentity.ForControl(typeof(global::AtomUI.Toolkits.GalleryBase.Controls.GalleryShowCaseHeader), \"AtomUI\", \"GalleryShowCaseHeader\")");
        tokenResources.ShouldContain("GalleryShowCaseHeaderTokenResourceExtension");
        languageResources.ShouldContain("enum GalleryShowCaseHeaderLangResourceKind");
        languageResources.ShouldContain("[LanguageCatalog]");
        languageResources.ShouldContain("NamespaceLabel");
        languageResources.ShouldContain("PackageLabel");
        languageResources.ShouldContain("BaseClassLabel");
        languageModuleRegistration.ShouldContain("LanguageModuleManifestAttribute(1, \"AtomUI.Toolkits.GalleryBase\"");
        languageModuleRegistration.ShouldContain("builder.AddModuleData(\"AtomUI.Toolkits.GalleryBase\", Data)");
        languageModuleRegistration.ShouldContain("GalleryShowCaseHeaderLangResourceKind");
        languageModuleRegistration.ShouldContain("en-US");
        languageModuleRegistration.ShouldContain("zh-CN");
        languageModuleRegistration.ShouldContain("zh-TW");
    }

    [Fact]
    public void Standard_ShowCases_Use_Shared_Header_Instead_Of_Hand_Written_Header_Layout()
    {
        var showCasesRoot = GetRepoFile("controlgallery/AtomUIGallery/ShowCases");
        var allShowCaseFiles = Directory.GetFiles(showCasesRoot, "*ShowCase.axaml", SearchOption.AllDirectories)
                                        .Where(IsMainShowCasePage)
                                        .OrderBy(path => path, StringComparer.Ordinal)
                                        .ToList();

        allShowCaseFiles.Count.ShouldBeGreaterThan(0);
        foreach (var showCasePath in allShowCaseFiles)
        {
            var source = ReadRepoFile(showCasePath);

            CountOccurrences(source, "<gallery:GalleryShowCaseHeader").ShouldBe(1);
            if (source.Contains("<gallery:GalleryStickyTabsHost", StringComparison.Ordinal))
            {
                source.ShouldContain("<gallery:GalleryStickyTabsHost.Header>");
            }

            source.ShouldNotContain("InfoNamespaceLabel");
            source.ShouldNotContain("InfoPackageLabel");
            source.ShouldNotContain("InfoBaseClassLabel");
            source.ShouldNotContain("<Border Background=\"{atom:SharedTokenResource ColorBgContainer}\"");
        }
    }

    private static bool IsMainShowCasePage(string path)
    {
        var fileName     = Path.GetFileNameWithoutExtension(path);
        var controlName  = Directory.GetParent(path)?.Parent?.Name;
        return string.Equals(fileName, $"{controlName}ShowCase", StringComparison.Ordinal);
    }

    [Fact]
    public void ShowCaseItem_Integrates_RibbonBadge_For_Feature_Version_Marker()
    {
        var itemSource = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/ShowCaseItem.axaml.cs");
        var itemTheme  = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/Themes/ShowCaseItemTheme.axaml");

        itemSource.ShouldContain("BadgeTextProperty");
        itemSource.ShouldContain("BadgeColorProperty");
        itemSource.ShouldContain("IsBadgeVisibleProperty");
        itemTheme.ShouldContain("Selector=\"^[IsBadgeVisible=True]\"");
        itemTheme.ShouldContain("<atom:RibbonBadge");
        itemTheme.ShouldContain("Text=\"{TemplateBinding BadgeText}\"");
        itemTheme.ShouldContain("RibbonColor=\"{TemplateBinding BadgeColor}\"");
        itemTheme.ShouldContain("<Setter Property=\"ClipToBounds\" Value=\"False\" />");
        itemTheme.ShouldNotContain("Offset=\"-8,0\"");
        itemTheme.ShouldNotContain("RibbonBadgeText");
        itemTheme.ShouldNotContain("FeatureBadgeText");
        itemTheme.ShouldNotContain("PART_FeatureBadge");
        itemTheme.ShouldNotContain("HorizontalAlignment=\"Right\"");
        itemTheme.ShouldNotContain("VerticalAlignment=\"Top\"");
        itemTheme.ShouldNotContain("IsHitTestVisible=\"False\"");
    }

    [Fact]
    public void ShowCaseItem_Badge_Template_Keeps_Card_Content()
    {
        AvaloniaTestApp.EnsureInitialized();

        var item = new ShowCaseItem
        {
            Title       = "Feature",
            Description = "Feature item",
            BadgeText   = "v6.0.5",
            Content     = new TextBlock
            {
                Text = "content"
            }
        };

        ShowInWindow(item, () =>
        {
            var ribbonBadge = item.GetVisualDescendants()
                                  .OfType<AtomRibbonBadge>()
                                  .SingleOrDefault();
            ribbonBadge.ShouldNotBeNull();
            ribbonBadge.Text.ShouldBe("v6.0.5");
            ribbonBadge.Offset.ShouldBe(default);

            item.GetVisualDescendants()
                .OfType<AtomSeparator>()
                .Single()
                .Title
                .ShouldBe("Feature");
            item.GetVisualDescendants()
                .OfType<TextBlock>()
                .Any(textBlock => textBlock.Text == "Feature item")
                .ShouldBeTrue();
            item.GetVisualDescendants()
                .OfType<TextBlock>()
                .Any(textBlock => textBlock.Text == "content")
                .ShouldBeTrue();
        });
    }

    [Fact]
    public void ShowCaseItem_Badge_Template_Anchors_Ribbon_To_Card_Right_Edge()
    {
        AvaloniaTestApp.EnsureInitialized();

        var item = new ShowCaseItem
        {
            Title       = "Feature",
            Description = "Feature item",
            BadgeText   = "v6.0.6",
            Content     = new TextBlock { Text = "content" }
        };
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = item
        };

        ShowInWindow(visualLayerManager, () =>
        {
            var ribbonBadge = item.GetVisualDescendants()
                                  .OfType<AtomRibbonBadge>()
                                  .Single();
            var label = visualLayerManager.GetVisualDescendants()
                                          .OfType<TextBlock>()
                                          .Single(textBlock => textBlock.Text == "v6.0.6");

            item.ClipToBounds.ShouldBeFalse(
                "the version ribbon intentionally overhangs the ShowCaseItem right edge and must not be clipped by the item host.");
            var labelRight = label.TranslatePoint(new Point(label.Bounds.Width, 0), visualLayerManager);
            var targetRight = ribbonBadge.TranslatePoint(new Point(ribbonBadge.Bounds.Width, 0), visualLayerManager);

            labelRight.ShouldNotBeNull();
            targetRight.ShouldNotBeNull();
            labelRight.Value.X.ShouldBeGreaterThan(targetRight.Value.X,
                "ShowCaseItem version RibbonBadge should overhang the card's final right edge instead of stopping inside it.");
        });
    }

    [Fact]
    public void ShowCaseItem_Badge_Template_Reserves_Top_Space_For_Ribbon_Label()
    {
        AvaloniaTestApp.EnsureInitialized();

        var previewContent = new Border
        {
            Width  = 520,
            Height = 40
        };
        var item = new ShowCaseItem
        {
            Title       = "Feature",
            Description = "Feature item",
            BadgeText   = "v6.0.8",
            Content     = previewContent
        };
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = item
        };

        ShowInWindow(visualLayerManager, () =>
        {
            var label = visualLayerManager.GetVisualDescendants()
                                          .OfType<TextBlock>()
                                          .Single(textBlock => textBlock.Text == "v6.0.8");

            var labelBottom = label.TranslatePoint(new Point(0, label.Bounds.Height), visualLayerManager);
            var contentTop  = previewContent.TranslatePoint(new Point(0, 0), visualLayerManager);

            labelBottom.ShouldNotBeNull();
            contentTop.ShouldNotBeNull();
            contentTop.Value.Y.ShouldBeGreaterThanOrEqualTo(labelBottom.Value.Y,
                "ShowCaseItem version badge must not overlap the top preview content.");
        });
    }

    [Fact]
    public void RibbonBadge_In_ShowCasePanel_Anchors_To_Decorated_Target_Right_Edge()
    {
        AvaloniaTestApp.EnsureInitialized();

        var target = new Border
        {
            Padding         = new Thickness(10, 0),
            BorderThickness = new Thickness(1),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = "Pushes open the window" },
                    new TextBlock { Text = "and raises the spyglass." }
                }
            }
        };
        var ribbonBadge = new AtomRibbonBadge
        {
            Text            = "Hippies",
            RibbonColor     = "purple",
            DecoratedTarget = target
        };
        var item = new ShowCaseItem
        {
            Title       = "Ribbon",
            Description = "Ribbon item",
            Content = new StackPanel
            {
                Margin   = new Thickness(20, 0),
                Children = { ribbonBadge }
            }
        };
        var panel = new ShowCasePanel
        {
            IsScrollEnabled = false,
            ContentMargin   = new Thickness(28),
            Children        = { item }
        };
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = panel
        };

        ShowInWindow(visualLayerManager, () =>
        {
            var label = visualLayerManager.GetVisualDescendants()
                                          .OfType<TextBlock>()
                                          .Single(textBlock => textBlock.Text == "Hippies");

            var labelRight = label.TranslatePoint(new Point(label.Bounds.Width, 0), visualLayerManager);
            var targetRight = target.TranslatePoint(new Point(target.Bounds.Width, 0), visualLayerManager);

            labelRight.ShouldNotBeNull();
            targetRight.ShouldNotBeNull();
            labelRight.Value.X.ShouldBeGreaterThan(targetRight.Value.X,
                "RibbonBadge should overhang the target's final right edge in ShowCasePanel, matching the ShowCaseItem badge behavior.");
        });
    }

    [Fact]
    public void ShowCaseItem_Deferred_Content_Template_Builds_Only_When_Materialized()
    {
        AvaloniaTestApp.EnsureInitialized();
        var oldValue = SetDeferredLoadingEnvironment(null);
        GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        try
        {
            var buildCount = 0;
            var item = new ShowCaseItem
            {
                IsDeferredContentEnabled = true,
                DataContext              = "demo data",
                DeferredContentTemplate  = new FuncDataTemplate<object?>((data, _) =>
                {
                    buildCount++;
                    return new TextBlock
                    {
                        Text = data?.ToString()
                    };
                }, true)
            };

            buildCount.ShouldBe(0);
            item.IsDeferredContentMaterialized.ShouldBeFalse();

            item.MaterializeDeferredContent();

            buildCount.ShouldBe(1);
            item.IsDeferredContentMaterialized.ShouldBeTrue();
            item.Content.ShouldBeOfType<TextBlock>().Text.ShouldBe("demo data");

            item.MaterializeDeferredContent();
            buildCount.ShouldBe(1);
        }
        finally
        {
            SetDeferredLoadingEnvironment(oldValue);
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        }
    }

    [Fact]
    public void ShowCaseItem_Materializes_Deferred_Template_Immediately_When_Diagnostics_Disables_Defer()
    {
        AvaloniaTestApp.EnsureInitialized();
        var oldValue = SetDeferredLoadingEnvironment(null);
        GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        try
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = true;

            var buildCount = 0;
            var item = new ShowCaseItem
            {
                IsDeferredContentEnabled = true,
                DataContext              = "diagnostic data",
                DeferredContentTemplate  = new FuncDataTemplate<object?>((data, _) =>
                {
                    buildCount++;
                    return new TextBlock
                    {
                        Text = data?.ToString()
                    };
                }, true)
            };

            buildCount.ShouldBe(1);
            item.IsDeferredContentMaterialized.ShouldBeTrue();
            item.Content.ShouldBeOfType<TextBlock>().Text.ShouldBe("diagnostic data");
        }
        finally
        {
            SetDeferredLoadingEnvironment(oldValue);
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        }
    }

    [Fact]
    public void ShowCase_Runtime_Options_Do_Not_Let_Runtime_Stop_Override_Environment_Disable()
    {
        var oldValue = SetDeferredLoadingEnvironment("1");
        GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        try
        {
            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeTrue();

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled = false;

            GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled.ShouldBeTrue();
        }
        finally
        {
            SetDeferredLoadingEnvironment(oldValue);
            GalleryShowCaseRuntimeOptions.ResetDeferredLoadingDisabledOverride();
        }
    }

    [Fact]
    public void ShowCasePanel_Supports_Opt_In_Viewport_Driven_Deferred_Loading()
    {
        var panelSource = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/ShowCasePanel.axaml.cs");
        var itemSource  = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/ShowCaseItem.axaml.cs");
        var itemTheme   = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/Themes/ShowCaseItemTheme.axaml");
        var itemToken   = ReadRepoFile("src/AtomUI.Toolkits.GalleryBase/Controls/ShowCaseItemToken.cs");

        panelSource.ShouldContain("IsDeferredLoadingEnabledProperty");
        panelSource.ShouldContain("InitialDeferredLoadItemCountProperty");
        panelSource.ShouldContain("DeferredLoadBatchSizeProperty");
        panelSource.ShouldContain("DeferredLoadViewportBufferProperty");
        panelSource.ShouldContain("EffectiveViewportChanged += HandleEffectiveViewportChanged");
        panelSource.ShouldContain("EffectiveViewportChanged -= HandleEffectiveViewportChanged");
        CountOccurrences(panelSource, "EffectiveViewportChanged +=").ShouldBe(1);
        panelSource.ShouldContain("MaterializeDeferredContentInViewport");
        panelSource.ShouldContain("MaterializeAllDeferredContent");
        panelSource.ShouldContain("GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled");
        panelSource.ShouldContain("!GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled");
        panelSource.ShouldContain("GalleryShowCaseRuntimeOptions.DeferredLoadingDisabledChanged += HandleDeferredLoadingDisabledChanged");
        panelSource.ShouldContain("GalleryShowCaseRuntimeOptions.DeferredLoadingDisabledChanged -= HandleDeferredLoadingDisabledChanged");

        itemSource.ShouldContain("IsDeferredContentEnabledProperty");
        itemSource.ShouldContain("DeferredContentTemplateProperty");
        itemSource.ShouldContain("DeferredContentProperty");
        itemSource.ShouldContain("DeferredPlaceholderHeightProperty");
        itemSource.ShouldContain("public void MaterializeDeferredContent()");
        itemSource.ShouldContain("GalleryShowCaseRuntimeOptions.IsDeferredLoadingDisabled");
        itemTheme.ShouldContain("PART_DeferredPlaceholder");
        itemTheme.ShouldContain("DeferredPlaceholderHeight");
        itemTheme.ShouldContain("ShowCaseItemTokenResource DeferredPlaceholderHeight");
        itemToken.ShouldContain("DeferredPlaceholderHeight");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
    }

    private static string GetRepoFile(string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
        {
            return relativePath;
        }

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate) || Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }

    private static int CountOccurrences(string source, string value)
    {
        var count      = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
        }
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Content = content,
            Width   = 640,
            Height  = 480
        };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static string? SetDeferredLoadingEnvironment(string? value)
    {
        var oldValue = Environment.GetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable);
        Environment.SetEnvironmentVariable(
            GalleryShowCaseRuntimeOptions.DisableDeferredLoadingEnvironmentVariable,
            value);
        return oldValue;
    }
}
