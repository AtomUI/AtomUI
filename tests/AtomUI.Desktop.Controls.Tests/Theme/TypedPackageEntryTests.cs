using Avalonia;
using Avalonia.Headless.XUnit;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class TypedPackageEntryTests
{
    [AvaloniaTheory]
    [InlineData("DataGrid", true)]
    [InlineData("ColorPicker", true)]
    [InlineData("Extras", false)]
    public void Optional_entry_stages_only_its_package_and_rejects_duplicate_before_completion(string name, bool hasLanguage)
    {
        var builder = new AtomUIBuilder(new Application());
        Register(builder, name);
        var package = builder.ThemeManagerBuilder.ControlPackages.ShouldHaveSingleItem();
        package.Id.ShouldBe("AtomUI.Desktop.Controls." + name);
        package.ControlThemesProvider.Id.ShouldBe(package.Id);
        var catalogs = builder.LocalizationBuilder.Catalogs.Count;
        (catalogs > 0).ShouldBe(hasLanguage);
        builder.ThemeManagerBuilder.Initializers.Count.ShouldBe(0);
        Should.Throw<InvalidOperationException>(() => Register(builder, name));
        builder.LocalizationBuilder.Catalogs.Count.ShouldBe(catalogs);
        builder.ThemeManagerBuilder.ControlPackages.ShouldHaveSingleItem().ShouldBeSameAs(package);
    }

    [AvaloniaFact]
    public void Common_dependency_runs_services_and_localization_once_before_desktop_commit()
    {
        var builder = new AtomUIBuilder(new Application());
        AtomUI.Controls.ThemeManagerBuilderExtensions.UseCommonControls(builder);
        var services = builder.OwnedServiceRegistrations.Count;
        var catalogs = builder.LocalizationBuilder.Catalogs.Count;
        services.ShouldBeGreaterThan(0);
        catalogs.ShouldBeGreaterThan(0);
        AtomUI.Controls.ThemeManagerBuilderExtensions.UseCommonControls(builder);
        builder.OwnedServiceRegistrations.Count.ShouldBe(services);
        builder.LocalizationBuilder.Catalogs.Count.ShouldBe(catalogs);
        builder.UseDesktopControls();
        builder.ThemeManagerBuilder.ControlPackages.Select(p => p.Id).ShouldBe(new[] { "AtomUI.Controls.Common", "AtomUI.Desktop.Controls" });
        builder.OwnedServiceRegistrations.Count.ShouldBe(services);
        builder.ThemeManagerBuilder.Initializers.Count.ShouldBe(1);
        var desktopCatalogs = builder.LocalizationBuilder.Catalogs.Count;
        Should.Throw<InvalidOperationException>(() => builder.UseDesktopControls());
        builder.LocalizationBuilder.Catalogs.Count.ShouldBe(desktopCatalogs);
        builder.ThemeManagerBuilder.Initializers.Count.ShouldBe(1);
    }

    [AvaloniaFact]
    public void Generated_descriptors_are_reused_without_sharing_application_state()
    {
        var firstBuilder = new AtomUIBuilder(new Application());
        var secondBuilder = new AtomUIBuilder(new Application());
        firstBuilder.UseDesktopControls();
        secondBuilder.UseDesktopControls();
        var stageOneAssets = firstBuilder.ThemeManagerBuilder.ControlPackages
            .Single(package => package.Id == "AtomUI.Desktop.Controls")
            .ThemeAssets
            .Select(asset => asset.AssetUri.AbsolutePath)
            .ToArray();
        foreach (var chromeAsset in new[]
                 {
                     "/Window/Themes/WindowDrawnDecorationsTheme.axaml",
                     "/WindowTitleBar/Themes/WindowTitleBarTheme.axaml",
                     "/Menu/Themes/MenuTheme.axaml",
                     "/Menu/Themes/MenuItemTheme.axaml"
                 })
        {
            stageOneAssets.ShouldContain(chromeAsset);
        }
        // 分区包：余量段须先收集，才能看到包的完整贡献（关键集 + 余量）。
        firstBuilder.ThemeManagerBuilder.RunDeferredCollects();
        secondBuilder.ThemeManagerBuilder.RunDeferredCollects();

        var firstContributions = firstBuilder.ThemeManagerBuilder
            .EnumerateContributions()
            .Where(package => package.Id == "AtomUI.Desktop.Controls")
            .ToArray();
        var secondContributions = secondBuilder.ThemeManagerBuilder
            .EnumerateContributions()
            .Where(package => package.Id == "AtomUI.Desktop.Controls")
            .ToArray();
        var firstPackage = firstContributions[0];
        var secondPackage = secondContributions[0];

        firstPackage.ShouldNotBeSameAs(secondPackage);
        firstPackage.ControlThemesProvider.ShouldNotBeSameAs(secondPackage.ControlThemesProvider);
        var assetPaths = firstContributions
            .SelectMany(package => package.ThemeAssets)
            .Select(asset => asset.AssetUri.AbsolutePath)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var leafAsset in new[]
                 {
                     "/Drawer/Themes/DrawerTheme.axaml",
                     "/Primitives/ArrowDecoratedBox/Themes/DualMonthArrowDecoratedBoxTheme.axaml",
                     "/ImagePreviewer/Themes/ImagePreviewerOverlayHostTheme.axaml",
                     "/Statistic/Themes/AbstractStatisticDefaultTheme.axaml"
                 })
        {
            assetPaths.ShouldContain(leafAsset);
        }
        AssertSharedDescriptors(
            firstContributions.SelectMany(package => package.Controls).ToArray(),
            secondContributions.SelectMany(package => package.Controls).ToArray());
        AssertSharedDescriptors(
            firstContributions.SelectMany(package => package.SemanticControls).ToArray(),
            secondContributions.SelectMany(package => package.SemanticControls).ToArray());
        AssertSharedDescriptors(
            firstContributions.SelectMany(package => package.ThemeAssets).ToArray(),
            secondContributions.SelectMany(package => package.ThemeAssets).ToArray());
        AssertSharedDescriptors(
            firstContributions.SelectMany(package => package.Resources).ToArray(),
            secondContributions.SelectMany(package => package.Resources).ToArray());
        firstPackage.Resources[0].Factory().ShouldNotBeSameAs(secondPackage.Resources[0].Factory());
        using var firstManager = firstBuilder.ThemeManagerBuilder.Build();
        using var secondManager = secondBuilder.ThemeManagerBuilder.Build();
        firstManager.ShouldNotBeSameAs(secondManager);
        firstManager.SemanticParts.ShouldNotBeSameAs(secondManager.SemanticParts);
    }

    [AvaloniaFact]
    public void All_optional_packages_initialize_and_freeze_with_desktop_enabled_explicitly()
    {
        var app = new Application();
        IAtomUIBuilder? saved = null;
        app.UseAtomUI(builder =>
        {
            saved = builder;
            builder.UseDesktopControls().UseDesktopDataGrid().UseDesktopColorPicker().UseDesktopExtras();
        });
        app.GetThemeManager().ShouldNotBeNull().CurrentTheme.ShouldNotBeNull();
        Should.Throw<InvalidOperationException>(() => saved!.UseDesktopDataGrid()).Message.ShouldContain("frozen");
    }

    private static void Register(IAtomUIBuilder builder, string name)
    {
        switch (name)
        {
            case "DataGrid": builder.UseDesktopDataGrid(); break;
            case "ColorPicker": builder.UseDesktopColorPicker(); break;
            case "Extras": builder.UseDesktopExtras(); break;
            default: throw new ArgumentException(name);
        }
    }

    private static void AssertSharedDescriptors<T>(IReadOnlyList<T> first, IReadOnlyList<T> second)
        where T : class
    {
        first.ShouldNotBeEmpty();
        first.Count.ShouldBe(second.Count);
        for (var index = 0; index < first.Count; index++)
        {
            first[index].ShouldBeSameAs(second[index], $"generated descriptor {index} must be reused");
        }
    }
}
