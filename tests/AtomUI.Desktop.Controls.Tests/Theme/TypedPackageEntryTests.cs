using AtomUI.Registration;
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
    public void Optional_entry_declares_dependencies_and_rejects_duplicate_without_mutating_state(string name, bool hasLanguage)
    {
        var builder = new AtomUIBuilder(typeof(Application));
        Register(builder, name);
        var packageId = "AtomUI.Desktop.Controls." + name;
        builder.PackageDeclarations.Select(package => package.Definition.Id)
            .ShouldBe(["AtomUI.Controls.Common", "AtomUI.Desktop.Controls", packageId]);
        var package = builder.PackageDeclarations[^1];
        var catalogs = builder.LocalizationBuilder.Catalogs.Count;
        builder.LocalizationBuilder.ModuleData.ContainsKey(packageId).ShouldBe(hasLanguage);
        Should.Throw<InvalidOperationException>(() => Register(builder, name));
        builder.LocalizationBuilder.Catalogs.Count.ShouldBe(catalogs);
        builder.PackageDeclarations.Count.ShouldBe(3);
        builder.PackageDeclarations[^1].ShouldBeSameAs(package);
    }

    [AvaloniaFact]
    public void Common_dependency_runs_services_and_localization_once_before_desktop_commit()
    {
        var builder = new AtomUIBuilder(typeof(Application));
        var tooltip = AvaloniaLocator.Current.GetService<ToolTipService>();
        AtomUI.Controls.ThemeManagerBuilderExtensions.UseCommonControls(builder);
        var services = builder.OwnedServiceRegistrations.Count;
        var catalogs = builder.LocalizationBuilder.Catalogs.Count;
        services.ShouldBeGreaterThan(0);
        catalogs.ShouldBeGreaterThan(0);
        AtomUI.Controls.ThemeManagerBuilderExtensions.UseCommonControls(builder);
        builder.OwnedServiceRegistrations.Count.ShouldBe(services);
        builder.LocalizationBuilder.Catalogs.Count.ShouldBe(catalogs);
        builder.UseDesktopControls();
        builder.PackageDeclarations.Select(package => package.Definition.Id)
            .ShouldBe(["AtomUI.Controls.Common", "AtomUI.Desktop.Controls"]);
        builder.OwnedServiceRegistrations.Count.ShouldBe(services);
        AvaloniaLocator.Current.GetService<ToolTipService>().ShouldBeSameAs(tooltip);
        var desktopCatalogs = builder.LocalizationBuilder.Catalogs.Count;
        Should.Throw<InvalidOperationException>(() => builder.UseDesktopControls());
        builder.LocalizationBuilder.Catalogs.Count.ShouldBe(desktopCatalogs);
        builder.PackageDeclarations.Count.ShouldBe(2);
        AvaloniaLocator.Current.GetService<ToolTipService>().ShouldBeSameAs(tooltip);
    }

    [AvaloniaFact]
    public void Generated_descriptors_are_reused_without_sharing_application_state()
    {
        var firstBuilder = new AtomUIBuilder(typeof(Application));
        var secondBuilder = new AtomUIBuilder(typeof(Application));
        firstBuilder.UseDesktopControls();
        secondBuilder.UseDesktopControls();
        var firstPackage = firstBuilder.PackageDeclarations.Single(package => package.Definition.Id == "AtomUI.Desktop.Controls");
        var secondPackage = secondBuilder.PackageDeclarations.Single(package => package.Definition.Id == "AtomUI.Desktop.Controls");

        firstPackage.ShouldNotBeSameAs(secondPackage);
        firstPackage.Definition.ShouldBeSameAs(secondPackage.Definition);
        var firstIndex = StartupPackageIndex.Create(firstBuilder.PackageDeclarations);
        var secondIndex = StartupPackageIndex.Create(secondBuilder.PackageDeclarations);
        var firstAssets = firstIndex.Assets.Where(asset => ReferenceEquals(asset.Package, firstPackage)).ToArray();
        var secondAssets = secondIndex.Assets.Where(asset => ReferenceEquals(asset.Package, secondPackage)).ToArray();
        var assetPaths = firstAssets
            .Select(asset => new Uri(asset.Data.Uri).AbsolutePath)
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
        firstIndex.Controls.Count.ShouldBeGreaterThan(0);
        firstIndex.Controls.Count.ShouldBe(secondIndex.Controls.Count);
        for (var index = 0; index < firstIndex.Controls.Count; index++)
        {
            firstIndex.Controls[index].ShouldNotBeSameAs(secondIndex.Controls[index]);
            var first = firstIndex.Controls[index].GetDescriptor();
            var second = secondIndex.Controls[index].GetDescriptor();
            first.CompilationIdentity.ShouldBeSameAs(second.CompilationIdentity);
            first.OwnTokens.ShouldBeSameAs(second.OwnTokens);
            first.Slot.ShouldBe(second.Slot);
        }
        AssertSharedDescriptors(firstIndex.SemanticEntries.Select(entry => entry.GetSemantic()).ToArray(),
            secondIndex.SemanticEntries.Select(entry => entry.GetSemantic()).ToArray());
        var firstResources = firstAssets.Select(asset => asset.GetRegistration()).ToArray();
        var secondResources = secondAssets.Select(asset => asset.GetRegistration()).ToArray();
        AssertSharedDescriptors(firstResources, secondResources);
        firstResources[0].Factory().ShouldNotBeSameAs(secondResources[0].Factory());
        using var firstManager = firstBuilder.ThemeManagerBuilder.Build();
        using var secondManager = secondBuilder.ThemeManagerBuilder.Build();
        firstManager.ShouldNotBeSameAs(secondManager);
        firstManager.SemanticParts.ShouldNotBeSameAs(secondManager.SemanticParts);
    }

    [AvaloniaFact]
    public void All_optional_packages_initialize_and_freeze_with_desktop_enabled_explicitly()
    {
        var app = new Application();
        var tooltipBeforeCommit = AvaloniaLocator.Current.GetService<ToolTipService>();
        IAtomUIBuilder? saved = null;
        ApplicationStartup.AttachWith(app, builder =>
        {
            saved = builder;
            builder.UseDesktopControls().UseDesktopDataGrid().UseDesktopColorPicker().UseDesktopExtras();
        });
        using var scope = ApplicationScopeRegistry.Get(app);
        var tooltipAfterCommit = AvaloniaLocator.Current.GetService<ToolTipService>();
        tooltipAfterCommit.ShouldNotBeNull();
        tooltipAfterCommit.ShouldNotBeSameAs(tooltipBeforeCommit);
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
